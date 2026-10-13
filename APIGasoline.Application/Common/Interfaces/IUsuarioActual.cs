using APIGasoline.Domain.Enums;

namespace APIGasoline.Application.Common.Interfaces;

/// <summary>
/// Usuario autenticado de la petición actual, leído del token JWT.
/// Fuera de una petición HTTP (workers en segundo plano) no hay usuario y el origen es Sistema.
/// La identidad SIEMPRE se toma del token, nunca de los datos que envía el cliente.
/// </summary>
public interface IUsuarioActual
{
    bool EstaAutenticado { get; }

    Guid? UsuarioId { get; }

    Guid? EmpleadoId { get; }

    Guid? EmpresaId { get; }

    IReadOnlyCollection<string> Roles { get; }

    /// <summary>Móvil, web o sistema (header X-Origen: movil | web).</summary>
    OrigenAuditoria Origen { get; }

    bool TieneRol(string rol);
}
