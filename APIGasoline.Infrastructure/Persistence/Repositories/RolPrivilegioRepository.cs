using APIGasoline.Domain.Entities;
using APIGasoline.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace APIGasoline.Infrastructure.Persistence.Repositories;

public class RolPrivilegioRepository : IRolPrivilegioRepository
{
    private readonly AppDbContext _context;

    public RolPrivilegioRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<RolPrivilegio?> GetByIdAsync(Guid rolPrivilegioId, CancellationToken cancellationToken = default)
        => _context.RolPrivilegios.FirstOrDefaultAsync(rp => rp.RolPrivilegioId == rolPrivilegioId, cancellationToken);

    public async Task<IReadOnlyList<RolPrivilegio>> GetAllAsync(Guid? rolId = null, Guid? privilegioId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.RolPrivilegios.AsNoTracking();

        if (rolId is not null)
            query = query.Where(rp => rp.RolId == rolId);

        if (privilegioId is not null)
            query = query.Where(rp => rp.PrivilegioId == privilegioId);

        return await query
            .OrderBy(rp => rp.RolPrivilegioId)
            .ToListAsync(cancellationToken);
    }

    public Task<bool> AsignacionExistsAsync(Guid rolId, Guid privilegioId, Guid? excludeRolPrivilegioId = null, CancellationToken cancellationToken = default)
        => _context.RolPrivilegios.AnyAsync(
            rp => rp.RolId == rolId
                  && rp.PrivilegioId == privilegioId
                  && (excludeRolPrivilegioId == null || rp.RolPrivilegioId != excludeRolPrivilegioId),
            cancellationToken);

    public Task<bool> AsignadoARolDeEmpresaAsync(Guid privilegioId, CancellationToken cancellationToken = default)
        => _context.RolPrivilegios.AnyAsync(
            rp => rp.PrivilegioId == privilegioId
                  && _context.Roles.Any(r => r.RolId == rp.RolId && r.EmpresaId != null),
            cancellationToken);

    public async Task AddAsync(RolPrivilegio rolPrivilegio, CancellationToken cancellationToken = default)
        => await _context.RolPrivilegios.AddAsync(rolPrivilegio, cancellationToken);

    public void Remove(RolPrivilegio rolPrivilegio)
        => _context.RolPrivilegios.Remove(rolPrivilegio);
}
