using APIGasoline.Domain.Entities;
using APIGasoline.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace APIGasoline.Infrastructure.Persistence.Repositories;

public class CambioEstatusCarroRepository : ICambioEstatusCarroRepository
{
    private readonly AppDbContext _context;

    public CambioEstatusCarroRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<CambioEstatusCarro?> GetByIdAsync(Guid cambioEstatusCarroId, CancellationToken cancellationToken = default)
        => _context.CambiosEstatusCarro
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.CambioEstatusCarroId == cambioEstatusCarroId, cancellationToken);

    public Task<CambioEstatusCarro?> GetByIdParaActualizarAsync(Guid cambioEstatusCarroId, CancellationToken cancellationToken = default)
        => _context.CambiosEstatusCarro
            .FirstOrDefaultAsync(c => c.CambioEstatusCarroId == cambioEstatusCarroId, cancellationToken);

    public async Task<IReadOnlyList<CambioEstatusCarro>> GetAllAsync(
        Guid? empresaId = null,
        Guid? carroId = null,
        DateTime? desde = null,
        DateTime? hasta = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.CambiosEstatusCarro.AsNoTracking();

        if (empresaId is not null)
            query = query.Where(c => c.EmpresaId == empresaId);

        if (carroId is not null)
            query = query.Where(c => c.CarroId == carroId);

        if (desde is not null)
            query = query.Where(c => c.FechaCambio >= desde);

        if (hasta is not null)
            query = query.Where(c => c.FechaCambio <= hasta);

        return await query
            .OrderByDescending(c => c.FechaCambio)
            .ThenByDescending(c => c.CambioEstatusCarroId)
            .ToListAsync(cancellationToken);
    }

    public Task<bool> ExistsByCarroAsync(Guid carroId, CancellationToken cancellationToken = default)
        => _context.CambiosEstatusCarro.AnyAsync(c => c.CarroId == carroId, cancellationToken);

    public Task<bool> ExistsByViajeAsync(Guid viajeId, CancellationToken cancellationToken = default)
        => _context.CambiosEstatusCarro.AnyAsync(c => c.ViajeAfectadoId == viajeId, cancellationToken);

    public async Task AddAsync(CambioEstatusCarro cambio, CancellationToken cancellationToken = default)
        => await _context.CambiosEstatusCarro.AddAsync(cambio, cancellationToken);
}
