using APIGasoline.Application.Features.RolPrivilegios.Commands;
using APIGasoline.Application.Features.RolPrivilegios.DTOs;
using APIGasoline.Application.Features.RolPrivilegios.Queries;
using APIGasoline.Contracts.RolPrivilegios;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using APIGasoline.Application.Common.Security;
using APIGasoline.Authorization;

namespace APIGasoline.Controllers;

[ApiController]
[Route("api/rol-privilegios")]
public class RolPrivilegiosController : ControllerBase
{
    private readonly ISender _sender;

    public RolPrivilegiosController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Lista las asignaciones. Filtros opcionales: ?rolId=1 y/o ?privilegioId=2
    /// </summary>
    [RequierePrivilegio(Permisos.RolVer, Permisos.RolGestionar)]
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<RolPrivilegioDto>>> GetAll(
        [FromQuery] Guid? rolId,
        [FromQuery] Guid? privilegioId,
        CancellationToken cancellationToken)
    {
        var rolPrivilegios = await _sender.Send(new GetRolPrivilegiosQuery(rolId, privilegioId), cancellationToken);
        return Ok(rolPrivilegios);
    }

    [RequierePrivilegio(Permisos.RolVer, Permisos.RolGestionar)]
    [HttpGet("{rolPrivilegioId:guid}")]
    public async Task<ActionResult<RolPrivilegioDto>> GetById(Guid rolPrivilegioId, CancellationToken cancellationToken)
    {
        var rolPrivilegio = await _sender.Send(new GetRolPrivilegioByIdQuery(rolPrivilegioId), cancellationToken);
        return Ok(rolPrivilegio);
    }

    [RequierePrivilegio(Permisos.RolGestionar)]
    [HttpPost]
    public async Task<ActionResult<RolPrivilegioDto>> Create(CreateRolPrivilegioRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateRolPrivilegioCommand(request.RolId, request.PrivilegioId);
        var rolPrivilegio = await _sender.Send(command, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { rolPrivilegioId = rolPrivilegio.RolPrivilegioId }, rolPrivilegio);
    }

    [RequierePrivilegio(Permisos.RolGestionar)]
    [HttpPut("{rolPrivilegioId:guid}")]
    public async Task<ActionResult<RolPrivilegioDto>> Update(Guid rolPrivilegioId, UpdateRolPrivilegioRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateRolPrivilegioCommand(rolPrivilegioId, request.RolId, request.PrivilegioId);
        var rolPrivilegio = await _sender.Send(command, cancellationToken);

        return Ok(rolPrivilegio);
    }

    [RequierePrivilegio(Permisos.RolGestionar)]
    [HttpDelete("{rolPrivilegioId:guid}")]
    public async Task<IActionResult> Delete(Guid rolPrivilegioId, CancellationToken cancellationToken)
    {
        await _sender.Send(new DeleteRolPrivilegioCommand(rolPrivilegioId), cancellationToken);
        return NoContent();
    }
}
