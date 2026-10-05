using APIGasoline.Application.Features.CargasCombustible.Commands;
using APIGasoline.Application.Features.CargasCombustible.DTOs;
using APIGasoline.Application.Features.CargasCombustible.Queries;
using APIGasoline.Contracts.CargasCombustible;
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
        [FromQuery] int? empresaId,
        [FromQuery] int? viajeId,
        [FromQuery] DateTime? desde,
        [FromQuery] DateTime? hasta,
        CancellationToken cancellationToken)
    {
        var cargas = await _sender.Send(new GetCargasCombustibleQuery(empresaId, viajeId, desde, hasta), cancellationToken);
        return Ok(cargas);
    }

    [HttpGet("{cargaCombustibleId:int}")]
    public async Task<ActionResult<CargaCombustibleDto>> GetById(int cargaCombustibleId, CancellationToken cancellationToken)
    {
        var carga = await _sender.Send(new GetCargaCombustibleByIdQuery(cargaCombustibleId), cancellationToken);
        return Ok(carga);
    }

    [HttpPost]
    public async Task<ActionResult<CargaCombustibleDto>> Create(CreateCargaCombustibleRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateCargaCombustibleCommand(
            request.ViajeId,
            request.Litros!.Value,
            request.Monto,
            request.FotoComprobante,
            request.FechaCarga!.Value,
            request.UbicacionLat,
            request.UbicacionLng,
            request.Sincronizado);

        var carga = await _sender.Send(command, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { cargaCombustibleId = carga.CargaCombustibleId }, carga);
    }

    [HttpPut("{cargaCombustibleId:int}")]
    public async Task<ActionResult<CargaCombustibleDto>> Update(int cargaCombustibleId, UpdateCargaCombustibleRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateCargaCombustibleCommand(
            cargaCombustibleId,
            request.Litros!.Value,
            request.Monto,
            request.FotoComprobante,
            request.FechaCarga!.Value,
            request.UbicacionLat,
            request.UbicacionLng);

        return Ok(await _sender.Send(command, cancellationToken));
    }

    [HttpDelete("{cargaCombustibleId:int}")]
    public async Task<IActionResult> Delete(int cargaCombustibleId, CancellationToken cancellationToken)
    {
        await _sender.Send(new DeleteCargaCombustibleCommand(cargaCombustibleId), cancellationToken);
        return NoContent();
    }
}
