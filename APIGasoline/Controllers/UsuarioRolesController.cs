using APIGasoline.Application.Features.UsuarioRoles.Commands;
using APIGasoline.Application.Features.UsuarioRoles.DTOs;
using APIGasoline.Application.Features.UsuarioRoles.Queries;
using APIGasoline.Contracts.UsuarioRoles;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace APIGasoline.Controllers;

[ApiController]
[Route("api/usuario-roles")]
public class UsuarioRolesController : ControllerBase
{
    private readonly ISender _sender;

    public UsuarioRolesController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Lista las asignaciones. Filtros opcionales: ?usuarioId=1 y/o ?rolId=2
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<UsuarioRolDto>>> GetAll(
        [FromQuery] int? usuarioId,
        [FromQuery] int? rolId,
        CancellationToken cancellationToken)
    {
        var usuarioRoles = await _sender.Send(new GetUsuarioRolesQuery(usuarioId, rolId), cancellationToken);
        return Ok(usuarioRoles);
    }

    [HttpGet("{usuarioRolId:int}")]
    public async Task<ActionResult<UsuarioRolDto>> GetById(int usuarioRolId, CancellationToken cancellationToken)
    {
        var usuarioRol = await _sender.Send(new GetUsuarioRolByIdQuery(usuarioRolId), cancellationToken);
        return Ok(usuarioRol);
    }

    [HttpPost]
    public async Task<ActionResult<UsuarioRolDto>> Create(CreateUsuarioRolRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateUsuarioRolCommand(request.UsuarioId, request.RolId);
        var usuarioRol = await _sender.Send(command, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { usuarioRolId = usuarioRol.UsuarioRolId }, usuarioRol);
    }

    [HttpPut("{usuarioRolId:int}")]
    public async Task<ActionResult<UsuarioRolDto>> Update(int usuarioRolId, UpdateUsuarioRolRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateUsuarioRolCommand(usuarioRolId, request.UsuarioId, request.RolId);
        var usuarioRol = await _sender.Send(command, cancellationToken);

        return Ok(usuarioRol);
    }

    [HttpDelete("{usuarioRolId:int}")]
    public async Task<IActionResult> Delete(int usuarioRolId, CancellationToken cancellationToken)
    {
        await _sender.Send(new DeleteUsuarioRolCommand(usuarioRolId), cancellationToken);
        return NoContent();
    }
}
