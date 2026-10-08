using APIGasoline.Application.Features.RutasProcesadas.DTOs;
using APIGasoline.Domain.Interfaces;
using AutoMapper;
using MediatR;

namespace APIGasoline.Application.Features.RutasProcesadas.Queries;

/// <summary>
/// Viajes cuyo kilometraje declarado no cuadra con el GPS (posible fraude o error de captura).
/// </summary>
public record GetRutasConAlertaQuery(int? EmpresaId = null) : IRequest<IReadOnlyList<RutaProcesadaDto>>;

public class GetRutasConAlertaQueryHandler : IRequestHandler<GetRutasConAlertaQuery, IReadOnlyList<RutaProcesadaDto>>
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
