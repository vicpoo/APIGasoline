using APIGasoline.Domain.Entities;

namespace APIGasoline.Domain.Interfaces;

public interface IDivisionRepository
{
    Task<Division?> GetByIdAsync(Guid divisionId, CancellationToken cancellationToken = default);
    /// <param name="empresaId">Solo las de esa empresa (null = las que puede ver el usuario).</param>
    /// <param name="corporativoId">Solo las divisiones ligadas a ese corporativo.</param>
    Task<IReadOnlyList<Division>> GetAllAsync(Guid? empresaId = null, Guid? corporativoId = null, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid divisionId, CancellationToken cancellationToken = default);

    /// <summary>El nombre es único dentro de la empresa.</summary>
    Task<bool> NombreExistsAsync(Guid empresaId, string nombre, Guid? excludeDivisionId = null, CancellationToken cancellationToken = default);
    Task AddAsync(Division division, CancellationToken cancellationToken = default);
    void Remove(Division division);
}
