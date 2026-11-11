using APIGasoline.Application.Features.RutasProcesadas.DTOs;
using APIGasoline.Domain.Interfaces;
using AutoMapper;
using APIGasoline.Application.Common.Messaging;

namespace APIGasoline.Application.Features.RutasProcesadas.Queries;

/// <summary>
/// Viajes cuyo kilometraje declarado no cuadra con el GPS (posible fraude o error de captura).
/// </summary>
public sealed record GetRutasConAlertaQuery(Guid? EmpresaId = null) : IQuery<IReadOnlyList<RutaProcesadaDto>>;

public sealed class GetRutasConAlertaQueryHandler : IQueryHandler<GetRutasConAlertaQuery, IReadOnlyList<RutaProcesadaDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetRutasConAlertaQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<RutaProcesadaDto>> Handle(GetRutasConAlertaQuery request, CancellationToken cancellationToken)
    {
        var rutas = await _unitOfWork.RutasProcesadas.GetConAlertaAsync(request.EmpresaId, cancellationToken);
        return _mapper.Map<IReadOnlyList<RutaProcesadaDto>>(rutas);
    }
}
