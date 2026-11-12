using APIGasoline.Domain.Entities;
using APIGasoline.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace APIGasoline.Infrastructure.Persistence.Repositories;

public class CorporativoDivisionRepository : ICorporativoDivisionRepository
{
    private readonly AppDbContext _context;

    public CorporativoDivisionRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<CorporativoDivision?> GetAsync(Guid corporativoId, Guid divisionId, CancellationToken cancellationToken = default)
        => _context.CorporativoDivisiones.FirstOrDefaultAsync(l => l.CorporativoId == corporativoId && l.DivisionId == divisionId, cancellationToken);

    public Task<bool> ExistsAsync(Guid corporativoId, Guid divisionId, CancellationToken cancellationToken = default)
        => _context.CorporativoDivisiones.AnyAsync(l => l.CorporativoId == corporativoId && l.DivisionId == divisionId, cancellationToken);

    public Task<bool> ExistsByCorporativoAsync(Guid corporativoId, CancellationToken cancellationToken = default)
        => _context.CorporativoDivisiones.AnyAsync(l => l.CorporativoId == corporativoId, cancellationToken);

    public Task<bool> ExistsByDivisionAsync(Guid divisionId, CancellationToken cancellationToken = default)
        => _context.CorporativoDivisiones.AnyAsync(l => l.DivisionId == divisionId, cancellationToken);

    public async Task AddAsync(CorporativoDivision liga, CancellationToken cancellationToken = default)
        => await _context.CorporativoDivisiones.AddAsync(liga, cancellationToken);

    public void Remove(CorporativoDivision liga)
        => _context.CorporativoDivisiones.Remove(liga);
}
