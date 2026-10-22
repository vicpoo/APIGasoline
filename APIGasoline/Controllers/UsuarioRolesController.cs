using APIGasoline.Application.Features.UsuarioRoles.Commands;
using APIGasoline.Application.Features.UsuarioRoles.DTOs;
using APIGasoline.Application.Features.UsuarioRoles.Queries;
using APIGasoline.Contracts.UsuarioRoles;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using APIGasoline.Application.Common.Security;
using APIGasoline.Authorization;

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
    [RequierePrivilegio(Permisos.RolVer, Permisos.RolAsignar)]
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<UsuarioRolDto>>> GetAll(
        [FromQuery] Guid? usuarioId,
        [FromQuery] Guid? rolId,
        CancellationToken cancellationToken)
    {
        var usuarioRoles = await _sender.Send(new GetUsuarioRolesQuery(usuarioId, rolId), cancellationToken);
        return Ok(usuarioRoles);
    }

    [RequierePrivilegio(Permisos.RolVer, Permisos.RolAsignar)]
    [HttpGet("{usuarioRolId:guid}")]
    public async Task<ActionResult<UsuarioRolDto>> GetById(Guid usuarioRolId, CancellationToken cancellationToken)
    {
        var usuarioRol = await _sender.Send(new GetUsuarioRolByIdQuery(usuarioRolId), cancellationToken);
        return Ok(usuarioRol);
    }

    [RequierePrivilegio(Permisos.RolAsignar)]
    [HttpPost]
    public async Task<ActionResult<UsuarioRolDto>> Create(CreateUsuarioRolRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateUsuarioRolCommand(request.UsuarioId, request.RolId);
        var usuarioRol = await _sender.Send(command, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { usuarioRolId = usuarioRol.UsuarioRolId }, usuarioRol);
    }

    [RequierePrivilegio(Permisos.RolAsignar)]
    [HttpPut("{usuarioRolId:guid}")]
    public async Task<ActionResult<UsuarioRolDto>> Update(Guid usuarioRolId, UpdateUsuarioRolRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateUsuarioRolCommand(usuarioRolId, request.UsuarioId, request.RolId);
        var usuarioRol = await _sender.Send(command, cancellationToken);

        return Ok(usuarioRol);
    }

    [RequierePrivilegio(Permisos.RolAsignar)]
    [HttpDelete("{usuarioRolId:guid}")]
    public async Task<IActionResult> Delete(Guid usuarioRolId, CancellationToken cancellationToken)
    {
        await _sender.Send(new DeleteUsuarioRolCommand(usuarioRolId), cancellationToken);
        return NoContent();
    }
}
