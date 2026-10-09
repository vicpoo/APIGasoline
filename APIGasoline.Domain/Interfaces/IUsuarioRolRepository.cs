using APIGasoline.Domain.Entities;

namespace APIGasoline.Domain.Interfaces;

public interface IUsuarioRolRepository
{
    Task<UsuarioRol?> GetByIdAsync(Guid usuarioRolId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UsuarioRol>> GetAllAsync(Guid? usuarioId = null, Guid? rolId = null, CancellationToken cancellationToken = default);
    Task<bool> AsignacionExistsAsync(Guid usuarioId, Guid rolId, Guid? excludeUsuarioRolId = null, CancellationToken cancellationToken = default);
    Task AddAsync(UsuarioRol usuarioRol, CancellationToken cancellationToken = default);
    void Remove(UsuarioRol usuarioRol);
}
