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

    public GetCargasCombustibleQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<CargaCombustibleDto>> Handle(GetCargasCombustibleQuery request, CancellationToken cancellationToken)
    {
        var cargas = await _unitOfWork.CargasCombustible.GetAllAsync(
            request.EmpresaId, request.ViajeId, request.Desde, request.Hasta, cancellationToken);

        return _mapper.Map<IReadOnlyList<CargaCombustibleDto>>(cargas);
    }
}
