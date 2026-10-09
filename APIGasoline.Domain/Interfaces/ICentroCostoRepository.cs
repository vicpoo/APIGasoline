using APIGasoline.Domain.Entities;

namespace APIGasoline.Domain.Interfaces;

public interface ICentroCostoRepository
{
    Task<CentroCosto?> GetByIdAsync(Guid centroCostoId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CentroCosto>> GetAllAsync(Guid? empresaId = null, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid centroCostoId, CancellationToken cancellationToken = default);
    Task<bool> ClaveExistsAsync(Guid empresaId, string clave, Guid? excludeCentroCostoId = null, CancellationToken cancellationToken = default);
    Task AddAsync(CentroCosto centroCosto, CancellationToken cancellationToken = default);
    void Remove(CentroCosto centroCosto);
}
