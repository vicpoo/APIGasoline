using APIGasoline.Application.Features.Divisiones.Commands;
using APIGasoline.Application.Features.Divisiones.DTOs;
using APIGasoline.Application.Features.Divisiones.Queries;
using APIGasoline.Contracts.Divisiones;
using MediatR;
using Microsoft.AspNetCore.Mvc;

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
    /// Lista las divisiones. Filtro opcional: ?corporativoId=1
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<DivisionDto>>> GetAll([FromQuery] int? corporativoId, CancellationToken cancellationToken)
    {
        var divisiones = await _sender.Send(new GetDivisionesQuery(corporativoId), cancellationToken);
        return Ok(divisiones);
    }

    [HttpGet("{divisionId:int}")]
    public async Task<ActionResult<DivisionDto>> GetById(int divisionId, CancellationToken cancellationToken)
    {
        var division = await _sender.Send(new GetDivisionByIdQuery(divisionId), cancellationToken);
        return Ok(division);
    }

    [HttpPost]
    public async Task<ActionResult<DivisionDto>> Create(CreateDivisionRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateDivisionCommand(request.CorporativoId, request.Nombre, request.Activo);
        var division = await _sender.Send(command, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { divisionId = division.DivisionId }, division);
    }

    [HttpPut("{divisionId:int}")]
    public async Task<ActionResult<DivisionDto>> Update(int divisionId, UpdateDivisionRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateDivisionCommand(divisionId, request.CorporativoId, request.Nombre, request.Activo);
        var division = await _sender.Send(command, cancellationToken);

        return Ok(division);
    }

    [HttpDelete("{divisionId:int}")]
    public async Task<IActionResult> Delete(int divisionId, CancellationToken cancellationToken)
    {
        await _sender.Send(new DeleteDivisionCommand(divisionId), cancellationToken);
        return NoContent();
    }
}
