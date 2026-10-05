using APIGasoline.Domain.Entities;

namespace APIGasoline.Domain.Interfaces;

public interface ICentroCostoRepository
{
    Task<CentroCosto?> GetByIdAsync(int centroCostoId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CentroCosto>> GetAllAsync(int? empresaId = null, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(int centroCostoId, CancellationToken cancellationToken = default);
    Task<bool> ClaveExistsAsync(int empresaId, string clave, int? excludeCentroCostoId = null, CancellationToken cancellationToken = default);
    Task AddAsync(CentroCosto centroCosto, CancellationToken cancellationToken = default);
    void Remove(CentroCosto centroCosto);
}
