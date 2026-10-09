using APIGasoline.Domain.Entities;

namespace APIGasoline.Domain.Interfaces;

public interface ICorporativoRepository
{
    Task<Corporativo?> GetByIdAsync(Guid corporativoId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Corporativo>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid corporativoId, CancellationToken cancellationToken = default);
    Task AddAsync(Corporativo corporativo, CancellationToken cancellationToken = default);
    void Remove(Corporativo corporativo);
}
