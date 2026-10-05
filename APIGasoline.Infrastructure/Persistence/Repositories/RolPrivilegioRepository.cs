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

    public Task<RolPrivilegio?> GetByIdAsync(int rolPrivilegioId, CancellationToken cancellationToken = default)
        => _context.RolPrivilegios.FirstOrDefaultAsync(rp => rp.RolPrivilegioId == rolPrivilegioId, cancellationToken);

    public async Task<IReadOnlyList<RolPrivilegio>> GetAllAsync(int? rolId = null, int? privilegioId = null, CancellationToken cancellationToken = default)
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

    public Task<bool> AsignacionExistsAsync(int rolId, int privilegioId, int? excludeRolPrivilegioId = null, CancellationToken cancellationToken = default)
        => _context.RolPrivilegios.AnyAsync(
            rp => rp.RolId == rolId
                  && rp.PrivilegioId == privilegioId
                  && (excludeRolPrivilegioId == null || rp.RolPrivilegioId != excludeRolPrivilegioId),
            cancellationToken);

    public async Task AddAsync(RolPrivilegio rolPrivilegio, CancellationToken cancellationToken = default)
        => await _context.RolPrivilegios.AddAsync(rolPrivilegio, cancellationToken);

    public void Remove(RolPrivilegio rolPrivilegio)
        => _context.RolPrivilegios.Remove(rolPrivilegio);
}
