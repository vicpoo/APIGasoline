using APIGasoline.Application.Features.Corporativos.Commands;
using APIGasoline.Application.Features.Corporativos.DTOs;
using APIGasoline.Application.Features.Corporativos.Queries;
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

    [RequierePrivilegio(Permisos.CorporativoVer, Permisos.CorporativoGestionar)]
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CorporativoDto>>> GetAll(CancellationToken cancellationToken)
    {
        var corporativos = await _sender.Send(new GetCorporativosQuery(), cancellationToken);
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
        var command = new CreateCorporativoCommand(request.Nombre, request.Activo);
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
}
