using APIGasoline.Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace APIGasoline.Authorization;

/// <summary>
/// Revisa los privilegios efectivos del usuario (de la BD, con caché) contra el requisito.
/// No importa cómo se llamen sus roles: solo qué privilegios tienen.
/// </summary>
public sealed class PrivilegioAuthorizationHandler : AuthorizationHandler<PrivilegioRequirement>
{
    private readonly IUsuarioActual _usuarioActual;

    public PrivilegioAuthorizationHandler(IUsuarioActual usuarioActual)
    {
        _usuarioActual = usuarioActual;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PrivilegioRequirement requirement)
    {
        // Sin sesión: lo resuelve la política (401)
        if (context.User.Identity?.IsAuthenticated != true)
            return;

        var privilegios = await _usuarioActual.GetPrivilegiosAsync();

        if (requirement.Claves.Any(privilegios.Contains))
        {
            context.Succeed(requirement);
            return;
        }

        context.Fail(new AuthorizationFailureReason(
            this,
            requirement.Claves.Count == 1
                ? $"Necesitas el privilegio '{requirement.Claves[0]}'."
                : $"Necesitas alguno de estos privilegios: {string.Join(", ", requirement.Claves.Select(c => $"'{c}'"))}."));
    }
}
