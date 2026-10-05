using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Features.PuntosRuta.DTOs;
using APIGasoline.Domain.Interfaces;
using AutoMapper;
using MediatR;

namespace APIGasoline.Application.Features.PuntosRuta.Queries;

/// <summary>
/// Puntos del viaje en orden de captura, listos para dibujar la ruta en el mapa.
/// </summary>
public record GetPuntosRutaByViajeQuery(int ViajeId) : IRequest<IReadOnlyList<PuntoRutaDto>>;

public class GetPuntosRutaByViajeQueryHandler : IRequestHandler<GetPuntosRutaByViajeQuery, IReadOnlyList<PuntoRutaDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetPuntosRutaByViajeQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<PuntoRutaDto>> Handle(GetPuntosRutaByViajeQuery request, CancellationToken cancellationToken)
    {
        if (await _unitOfWork.Viajes.GetByIdAsync(request.ViajeId, cancellationToken) is null)
            throw new NotFoundException($"No existe un viaje con el id {request.ViajeId}.");

        var puntos = await _unitOfWork.PuntosRuta.GetByViajeAsync(request.ViajeId, cancellationToken);
        return _mapper.Map<IReadOnlyList<PuntoRutaDto>>(puntos);
    }
}
