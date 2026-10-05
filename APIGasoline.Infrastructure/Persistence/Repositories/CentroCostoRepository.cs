using APIGasoline.Domain.Entities;
using APIGasoline.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace APIGasoline.Infrastructure.Persistence.Repositories;

public class CentroCostoRepository : ICentroCostoRepository
{
    private readonly AppDbContext _context;

    public CentroCostoRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<CentroCosto?> GetByIdAsync(int centroCostoId, CancellationToken cancellationToken = default)
        => _context.CentrosCosto.FirstOrDefaultAsync(cc => cc.CentroCostoId == centroCostoId, cancellationToken);

    public async Task<IReadOnlyList<CentroCosto>> GetAllAsync(int? empresaId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.CentrosCosto.AsNoTracking();

        if (empresaId is not null)
            query = query.Where(cc => cc.EmpresaId == empresaId);

        return await query
            .OrderBy(cc => cc.CentroCostoId)
            .ToListAsync(cancellationToken);
    }

    public Task<bool> ExistsAsync(int centroCostoId, CancellationToken cancellationToken = default)
        => _context.CentrosCosto.AnyAsync(cc => cc.CentroCostoId == centroCostoId, cancellationToken);

    public Task<bool> ClaveExistsAsync(int empresaId, string clave, int? excludeCentroCostoId = null, CancellationToken cancellationToken = default)
        => _context.CentrosCosto.AnyAsync(
            cc => cc.EmpresaId == empresaId
                  && cc.Clave == clave
                  && (excludeCentroCostoId == null || cc.CentroCostoId != excludeCentroCostoId),
            cancellationToken);

    public async Task AddAsync(CentroCosto centroCosto, CancellationToken cancellationToken = default)
        => await _context.CentrosCosto.AddAsync(centroCosto, cancellationToken);

    public void Remove(CentroCosto centroCosto)
        => _context.CentrosCosto.Remove(centroCosto);
}
