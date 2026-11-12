using APIGasoline.Domain.Entities;

namespace APIGasoline.Domain.Interfaces;

/// <summary>Ligas N:M entre corporativos y divisiones de una empresa.</summary>
public interface ICorporativoDivisionRepository
{
    Task<CorporativoDivision?> GetAsync(Guid corporativoId, Guid divisionId, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid corporativoId, Guid divisionId, CancellationToken cancellationToken = default);
    Task<bool> ExistsByCorporativoAsync(Guid corporativoId, CancellationToken cancellationToken = default);
    Task<bool> ExistsByDivisionAsync(Guid divisionId, CancellationToken cancellationToken = default);
    Task AddAsync(CorporativoDivision liga, CancellationToken cancellationToken = default);
    void Remove(CorporativoDivision liga);
}
