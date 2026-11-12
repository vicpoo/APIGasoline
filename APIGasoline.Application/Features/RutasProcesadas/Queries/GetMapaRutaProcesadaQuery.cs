using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Common.Interfaces;
using APIGasoline.Application.Features.Viajes.Common;
using APIGasoline.Application.Features.RutasProcesadas.Common;
using APIGasoline.Application.Features.RutasProcesadas.DTOs;
using APIGasoline.Domain.Interfaces;
using APIGasoline.Application.Common.Messaging;

namespace APIGasoline.Application.Features.RutasProcesadas.Queries;

public sealed record GetMapaRutaProcesadaQuery(Guid ViajeId) : IQuery<MapaRutaDto>;

public sealed class GetMapaRutaProcesadaQueryHandler : IQueryHandler<GetMapaRutaProcesadaQuery, MapaRutaDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUsuarioActual _usuarioActual;

    public GetMapaRutaProcesadaQueryHandler(IUnitOfWork unitOfWork, IUsuarioActual usuarioActual)
    {
        _unitOfWork = unitOfWork;
        _usuarioActual = usuarioActual;
    }

    public async Task<MapaRutaDto> Handle(GetMapaRutaProcesadaQuery request, CancellationToken cancellationToken)
    {
        var viaje = await _unitOfWork.Viajes.GetByIdAsync(request.ViajeId, cancellationToken)
            ?? throw new NotFoundException($"No existe un viaje con el id {request.ViajeId}.");

        await AccesoViaje.ValidarLecturaDetalleAsync(_usuarioActual, viaje, cancellationToken);

        var ruta = await _unitOfWork.RutasProcesadas.GetByViajeAsync(request.ViajeId, cancellationToken)
            ?? throw new NotFoundException($"El viaje {request.ViajeId} aún no tiene ruta procesada.");

        return new MapaRutaDto(ruta.ViajeId, ruta.Polyline, MapaRuta.Coordenadas(ruta.Polyline));
    }
}
