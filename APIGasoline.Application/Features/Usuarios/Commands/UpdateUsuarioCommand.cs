using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Features.Usuarios.DTOs;
using APIGasoline.Domain.Enums;
using APIGasoline.Domain.Interfaces;
using AutoMapper;
using MediatR;

namespace APIGasoline.Application.Features.Usuarios.Commands;

public record UpdateUsuarioCommand(Guid UsuarioId, string Email, EstadoUsuario Estado) : IRequest<UsuarioDto>;

public class UpdateUsuarioCommandHandler : IRequestHandler<UpdateUsuarioCommand, UsuarioDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdateUsuarioCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<UsuarioDto> Handle(UpdateUsuarioCommand request, CancellationToken cancellationToken)
    {
        var usuario = await _unitOfWork.Usuarios.GetByIdAsync(request.UsuarioId, cancellationToken)
            ?? throw new NotFoundException($"No existe un usuario con el id {request.UsuarioId}.");

        // El dominio valida y normaliza los datos
        usuario.CambiarEmail(request.Email);
        usuario.CambiarEstado(request.Estado);

        if (await _unitOfWork.Usuarios.EmailExistsAsync(usuario.Email, usuario.UsuarioId, cancellationToken))
            throw new ConflictException($"Ya existe otro usuario con el email '{usuario.Email}'.");

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<UsuarioDto>(usuario);
    }
}
