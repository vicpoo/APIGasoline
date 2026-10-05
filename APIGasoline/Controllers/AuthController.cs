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

        if (!int.TryParse(sub, out var usuarioId))
            return Unauthorized();

        var perfil = await _sender.Send(new GetPerfilQuery(usuarioId), cancellationToken);
        return Ok(perfil);
    }
}
