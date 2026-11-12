using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Common.Security;
using APIGasoline.Application.Common.Interfaces;
using APIGasoline.Application.Features.UsuarioRoles.DTOs;
using APIGasoline.Domain.Interfaces;
using AutoMapper;
using APIGasoline.Application.Common.Messaging;

namespace APIGasoline.Application.Features.UsuarioRoles.Commands;

public sealed record UpdateUsuarioRolCommand(Guid UsuarioRolId, Guid UsuarioId, Guid RolId) : ICommand<UsuarioRolDto>;

public sealed class UpdateUsuarioRolCommandHandler : ICommandHandler<UpdateUsuarioRolCommand, UsuarioRolDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    private readonly IUsuarioActual _usuarioActual;

    public UpdateUsuarioRolCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, IUsuarioActual usuarioActual)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _usuarioActual = usuarioActual;
    }

    public async Task<UsuarioRolDto> Handle(UpdateUsuarioRolCommand request, CancellationToken cancellationToken)
    {
        var usuarioRol = await _unitOfWork.UsuarioRoles.GetByIdAsync(request.UsuarioRolId, cancellationToken)
            ?? throw new NotFoundException($"No existe una asignación usuario-rol con el id {request.UsuarioRolId}.");

        // Techo de privilegios: el rol que se quita...
        await TechoPrivilegios.ExigirRolAsync(_unitOfWork, _usuarioActual, usuarioRol.RolId, cancellationToken);

        // El dominio valida los datos
        usuarioRol.Reasignar(request.UsuarioId, request.RolId);

        if (!await _unitOfWork.Usuarios.ExistsAsync(usuarioRol.UsuarioId, cancellationToken))
            throw new NotFoundException($"No existe un usuario con el id {usuarioRol.UsuarioId}.");

        var rol = await _unitOfWork.Roles.GetByIdAsync(usuarioRol.RolId, cancellationToken)
            ?? throw new NotFoundException($"No existe un rol con el id {usuarioRol.RolId}.");

        // Un rol exclusivo de empresa solo se asigna a usuarios cuyo empleado es de esa empresa
        if (!rol.EsGlobal)
        {
            var empleado = await _unitOfWork.Empleados.GetByUsuarioIdAsync(usuarioRol.UsuarioId, cancellationToken);

            if (!rol.DisponiblePara(empleado?.EmpresaId))
                throw new BadRequestException(
                    $"El rol '{rol.Nombre}' es exclusivo de la empresa {rol.EmpresaId} y el usuario no pertenece a ella.");
        }

        // ...y el que se da
        await TechoPrivilegios.ExigirRolAsync(_unitOfWork, _usuarioActual, rol.RolId, cancellationToken);

        if (await _unitOfWork.UsuarioRoles.AsignacionExistsAsync(usuarioRol.UsuarioId, usuarioRol.RolId, usuarioRol.UsuarioRolId, cancellationToken))
            throw new ConflictException($"El usuario {usuarioRol.UsuarioId} ya tiene asignado el rol {usuarioRol.RolId}.");

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<UsuarioRolDto>(usuarioRol);
    }
}
