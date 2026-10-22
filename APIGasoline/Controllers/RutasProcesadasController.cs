using APIGasoline.Application.Features.RutasProcesadas.Commands;
using APIGasoline.Application.Features.RutasProcesadas.DTOs;
using APIGasoline.Application.Features.RutasProcesadas.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using APIGasoline.Application.Common.Security;
using APIGasoline.Authorization;

namespace APIGasoline.Controllers;

/// <summary>
/// Resumen de la ruta GPS de cada viaje calculado en el servidor (distancia, tiempos,
/// comparación con el odómetro, km/L y polyline). Se genera solo al cerrar el viaje.
/// </summary>
[ApiController]
public class RutasProcesadasController : ControllerBase
{
    private readonly ISender _sender;

    public RutasProcesadasController(ISender sender)
    {
        _sender = sender;
    }

    [RequierePrivilegio(Permisos.ViajeVerDetalle)]
    [HttpGet("api/viajes/{viajeId:guid}/ruta-procesada")]
    public async Task<ActionResult<RutaProcesadaDto>> GetByViaje(Guid viajeId, CancellationToken cancellationToken)
    {
        var ruta = await _sender.Send(new GetRutaProcesadaByViajeQuery(viajeId), cancellationToken);
        return Ok(ruta);
    }

    /// <summary>La polyline decodificada en coordenadas (para dibujar o revisar la ruta).</summary>
    [RequierePrivilegio(Permisos.ViajeVerDetalle)]
    [HttpGet("api/viajes/{viajeId:guid}/ruta-procesada/mapa")]
    public async Task<ActionResult<MapaRutaDto>> GetMapa(Guid viajeId, CancellationToken cancellationToken)
    {
        var mapa = await _sender.Send(new GetMapaRutaProcesadaQuery(viajeId), cancellationToken);
        return Ok(mapa);
    }

    /// <summary>
    /// Vuelve a calcular la ruta con los puntos y cargas actuales (por ejemplo, si falló el
    /// proceso automático o si mejoró el algoritmo). Solo para viajes finalizados o interrumpidos.
    /// </summary>
    [RequierePrivilegio(Permisos.ViajeVerTodos)]
    [HttpPost("api/viajes/{viajeId:guid}/ruta-procesada/recalcular")]
    public async Task<ActionResult<RutaProcesadaDto>> Recalcular(Guid viajeId, CancellationToken cancellationToken)
    {
        var ruta = await _sender.Send(new ProcesarRutaViajeCommand(viajeId), cancellationToken);
        return Ok(ruta);
    }

    /// <summary>
    /// Viajes cuyo kilometraje del odómetro no cuadra con el GPS (más del 10 % y más de 2 km).
    /// Filtro opcional: ?empresaId=
    /// </summary>
    [RequierePrivilegio(Permisos.ViajeVerTodos, Permisos.AuditoriaVer)]
    [HttpGet("api/rutas-procesadas/alertas")]
    public async Task<ActionResult<IReadOnlyList<RutaProcesadaDto>>> GetAlertas([FromQuery] Guid? empresaId, CancellationToken cancellationToken)
    {
        var rutas = await _sender.Send(new GetRutasConAlertaQuery(empresaId), cancellationToken);
        return Ok(rutas);
    }
}
