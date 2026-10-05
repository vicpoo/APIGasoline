using APIGasoline.Domain.Entities;

namespace APIGasoline.Domain.Interfaces;

public interface IUsuarioRepository
{
    Task<Usuario?> GetByIdAsync(int usuarioId, CancellationToken cancellationToken = default);
    Task<Usuario?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Usuario>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(int usuarioId, CancellationToken cancellationToken = default);
    Task<bool> EmailExistsAsync(string email, int? excludeUsuarioId = null, CancellationToken cancellationToken = default);
    Task AddAsync(Usuario usuario, CancellationToken cancellationToken = default);
    void Remove(Usuario usuario);
}
