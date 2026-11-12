using APIGasoline.Application.Features.CentrosCosto.Commands;
using APIGasoline.Application.Features.CentrosCosto.DTOs;
using APIGasoline.Application.Features.CentrosCosto.Queries;
using APIGasoline.Contracts.CentrosCosto;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using APIGasoline.Application.Common.Security;
using APIGasoline.Authorization;

namespace APIGasoline.Controllers;

[ApiController]
[Route("api/centros-costo")]
public class CentrosCostoController : ControllerBase
{
    private readonly ISender _sender;

    public CentrosCostoController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Lista los centros de costo. Filtros opcionales: ?empresaId=, ?corporativoId= y ?divisionId=
    /// </summary>
    [RequierePrivilegio(Permisos.CentroCostoVer, Permisos.ViajeRegistrar, Permisos.CargaRegistrar)]
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CentroCostoDto>>> GetAll(
        [FromQuery] Guid? empresaId,
        [FromQuery] Guid? corporativoId,
        [FromQuery] Guid? divisionId,
        CancellationToken cancellationToken)
    {
        var centrosCosto = await _sender.Send(new GetCentrosCostoQuery(empresaId, corporativoId, divisionId), cancellationToken);
        return Ok(centrosCosto);
    }

    [RequierePrivilegio(Permisos.CentroCostoVer, Permisos.ViajeRegistrar, Permisos.CargaRegistrar)]
    [HttpGet("{centroCostoId:guid}")]
    public async Task<ActionResult<CentroCostoDto>> GetById(Guid centroCostoId, CancellationToken cancellationToken)
    {
        var centroCosto = await _sender.Send(new GetCentroCostoByIdQuery(centroCostoId), cancellationToken);
        return Ok(centroCosto);
    }

    [RequierePrivilegio(Permisos.CentroCostoGestionar)]
    [HttpPost]
    public async Task<ActionResult<CentroCostoDto>> Create(CreateCentroCostoRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateCentroCostoCommand(request.EmpresaId, request.Nombre, request.Clave, request.Activo, request.CorporativoId, request.DivisionId);
        var centroCosto = await _sender.Send(command, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { centroCostoId = centroCosto.CentroCostoId }, centroCosto);
    }

    [RequierePrivilegio(Permisos.CentroCostoGestionar)]
    [HttpPut("{centroCostoId:guid}")]
    public async Task<ActionResult<CentroCostoDto>> Update(Guid centroCostoId, UpdateCentroCostoRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateCentroCostoCommand(centroCostoId, request.EmpresaId, request.Nombre, request.Clave, request.Activo, request.CorporativoId, request.DivisionId);
        var centroCosto = await _sender.Send(command, cancellationToken);

        return Ok(centroCosto);
    }

    [RequierePrivilegio(Permisos.CentroCostoGestionar)]
    [HttpDelete("{centroCostoId:guid}")]
    public async Task<IActionResult> Delete(Guid centroCostoId, CancellationToken cancellationToken)
    {
        await _sender.Send(new DeleteCentroCostoCommand(centroCostoId), cancellationToken);
        return NoContent();
    }
}
