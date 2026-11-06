using APIGasoline.Domain.Entities;

namespace APIGasoline.Domain.Interfaces;

/// <summary>
/// Sesiones (refresh tokens) de los usuarios. Las lecturas devuelven entidades con seguimiento
/// de cambios porque casi siempre es para rotar o revocar.
/// </summary>
public interface ISesionUsuarioRepository
{
    Task<SesionUsuario?> GetByIdAsync(Guid sesionUsuarioId, CancellationToken cancellationToken = default);

    /// <summary>Busca por el hash del token ACTUAL o del ANTERIOR (para detectar reuso).</summary>
    Task<SesionUsuario?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default);

    /// <summary>Sesiones no revocadas y no expiradas del usuario, la más reciente primero.</summary>
    Task<IReadOnlyList<SesionUsuario>> GetActivasByUsuarioAsync(Guid usuarioId, DateTime ahora, CancellationToken cancellationToken = default);

    /// <summary>Sesiones no revocadas y no expiradas iniciadas por empleados de la empresa.</summary>
    Task<IReadOnlyList<SesionUsuario>> GetActivasByEmpresaAsync(Guid empresaId, DateTime ahora, CancellationToken cancellationToken = default);

    Task AddAsync(SesionUsuario sesion, CancellationToken cancellationToken = default);

    /// <summary>
    /// Borra (directo en la BD) las sesiones que expiraron o se revocaron antes de <paramref name="limite"/>.
    /// Devuelve cuántas borró.
    /// </summary>
    Task<int> EliminarObsoletasAsync(DateTime limite, CancellationToken cancellationToken = default);
}
