using System;
using APIGasoline.Application.Features.Sesiones.Commands;
using APIGasoline.Application.Features.Sesiones.DTOs;
using APIGasoline.Application.Features.Sesiones.Queries;
using APIGasoline.Application.Features.Usuarios.Commands;
using APIGasoline.Application.Features.Usuarios.DTOs;
using APIGasoline.Application.Features.Usuarios.Queries;
using APIGasoline.Contracts.Auth;
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
        var command = new CreateUsuarioCommand(request.Email, request.Password, request.Estado, request.EmpresaId);
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

    /// <summary>
    /// Sesiones abiertas (dispositivos) del usuario. Las propias, o las de cualquiera con usuario.gestionar.
    /// </summary>
    [HttpGet("{usuarioId:guid}/sesiones")]
    public async Task<ActionResult<IReadOnlyList<SesionUsuarioDto>>> GetSesiones(Guid usuarioId, CancellationToken cancellationToken)
    {
        var sesiones = await _sender.Send(new GetSesionesUsuarioQuery(usuarioId), cancellationToken);
        return Ok(sesiones);
    }

    /// <summary>
    /// Cierra una sesión del usuario (ya no podrá renovar su token). Las propias, o las de cualquiera con usuario.gestionar.
    /// </summary>
    [HttpDelete("{usuarioId:guid}/sesiones/{sesionId:guid}")]
    public async Task<IActionResult> CerrarSesion(Guid usuarioId, Guid sesionId, CancellationToken cancellationToken)
    {
        await _sender.Send(new CerrarSesionUsuarioCommand(usuarioId, sesionId), cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Cierra TODAS las sesiones del usuario (por ejemplo, si reporta que le robaron el celular).
    /// Las propias, o las de cualquiera con usuario.gestionar. Devuelve cuántas cerró.
    /// </summary>
    [HttpPost("{usuarioId:guid}/sesiones/cerrar-todas")]
    public async Task<ActionResult<SesionesCerradasResponse>> CerrarTodasLasSesiones(Guid usuarioId, CancellationToken cancellationToken)
    {
        var cerradas = await _sender.Send(new CerrarSesionesUsuarioCommand(usuarioId, ConservarActual: false), cancellationToken);
        return Ok(new SesionesCerradasResponse(cerradas));
    }

    [RequierePrivilegio(Permisos.UsuarioGestionar)]
    [HttpDelete("{usuarioId:guid}")]
    public async Task<IActionResult> Delete(Guid usuarioId, CancellationToken cancellationToken)
    {
        await _sender.Send(new DeleteUsuarioCommand(usuarioId), cancellationToken);
        return NoContent();
    }
}