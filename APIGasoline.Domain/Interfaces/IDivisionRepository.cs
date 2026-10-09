using APIGasoline.Domain.Entities;

namespace APIGasoline.Domain.Interfaces;

public interface IDivisionRepository
{
    Task<Division?> GetByIdAsync(Guid divisionId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Division>> GetAllAsync(Guid? corporativoId = null, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid divisionId, CancellationToken cancellationToken = default);
    Task<bool> ExistsByCorporativoAsync(Guid corporativoId, CancellationToken cancellationToken = default);
    Task AddAsync(Division division, CancellationToken cancellationToken = default);
    void Remove(Division division);
}
