using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Features.Roles.DTOs;
using APIGasoline.Domain.Interfaces;
using AutoMapper;
using MediatR;

namespace APIGasoline.Application.Features.Roles.Commands;

/// <summary>
/// La empresa del rol no se cambia (un rol global ya asignado no puede volverse exclusivo).
/// </summary>
public record UpdateRolCommand(int RolId, string Nombre, string? Descripcion, bool Activo) : IRequest<RolDto>;

public class UpdateRolCommandHandler : IRequestHandler<UpdateRolCommand, RolDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdateRolCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<RolDto> Handle(UpdateRolCommand request, CancellationToken cancellationToken)
    {
        var rol = await _unitOfWork.Roles.GetByIdAsync(request.RolId, cancellationToken)
            ?? throw new NotFoundException($"No existe un rol con el id {request.RolId}.");

        // El dominio valida y normaliza los datos
        rol.CambiarNombre(request.Nombre);
        rol.CambiarDescripcion(request.Descripcion);

        if (request.Activo)
            rol.Activar();
        else
            rol.Desactivar();

        if (await _unitOfWork.Roles.NombreExistsAsync(rol.Nombre, rol.EmpresaId, rol.RolId, cancellationToken))
            throw new ConflictException($"Ya existe otro rol con el nombre '{rol.Nombre}' en el mismo ámbito.");

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<RolDto>(rol);
    }
}
