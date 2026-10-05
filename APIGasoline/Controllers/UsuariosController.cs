using System;
using APIGasoline.Application.Features.Usuarios.Commands;
using APIGasoline.Application.Features.Usuarios.DTOs;
using APIGasoline.Application.Features.Usuarios.Queries;
using APIGasoline.Contracts.Usuarios;
using MediatR;
using Microsoft.AspNetCore.Mvc;

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

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<UsuarioDto>>> GetAll(CancellationToken cancellationToken)
    {
        var usuarios = await _sender.Send(new GetUsuariosQuery(), cancellationToken);
        return Ok(usuarios);
    }

    [HttpGet("{usuarioId:int}")]
    public async Task<ActionResult<UsuarioDto>> GetById(int usuarioId, CancellationToken cancellationToken)
    {
        var usuario = await _sender.Send(new GetUsuarioByIdQuery(usuarioId), cancellationToken);
        return Ok(usuario);
    }

    [HttpPost]
    public async Task<ActionResult<UsuarioDto>> Create(CreateUsuarioRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateUsuarioCommand(request.Email, request.Password, request.Estado);
        var usuario = await _sender.Send(command, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { usuarioId = usuario.UsuarioId }, usuario);
    }

    [HttpPut("{usuarioId:int}")]
    public async Task<ActionResult<UsuarioDto>> Update(int usuarioId, UpdateUsuarioRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateUsuarioCommand(usuarioId, request.Email, request.Estado);
        var usuario = await _sender.Send(command, cancellationToken);

        return Ok(usuario);
    }

    /// <summary>
    /// Cambia la contraseña verificando la actual.
    /// </summary>
    [HttpPost("{usuarioId:int}/cambiar-password")]
    public async Task<IActionResult> CambiarPassword(int usuarioId, CambiarPasswordRequest request, CancellationToken cancellationToken)
    {
        await _sender.Send(new CambiarPasswordCommand(usuarioId, request.PasswordActual, request.PasswordNueva), cancellationToken);
        return NoContent();
    }

    [HttpDelete("{usuarioId:int}")]
    public async Task<IActionResult> Delete(int usuarioId, CancellationToken cancellationToken)
    {
        await _sender.Send(new DeleteUsuarioCommand(usuarioId), cancellationToken);
        return NoContent();
    }
}