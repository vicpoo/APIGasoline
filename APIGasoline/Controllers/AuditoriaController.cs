using APIGasoline.Application.Features.Auditoria.Commands;
using APIGasoline.Application.Features.Auditoria.DTOs;
using APIGasoline.Application.Features.Auditoria.Queries;
using APIGasoline.Contracts.Auditoria;
using APIGasoline.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using APIGasoline.Application.Common.Security;
using APIGasoline.Authorization;

namespace APIGasoline.Controllers;

/// <summary>
/// Módulo secundario de auditoría y trazabilidad. Requiere token y un perfil autorizado
/// (Administrador, Consultor/Reportes, Confirmador o Gestor de activos).
/// Un empleado solo ve la información de su empresa.
///   A. Historial de cambios        → /eventos
///   B. Revisión de combustible     → /revision-combustible (alertas calculadas al consultar)
///   C. Revisiones y observaciones  → /revisiones
/// </summary>
[ApiController]
[Authorize]
[Route("api/auditoria")]
public class AuditoriaController : ControllerBase
{
    private readonly ISender _sender;

    public AuditoriaController(ISender sender)
    {
        _sender = sender;
    }

    // ---------- A. Historial de cambios ----------

    /// <summary>
    /// Historial del más reciente al más antiguo.
    /// Filtros: ?entidad=carga_combustible&amp;entidadId=, ?usuarioId=, ?accion=corregir, ?desde=, ?hasta=, ?maximo=500
    /// </summary>
    [RequierePrivilegio(Permisos.AuditoriaVer)]
    [HttpGet("eventos")]
    public async Task<ActionResult<IReadOnlyList<AuditoriaEventoDto>>> GetEventos(
        [FromQuery] Guid? empresaId,
        [FromQuery] string? entidad,
        [FromQuery] Guid? entidadId,
        [FromQuery] Guid? usuarioId,
        [FromQuery] AccionAuditoria? accion,
        [FromQuery] DateTime? desde,
        [FromQuery] DateTime? hasta,
        [FromQuery] int maximo = 500,
        CancellationToken cancellationToken = default)
    {
        var eventos = await _sender.Send(
            new GetAuditoriaEventosQuery(empresaId, entidad, entidadId, usuarioId, accion, desde, hasta, maximo),
            cancellationToken);

        return Ok(eventos);
    }

    [RequierePrivilegio(Permisos.AuditoriaVer)]
    [HttpGet("eventos/{auditoriaEventoId:guid}")]
    public async Task<ActionResult<AuditoriaEventoDto>> GetEvento(Guid auditoriaEventoId, CancellationToken cancellationToken)
        => Ok(await _sender.Send(new GetAuditoriaEventoByIdQuery(auditoriaEventoId), cancellationToken));

    // ---------- B. Revisión de combustible ----------

    /// <summary>
    /// Resumen del periodo (viajes, litros, monto, comprobantes) y observaciones detectadas.
    /// Ejemplo: ?carroId=...&amp;desde=2026-09-01&amp;hasta=2026-09-30T23:59:59
    /// Una alerta no significa fraude ni error: indica que alguien debería revisar el registro.
    /// </summary>
    [RequierePrivilegio(Permisos.AuditoriaVer)]
    [HttpGet("revision-combustible")]
    public async Task<ActionResult<RevisionCombustibleDto>> GetRevisionCombustible(
        [FromQuery] Guid? empresaId,
        [FromQuery] Guid? carroId,
        [FromQuery] DateTime desde,
        [FromQuery] DateTime hasta,
        CancellationToken cancellationToken)
    {
        var revision = await _sender.Send(new GetRevisionCombustibleQuery(empresaId, carroId, desde, hasta), cancellationToken);
        return Ok(revision);
    }

    // ---------- C. Revisiones y observaciones ----------

    /// <summary>Filtros: ?viajeId=, ?carroId=, ?estado=conObservaciones, ?abiertas=true</summary>
    [RequierePrivilegio(Permisos.AuditoriaVer)]
    [HttpGet("revisiones")]
    public async Task<ActionResult<IReadOnlyList<RevisionAuditoriaDto>>> GetRevisiones(
        [FromQuery] Guid? empresaId,
        [FromQuery] Guid? viajeId,
        [FromQuery] Guid? carroId,
        [FromQuery] EstadoRevision? estado,
        [FromQuery] bool? abiertas,
        CancellationToken cancellationToken)
    {
        var revisiones = await _sender.Send(new GetRevisionesAuditoriaQuery(empresaId, viajeId, carroId, estado, abiertas), cancellationToken);
        return Ok(revisiones);
    }

    [RequierePrivilegio(Permisos.AuditoriaVer)]
    [HttpGet("revisiones/{revisionAuditoriaId:guid}")]
    public async Task<ActionResult<RevisionAuditoriaDto>> GetRevision(Guid revisionAuditoriaId, CancellationToken cancellationToken)
        => Ok(await _sender.Send(new GetRevisionAuditoriaByIdQuery(revisionAuditoriaId), cancellationToken));

    /// <summary>Abre una revisión de un viaje, o de un carro en un periodo. El revisor es el empleado del token.</summary>
    [RequierePrivilegio(Permisos.AuditoriaRevisar)]
    [HttpPost("revisiones")]
    public async Task<ActionResult<RevisionAuditoriaDto>> CrearRevision(CrearRevisionAuditoriaRequest request, CancellationToken cancellationToken)
    {
        var revision = await _sender.Send(
            new CrearRevisionAuditoriaCommand(request.ViajeId, request.CarroId, request.PeriodoDesde, request.PeriodoHasta, request.Observaciones),
            cancellationToken);

        return CreatedAtAction(nameof(GetRevision), new { revisionAuditoriaId = revision.RevisionAuditoriaId }, revision);
    }

    /// <summary>Registra el resultado (revisado, con observaciones o pendiente de información) y opcionalmente la cierra.</summary>
    [RequierePrivilegio(Permisos.AuditoriaRevisar)]
    [HttpPut("revisiones/{revisionAuditoriaId:guid}/resultado")]
    public async Task<ActionResult<RevisionAuditoriaDto>> RegistrarResultado(
        Guid revisionAuditoriaId,
        RegistrarResultadoRevisionRequest request,
        CancellationToken cancellationToken)
    {
        var revision = await _sender.Send(
            new RegistrarResultadoRevisionCommand(revisionAuditoriaId, request.EstadoRevision!.Value, request.Observaciones, request.Cerrar),
            cancellationToken);

        return Ok(revision);
    }
}
