using APIGasoline.Application.Features.CargasCombustible.Commands;
using APIGasoline.Application.Features.CargasCombustible.DTOs;
using APIGasoline.Application.Features.CargasCombustible.Queries;
using APIGasoline.Contracts.CargasCombustible;
using APIGasoline.Extensions;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace APIGasoline.Controllers;

[ApiController]
[Route("api/cargas-combustible")]
public class CargasCombustibleController : ControllerBase
{
    private readonly ISender _sender;

    public CargasCombustibleController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Lista de la más reciente a la más antigua.
    /// Filtros opcionales: ?empresaId=, ?viajeId=, ?desde=2026-10-01&amp;hasta=2026-10-31
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CargaCombustibleDto>>> GetAll(
        [FromQuery] Guid? empresaId,
        [FromQuery] Guid? viajeId,
        [FromQuery] DateTime? desde,
        [FromQuery] DateTime? hasta,
        CancellationToken cancellationToken)
    {
        var cargas = await _sender.Send(new GetCargasCombustibleQuery(empresaId, viajeId, desde, hasta), cancellationToken);
        return Ok(cargas);
    }

    [HttpGet("{cargaCombustibleId:guid}")]
    public async Task<ActionResult<CargaCombustibleDto>> GetById(Guid cargaCombustibleId, CancellationToken cancellationToken)
    {
        var carga = await _sender.Send(new GetCargaCombustibleByIdQuery(cargaCombustibleId), cancellationToken);
        return Ok(carga);
    }

    /// <summary>
    /// Registra una carga. Se envía como multipart/form-data con el archivo "fotoComprobante"
    /// (jpg, png o webp, máx. 5 MB). En la BD solo se guarda la llave del archivo.
    /// </summary>
    [HttpPost]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(FormFileExtensions.LimiteCuerpoBytes)]
    [RequestFormLimits(MultipartBodyLengthLimit = FormFileExtensions.LimiteCuerpoBytes)]
    public async Task<ActionResult<CargaCombustibleDto>> Create([FromForm] CreateCargaCombustibleRequest request, CancellationToken cancellationToken)
    {
        await using var contenido = request.FotoComprobante!.OpenReadStream();

        var command = new CreateCargaCombustibleCommand(
            request.ViajeId,
            request.Litros!.Value,
            request.Monto,
            request.FotoComprobante.ToArchivoEntrada(contenido),
            request.FechaCarga!.Value,
            request.UbicacionLat,
            request.UbicacionLng);

        var carga = await _sender.Send(command, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { cargaCombustibleId = carga.CargaCombustibleId }, carga);
    }

    /// <summary>
    /// Corrige una carga. Se envía como multipart/form-data; "fotoComprobante" es opcional
    /// (si no se manda, se conserva el comprobante actual) y "motivo" es obligatorio.
    /// Requiere token. El valor anterior queda en el historial de auditoría.
    /// </summary>
    [HttpPut("{cargaCombustibleId:guid}")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(FormFileExtensions.LimiteCuerpoBytes)]
    [RequestFormLimits(MultipartBodyLengthLimit = FormFileExtensions.LimiteCuerpoBytes)]
    public async Task<ActionResult<CargaCombustibleDto>> Update(Guid cargaCombustibleId, [FromForm] UpdateCargaCombustibleRequest request, CancellationToken cancellationToken)
    {
        await using var contenido = request.FotoComprobante?.OpenReadStream();

        var command = new UpdateCargaCombustibleCommand(
            cargaCombustibleId,
            request.Litros!.Value,
            request.Monto,
            request.FotoComprobante is null || contenido is null ? null : request.FotoComprobante.ToArchivoEntrada(contenido),
            request.FechaCarga!.Value,
            request.UbicacionLat,
            request.UbicacionLng,
            request.Motivo);

        return Ok(await _sender.Send(command, cancellationToken));
    }

    [HttpDelete("{cargaCombustibleId:guid}")]
    public async Task<IActionResult> Delete(Guid cargaCombustibleId, CancellationToken cancellationToken)
    {
        await _sender.Send(new DeleteCargaCombustibleCommand(cargaCombustibleId), cancellationToken);
        return NoContent();
    }
}
