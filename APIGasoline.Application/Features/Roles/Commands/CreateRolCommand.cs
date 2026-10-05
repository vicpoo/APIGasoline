using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Features.Roles.DTOs;
using APIGasoline.Domain.Entities;
using APIGasoline.Domain.Interfaces;
using AutoMapper;
using MediatR;

namespace APIGasoline.Application.Features.Roles.Commands;

/// <summary>EmpresaId null = rol global; con valor = rol exclusivo de esa empresa.</summary>
public record CreateRolCommand(int? EmpresaId, string Nombre, string? Descripcion, bool Activo) : IRequest<RolDto>;

public class CreateRolCommandHandler : IRequestHandler<CreateRolCommand, RolDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateRolCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<RolDto> Handle(CreateRolCommand request, CancellationToken cancellationToken)
    {
        // El dominio valida y normaliza los datos
        var rol = Rol.Crear(request.Nombre, request.Descripcion, request.Activo, request.EmpresaId);

        if (rol.EmpresaId is not null && !await _unitOfWork.Empresas.ExistsAsync(rol.EmpresaId.Value, cancellationToken))
            throw new NotFoundException($"No existe una empresa con el id {rol.EmpresaId}.");

        // Único por empresa. MySQL permite varios NULL en un índice único, por eso
        // la unicidad entre roles globales se valida aquí.
        if (await _unitOfWork.Roles.NombreExistsAsync(rol.Nombre, rol.EmpresaId, null, cancellationToken))
            throw new ConflictException(rol.EsGlobal
                ? $"Ya existe un rol global con el nombre '{rol.Nombre}'."
                : $"La empresa {rol.EmpresaId} ya tiene un rol con el nombre '{rol.Nombre}'.");

        await _unitOfWork.Roles.AddAsync(rol, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // RolId ya viene asignado por la base de datos (autoincremento)
        return _mapper.Map<RolDto>(rol);
    }
}
