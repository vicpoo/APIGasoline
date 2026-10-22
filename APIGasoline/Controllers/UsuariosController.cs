using System;
using APIGasoline.Application.Features.Usuarios.Commands;
using APIGasoline.Application.Features.Usuarios.DTOs;
using APIGasoline.Application.Features.Usuarios.Queries;
using APIGasoline.Contracts.Usuarios;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using APIGasoline.Application.Common.Security;
using APIGasoline.Authorization;

namespace APIGasoline.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsuariosController : ControllerBase
{
    private readonly ISender _sender;

    public UsuariosController(ISender sender)
    {
        _sender = sender;
    }

    [RequierePrivilegio(Permisos.UsuarioVer, Permisos.UsuarioGestionar)]
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<UsuarioDto>>> GetAll(CancellationToken cancellationToken)
    {
        var usuarios = await _sender.Send(new GetUsuariosQuery(), cancellationToken);
        return Ok(usuarios);
    }

    [RequierePrivilegio(Permisos.UsuarioVer, Permisos.UsuarioGestionar)]
    [HttpGet("{usuarioId:guid}")]
    public async Task<ActionResult<UsuarioDto>> GetById(Guid usuarioId, CancellationToken cancellationToken)
    {
        var usuario = await _sender.Send(new GetUsuarioByIdQuery(usuarioId), cancellationToken);
        return Ok(usuario);
    }

    [RequierePrivilegio(Permisos.UsuarioGestionar)]
    [HttpPost]
    public async Task<ActionResult<UsuarioDto>> Create(CreateUsuarioRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateUsuarioCommand(request.Email, request.Password, request.Estado);
        var usuario = await _sender.Send(command, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { usuarioId = usuario.UsuarioId }, usuario);
    }

    [RequierePrivilegio(Permisos.UsuarioGestionar)]
    [HttpPut("{usuarioId:guid}")]
    public async Task<ActionResult<UsuarioDto>> Update(Guid usuarioId, UpdateUsuarioRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateUsuarioCommand(usuarioId, request.Email, request.Estado);
        var usuario = await _sender.Send(command, cancellationToken);

        return Ok(usuario);
    }

    /// <summary>
    /// Cambia la contraseña verificando la actual.
    /// </summary>
    [HttpPost("{usuarioId:guid}/cambiar-password")]
    public async Task<IActionResult> CambiarPassword(Guid usuarioId, CambiarPasswordRequest request, CancellationToken cancellationToken)
    {
        await _sender.Send(new CambiarPasswordCommand(usuarioId, request.PasswordActual, request.PasswordNueva), cancellationToken);
        return NoContent();
    }

    [RequierePrivilegio(Permisos.UsuarioGestionar)]
    [HttpDelete("{usuarioId:guid}")]
    public async Task<IActionResult> Delete(Guid usuarioId, CancellationToken cancellationToken)
    {
        await _sender.Send(new DeleteUsuarioCommand(usuarioId), cancellationToken);
        return NoContent();
    }
}