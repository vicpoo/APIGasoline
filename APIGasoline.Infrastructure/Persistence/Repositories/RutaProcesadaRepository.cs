using APIGasoline.Domain.Entities;
using APIGasoline.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace APIGasoline.Infrastructure.Persistence.Repositories;

public class RutaProcesadaRepository : IRutaProcesadaRepository
{
    private readonly AppDbContext _context;

    public RutaProcesadaRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<RutaProcesada?> GetByViajeAsync(Guid viajeId, CancellationToken cancellationToken = default)
        => _context.RutasProcesadas.FirstOrDefaultAsync(r => r.ViajeId == viajeId, cancellationToken);

    public async Task<IReadOnlyList<RutaProcesada>> GetConAlertaAsync(Guid? empresaId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.RutasProcesadas
            .AsNoTracking()
            .Where(r => r.AlertaKilometraje);

        if (empresaId.HasValue)
            query = query.Where(r => r.EmpresaId == empresaId.Value);

        return await query
            .OrderByDescending(r => r.FechaProceso)
            .ThenByDescending(r => r.RutaProcesadaId)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(RutaProcesada rutaProcesada, CancellationToken cancellationToken = default)
        => await _context.RutasProcesadas.AddAsync(rutaProcesada, cancellationToken);

    public void Remove(RutaProcesada rutaProcesada)
        => _context.RutasProcesadas.Remove(rutaProcesada);
}
