using APIGasoline.Application.Features.Corporativos.Commands;
using APIGasoline.Application.Features.Corporativos.DTOs;
using APIGasoline.Application.Features.Corporativos.Queries;
using APIGasoline.Application.Features.Divisiones.DTOs;
using APIGasoline.Contracts.Corporativos;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using APIGasoline.Application.Common.Security;
using APIGasoline.Authorization;

namespace APIGasoline.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CorporativosController : ControllerBase
{
    private readonly ISender _sender;

    public CorporativosController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Lista los corporativos de la empresa (la plataforma ve todos). Filtros opcionales: ?empresaId= y ?divisionId=
    /// </summary>
    [RequierePrivilegio(Permisos.CorporativoVer, Permisos.CorporativoGestionar)]
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CorporativoDto>>> GetAll(
        [FromQuery] Guid? empresaId,
        [FromQuery] Guid? divisionId,
        CancellationToken cancellationToken)
    {
        var corporativos = await _sender.Send(new GetCorporativosQuery(empresaId, divisionId), cancellationToken);
        return Ok(corporativos);
    }

    [RequierePrivilegio(Permisos.CorporativoVer, Permisos.CorporativoGestionar)]
    [HttpGet("{corporativoId:guid}")]
    public async Task<ActionResult<CorporativoDto>> GetById(Guid corporativoId, CancellationToken cancellationToken)
    {
        var corporativo = await _sender.Send(new GetCorporativoByIdQuery(corporativoId), cancellationToken);
        return Ok(corporativo);
    }

    [RequierePrivilegio(Permisos.CorporativoGestionar)]
    [HttpPost]
    public async Task<ActionResult<CorporativoDto>> Create(CreateCorporativoRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateCorporativoCommand(request.EmpresaId, request.Nombre, request.Activo);
        var corporativo = await _sender.Send(command, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { corporativoId = corporativo.CorporativoId }, corporativo);
    }

    [RequierePrivilegio(Permisos.CorporativoGestionar)]
    [HttpPut("{corporativoId:guid}")]
    public async Task<ActionResult<CorporativoDto>> Update(Guid corporativoId, UpdateCorporativoRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateCorporativoCommand(corporativoId, request.Nombre, request.Activo);
        var corporativo = await _sender.Send(command, cancellationToken);

        return Ok(corporativo);
    }

    [RequierePrivilegio(Permisos.CorporativoGestionar)]
    [HttpDelete("{corporativoId:guid}")]
    public async Task<IActionResult> Delete(Guid corporativoId, CancellationToken cancellationToken)
    {
        await _sender.Send(new DeleteCorporativoCommand(corporativoId), cancellationToken);
        return NoContent();
    }

    /// <summary>Divisiones ligadas al corporativo.</summary>
    [RequierePrivilegio(Permisos.CorporativoVer, Permisos.CorporativoGestionar)]
    [HttpGet("{corporativoId:guid}/divisiones")]
    public async Task<ActionResult<IReadOnlyList<DivisionDto>>> GetDivisiones(Guid corporativoId, CancellationToken cancellationToken)
    {
        var divisiones = await _sender.Send(new GetDivisionesDeCorporativoQuery(corporativoId), cancellationToken);
        return Ok(divisiones);
    }

    [RequierePrivilegio(Permisos.CorporativoVer, Permisos.CorporativoGestionar)]
    [HttpGet("{corporativoId:guid}/divisiones/{divisionId:guid}")]
    public async Task<ActionResult<CorporativoDivisionDto>> GetLiga(Guid corporativoId, Guid divisionId, CancellationToken cancellationToken)
    {
        var liga = await _sender.Send(new GetLigaDivisionQuery(corporativoId, divisionId), cancellationToken);
        return Ok(liga);
    }

    /// <summary>Liga una división (de la misma empresa) con el corporativo.</summary>
    [RequierePrivilegio(Permisos.CorporativoGestionar)]
    [HttpPost("{corporativoId:guid}/divisiones/{divisionId:guid}")]
    public async Task<ActionResult<CorporativoDivisionDto>> LigarDivision(Guid corporativoId, Guid divisionId, CancellationToken cancellationToken)
    {
        var liga = await _sender.Send(new LigarDivisionCommand(corporativoId, divisionId), cancellationToken);
        return CreatedAtAction(nameof(GetLiga), new { corporativoId, divisionId }, liga);
    }

    /// <summary>Quita la liga (409 si hay centros de costo de ese corporativo y esa división).</summary>
    [RequierePrivilegio(Permisos.CorporativoGestionar)]
    [HttpDelete("{corporativoId:guid}/divisiones/{divisionId:guid}")]
    public async Task<IActionResult> DesligarDivision(Guid corporativoId, Guid divisionId, CancellationToken cancellationToken)
    {
        await _sender.Send(new DesligarDivisionCommand(corporativoId, divisionId), cancellationToken);
        return NoContent();
    }
}
