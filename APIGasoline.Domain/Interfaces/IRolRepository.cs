using APIGasoline.Domain.Entities;

namespace APIGasoline.Domain.Interfaces;

public interface IRolRepository
{
    Task<Rol?> GetByIdAsync(Guid rolId, CancellationToken cancellationToken = default);

    /// <summary>
    /// soloGlobales = true → solo roles globales.
    /// empresaId con valor → roles disponibles para esa empresa (globales + exclusivos de ella).
    /// Sin filtros → todos.
    /// </summary>
    Task<IReadOnlyList<Rol>> GetAllAsync(Guid? empresaId = null, bool soloGlobales = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Roles activos asignados al usuario que aplican a su empresa (globales + exclusivos de esa empresa).
    /// </summary>
    Task<IReadOnlyList<Rol>> GetActivosByUsuarioAsync(Guid usuarioId, Guid? empresaId, CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(Guid rolId, CancellationToken cancellationToken = default);

    /// <summary>El nombre es único por empresa (o entre los globales cuando empresaId es null).</summary>
    Task<bool> NombreExistsAsync(string nombre, Guid? empresaId, Guid? excludeRolId = null, CancellationToken cancellationToken = default);

    Task AddAsync(Rol rol, CancellationToken cancellationToken = default);
    void Remove(Rol rol);
}
