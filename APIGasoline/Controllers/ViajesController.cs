using APIGasoline.Application.Features.Viajes.Commands;
using APIGasoline.Application.Features.Viajes.DTOs;
using APIGasoline.Application.Features.Viajes.Queries;
using APIGasoline.Contracts.Viajes;
using APIGasoline.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace APIGasoline.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ViajesController : ControllerBase
{
    private readonly ISender _sender;

    public ViajesController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Lista del más reciente al más antiguo.
    /// Filtros opcionales: ?empresaId=, ?carroId=, ?empleadoOperadorId=, ?solicitudReservaId=, ?estado=enCurso
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ViajeDto>>> GetAll(
        [FromQuery] int? empresaId,
        [FromQuery] int? carroId,
        [FromQuery] int? empleadoOperadorId,
        [FromQuery] int? solicitudReservaId,
        [FromQuery] EstadoViaje? estado,
        CancellationToken cancellationToken)
    {
        var viajes = await _sender.Send(
            new GetViajesQuery(empresaId, carroId, empleadoOperadorId, solicitudReservaId, estado), cancellationToken);

        return Ok(viajes);
    }

    [HttpGet("{viajeId:int}")]
    public async Task<ActionResult<ViajeDto>> GetById(int viajeId, CancellationToken cancellationToken)
    {
        var viaje = await _sender.Send(new GetViajeByIdQuery(viajeId), cancellationToken);
        return Ok(viaje);
    }

    /// <summary>
    /// Inicia un viaje (en línea).
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ViajeDto>> Iniciar(IniciarViajeRequest request, CancellationToken cancellationToken)
    {
        var command = new IniciarViajeCommand(
            request.EmpresaId,
            request.CarroId,
            request.SolicitudReservaId,
            request.EmpleadoOperadorId,
            request.FechaInicio!.Value,
            request.UbicacionInicioLat,
            request.UbicacionInicioLng,
            request.KilometrajeInicial);

        var viaje = await _sender.Send(command, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { viajeId = viaje.ViajeId }, viaje);
    }

    [HttpPost("{viajeId:int}/finalizar")]
    public async Task<ActionResult<ViajeDto>> Finalizar(int viajeId, FinalizarViajeRequest request, CancellationToken cancellationToken)
    {
        var command = new FinalizarViajeCommand(
            viajeId,
            request.FechaFin!.Value,
            request.UbicacionFinLat,
            request.UbicacionFinLng,
            request.KilometrajeFinal!.Value);

        return Ok(await _sender.Send(command, cancellationToken));
    }

    [HttpPost("{viajeId:int}/interrumpir")]
    public async Task<ActionResult<ViajeDto>> Interrumpir(int viajeId, InterrumpirViajeRequest request, CancellationToken cancellationToken)
    {
        var command = new InterrumpirViajeCommand(
            viajeId,
            request.Motivo,
            request.FechaFin,
            request.UbicacionFinLat,
            request.UbicacionFinLng,
            request.KilometrajeFinal);

        return Ok(await _sender.Send(command, cancellationToken));
    }

    /// <summary>
    /// Sube un viaje registrado sin conexión (queda con sincronizado = true).
    /// </summary>
    [HttpPost("sincronizar")]
    public async Task<ActionResult<ViajeDto>> Sincronizar(SincronizarViajeRequest request, CancellationToken cancellationToken)
    {
        var command = new SincronizarViajeCommand(
            request.EmpresaId,
            request.CarroId,
            request.SolicitudReservaId,
            request.EmpleadoOperadorId,
            request.FechaInicio!.Value,
            request.UbicacionInicioLat,
            request.UbicacionInicioLng,
            request.KilometrajeInicial,
            request.Estado,
            request.FechaFin,
            request.UbicacionFinLat,
            request.UbicacionFinLng,
            request.KilometrajeFinal,
            request.MotivoInterrupcion);

        var viaje = await _sender.Send(command, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { viajeId = viaje.ViajeId }, viaje);
    }

    [HttpDelete("{viajeId:int}")]
    public async Task<IActionResult> Delete(int viajeId, CancellationToken cancellationToken)
    {
        await _sender.Send(new DeleteViajeCommand(viajeId), cancellationToken);
        return NoContent();
    }
}
