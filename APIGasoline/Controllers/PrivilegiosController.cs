using APIGasoline.Application.Features.Privilegios.Commands;
using APIGasoline.Application.Features.Privilegios.DTOs;
using APIGasoline.Application.Features.Privilegios.Queries;
using APIGasoline.Contracts.Privilegios;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using APIGasoline.Application.Common.Security;
using APIGasoline.Authorization;

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

    /// <summary>
    /// Catálogo de privilegios ordenado por módulo y clave. Filtro opcional: ?modulo=carro
    /// </summary>
    [RequierePrivilegio(Permisos.PrivilegioVer, Permisos.PrivilegioGestionar, Permisos.RolGestionar)]
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<PrivilegioDto>>> GetAll([FromQuery] string? modulo, CancellationToken cancellationToken)
    {
        var privilegios = await _sender.Send(new GetPrivilegiosQuery(modulo), cancellationToken);
        return Ok(privilegios);
    }

    [RequierePrivilegio(Permisos.PrivilegioVer, Permisos.PrivilegioGestionar, Permisos.RolGestionar)]
    [HttpGet("{privilegioId:guid}")]
    public async Task<ActionResult<PrivilegioDto>> GetById(Guid privilegioId, CancellationToken cancellationToken)
    {
        var privilegio = await _sender.Send(new GetPrivilegioByIdQuery(privilegioId), cancellationToken);
        return Ok(privilegio);
    }

    [RequierePrivilegio(Permisos.PrivilegioGestionar)]
    [HttpPost]
    public async Task<ActionResult<PrivilegioDto>> Create(CreatePrivilegioRequest request, CancellationToken cancellationToken)
    {
        var command = new CreatePrivilegioCommand(
            request.Clave, request.Modulo, request.Nombre, request.Descripcion, request.Ambito, request.Activo);
        var privilegio = await _sender.Send(command, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { privilegioId = privilegio.PrivilegioId }, privilegio);
    }

    [RequierePrivilegio(Permisos.PrivilegioGestionar)]
    [HttpPut("{privilegioId:guid}")]
    public async Task<ActionResult<PrivilegioDto>> Update(Guid privilegioId, UpdatePrivilegioRequest request, CancellationToken cancellationToken)
    {
        // La clave no se modifica
        var command = new UpdatePrivilegioCommand(
            privilegioId, request.Modulo, request.Nombre, request.Descripcion, request.Ambito, request.Activo);
        var privilegio = await _sender.Send(command, cancellationToken);

        return Ok(privilegio);
    }

    [RequierePrivilegio(Permisos.PrivilegioGestionar)]
    [HttpDelete("{privilegioId:guid}")]
    public async Task<IActionResult> Delete(Guid privilegioId, CancellationToken cancellationToken)
    {
        await _sender.Send(new DeletePrivilegioCommand(privilegioId), cancellationToken);
        return NoContent();
    }
}
