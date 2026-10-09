using APIGasoline.Application.Features.Privilegios.Commands;
using APIGasoline.Application.Features.Privilegios.DTOs;
using APIGasoline.Application.Features.Privilegios.Queries;
using APIGasoline.Contracts.Privilegios;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace APIGasoline.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PrivilegiosController : ControllerBase
{
    private readonly ISender _sender;

    public PrivilegiosController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<PrivilegioDto>>> GetAll(CancellationToken cancellationToken)
    {
        var privilegios = await _sender.Send(new GetPrivilegiosQuery(), cancellationToken);
        return Ok(privilegios);
    }

    [HttpGet("{privilegioId:guid}")]
    public async Task<ActionResult<PrivilegioDto>> GetById(Guid privilegioId, CancellationToken cancellationToken)
    {
        var privilegio = await _sender.Send(new GetPrivilegioByIdQuery(privilegioId), cancellationToken);
        return Ok(privilegio);
    }

    [HttpPost]
    public async Task<ActionResult<PrivilegioDto>> Create(CreatePrivilegioRequest request, CancellationToken cancellationToken)
    {
        var command = new CreatePrivilegioCommand(request.Nombre, request.Descripcion, request.Activo);
        var privilegio = await _sender.Send(command, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { privilegioId = privilegio.PrivilegioId }, privilegio);
    }

    [HttpPut("{privilegioId:guid}")]
    public async Task<ActionResult<PrivilegioDto>> Update(Guid privilegioId, UpdatePrivilegioRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdatePrivilegioCommand(privilegioId, request.Nombre, request.Descripcion, request.Activo);
        var privilegio = await _sender.Send(command, cancellationToken);

        return Ok(privilegio);
    }

    [HttpDelete("{privilegioId:guid}")]
    public async Task<IActionResult> Delete(Guid privilegioId, CancellationToken cancellationToken)
    {
        await _sender.Send(new DeletePrivilegioCommand(privilegioId), cancellationToken);
        return NoContent();
    }
}
