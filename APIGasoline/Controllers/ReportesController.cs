using APIGasoline.Application.Common.Security;
using APIGasoline.Application.Features.Reportes.DTOs;
using APIGasoline.Application.Features.Reportes.Queries;
using APIGasoline.Authorization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace APIGasoline.Controllers;

/// <summary>
/// Reportes de la flota (parte del módulo de auditoría: privilegio auditoria.ver).
/// Por cada carro: viajes, km recorridos, cargas, litros y dinero gastado en el periodo.
/// Un empleado solo ve su empresa (la del token), salvo plataforma.multiempresa.
///
/// Filtros (todos opcionales):
///   ?carroId=   un solo vehículo (sin él: toda la flota)
///   ?desde=2026-10-01&amp;hasta=2026-10-31T23:59:59   (sin fechas: todo el historial)
///   ?empresaId= solo con plataforma.multiempresa
/// </summary>
[ApiController]
[Authorize]
[Route("api/reportes")]
public class ReportesController : ControllerBase
{
    private readonly ISender _sender;

    public ReportesController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>Datos del reporte en JSON (la app los muestra como vista previa).</summary>
    [RequierePrivilegio(Permisos.AuditoriaVer)]
    [HttpGet("flota")]
    public async Task<ActionResult<ReporteFlotaDto>> GetFlota(
        [FromQuery] Guid? empresaId,
        [FromQuery] Guid? carroId,
        [FromQuery] DateTime? desde,
        [FromQuery] DateTime? hasta,
        CancellationToken cancellationToken)
        => Ok(await _sender.Send(new GetReporteFlotaQuery(empresaId, carroId, desde, hasta), cancellationToken));

    /// <summary>El reporte en PDF (para presentar o imprimir).</summary>
    [RequierePrivilegio(Permisos.AuditoriaVer)]
    [HttpGet("flota/pdf")]
    public Task<IActionResult> GetFlotaPdf(
        [FromQuery] Guid? empresaId,
        [FromQuery] Guid? carroId,
        [FromQuery] DateTime? desde,
        [FromQuery] DateTime? hasta,
        CancellationToken cancellationToken)
        => Descargar(empresaId, carroId, desde, hasta, FormatoReporte.Pdf, cancellationToken);

    /// <summary>El reporte en Excel .xlsx (para analizar: los números van como números).</summary>
    [RequierePrivilegio(Permisos.AuditoriaVer)]
    [HttpGet("flota/excel")]
    public Task<IActionResult> GetFlotaExcel(
        [FromQuery] Guid? empresaId,
        [FromQuery] Guid? carroId,
        [FromQuery] DateTime? desde,
        [FromQuery] DateTime? hasta,
        CancellationToken cancellationToken)
        => Descargar(empresaId, carroId, desde, hasta, FormatoReporte.Excel, cancellationToken);

    private async Task<IActionResult> Descargar(
        Guid? empresaId,
        Guid? carroId,
        DateTime? desde,
        DateTime? hasta,
        FormatoReporte formato,
        CancellationToken cancellationToken)
    {
        var archivo = await _sender.Send(new GetReporteFlotaArchivoQuery(empresaId, carroId, desde, hasta, formato), cancellationToken);
        return File(archivo.Contenido, archivo.TipoContenido, archivo.NombreArchivo);
    }
}
