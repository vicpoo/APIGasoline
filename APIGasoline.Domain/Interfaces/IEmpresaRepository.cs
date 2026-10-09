using APIGasoline.Domain.Entities;

namespace APIGasoline.Domain.Interfaces;

/// <summary>
/// Las consultas solo devuelven empresas no eliminadas (soft delete).
/// </summary>
public interface IEmpresaRepository
{
    Task<Empresa?> GetByIdAsync(Guid empresaId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Empresa>> GetAllAsync(Guid? corporativoId = null, Guid? divisionId = null, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid empresaId, CancellationToken cancellationToken = default);

    /// <summary>Incluye empresas eliminadas, porque siguen referenciando al corporativo.</summary>
    Task<bool> ExistsByCorporativoAsync(Guid corporativoId, CancellationToken cancellationToken = default);

    /// <summary>Incluye empresas eliminadas, porque siguen referenciando a la división.</summary>
    Task<bool> ExistsByDivisionAsync(Guid divisionId, CancellationToken cancellationToken = default);

    Task AddAsync(Empresa empresa, CancellationToken cancellationToken = default);
}
