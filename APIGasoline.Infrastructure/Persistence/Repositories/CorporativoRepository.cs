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

    public async Task<IReadOnlyList<Corporativo>> GetAllAsync(Guid? empresaId = null, Guid? divisionId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Corporativos.AsNoTracking();

        if (empresaId is not null)
            query = query.Where(c => c.EmpresaId == empresaId);

        if (divisionId is not null)
            query = query.Where(c => _context.CorporativoDivisiones.Any(l => l.CorporativoId == c.CorporativoId && l.DivisionId == divisionId));

        return await query
            .OrderBy(c => c.CorporativoId)
            .ToListAsync(cancellationToken);
    }

    public Task<bool> ExistsAsync(Guid corporativoId, CancellationToken cancellationToken = default)
        => _context.Corporativos.AnyAsync(c => c.CorporativoId == corporativoId, cancellationToken);

    public Task<bool> NombreExistsAsync(Guid empresaId, string nombre, Guid? excludeCorporativoId = null, CancellationToken cancellationToken = default)
        => _context.Corporativos.AnyAsync(
            c => c.EmpresaId == empresaId && c.Nombre == nombre && (excludeCorporativoId == null || c.CorporativoId != excludeCorporativoId),
            cancellationToken);

    public async Task AddAsync(Corporativo corporativo, CancellationToken cancellationToken = default)
        => await _context.Corporativos.AddAsync(corporativo, cancellationToken);

    public void Remove(Corporativo corporativo)
        => _context.Corporativos.Remove(corporativo);
}
