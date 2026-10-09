using APIGasoline.Application.Features.Notificaciones.Commands;
using APIGasoline.Application.Features.Notificaciones.DTOs;
using APIGasoline.Application.Features.Notificaciones.Queries;
using APIGasoline.Contracts.Notificaciones;
using APIGasoline.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace APIGasoline.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotificacionesController : ControllerBase
{
    private readonly ISender _sender;

    public NotificacionesController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Bandeja de la más reciente a la más antigua.
    /// Filtros opcionales: ?empleadoDestinatarioId=, ?soloNoLeidas=true, ?tipo=nuevaSolicitud, ?empresaId=
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<NotificacionDto>>> GetAll(
        [FromQuery] Guid? empresaId,
        [FromQuery] Guid? empleadoDestinatarioId,
        [FromQuery] bool soloNoLeidas,
        [FromQuery] TipoNotificacion? tipo,
        CancellationToken cancellationToken)
    {
        var notificaciones = await _sender.Send(
            new GetNotificacionesQuery(empresaId, empleadoDestinatarioId, soloNoLeidas, tipo), cancellationToken);

        return Ok(notificaciones);
    }

    [HttpGet("{notificacionId:guid}")]
    public async Task<ActionResult<NotificacionDto>> GetById(Guid notificacionId, CancellationToken cancellationToken)
    {
        var notificacion = await _sender.Send(new GetNotificacionByIdQuery(notificacionId), cancellationToken);
        return Ok(notificacion);
    }

    /// <summary>
    /// Número de no leídas de un empleado (badge de la campana).
    /// </summary>
    [HttpGet("no-leidas/{empleadoDestinatarioId:guid}")]
    public async Task<ActionResult<object>> ContarNoLeidas(Guid empleadoDestinatarioId, CancellationToken cancellationToken)
    {
        var total = await _sender.Send(new ContarNotificacionesNoLeidasQuery(empleadoDestinatarioId), cancellationToken);
        return Ok(new { empleadoDestinatarioId, noLeidas = total });
    }

    /// <summary>
    /// Alta manual. Las de reservas y cambios de estatus se generan solas.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<NotificacionDto>> Create(CreateNotificacionRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateNotificacionCommand(
            request.EmpleadoDestinatarioId,
            request.Tipo!.Value,
            request.ReferenciaId,
            request.Mensaje);

        var notificacion = await _sender.Send(command, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { notificacionId = notificacion.NotificacionId }, notificacion);
    }

    [HttpPost("{notificacionId:guid}/marcar-leida")]
    public async Task<ActionResult<NotificacionDto>> MarcarLeida(Guid notificacionId, CancellationToken cancellationToken)
    {
        return Ok(await _sender.Send(new MarcarNotificacionLeidaCommand(notificacionId), cancellationToken));
    }

    [HttpPost("marcar-todas-leidas/{empleadoDestinatarioId:guid}")]
    public async Task<ActionResult<object>> MarcarTodasLeidas(Guid empleadoDestinatarioId, CancellationToken cancellationToken)
    {
        var marcadas = await _sender.Send(new MarcarTodasNotificacionesLeidasCommand(empleadoDestinatarioId), cancellationToken);
        return Ok(new { empleadoDestinatarioId, marcadas });
    }

    [HttpDelete("{notificacionId:guid}")]
    public async Task<IActionResult> Delete(Guid notificacionId, CancellationToken cancellationToken)
    {
        await _sender.Send(new DeleteNotificacionCommand(notificacionId), cancellationToken);
        return NoContent();
    }
}
