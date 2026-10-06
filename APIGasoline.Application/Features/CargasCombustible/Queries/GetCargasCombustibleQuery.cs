using APIGasoline.Application.Common.Interfaces;
using APIGasoline.Application.Common.Storage;
using APIGasoline.Application.Features.CargasCombustible.DTOs;
using APIGasoline.Domain.Interfaces;
using AutoMapper;
using MediatR;

namespace APIGasoline.Application.Features.CargasCombustible.Queries;

/// <summary>
/// Lista las cargas de la más reciente a la más antigua.
/// Filtros opcionales: empresa, viaje y rango de fechas.
/// </summary>
public record GetCargasCombustibleQuery(
    int? EmpresaId = null,
    int? ViajeId = null,
    DateTime? Desde = null,
    DateTime? Hasta = null) : IRequest<IReadOnlyList<CargaCombustibleDto>>;

public class GetCargasCombustibleQueryHandler : IRequestHandler<GetCargasCombustibleQuery, IReadOnlyList<CargaCombustibleDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IFileStorageService _storage;

    public GetCargasCombustibleQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, IFileStorageService storage)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _storage = storage;
    }

    public async Task<IReadOnlyList<CargaCombustibleDto>> Handle(GetCargasCombustibleQuery request, CancellationToken cancellationToken)
    {
        var cargas = await _unitOfWork.CargasCombustible.GetAllAsync(
            request.EmpresaId, request.ViajeId, request.Desde, request.Hasta, cancellationToken);

        return await _mapper.Map<IReadOnlyList<CargaCombustibleDto>>(cargas).ConUrlsAsync(_storage, cancellationToken);
    }
}
