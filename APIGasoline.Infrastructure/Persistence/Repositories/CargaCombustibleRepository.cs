using APIGasoline.Domain.Entities;
using APIGasoline.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace APIGasoline.Infrastructure.Persistence.Repositories;

public class CargaCombustibleRepository : ICargaCombustibleRepository
{
    private readonly AppDbContext _context;

    public CargaCombustibleRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<CargaCombustible?> GetByIdAsync(int cargaCombustibleId, CancellationToken cancellationToken = default)
        => _context.CargasCombustible.FirstOrDefaultAsync(c => c.CargaCombustibleId == cargaCombustibleId, cancellationToken);

    public async Task<IReadOnlyList<CargaCombustible>> GetAllAsync(
        int? empresaId = null,
        int? viajeId = null,
        DateTime? desde = null,
        DateTime? hasta = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.CargasCombustible.AsNoTracking();

        if (empresaId is not null)
            query = query.Where(c => c.EmpresaId == empresaId);

        if (viajeId is not null)
            query = query.Where(c => c.ViajeId == viajeId);

        if (desde is not null)
            query = query.Where(c => c.FechaCarga >= desde);

        if (hasta is not null)
            query = query.Where(c => c.FechaCarga <= hasta);

        return await query
            .OrderByDescending(c => c.FechaCarga)
            .ThenByDescending(c => c.CargaCombustibleId)
            .ToListAsync(cancellationToken);
    }

    public Task<bool> ExisteEnFechaAsync(int viajeId, DateTime fechaCarga, int? excludeCargaCombustibleId = null, CancellationToken cancellationToken = default)
        => _context.CargasCombustible.AnyAsync(
            c => c.ViajeId == viajeId
                 && c.FechaCarga == fechaCarga
                 && (excludeCargaCombustibleId == null || c.CargaCombustibleId != excludeCargaCombustibleId),
            cancellationToken);

    public Task<bool> ExistsByViajeAsync(int viajeId, CancellationToken cancellationToken = default)
        => _context.CargasCombustible.AnyAsync(c => c.ViajeId == viajeId, cancellationToken);

    public async Task AddAsync(CargaCombustible cargaCombustible, CancellationToken cancellationToken = default)
        => await _context.CargasCombustible.AddAsync(cargaCombustible, cancellationToken);

    public void Remove(CargaCombustible cargaCombustible)
        => _context.CargasCombustible.Remove(cargaCombustible);
}
