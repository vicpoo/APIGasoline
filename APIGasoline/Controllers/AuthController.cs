using APIGasoline.Application.Features.Auth.Commands;
using APIGasoline.Application.Features.Auth.DTOs;
using APIGasoline.Application.Features.Auth.Queries;
using APIGasoline.Contracts.Auth;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;

namespace APIGasoline.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ISender _sender;

    public AuthController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Inicia sesión con email y contraseña. Devuelve el token JWT, los datos del usuario,
    /// su empleado (si tiene) y sus roles y privilegios.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponseDto>> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var resultado = await _sender.Send(new LoginCommand(request.Email, request.Password), cancellationToken);
        return Ok(resultado);
    }

    /// <summary>
    /// Perfil del usuario del token. Requiere el header: Authorization: Bearer {token}
    /// </summary>
    [Authorize]
    [HttpGet("perfil")]
    public async Task<ActionResult<PerfilUsuarioDto>> Perfil(CancellationToken cancellationToken)
    {
        var sub = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

        if (!Guid.TryParse(sub, out var usuarioId))
            return Unauthorized();

        var perfil = await _sender.Send(new GetPerfilQuery(usuarioId), cancellationToken);
        return Ok(perfil);
    }

    /// <summary>
    /// Privilegios efectivos del usuario del token (claves) y su versión, para la app offline-first.
    /// Manda la versión guardada en el header If-None-Match: si no cambió responde 304 sin cuerpo
    /// y la app conserva su copia local; si cambió responde 200 con las claves nuevas.
    /// La versión también va en el header ETag.
    /// </summary>
    [Authorize]
    [HttpGet("privilegios")]
    [ProducesResponseType<PrivilegiosUsuarioDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status304NotModified)]
    public async Task<ActionResult<PrivilegiosUsuarioDto>> Privilegios(CancellationToken cancellationToken)
    {
        var sub = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

        if (!Guid.TryParse(sub, out var usuarioId))
            return Unauthorized();

        var privilegios = await _sender.Send(new GetPrivilegiosUsuarioQuery(usuarioId), cancellationToken);

        var etag = $"\"{privilegios.Version}\"";
        Response.Headers.ETag = etag;
        Response.Headers.CacheControl = "no-cache";

        var versionCliente = Request.Headers.IfNoneMatch.ToString().Trim();

        if (versionCliente == etag || versionCliente.Trim('"') == privilegios.Version)
            return StatusCode(StatusCodes.Status304NotModified);

        return Ok(privilegios);
    }
}
