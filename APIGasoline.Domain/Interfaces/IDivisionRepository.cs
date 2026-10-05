using APIGasoline.Domain.Entities;

namespace APIGasoline.Domain.Interfaces;

public interface IDivisionRepository
{
    Task<Division?> GetByIdAsync(int divisionId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Division>> GetAllAsync(int? corporativoId = null, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(int divisionId, CancellationToken cancellationToken = default);
    Task<bool> ExistsByCorporativoAsync(int corporativoId, CancellationToken cancellationToken = default);
    Task AddAsync(Division division, CancellationToken cancellationToken = default);
    void Remove(Division division);
}
