using APIGasoline.Domain.Entities;

namespace APIGasoline.Domain.Interfaces;

public interface IUsuarioRolRepository
{
    Task<UsuarioRol?> GetByIdAsync(int usuarioRolId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UsuarioRol>> GetAllAsync(int? usuarioId = null, int? rolId = null, CancellationToken cancellationToken = default);
    Task<bool> AsignacionExistsAsync(int usuarioId, int rolId, int? excludeUsuarioRolId = null, CancellationToken cancellationToken = default);
    Task AddAsync(UsuarioRol usuarioRol, CancellationToken cancellationToken = default);
    void Remove(UsuarioRol usuarioRol);
}
