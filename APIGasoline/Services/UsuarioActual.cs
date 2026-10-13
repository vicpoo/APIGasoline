using APIGasoline.Application.Common.Interfaces;
using APIGasoline.Domain.Enums;
using Microsoft.IdentityModel.JsonWebTokens;

namespace APIGasoline.Services;

/// <summary>
/// Lee el usuario del token JWT de la petición actual (claims sub, empleado_id, empresa_id y role).
/// Sin petición HTTP (workers) no hay usuario y el origen es Sistema.
/// </summary>
public sealed class UsuarioActual : IUsuarioActual
{
    public const string HeaderOrigen = "X-Origen";

    private const string ClaimEmpleadoId = "empleado_id";
    private const string ClaimEmpresaId = "empresa_id";
    private const string ClaimRol = "role";

    private readonly IHttpContextAccessor _httpContextAccessor;

    public UsuarioActual(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private HttpContext? Contexto => _httpContextAccessor.HttpContext;

    public bool EstaAutenticado => Contexto?.User.Identity?.IsAuthenticated == true;

    public Guid? UsuarioId => LeerGuid(JwtRegisteredClaimNames.Sub);

    public Guid? EmpleadoId => LeerGuid(ClaimEmpleadoId);

    public Guid? EmpresaId => LeerGuid(ClaimEmpresaId);

    public IReadOnlyCollection<string> Roles
        => EstaAutenticado
            ? Contexto!.User.FindAll(ClaimRol).Select(c => c.Value).ToArray()
            : Array.Empty<string>();

    public OrigenAuditoria Origen
    {
        get
        {
            if (Contexto is null)
                return OrigenAuditoria.Sistema;

            var origen = Contexto.Request.Headers[HeaderOrigen].ToString();

            return origen.Equals("movil", StringComparison.OrdinalIgnoreCase)
                || origen.Equals("móvil", StringComparison.OrdinalIgnoreCase)
                || origen.Equals("mobile", StringComparison.OrdinalIgnoreCase)
                ? OrigenAuditoria.Movil
                : OrigenAuditoria.Web;
        }
    }

    public bool TieneRol(string rol)
        => Roles.Any(r => string.Equals(r, rol, StringComparison.OrdinalIgnoreCase));

    private Guid? LeerGuid(string claim)
    {
        if (!EstaAutenticado)
            return null;

        var valor = Contexto!.User.FindFirst(claim)?.Value;
        return Guid.TryParse(valor, out var id) ? id : null;
    }
}
