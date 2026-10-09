using APIGasoline.Application.Features.SolicitudesReserva.Commands;
using APIGasoline.Application.Features.SolicitudesReserva.DTOs;
using APIGasoline.Application.Features.SolicitudesReserva.Queries;
using APIGasoline.Contracts.SolicitudesReserva;
using APIGasoline.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace APIGasoline.Controllers;

[ApiController]
[Route("api/solicitudes-reserva")]
public class SolicitudesReservaController : ControllerBase
{
    private readonly ISender _sender;

    public SolicitudesReservaController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Lista de la más antigua a la más reciente.
    /// Filtros opcionales: ?empresaId=, ?carroId=, ?empleadoSolicitanteId=, ?estado=pendiente
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<SolicitudReservaDto>>> GetAll(
        [FromQuery] Guid? empresaId,
        [FromQuery] Guid? carroId,
        [FromQuery] Guid? empleadoSolicitanteId,
        [FromQuery] EstadoSolicitudReserva? estado,
        CancellationToken cancellationToken)
    {
        var solicitudes = await _sender.Send(
            new GetSolicitudesReservaQuery(empresaId, carroId, empleadoSolicitanteId, estado), cancellationToken);

        return Ok(solicitudes);
    }

    [HttpGet("{solicitudReservaId:guid}")]
    public async Task<ActionResult<SolicitudReservaDto>> GetById(Guid solicitudReservaId, CancellationToken cancellationToken)
    {
        var solicitud = await _sender.Send(new GetSolicitudReservaByIdQuery(solicitudReservaId), cancellationToken);
        return Ok(solicitud);
    }

    [HttpPost]
    public async Task<ActionResult<SolicitudReservaDto>> Create(CreateSolicitudReservaRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateSolicitudReservaCommand(request.EmpresaId, request.CarroId, request.EmpleadoSolicitanteId);
        var solicitud = await _sender.Send(command, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { solicitudReservaId = solicitud.SolicitudReservaId }, solicitud);
    }

    /// <summary>
    /// Aprueba la solicitud, ocupa el carro y rechaza las demás pendientes de ese carro.
    /// </summary>
    [HttpPost("{solicitudReservaId:guid}/aprobar")]
    public async Task<ActionResult<SolicitudReservaDto>> Aprobar(Guid solicitudReservaId, AprobarSolicitudReservaRequest request, CancellationToken cancellationToken)
    {
        var command = new AprobarSolicitudReservaCommand(solicitudReservaId, request.EmpleadoConfirmadorId, request.Motivo);
        return Ok(await _sender.Send(command, cancellationToken));
    }

    [HttpPost("{solicitudReservaId:guid}/rechazar")]
    public async Task<ActionResult<SolicitudReservaDto>> Rechazar(Guid solicitudReservaId, RechazarSolicitudReservaRequest request, CancellationToken cancellationToken)
    {
        var command = new RechazarSolicitudReservaCommand(solicitudReservaId, request.EmpleadoConfirmadorId, request.Motivo);
        return Ok(await _sender.Send(command, cancellationToken));
    }

    /// <summary>
    /// Cancela una solicitud pendiente o aprobada (si estaba aprobada, libera el carro).
    /// </summary>
    [HttpPost("{solicitudReservaId:guid}/cancelar")]
    public async Task<ActionResult<SolicitudReservaDto>> Cancelar(Guid solicitudReservaId, CancelarSolicitudReservaRequest request, CancellationToken cancellationToken)
    {
        var command = new CancelarSolicitudReservaCommand(solicitudReservaId, request.Motivo);
        return Ok(await _sender.Send(command, cancellationToken));
    }

    [HttpDelete("{solicitudReservaId:guid}")]
    public async Task<IActionResult> Delete(Guid solicitudReservaId, CancellationToken cancellationToken)
    {
        await _sender.Send(new DeleteSolicitudReservaCommand(solicitudReservaId), cancellationToken);
        return NoContent();
    }
}
