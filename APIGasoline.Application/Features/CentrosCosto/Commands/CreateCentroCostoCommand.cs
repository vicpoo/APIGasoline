using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Features.CentrosCosto.Common;
using APIGasoline.Application.Features.CentrosCosto.DTOs;
using APIGasoline.Domain.Entities;
using APIGasoline.Domain.Interfaces;
using AutoMapper;
using APIGasoline.Application.Common.Messaging;

namespace APIGasoline.Application.Features.CentrosCosto.Commands;

/// <param name="CorporativoId">Opcional: corporativo de la misma empresa.</param>
/// <param name="DivisionId">Opcional: división de la misma empresa (ligada al corporativo si vienen los dos).</param>
public sealed record CreateCentroCostoCommand(Guid EmpresaId, string Nombre, string Clave, bool Activo, Guid? CorporativoId = null, Guid? DivisionId = null) : ICommand<CentroCostoDto>;

public sealed class CreateCentroCostoCommandHandler : ICommandHandler<CreateCentroCostoCommand, CentroCostoDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateCentroCostoCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<CentroCostoDto> Handle(CreateCentroCostoCommand request, CancellationToken cancellationToken)
    {
        // El dominio valida y normaliza los datos
        var centroCosto = CentroCosto.Crear(request.EmpresaId, request.Nombre, request.Clave, request.Activo, request.CorporativoId, request.DivisionId);

        // Las empresas eliminadas (soft delete) no cuentan como existentes
        if (!await _unitOfWork.Empresas.ExistsAsync(centroCosto.EmpresaId, cancellationToken))
            throw new NotFoundException($"No existe una empresa con el id {centroCosto.EmpresaId}.");

        await CentroCostoOrganizacionValidator.ValidarAsync(_unitOfWork, centroCosto, cancellationToken);

        if (await _unitOfWork.CentrosCosto.ClaveExistsAsync(centroCosto.EmpresaId, centroCosto.Clave, null, cancellationToken))
            throw new ConflictException($"La empresa {centroCosto.EmpresaId} ya tiene un centro de costo con la clave '{centroCosto.Clave}'.");

        await _unitOfWork.CentrosCosto.AddAsync(centroCosto, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // CentroCostoId y FechaRegistro ya vienen asignados por la base de datos
        return _mapper.Map<CentroCostoDto>(centroCosto);
    }
}
