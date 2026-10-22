using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Mvc;

namespace APIGasoline.Authorization;

/// <summary>
/// Responde 401 / 403 con el mismo formato ProblemDetails que el resto de la API
/// (y en el 403 dice QUÉ privilegio falta, útil para la app y para Postman).
/// </summary>
public sealed class RespuestaAutorizacionHandler : IAuthorizationMiddlewareResultHandler
{
    private readonly AuthorizationMiddlewareResultHandler _porDefecto = new();

    public async Task HandleAsync(
        RequestDelegate next,
        HttpContext context,
        AuthorizationPolicy policy,
        PolicyAuthorizationResult authorizeResult)
    {
        if (authorizeResult.Challenged)
        {
            // Deja que JwtBearer agregue el encabezado WWW-Authenticate y luego escribe el cuerpo
            await context.ChallengeAsync();

            if (!context.Response.HasStarted)
            {
                await context.Response.WriteAsJsonAsync(new ProblemDetails
                {
                    Status = StatusCodes.Status401Unauthorized,
                    Title = "No autorizado",
                    Detail = "Debes iniciar sesión (encabezado Authorization: Bearer <token>)."
                });
            }

            return;
        }

        if (authorizeResult.Forbidden)
        {
            var motivo = authorizeResult.AuthorizationFailure?.FailureReasons
                .Select(r => r.Message)
                .FirstOrDefault(m => !string.IsNullOrWhiteSpace(m))
                ?? "Tu perfil no tiene permiso para realizar esta acción.";

            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsJsonAsync(new ProblemDetails
            {
                Status = StatusCodes.Status403Forbidden,
                Title = "Acceso denegado",
                Detail = motivo
            });

            return;
        }

        await _porDefecto.HandleAsync(next, context, policy, authorizeResult);
    }
}
