using APIGasoline.Domain.Entities;
using APIGasoline.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace APIGasoline.Infrastructure.Persistence.Repositories;

public class DivisionRepository : IDivisionRepository
{
    private readonly AppDbContext _context;

    public DivisionRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<Division?> GetByIdAsync(Guid divisionId, CancellationToken cancellationToken = default)
        => _context.Divisiones.FirstOrDefaultAsync(d => d.DivisionId == divisionId, cancellationToken);

    public async Task<IReadOnlyList<Division>> GetAllAsync(Guid? corporativoId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Divisiones.AsNoTracking();

        if (corporativoId is not null)
            query = query.Where(d => d.CorporativoId == corporativoId);

        return await query
            .OrderBy(d => d.DivisionId)
            .ToListAsync(cancellationToken);
    }

    public Task<bool> ExistsAsync(Guid divisionId, CancellationToken cancellationToken = default)
        => _context.Divisiones.AnyAsync(d => d.DivisionId == divisionId, cancellationToken);

    public Task<bool> ExistsByCorporativoAsync(Guid corporativoId, CancellationToken cancellationToken = default)
        => _context.Divisiones.AnyAsync(d => d.CorporativoId == corporativoId, cancellationToken);

    public async Task AddAsync(Division division, CancellationToken cancellationToken = default)
        => await _context.Divisiones.AddAsync(division, cancellationToken);

    public void Remove(Division division)
        => _context.Divisiones.Remove(division);
}
