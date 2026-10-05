using APIGasoline.Domain.Entities;

namespace APIGasoline.Domain.Interfaces;

/// <summary>
/// Las consultas solo devuelven empresas no eliminadas (soft delete).
/// </summary>
public interface IEmpresaRepository
{
    Task<Empresa?> GetByIdAsync(int empresaId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Empresa>> GetAllAsync(int? corporativoId = null, int? divisionId = null, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(int empresaId, CancellationToken cancellationToken = default);

    /// <summary>Incluye empresas eliminadas, porque siguen referenciando al corporativo.</summary>
    Task<bool> ExistsByCorporativoAsync(int corporativoId, CancellationToken cancellationToken = default);

    /// <summary>Incluye empresas eliminadas, porque siguen referenciando a la división.</summary>
    Task<bool> ExistsByDivisionAsync(int divisionId, CancellationToken cancellationToken = default);

    Task AddAsync(Empresa empresa, CancellationToken cancellationToken = default);
}
