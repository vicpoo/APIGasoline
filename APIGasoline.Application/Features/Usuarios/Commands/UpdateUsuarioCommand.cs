using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Common.Interfaces;
using APIGasoline.Application.Common.Sesiones;
using APIGasoline.Application.Features.Usuarios.DTOs;
using APIGasoline.Domain.Enums;
using APIGasoline.Domain.Interfaces;
using AutoMapper;
using APIGasoline.Application.Common.Messaging;

namespace APIGasoline.Application.Features.Usuarios.Commands;

public sealed record UpdateUsuarioCommand(Guid UsuarioId, string Email, EstadoUsuario Estado) : ICommand<UsuarioDto>;

public sealed class UpdateUsuarioCommandHandler : ICommandHandler<UpdateUsuarioCommand, UsuarioDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IUsuarioActual _usuarioActual;
    private readonly IDateTimeProvider _dateTime;

    public UpdateUsuarioCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, IUsuarioActual usuarioActual, IDateTimeProvider dateTime)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _usuarioActual = usuarioActual;
        _dateTime = dateTime;
    }

    public async Task<UsuarioDto> Handle(UpdateUsuarioCommand request, CancellationToken cancellationToken)
    {
        var usuario = await _unitOfWork.Usuarios.GetByIdAsync(request.UsuarioId, cancellationToken)
            ?? throw new NotFoundException($"No existe un usuario con el id {request.UsuarioId}.");

        var estabaActivo = usuario.EstaActivo;

        // El dominio valida y normaliza los datos
        usuario.CambiarEmail(request.Email);
        usuario.CambiarEstado(request.Estado);

        if (await _unitOfWork.Usuarios.EmailExistsAsync(usuario.Email, usuario.UsuarioId, cancellationToken))
            throw new ConflictException($"Ya existe otro usuario con el email '{usuario.Email}'.");

        // Desactivado: se cierran todas sus sesiones (ya no podrá renovar su token)
        if (estabaActivo && !usuario.EstaActivo)
        {
            await _unitOfWork.RevocarDeUsuarioAsync(
                usuario.UsuarioId, MotivoRevocacionSesion.UsuarioDesactivado, _dateTime.Now, _usuarioActual.UsuarioId,
                cancellationToken: cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<UsuarioDto>(usuario);
    }
}
