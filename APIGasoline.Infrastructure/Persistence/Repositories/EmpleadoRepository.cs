using APIGasoline.Domain.Entities;
using APIGasoline.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace APIGasoline.Infrastructure.Persistence.Repositories;

public class EmpleadoRepository : IEmpleadoRepository
{
    private readonly AppDbContext _context;

    public EmpleadoRepository(AppDbContext context)
    {
        _context = context;
    }

    // El filtro global (HasQueryFilter) ya excluye los empleados eliminados.

    public Task<Empleado?> GetByIdAsync(Guid empleadoId, CancellationToken cancellationToken = default)
        => _context.Empleados.FirstOrDefaultAsync(e => e.EmpleadoId == empleadoId, cancellationToken);

    public async Task<IReadOnlyList<Empleado>> GetByIdsAsync(IEnumerable<Guid> empleadoIds, CancellationToken cancellationToken = default)
    {
        var ids = empleadoIds.Distinct().ToList();

        if (ids.Count == 0)
            return [];

        // IgnoreQueryFilters: el historial debe mostrar también a empleados ya eliminados
        return await _context.Empleados
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(e => ids.Contains(e.EmpleadoId))
            .ToListAsync(cancellationToken);
    }

    public Task<Empleado?> GetByUsuarioIdAsync(Guid usuarioId, CancellationToken cancellationToken = default)
        => _context.Empleados.FirstOrDefaultAsync(e => e.UsuarioId == usuarioId, cancellationToken);

    public async Task<IReadOnlyList<Empleado>> GetAllAsync(Guid? empresaId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Empleados.AsNoTracking();

        if (empresaId is not null)
            query = query.Where(e => e.EmpresaId == empresaId);

        return await query
            .OrderBy(e => e.EmpleadoId)
            .ToListAsync(cancellationToken);
    }

    public Task<bool> ExistsAsync(Guid empleadoId, CancellationToken cancellationToken = default)
        => _context.Empleados.AnyAsync(e => e.EmpleadoId == empleadoId, cancellationToken);

    public Task<bool> UsuarioAsignadoAsync(Guid usuarioId, Guid? excludeEmpleadoId = null, CancellationToken cancellationToken = default)
        => _context.Empleados
            .IgnoreQueryFilters()
            .AnyAsync(
                e => e.UsuarioId == usuarioId
                     && (excludeEmpleadoId == null || e.EmpleadoId != excludeEmpleadoId),
                cancellationToken);

    public async Task<IReadOnlyList<Guid>> GetIdsConPrivilegioAsync(Guid empresaId, string clavePrivilegio, CancellationToken cancellationToken = default)
        => await _context.Empleados
            .AsNoTracking()
            .Where(e => e.EmpresaId == empresaId
                        && _context.UsuarioRoles.Any(ur => ur.UsuarioId == e.UsuarioId
                            && _context.Roles.Any(r => r.RolId == ur.RolId && r.Activo
                                && (r.EmpresaId == null || r.EmpresaId == empresaId)
                                && _context.RolPrivilegios.Any(rp => rp.RolId == r.RolId
                                    && _context.Privilegios.Any(p => p.PrivilegioId == rp.PrivilegioId
                                        && p.Activo && p.Clave == clavePrivilegio)))))
            .Select(e => e.EmpleadoId)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(Empleado empleado, CancellationToken cancellationToken = default)
        => await _context.Empleados.AddAsync(empleado, cancellationToken);
}
