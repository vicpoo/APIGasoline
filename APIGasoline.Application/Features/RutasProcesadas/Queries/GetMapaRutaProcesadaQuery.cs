using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Features.RutasProcesadas.Common;
using APIGasoline.Application.Features.RutasProcesadas.DTOs;
using APIGasoline.Domain.Interfaces;
using MediatR;

namespace APIGasoline.Application.Features.RutasProcesadas.Queries;

public record GetMapaRutaProcesadaQuery(Guid ViajeId) : IRequest<MapaRutaDto>;

public class GetMapaRutaProcesadaQueryHandler : IRequestHandler<GetMapaRutaProcesadaQuery, MapaRutaDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetMapaRutaProcesadaQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<MapaRutaDto> Handle(GetMapaRutaProcesadaQuery request, CancellationToken cancellationToken)
    {
        if (await _unitOfWork.Viajes.GetByIdAsync(request.ViajeId, cancellationToken) is null)
            throw new NotFoundException($"No existe un viaje con el id {request.ViajeId}.");

        var ruta = await _unitOfWork.RutasProcesadas.GetByViajeAsync(request.ViajeId, cancellationToken)
            ?? throw new NotFoundException($"El viaje {request.ViajeId} aún no tiene ruta procesada.");

        return new MapaRutaDto(ruta.ViajeId, ruta.Polyline, MapaRuta.Coordenadas(ruta.Polyline));
    }
}
