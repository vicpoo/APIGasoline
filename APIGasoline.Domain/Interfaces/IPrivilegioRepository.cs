using APIGasoline.Domain.Entities;

namespace APIGasoline.Domain.Interfaces;

public interface IPrivilegioRepository
{
    Task<Privilegio?> GetByIdAsync(Guid privilegioId, CancellationToken cancellationToken = default);

    /// <summary>Ordenados por módulo y clave. Con modulo, solo los de ese módulo.</summary>
    Task<IReadOnlyList<Privilegio>> GetAllAsync(string? modulo = null, CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(Guid privilegioId, CancellationToken cancellationToken = default);

    /// <summary>La clave es única e inmutable.</summary>
    Task<bool> ClaveExistsAsync(string clave, CancellationToken cancellationToken = default);

    /// <summary>Claves (sin repetir y ordenadas) de los privilegios activos de esos roles.</summary>
    Task<IReadOnlyList<string>> GetClavesActivasByRolesAsync(IReadOnlyList<Guid> rolIds, CancellationToken cancellationToken = default);

    Task AddAsync(Privilegio privilegio, CancellationToken cancellationToken = default);
    void Remove(Privilegio privilegio);
}
