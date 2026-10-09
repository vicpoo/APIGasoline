using APIGasoline.Application.Features.Corporativos.Commands;
using APIGasoline.Application.Features.Corporativos.DTOs;
using APIGasoline.Application.Features.Corporativos.Queries;
using APIGasoline.Contracts.Corporativos;
using MediatR;
using Microsoft.AspNetCore.Mvc;

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

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CorporativoDto>>> GetAll(CancellationToken cancellationToken)
    {
        var corporativos = await _sender.Send(new GetCorporativosQuery(), cancellationToken);
        return Ok(corporativos);
    }

    [HttpGet("{corporativoId:guid}")]
    public async Task<ActionResult<CorporativoDto>> GetById(Guid corporativoId, CancellationToken cancellationToken)
    {
        var corporativo = await _sender.Send(new GetCorporativoByIdQuery(corporativoId), cancellationToken);
        return Ok(corporativo);
    }

    [HttpPost]
    public async Task<ActionResult<CorporativoDto>> Create(CreateCorporativoRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateCorporativoCommand(request.Nombre, request.Activo);
        var corporativo = await _sender.Send(command, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { corporativoId = corporativo.CorporativoId }, corporativo);
    }

    [HttpPut("{corporativoId:guid}")]
    public async Task<ActionResult<CorporativoDto>> Update(Guid corporativoId, UpdateCorporativoRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateCorporativoCommand(corporativoId, request.Nombre, request.Activo);
        var corporativo = await _sender.Send(command, cancellationToken);

        return Ok(corporativo);
    }

    [HttpDelete("{corporativoId:guid}")]
    public async Task<IActionResult> Delete(Guid corporativoId, CancellationToken cancellationToken)
    {
        await _sender.Send(new DeleteCorporativoCommand(corporativoId), cancellationToken);
        return NoContent();
    }
}
