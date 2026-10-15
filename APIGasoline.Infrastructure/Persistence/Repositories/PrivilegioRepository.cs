using APIGasoline.Domain.Entities;
using APIGasoline.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace APIGasoline.Infrastructure.Persistence.Repositories;

public class PrivilegioRepository : IPrivilegioRepository
{
    private readonly AppDbContext _context;

    public PrivilegioRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<Privilegio?> GetByIdAsync(Guid privilegioId, CancellationToken cancellationToken = default)
        => _context.Privilegios.FirstOrDefaultAsync(p => p.PrivilegioId == privilegioId, cancellationToken);

    public async Task<IReadOnlyList<Privilegio>> GetAllAsync(string? modulo = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Privilegios.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(modulo))
        {
            var normalizado = modulo.Trim().ToLowerInvariant();
            query = query.Where(p => p.Modulo == normalizado);
        }

        return await query
            .OrderBy(p => p.Modulo)
            .ThenBy(p => p.Clave)
            .ToListAsync(cancellationToken);
    }

    public Task<bool> ExistsAsync(Guid privilegioId, CancellationToken cancellationToken = default)
        => _context.Privilegios.AnyAsync(p => p.PrivilegioId == privilegioId, cancellationToken);

    public Task<bool> ClaveExistsAsync(string clave, CancellationToken cancellationToken = default)
        => _context.Privilegios.AnyAsync(p => p.Clave == clave, cancellationToken);

    public async Task<IReadOnlyList<string>> GetClavesActivasByRolesAsync(IReadOnlyList<Guid> rolIds, CancellationToken cancellationToken = default)
    {
        if (rolIds.Count == 0)
            return Array.Empty<string>();

        var ids = rolIds.ToList();

        return await _context.Privilegios
            .AsNoTracking()
            .Where(p => p.Activo
                        && _context.RolPrivilegios.Any(rp => ids.Contains(rp.RolId) && rp.PrivilegioId == p.PrivilegioId))
            .Select(p => p.Clave)
            .Distinct()
            .OrderBy(clave => clave)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Privilegio privilegio, CancellationToken cancellationToken = default)
        => await _context.Privilegios.AddAsync(privilegio, cancellationToken);

    public void Remove(Privilegio privilegio)
        => _context.Privilegios.Remove(privilegio);
}
