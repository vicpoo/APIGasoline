using APIGasoline.Domain.Entities;

namespace APIGasoline.Domain.Interfaces;

public interface IRolPrivilegioRepository
{
    Task<RolPrivilegio?> GetByIdAsync(int rolPrivilegioId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RolPrivilegio>> GetAllAsync(int? rolId = null, int? privilegioId = null, CancellationToken cancellationToken = default);
    Task<bool> AsignacionExistsAsync(int rolId, int privilegioId, int? excludeRolPrivilegioId = null, CancellationToken cancellationToken = default);
    Task AddAsync(RolPrivilegio rolPrivilegio, CancellationToken cancellationToken = default);
    void Remove(RolPrivilegio rolPrivilegio);
}
