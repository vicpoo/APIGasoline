using APIGasoline.Application.Features.Outbox.Commands;
using APIGasoline.Application.Features.Outbox.DTOs;
using APIGasoline.Application.Features.Outbox.Queries;
using APIGasoline.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using APIGasoline.Application.Common.Security;
using APIGasoline.Authorization;

namespace APIGasoline.Controllers;

/// <summary>
/// Diagnóstico de las notificaciones push: qué se envió, qué está en reintento y por qué falló.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class OutboxController : ControllerBase
{
    private readonly ISender _sender;

    public OutboxController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Del más reciente al más antiguo.
    /// Filtros opcionales: ?estado=pendiente|procesando|enviado|fallido, ?empresaId=, ?limite=50 (máximo 500)
    /// </summary>
    [RequierePrivilegio(Permisos.OutboxGestionar)]
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<OutboxMensajeDto>>> GetAll(
        [FromQuery] Guid? empresaId,
        [FromQuery] EstadoOutbox? estado,
        [FromQuery] int limite = 50,
        CancellationToken cancellationToken = default)
    {
        var mensajes = await _sender.Send(new GetOutboxMensajesQuery(empresaId, estado, limite), cancellationToken);
        return Ok(mensajes);
    }

    /// <summary>Vuelve a poner en cola un mensaje fallido.</summary>
    [RequierePrivilegio(Permisos.OutboxGestionar)]
    [HttpPost("{outboxMensajeId:guid}/reintentar")]
    public async Task<ActionResult<OutboxMensajeDto>> Reintentar(Guid outboxMensajeId, CancellationToken cancellationToken)
    {
        var mensaje = await _sender.Send(new ReintentarOutboxMensajeCommand(outboxMensajeId), cancellationToken);
        return Ok(mensaje);
    }
}
