using Microsoft.AspNetCore.Authorization;

namespace APIGasoline.Authorization;

/// <summary>
/// Autorización basada en permisos: el endpoint pide un PRIVILEGIO (clave de
/// <see cref="Application.Common.Security.Permisos"/>), no un rol.
///
/// <code>
/// [RequierePrivilegio(Permisos.SolicitudAprobar)]                          // necesita ese privilegio
/// [RequierePrivilegio(Permisos.ViajeVerPropios, Permisos.ViajeVerTodos)] // basta con UNO de la lista
/// </code>
/// Varios atributos en el mismo endpoint se combinan con Y (debe cumplir todos).
/// Sin sesión responde 401; con sesión pero sin el privilegio, 403.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public sealed class RequierePrivilegioAttribute : AuthorizeAttribute, IAuthorizationRequirementData
{
    public RequierePrivilegioAttribute(params string[] claves)
    {
        if (claves is null || claves.Length == 0)
            throw new ArgumentException("Indica al menos una clave de privilegio.", nameof(claves));

        Claves = claves;
    }

    public IReadOnlyList<string> Claves { get; }

    public IEnumerable<IAuthorizationRequirement> GetRequirements()
    {
        yield return new PrivilegioRequirement(Claves);
    }
}
