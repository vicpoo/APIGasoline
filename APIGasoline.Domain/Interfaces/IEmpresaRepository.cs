using APIGasoline.Domain.Entities;

namespace APIGasoline.Domain.Interfaces;

/// <summary>
/// Las consultas solo devuelven empresas no eliminadas (soft delete).
/// </summary>
public interface IEmpresaRepository
{
    Task<Empresa?> GetByIdAsync(Guid empresaId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Empresa>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid empresaId, CancellationToken cancellationToken = default);

    Task AddAsync(Empresa empresa, CancellationToken cancellationToken = default);
}
