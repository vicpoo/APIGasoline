using APIGasoline.Domain.Entities;
using APIGasoline.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace APIGasoline.Infrastructure.Persistence.Repositories;

public class PuntoRutaRepository : IPuntoRutaRepository
{
    private readonly AppDbContext _context;

    public PuntoRutaRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<PuntoRuta?> GetByIdAsync(Guid puntoRutaId, CancellationToken cancellationToken = default)
        => _context.PuntosRuta.FirstOrDefaultAsync(p => p.PuntoRutaId == puntoRutaId, cancellationToken);

    public async Task<IReadOnlyList<PuntoRuta>> GetByViajeAsync(Guid viajeId, CancellationToken cancellationToken = default)
        => await _context.PuntosRuta
            .AsNoTracking()
            .Where(p => p.ViajeId == viajeId)
            .OrderBy(p => p.TimestampCaptura)
            .ThenBy(p => p.PuntoRutaId)
            .ToListAsync(cancellationToken);

    public async Task<HashSet<DateTime>> GetTimestampsByViajeAsync(Guid viajeId, CancellationToken cancellationToken = default)
    {
        var timestamps = await _context.PuntosRuta
            .AsNoTracking()
            .Where(p => p.ViajeId == viajeId)
            .Select(p => p.TimestampCaptura)
            .ToListAsync(cancellationToken);

        return timestamps.ToHashSet();
    }

    public Task AddRangeAsync(IEnumerable<PuntoRuta> puntos, CancellationToken cancellationToken = default)
        => _context.PuntosRuta.AddRangeAsync(puntos, cancellationToken);

    public void Remove(PuntoRuta puntoRuta)
        => _context.PuntosRuta.Remove(puntoRuta);
}
