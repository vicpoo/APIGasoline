using APIGasoline.Domain.Entities;

namespace APIGasoline.Domain.Interfaces;

public interface ICentroCostoRepository
{
    Task<CentroCosto?> GetByIdAsync(Guid centroCostoId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CentroCosto>> GetAllAsync(Guid? empresaId = null, Guid? corporativoId = null, Guid? divisionId = null, CancellationToken cancellationToken = default);

    Task<bool> ExistsByCorporativoAsync(Guid corporativoId, CancellationToken cancellationToken = default);
    Task<bool> ExistsByDivisionAsync(Guid divisionId, CancellationToken cancellationToken = default);

    /// <summary>Hay centros que usan esa pareja corporativo + división.</summary>
    Task<bool> ExistsByLigaAsync(Guid corporativoId, Guid divisionId, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid centroCostoId, CancellationToken cancellationToken = default);
    Task<bool> ClaveExistsAsync(Guid empresaId, string clave, Guid? excludeCentroCostoId = null, CancellationToken cancellationToken = default);
    Task AddAsync(CentroCosto centroCosto, CancellationToken cancellationToken = default);
    void Remove(CentroCosto centroCosto);
}
