using APIGasoline.Domain.Entities;
using APIGasoline.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace APIGasoline.Infrastructure.Persistence.Repositories;

public class UsuarioRolRepository : IUsuarioRolRepository
{
    private readonly AppDbContext _context;

    public UsuarioRolRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<UsuarioRol?> GetByIdAsync(int usuarioRolId, CancellationToken cancellationToken = default)
        => _context.UsuarioRoles.FirstOrDefaultAsync(ur => ur.UsuarioRolId == usuarioRolId, cancellationToken);

    public async Task<IReadOnlyList<UsuarioRol>> GetAllAsync(int? usuarioId = null, int? rolId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.UsuarioRoles.AsNoTracking();

        if (usuarioId is not null)
            query = query.Where(ur => ur.UsuarioId == usuarioId);

        if (rolId is not null)
            query = query.Where(ur => ur.RolId == rolId);

        return await query
            .OrderBy(ur => ur.UsuarioRolId)
            .ToListAsync(cancellationToken);
    }

    public Task<bool> AsignacionExistsAsync(int usuarioId, int rolId, int? excludeUsuarioRolId = null, CancellationToken cancellationToken = default)
        => _context.UsuarioRoles.AnyAsync(
            ur => ur.UsuarioId == usuarioId
                  && ur.RolId == rolId
                  && (excludeUsuarioRolId == null || ur.UsuarioRolId != excludeUsuarioRolId),
            cancellationToken);

    public async Task AddAsync(UsuarioRol usuarioRol, CancellationToken cancellationToken = default)
        => await _context.UsuarioRoles.AddAsync(usuarioRol, cancellationToken);

    public void Remove(UsuarioRol usuarioRol)
        => _context.UsuarioRoles.Remove(usuarioRol);
}
