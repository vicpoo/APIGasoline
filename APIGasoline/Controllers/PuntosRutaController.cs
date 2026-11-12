using APIGasoline.Extensions;
using APIGasoline.Application.Features.PuntosRuta.Commands;
using APIGasoline.Application.Features.PuntosRuta.Common;
using APIGasoline.Application.Features.PuntosRuta.DTOs;
using APIGasoline.Application.Features.PuntosRuta.Queries;
using APIGasoline.Contracts.PuntosRuta;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using APIGasoline.Application.Common.Security;
using APIGasoline.Authorization;

namespace APIGasoline.Controllers;

/// <summary>
/// Los puntos se manejan por viaje: /api/viajes/{viajeId}/puntos-ruta
/// </summary>
[ApiController]
public class PuntosRutaController : ControllerBase
{
    private readonly ISender _sender;

    public PuntosRutaController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Ruta del viaje en orden de captura (para dibujarla en el mapa).
    /// </summary>
    [RequierePrivilegio(Permisos.ViajeVerDetalle, Permisos.ViajeRegistrar)]
    [HttpGet("api/viajes/{viajeId:guid}/puntos-ruta")]
    public async Task<ActionResult<IReadOnlyList<PuntoRutaDto>>> GetByViaje(
        Guid viajeId,
        [FromQuery] int? pagina,
        [FromQuery] int? tamanoPagina,
        CancellationToken cancellationToken)
    {
        // Página grande (1000 por defecto, máximo 5000): un punto pesa poco
        var puntos = await _sender.Send(new GetPuntosRutaByViajeQuery(viajeId, pagina, tamanoPagina), cancellationToken);
        return this.OkPaginado(puntos);
    }

    /// <summary>
    /// La ruta del viaje como archivo GPX, para compararla con otra app (Strava, GPX Studio, Google Earth).
    /// </summary>
    [RequierePrivilegio(Permisos.ViajeVerDetalle, Permisos.ViajeRegistrar)]
    [HttpGet("api/viajes/{viajeId:guid}/puntos-ruta/gpx")]
    public async Task<IActionResult> GetGpx(Guid viajeId, CancellationToken cancellationToken)
    {
        // El GPX lleva la ruta completa
        var puntos = await _sender.Send(new GetRutaCompletaViajeQuery(viajeId), cancellationToken);
        return File(ExportadorGpx.Generar(viajeId, puntos), "application/gpx+xml", $"viaje-{viajeId}.gpx");
    }

    /// <summary>
    /// Registra un lote de puntos GPS (máximo 1000). Se puede reenviar sin duplicar.
    /// </summary>
    [RequierePrivilegio(Permisos.ViajeRegistrar)]
    [HttpPost("api/viajes/{viajeId:guid}/puntos-ruta")]
    public async Task<ActionResult<LotePuntosRutaResultadoDto>> RegistrarLote(
        Guid viajeId,
        RegistrarLotePuntosRutaRequest request,
        CancellationToken cancellationToken)
    {
        var puntos = request.Puntos
            .Select(p => new PuntoRutaNuevoDto(p.Latitud, p.Longitud, p.TimestampCaptura!.Value, p.PrecisionMetros, p.Altitud, p.Velocidad))
            .ToList();

        var resultado = await _sender.Send(new RegistrarLotePuntosRutaCommand(viajeId, puntos), cancellationToken);
        return Ok(resultado);
    }

    [RequierePrivilegio(Permisos.ViajeVerDetalle, Permisos.ViajeRegistrar)]
    [HttpGet("api/puntos-ruta/{puntoRutaId:guid}")]
    public async Task<ActionResult<PuntoRutaDto>> GetById(Guid puntoRutaId, CancellationToken cancellationToken)
    {
        var punto = await _sender.Send(new GetPuntoRutaByIdQuery(puntoRutaId), cancellationToken);
        return Ok(punto);
    }

    [RequierePrivilegio(Permisos.ViajeEliminar)]
    [HttpDelete("api/puntos-ruta/{puntoRutaId:guid}")]
    public async Task<IActionResult> Delete(Guid puntoRutaId, CancellationToken cancellationToken)
    {
        await _sender.Send(new DeletePuntoRutaCommand(puntoRutaId), cancellationToken);
        return NoContent();
    }
}
