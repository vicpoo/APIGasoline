using APIGasoline.Application.Features.Roles.Commands;
using APIGasoline.Application.Features.Roles.DTOs;
using APIGasoline.Application.Features.Roles.Queries;
using APIGasoline.Contracts.Roles;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using APIGasoline.Application.Common.Security;
using APIGasoline.Authorization;

namespace APIGasoline.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RolesController : ControllerBase
{
    private readonly ISender _sender;

    public RolesController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// ?empresaId=1 → roles disponibles para esa empresa (globales + exclusivos).
    /// ?soloGlobales=true → solo los globales. Sin filtros → todos.
    /// </summary>
    [RequierePrivilegio(Permisos.RolVer, Permisos.RolGestionar, Permisos.RolAsignar)]
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<RolDto>>> GetAll(
        [FromQuery] Guid? empresaId,
        [FromQuery] bool soloGlobales,
        CancellationToken cancellationToken)
    {
        var roles = await _sender.Send(new GetRolesQuery(empresaId, soloGlobales), cancellationToken);
        return Ok(roles);
    }

    [RequierePrivilegio(Permisos.RolVer, Permisos.RolGestionar, Permisos.RolAsignar)]
    [HttpGet("{rolId:guid}")]
    public async Task<ActionResult<RolDto>> GetById(Guid rolId, CancellationToken cancellationToken)
    {
        var rol = await _sender.Send(new GetRolByIdQuery(rolId), cancellationToken);
        return Ok(rol);
    }

    [RequierePrivilegio(Permisos.RolGestionar)]
    [HttpPost]
    public async Task<ActionResult<RolDto>> Create(CreateRolRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateRolCommand(request.EmpresaId, request.Nombre, request.Descripcion, request.Activo);
        var rol = await _sender.Send(command, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { rolId = rol.RolId }, rol);
    }

    [RequierePrivilegio(Permisos.RolGestionar)]
    [HttpPut("{rolId:guid}")]
    public async Task<ActionResult<RolDto>> Update(Guid rolId, UpdateRolRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateRolCommand(rolId, request.Nombre, request.Descripcion, request.Activo);
        var rol = await _sender.Send(command, cancellationToken);

        return Ok(rol);
    }

    [RequierePrivilegio(Permisos.RolGestionar)]
    [HttpDelete("{rolId:guid}")]
    public async Task<IActionResult> Delete(Guid rolId, CancellationToken cancellationToken)
    {
        await _sender.Send(new DeleteRolCommand(rolId), cancellationToken);
        return NoContent();
    }
}
