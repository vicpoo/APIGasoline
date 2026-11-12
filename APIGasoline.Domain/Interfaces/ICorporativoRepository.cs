using APIGasoline.Domain.Entities;

namespace APIGasoline.Domain.Interfaces;

public interface ICorporativoRepository
{
    Task<Corporativo?> GetByIdAsync(Guid corporativoId, CancellationToken cancellationToken = default);
    /// <param name="empresaId">Solo los de esa empresa (null = los que puede ver el usuario).</param>
    /// <param name="divisionId">Solo los corporativos ligados a esa división.</param>
    Task<IReadOnlyList<Corporativo>> GetAllAsync(Guid? empresaId = null, Guid? divisionId = null, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid corporativoId, CancellationToken cancellationToken = default);

    /// <summary>El nombre es único dentro de la empresa.</summary>
    Task<bool> NombreExistsAsync(Guid empresaId, string nombre, Guid? excludeCorporativoId = null, CancellationToken cancellationToken = default);
    Task AddAsync(Corporativo corporativo, CancellationToken cancellationToken = default);
    void Remove(Corporativo corporativo);
}
