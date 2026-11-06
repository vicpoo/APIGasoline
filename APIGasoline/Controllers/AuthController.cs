using APIGasoline.Application.Common.Sesiones;
using APIGasoline.Application.Features.Auth.Commands;
using APIGasoline.Application.Features.Auth.DTOs;
using APIGasoline.Application.Features.Auth.Queries;
using APIGasoline.Application.Features.Sesiones.Commands;
using APIGasoline.Application.Features.Sesiones.DTOs;
using APIGasoline.Application.Features.Sesiones.Queries;
using APIGasoline.Configuration;
using APIGasoline.Contracts.Auth;
using APIGasoline.Domain.Enums;
using APIGasoline.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
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
    /// Inicia sesión con email y contraseña. Devuelve el JWT de acceso (corto), el refresh token
    /// (sesión larga, para POST /api/auth/refresh), los datos del usuario, su empleado (si tiene)
    /// y sus roles y privilegios.
    /// </summary>
    [AllowAnonymous]
    [EnableRateLimiting(LimiteAuthSettings.PoliticaLogin)]
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponseDto>> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var cliente = DatosCliente(request.Plataforma, request.NombreDispositivo);
        var resultado = await _sender.Send(new LoginCommand(request.Email, request.Password, cliente), cancellationToken);
        return Ok(resultado);
    }

    /// <summary>
    /// Renueva la sesión: con el refresh token devuelve un JWT de acceso nuevo y un refresh token
    /// nuevo (el anterior deja de servir). No requiere el header Authorization.
    /// 401 = la sesión ya no es válida: la app debe borrar sus datos de sesión y pedir login.
    /// </summary>
    [AllowAnonymous]
    [EnableRateLimiting(LimiteAuthSettings.PoliticaRefresh)]
    [HttpPost("refresh")]
    public async Task<ActionResult<SesionTokensDto>> Refresh(RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var resultado = await _sender.Send(new RefrescarSesionCommand(request.RefreshToken, DatosCliente()), cancellationToken);
        return Ok(resultado);
    }

    /// <summary>
    /// Cierra la sesión de este dispositivo (revoca su refresh token). Funciona aunque el JWT ya
    /// haya expirado. Siempre responde 204.
    /// </summary>
    [AllowAnonymous]
    [EnableRateLimiting(LimiteAuthSettings.PoliticaRefresh)]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout(RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        await _sender.Send(new CerrarSesionCommand(request.RefreshToken), cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Perfil del usuario del token. Requiere el header: Authorization: Bearer {token}
    /// </summary>
    [Authorize]
    [HttpGet("perfil")]
    public async Task<ActionResult<PerfilUsuarioDto>> Perfil(CancellationToken cancellationToken)
    {
        if (UsuarioDelToken() is not { } usuarioId)
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
        if (UsuarioDelToken() is not { } usuarioId)
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

    /// <summary>
    /// Mis sesiones abiertas (dispositivos con sesión vigente). La de esta petición viene con esActual = true.
    /// </summary>
    [Authorize]
    [HttpGet("sesiones")]
    public async Task<ActionResult<IReadOnlyList<SesionUsuarioDto>>> MisSesiones(CancellationToken cancellationToken)
    {
        if (UsuarioDelToken() is not { } usuarioId)
            return Unauthorized();

        var sesiones = await _sender.Send(new GetSesionesUsuarioQuery(usuarioId), cancellationToken);
        return Ok(sesiones);
    }

    /// <summary>Cierra una de mis sesiones (por ejemplo, la de un celular perdido).</summary>
    [Authorize]
    [HttpDelete("sesiones/{sesionId:guid}")]
    public async Task<IActionResult> CerrarMiSesion(Guid sesionId, CancellationToken cancellationToken)
    {
        if (UsuarioDelToken() is not { } usuarioId)
            return Unauthorized();

        await _sender.Send(new CerrarSesionUsuarioCommand(usuarioId, sesionId), cancellationToken);
        return NoContent();
    }

    /// <summary>Cierra todas mis sesiones excepto la de esta petición. Devuelve cuántas cerró.</summary>
    [Authorize]
    [HttpPost("sesiones/cerrar-otras")]
    public async Task<ActionResult<SesionesCerradasResponse>> CerrarMisOtrasSesiones(CancellationToken cancellationToken)
    {
        if (UsuarioDelToken() is not { } usuarioId)
            return Unauthorized();

        var cerradas = await _sender.Send(new CerrarSesionesUsuarioCommand(usuarioId, ConservarActual: true), cancellationToken);
        return Ok(new SesionesCerradasResponse(cerradas));
    }

    private Guid? UsuarioDelToken()
        => Guid.TryParse(User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value, out var id) ? id : null;

    /// <summary>Datos informativos del dispositivo (IP, user agent, plataforma) para la lista de sesiones.</summary>
    private DatosClienteSesion DatosCliente(PlataformaDispositivo? plataforma = null, string? nombreDispositivo = null)
    {
        if (plataforma is null
            && Request.Headers[UsuarioActual.HeaderOrigen].ToString().Equals("web", StringComparison.OrdinalIgnoreCase))
        {
            plataforma = PlataformaDispositivo.Web;
        }

        return new DatosClienteSesion(
            plataforma,
            nombreDispositivo,
            HttpContext.Connection.RemoteIpAddress?.ToString(),
            Request.Headers.UserAgent.ToString());
    }
}
