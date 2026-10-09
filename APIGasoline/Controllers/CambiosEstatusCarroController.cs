using APIGasoline.Application.Features.CambiosEstatusCarro.Commands;
using APIGasoline.Application.Features.CambiosEstatusCarro.DTOs;
using APIGasoline.Application.Features.CambiosEstatusCarro.Queries;
using APIGasoline.Contracts.CambiosEstatusCarro;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace APIGasoline.Controllers;

/// <summary>
/// Bitácora de estatus de carros. Solo se consulta y se agrega; no se edita ni se borra.
/// </summary>
[ApiController]
[Route("api/cambios-estatus-carro")]
public class CambiosEstatusCarroController : ControllerBase
{
    private readonly ISender _sender;

    public CambiosEstatusCarroController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Historial del más reciente al más antiguo. Filtros opcionales: ?empresaId=, ?carroId=, ?desde=, ?hasta=
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CambioEstatusCarroDto>>> GetAll(
        [FromQuery] Guid? empresaId,
        [FromQuery] Guid? carroId,
        [FromQuery] DateTime? desde,
        [FromQuery] DateTime? hasta,
        CancellationToken cancellationToken)
    {
        var cambios = await _sender.Send(new GetCambiosEstatusCarroQuery(empresaId, carroId, desde, hasta), cancellationToken);
        return Ok(cambios);
    }

    [HttpGet("{cambioEstatusCarroId:guid}")]
    public async Task<ActionResult<CambioEstatusCarroDto>> GetById(Guid cambioEstatusCarroId, CancellationToken cancellationToken)
    {
        var cambio = await _sender.Send(new GetCambioEstatusCarroByIdQuery(cambioEstatusCarroId), cancellationToken);
        return Ok(cambio);
    }

    /// <summary>
    /// Cambia el estatus del carro (disponible / enMantenimiento) y lo registra en la bitácora.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<CambioEstatusCarroDto>> Cambiar(CambiarEstatusCarroRequest request, CancellationToken cancellationToken)
    {
        var command = new CambiarEstatusCarroCommand(
            request.CarroId,
            request.EmpleadoId,
            request.EstatusNuevo!.Value,
            request.Motivo,
            request.ViajeAfectadoId,
            request.FechaCambio,
            request.Sincronizado);

        var cambio = await _sender.Send(command, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { cambioEstatusCarroId = cambio.CambioEstatusCarroId }, cambio);
    }
}
