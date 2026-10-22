using APIGasoline.Domain.Enums;

namespace APIGasoline.Application.Common.Interfaces;

/// <summary>
/// Usuario autenticado de la petición actual, leído del token JWT.
/// Fuera de una petición HTTP (workers en segundo plano) no hay usuario y el origen es Sistema.
/// La identidad SIEMPRE se toma del token, nunca de los datos que envía el cliente.
///
/// Autorización basada en permisos: para decidir qué puede hacer el usuario se usan sus
/// PRIVILEGIOS (claves de <see cref="Security.Permisos"/>), nunca el nombre de sus roles.
/// </summary>
public interface IUsuarioActual
{
    bool EstaAutenticado { get; }

    Guid? UsuarioId { get; }

    Guid? EmpleadoId { get; }

    Guid? EmpresaId { get; }

    /// <summary>Nombres de los roles (solo informativo: NO usar para autorizar).</summary>
    IReadOnlyCollection<string> Roles { get; }

    /// <summary>Móvil, web o sistema (header X-Origen: movil | web).</summary>
    OrigenAuditoria Origen { get; }

    /// <summary>
    /// Claves de los privilegios efectivos del usuario: privilegios activos de sus roles activos
    /// (globales o de su empresa). Se consultan en la BD (con caché corta), así que un cambio de
    /// privilegios aplica sin volver a iniciar sesión. Sin sesión: conjunto vacío.
    /// </summary>
    Task<IReadOnlySet<string>> GetPrivilegiosAsync(CancellationToken cancellationToken = default);

    Task<bool> TienePrivilegioAsync(string clave, CancellationToken cancellationToken = default);
}
