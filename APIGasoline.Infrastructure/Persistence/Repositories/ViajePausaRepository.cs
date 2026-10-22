using APIGasoline.Domain.Entities;
using APIGasoline.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace APIGasoline.Infrastructure.Persistence.Repositories;

public class ViajePausaRepository : IViajePausaRepository
{
    private readonly AppDbContext _context;

    public ViajePausaRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<ViajePausa?> GetUltimaByViajeAsync(Guid viajeId, CancellationToken cancellationToken = default)
        => _context.ViajePausas
            .Where(p => p.ViajeId == viajeId)
            .OrderByDescending(p => p.FechaInicio)
            .ThenByDescending(p => p.ViajePausaId)
            .FirstOrDefaultAsync(cancellationToken);

    public Task<ViajePausa?> GetAbiertaByViajeAsync(Guid viajeId, CancellationToken cancellationToken = default)
        => _context.ViajePausas
            .Where(p => p.ViajeId == viajeId && p.FechaFin == null)
            .OrderByDescending(p => p.FechaInicio)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<IReadOnlyList<ViajePausa>> GetByViajesAsync(IReadOnlyCollection<Guid> viajeIds, CancellationToken cancellationToken = default)
    {
        if (viajeIds.Count == 0)
            return [];

        var ids = viajeIds.ToList();

        return await _context.ViajePausas
            .AsNoTracking()
            .Where(p => ids.Contains(p.ViajeId))
            .OrderBy(p => p.FechaInicio)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(ViajePausa pausa, CancellationToken cancellationToken = default)
        => await _context.ViajePausas.AddAsync(pausa, cancellationToken);

    public async Task AddRangeAsync(IEnumerable<ViajePausa> pausas, CancellationToken cancellationToken = default)
        => await _context.ViajePausas.AddRangeAsync(pausas, cancellationToken);
}
