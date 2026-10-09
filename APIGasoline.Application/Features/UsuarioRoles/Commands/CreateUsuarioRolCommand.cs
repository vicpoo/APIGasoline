using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Features.UsuarioRoles.DTOs;
using APIGasoline.Domain.Entities;
using APIGasoline.Domain.Interfaces;
using AutoMapper;
using MediatR;

namespace APIGasoline.Application.Features.UsuarioRoles.Commands;

public record CreateUsuarioRolCommand(Guid UsuarioId, Guid RolId) : IRequest<UsuarioRolDto>;

public class CreateUsuarioRolCommandHandler : IRequestHandler<CreateUsuarioRolCommand, UsuarioRolDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateUsuarioRolCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<UsuarioRolDto> Handle(CreateUsuarioRolCommand request, CancellationToken cancellationToken)
    {
        // El dominio valida los datos
        var usuarioRol = UsuarioRol.Crear(request.UsuarioId, request.RolId);

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

        if (await _unitOfWork.UsuarioRoles.AsignacionExistsAsync(usuarioRol.UsuarioId, usuarioRol.RolId, null, cancellationToken))
            throw new ConflictException($"El usuario {usuarioRol.UsuarioId} ya tiene asignado el rol {usuarioRol.RolId}.");

        await _unitOfWork.UsuarioRoles.AddAsync(usuarioRol, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<UsuarioRolDto>(usuarioRol);
    }
}
