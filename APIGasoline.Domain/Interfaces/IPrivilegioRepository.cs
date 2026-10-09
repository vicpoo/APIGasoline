using APIGasoline.Domain.Entities;

namespace APIGasoline.Domain.Interfaces;

public interface IPrivilegioRepository
{
    Task<Privilegio?> GetByIdAsync(Guid privilegioId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Privilegio>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid privilegioId, CancellationToken cancellationToken = default);
    Task<bool> NombreExistsAsync(string nombre, Guid? excludePrivilegioId = null, CancellationToken cancellationToken = default);
    /// <summary>Nombres (sin repetir) de los privilegios activos de esos roles.</summary>
    Task<IReadOnlyList<string>> GetNombresActivosByRolesAsync(IReadOnlyList<Guid> rolIds, CancellationToken cancellationToken = default);

    Task AddAsync(Privilegio privilegio, CancellationToken cancellationToken = default);
    void Remove(Privilegio privilegio);
}
