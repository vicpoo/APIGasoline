using APIGasoline.Application.Features.SolicitudesReserva.Commands;
using APIGasoline.Application.Features.SolicitudesReserva.DTOs;
using APIGasoline.Application.Features.SolicitudesReserva.Queries;
using APIGasoline.Contracts.SolicitudesReserva;
using APIGasoline.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using APIGasoline.Application.Common.Security;
using APIGasoline.Authorization;

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
    /// Lista de la más antigua a la más reciente, con nombre y foto del solicitante y el carro.
    /// Filtros opcionales: ?empresaId=, ?carroId=, ?empleadoSolicitanteId=, ?estado=pendiente
    /// </summary>
    [RequierePrivilegio(Permisos.SolicitudVerPropias, Permisos.SolicitudVerTodas, Permisos.ViajeRegistrar)]
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

    /// <summary>
    /// Detalle: motivo, quién la pide (foto y licencia), qué carro y quién la resolvió.
    /// </summary>
    [RequierePrivilegio(Permisos.SolicitudVerPropias, Permisos.SolicitudVerTodas, Permisos.ViajeRegistrar)]
    [HttpGet("{solicitudReservaId:guid}")]
    public async Task<ActionResult<SolicitudReservaDetalleDto>> GetById(Guid solicitudReservaId, CancellationToken cancellationToken)
    {
        var solicitud = await _sender.Send(new GetSolicitudReservaByIdQuery(solicitudReservaId), cancellationToken);
        return Ok(solicitud);
    }

    [RequierePrivilegio(Permisos.SolicitudCrear)]
    [HttpPost]
    public async Task<ActionResult<SolicitudReservaDto>> Create(CreateSolicitudReservaRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateSolicitudReservaCommand(request.EmpresaId, request.CarroId, request.EmpleadoSolicitanteId, request.Motivo);
        var solicitud = await _sender.Send(command, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { solicitudReservaId = solicitud.SolicitudReservaId }, solicitud);
    }

    /// <summary>
    /// Aprueba la solicitud, ocupa el carro y rechaza las demás pendientes de ese carro.
    /// </summary>
    [RequierePrivilegio(Permisos.SolicitudAprobar)]
    [HttpPost("{solicitudReservaId:guid}/aprobar")]
    public async Task<ActionResult<SolicitudReservaDto>> Aprobar(Guid solicitudReservaId, AprobarSolicitudReservaRequest request, CancellationToken cancellationToken)
    {
        var command = new AprobarSolicitudReservaCommand(solicitudReservaId, request.EmpleadoConfirmadorId, request.Motivo);
        return Ok(await _sender.Send(command, cancellationToken));
    }

    [RequierePrivilegio(Permisos.SolicitudRechazar)]
    [HttpPost("{solicitudReservaId:guid}/rechazar")]
    public async Task<ActionResult<SolicitudReservaDto>> Rechazar(Guid solicitudReservaId, RechazarSolicitudReservaRequest request, CancellationToken cancellationToken)
    {
        var command = new RechazarSolicitudReservaCommand(solicitudReservaId, request.EmpleadoConfirmadorId, request.Motivo);
        return Ok(await _sender.Send(command, cancellationToken));
    }

    /// <summary>
    /// Cancela una solicitud pendiente o aprobada (si estaba aprobada, libera el carro).
    /// </summary>
    [RequierePrivilegio(Permisos.SolicitudCrear, Permisos.SolicitudAprobar)]
    [HttpPost("{solicitudReservaId:guid}/cancelar")]
    public async Task<ActionResult<SolicitudReservaDto>> Cancelar(Guid solicitudReservaId, CancelarSolicitudReservaRequest request, CancellationToken cancellationToken)
    {
        var command = new CancelarSolicitudReservaCommand(solicitudReservaId, request.Motivo);
        return Ok(await _sender.Send(command, cancellationToken));
    }

    /// <summary>
    /// HU-USO-04: el operador libera el carro al terminar sus viajes. La reserva queda "finalizada"
    /// y el carro vuelve a "disponible". No se puede liberar con un viaje en curso.
    /// </summary>
    [RequierePrivilegio(Permisos.ViajeRegistrar, Permisos.SolicitudAprobar)]
    [HttpPost("{solicitudReservaId:guid}/liberar")]
    public async Task<ActionResult<SolicitudReservaDto>> Liberar(Guid solicitudReservaId, LiberarSolicitudReservaRequest request, CancellationToken cancellationToken)
    {
        var command = new LiberarSolicitudReservaCommand(solicitudReservaId, request.FechaLiberacion, request.Observaciones);
        return Ok(await _sender.Send(command, cancellationToken));
    }

    [RequierePrivilegio(Permisos.SolicitudAprobar)]
    [HttpDelete("{solicitudReservaId:guid}")]
    public async Task<IActionResult> Delete(Guid solicitudReservaId, CancellationToken cancellationToken)
    {
        await _sender.Send(new DeleteSolicitudReservaCommand(solicitudReservaId), cancellationToken);
        return NoContent();
    }
}
