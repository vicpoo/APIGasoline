using APIGasoline.Domain.Entities;
using APIGasoline.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace APIGasoline.Infrastructure.Persistence.Repositories;

public class CorporativoRepository : ICorporativoRepository
{
    private readonly AppDbContext _context;

    public CorporativoRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<Corporativo?> GetByIdAsync(Guid corporativoId, CancellationToken cancellationToken = default)
        => _context.Corporativos.FirstOrDefaultAsync(c => c.CorporativoId == corporativoId, cancellationToken);

    public async Task<IReadOnlyList<Corporativo>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _context.Corporativos
            .AsNoTracking()
            .OrderBy(c => c.CorporativoId)
            .ToListAsync(cancellationToken);

    public Task<bool> ExistsAsync(Guid corporativoId, CancellationToken cancellationToken = default)
        => _context.Corporativos.AnyAsync(c => c.CorporativoId == corporativoId, cancellationToken);

    public async Task AddAsync(Corporativo corporativo, CancellationToken cancellationToken = default)
        => await _context.Corporativos.AddAsync(corporativo, cancellationToken);

    public void Remove(Corporativo corporativo)
        => _context.Corporativos.Remove(corporativo);
}
