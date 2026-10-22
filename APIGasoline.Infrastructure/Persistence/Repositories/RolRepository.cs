using APIGasoline.Domain.Entities;
using APIGasoline.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace APIGasoline.Infrastructure.Persistence.Repositories;

public class RolRepository : IRolRepository
{
    private readonly AppDbContext _context;

    public RolRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<Rol?> GetByIdAsync(Guid rolId, CancellationToken cancellationToken = default)
        => _context.Roles.FirstOrDefaultAsync(r => r.RolId == rolId, cancellationToken);

    public async Task<IReadOnlyList<Rol>> GetAllAsync(Guid? empresaId = null, bool soloGlobales = false, CancellationToken cancellationToken = default)
    {
        var query = _context.Roles.AsNoTracking();

        if (soloGlobales)
            query = query.Where(r => r.EmpresaId == null);
        else if (empresaId is not null)
            query = query.Where(r => r.EmpresaId == null || r.EmpresaId == empresaId);

        return await query
            .OrderBy(r => r.EmpresaId)
            .ThenBy(r => r.Nombre)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Rol>> GetActivosByUsuarioAsync(Guid usuarioId, Guid? empresaId, CancellationToken cancellationToken = default)
        => await _context.Roles
            .AsNoTracking()
            .Where(r => r.Activo
                        && (r.EmpresaId == null || r.EmpresaId == empresaId)
                        && _context.UsuarioRoles.Any(ur => ur.UsuarioId == usuarioId && ur.RolId == r.RolId))
            .OrderBy(r => r.Nombre)
            .ToListAsync(cancellationToken);

    public Task<bool> ExistsAsync(Guid rolId, CancellationToken cancellationToken = default)
        => _context.Roles.AnyAsync(r => r.RolId == rolId, cancellationToken);

    public Task<Rol?> GetByNombreAsync(string nombre, Guid? empresaId, CancellationToken cancellationToken = default)
        => _context.Roles.FirstOrDefaultAsync(r => r.Nombre == nombre && r.EmpresaId == empresaId, cancellationToken);

    public Task<bool> NombreExistsAsync(string nombre, Guid? empresaId, Guid? excludeRolId = null, CancellationToken cancellationToken = default)
        => _context.Roles.AnyAsync(
            r => r.Nombre == nombre
                 && r.EmpresaId == empresaId
                 && (excludeRolId == null || r.RolId != excludeRolId),
            cancellationToken);

    public async Task AddAsync(Rol rol, CancellationToken cancellationToken = default)
        => await _context.Roles.AddAsync(rol, cancellationToken);

    public void Remove(Rol rol)
        => _context.Roles.Remove(rol);
}
