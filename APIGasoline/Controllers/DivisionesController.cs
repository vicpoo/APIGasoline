using APIGasoline.Application.Features.Corporativos.DTOs;
using APIGasoline.Application.Features.Divisiones.Commands;
using APIGasoline.Application.Features.Divisiones.DTOs;
using APIGasoline.Application.Features.Divisiones.Queries;
using APIGasoline.Contracts.Divisiones;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using APIGasoline.Application.Common.Security;
using APIGasoline.Authorization;

namespace APIGasoline.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DivisionesController : ControllerBase
{
    private readonly ISender _sender;

    public DivisionesController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Lista las divisiones de la empresa (la plataforma ve todas). Filtros opcionales: ?empresaId= y ?corporativoId=
    /// </summary>
    [RequierePrivilegio(Permisos.CorporativoVer, Permisos.CorporativoGestionar)]
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<DivisionDto>>> GetAll(
        [FromQuery] Guid? empresaId,
        [FromQuery] Guid? corporativoId,
        CancellationToken cancellationToken)
    {
        var divisiones = await _sender.Send(new GetDivisionesQuery(empresaId, corporativoId), cancellationToken);
        return Ok(divisiones);
    }

    [RequierePrivilegio(Permisos.CorporativoVer, Permisos.CorporativoGestionar)]
    [HttpGet("{divisionId:guid}")]
    public async Task<ActionResult<DivisionDto>> GetById(Guid divisionId, CancellationToken cancellationToken)
    {
        var division = await _sender.Send(new GetDivisionByIdQuery(divisionId), cancellationToken);
        return Ok(division);
    }

    [RequierePrivilegio(Permisos.CorporativoGestionar)]
    [HttpPost]
    public async Task<ActionResult<DivisionDto>> Create(CreateDivisionRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateDivisionCommand(request.EmpresaId, request.Nombre, request.Activo);
        var division = await _sender.Send(command, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { divisionId = division.DivisionId }, division);
    }

    [RequierePrivilegio(Permisos.CorporativoGestionar)]
    [HttpPut("{divisionId:guid}")]
    public async Task<ActionResult<DivisionDto>> Update(Guid divisionId, UpdateDivisionRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateDivisionCommand(divisionId, request.Nombre, request.Activo);
        var division = await _sender.Send(command, cancellationToken);

        return Ok(division);
    }

    [RequierePrivilegio(Permisos.CorporativoGestionar)]
    [HttpDelete("{divisionId:guid}")]
    public async Task<IActionResult> Delete(Guid divisionId, CancellationToken cancellationToken)
    {
        await _sender.Send(new DeleteDivisionCommand(divisionId), cancellationToken);
        return NoContent();
    }

    /// <summary>Corporativos a los que está ligada la división.</summary>
    [RequierePrivilegio(Permisos.CorporativoVer, Permisos.CorporativoGestionar)]
    [HttpGet("{divisionId:guid}/corporativos")]
    public async Task<ActionResult<IReadOnlyList<CorporativoDto>>> GetCorporativos(Guid divisionId, CancellationToken cancellationToken)
    {
        var corporativos = await _sender.Send(new GetCorporativosDeDivisionQuery(divisionId), cancellationToken);
        return Ok(corporativos);
    }
}
