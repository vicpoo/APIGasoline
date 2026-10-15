using APIGasoline.Domain.Entities;

namespace APIGasoline.Domain.Interfaces;

public interface IRolPrivilegioRepository
{
    Task<RolPrivilegio?> GetByIdAsync(Guid rolPrivilegioId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RolPrivilegio>> GetAllAsync(Guid? rolId = null, Guid? privilegioId = null, CancellationToken cancellationToken = default);
    Task<bool> AsignacionExistsAsync(Guid rolId, Guid privilegioId, Guid? excludeRolPrivilegioId = null, CancellationToken cancellationToken = default);

    /// <summary>true si el privilegio está asignado a algún rol exclusivo de una empresa.</summary>
    Task<bool> AsignadoARolDeEmpresaAsync(Guid privilegioId, CancellationToken cancellationToken = default);

    Task AddAsync(RolPrivilegio rolPrivilegio, CancellationToken cancellationToken = default);
    void Remove(RolPrivilegio rolPrivilegio);
}
