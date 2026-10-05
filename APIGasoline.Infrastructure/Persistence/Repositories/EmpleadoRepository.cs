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

    public Task<Empleado?> GetByIdAsync(int empleadoId, CancellationToken cancellationToken = default)
        => _context.Empleados.FirstOrDefaultAsync(e => e.EmpleadoId == empleadoId, cancellationToken);

    public Task<Empleado?> GetByUsuarioIdAsync(int usuarioId, CancellationToken cancellationToken = default)
        => _context.Empleados.FirstOrDefaultAsync(e => e.UsuarioId == usuarioId, cancellationToken);

    public async Task<IReadOnlyList<Empleado>> GetAllAsync(int? empresaId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Empleados.AsNoTracking();

        if (empresaId is not null)
            query = query.Where(e => e.EmpresaId == empresaId);

        return await query
            .OrderBy(e => e.EmpleadoId)
            .ToListAsync(cancellationToken);
    }

    public Task<bool> ExistsAsync(int empleadoId, CancellationToken cancellationToken = default)
        => _context.Empleados.AnyAsync(e => e.EmpleadoId == empleadoId, cancellationToken);

    public Task<bool> UsuarioAsignadoAsync(int usuarioId, int? excludeEmpleadoId = null, CancellationToken cancellationToken = default)
        => _context.Empleados
            .IgnoreQueryFilters()
            .AnyAsync(
                e => e.UsuarioId == usuarioId
                     && (excludeEmpleadoId == null || e.EmpleadoId != excludeEmpleadoId),
                cancellationToken);

    public async Task<IReadOnlyList<int>> GetIdsConRolAsync(int empresaId, string nombreRol, CancellationToken cancellationToken = default)
        => await _context.Empleados
            .AsNoTracking()
            .Where(e => e.EmpresaId == empresaId
                        && _context.UsuarioRoles.Any(ur => ur.UsuarioId == e.UsuarioId
                            && _context.Roles.Any(r => r.RolId == ur.RolId && r.Activo && r.Nombre == nombreRol
                                && (r.EmpresaId == null || r.EmpresaId == empresaId))))
            .Select(e => e.EmpleadoId)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(Empleado empleado, CancellationToken cancellationToken = default)
        => await _context.Empleados.AddAsync(empleado, cancellationToken);
}
