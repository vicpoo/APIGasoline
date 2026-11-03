using APIGasoline.Application.Common.Security;
using APIGasoline.Application.Features.Asignaciones.Commands;
using APIGasoline.Application.Features.Asignaciones.DTOs;
using APIGasoline.Application.Features.Asignaciones.Queries;
using APIGasoline.Application.Features.SolicitudesReserva.DTOs;
using APIGasoline.Authorization;
using APIGasoline.Contracts.Asignaciones;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace APIGasoline.Controllers;

/// <summary>
/// Vista Asignaciones: quién tiene cada carro y desvincularlo para que otro empleado lo pueda usar.
/// Todo requiere asignacion.gestionar. (El empleado deja SU carro con POST /api/solicitudes-reserva/{id}/liberar.)
/// </summary>
[ApiController]
[Route("api/asignaciones")]
public class AsignacionesController : ControllerBase
{
    private readonly ISender _sender;

    public AsignacionesController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Solo lo vinculado: los carros que tiene algún empleado y los empleados con al menos un carro.
    /// Cada asignación indica si tiene un viaje en curso (entonces no se puede desvincular).
    /// </summary>
    [RequierePrivilegio(Permisos.AsignacionGestionar)]
    [HttpGet]
    public async Task<ActionResult<AsignacionesDto>> Get([FromQuery] Guid? empresaId, CancellationToken cancellationToken)
        => Ok(await _sender.Send(new GetAsignacionesQuery(empresaId), cancellationToken));

    /// <summary>
    /// Le quita el carro al empleado: la reserva queda finalizada y el carro disponible.
    /// 409 si hay un viaje en curso o en pausa, o si la reserva ya no está aprobada.
    /// </summary>
    [RequierePrivilegio(Permisos.AsignacionGestionar)]
    [HttpPost("{solicitudReservaId:guid}/desvincular")]
    public async Task<ActionResult<SolicitudReservaDto>> Desvincular(
        Guid solicitudReservaId,
        DesvincularAsignacionRequest request,
        CancellationToken cancellationToken)
        => Ok(await _sender.Send(new DesvincularAsignacionCommand(solicitudReservaId, request.Motivo), cancellationToken));
}
