using APIGasoline.Domain.Entities;
using APIGasoline.Domain.Enums;
using APIGasoline.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace APIGasoline.Infrastructure.Persistence.Repositories;

public class RevisionAuditoriaRepository : IRevisionAuditoriaRepository
{
    private readonly AppDbContext _context;

    public RevisionAuditoriaRepository(AppDbContext context)
    {
        _context = context;
    }

    // Con tracking: se usa para registrar el resultado y cerrar la revisión
    public Task<RevisionAuditoria?> GetByIdAsync(Guid revisionAuditoriaId, CancellationToken cancellationToken = default)
        => _context.RevisionesAuditoria
            .FirstOrDefaultAsync(r => r.RevisionAuditoriaId == revisionAuditoriaId, cancellationToken);

    public async Task<IReadOnlyList<RevisionAuditoria>> GetAllAsync(
        Guid? empresaId = null,
        Guid? viajeId = null,
        Guid? carroId = null,
        EstadoRevision? estado = null,
        bool? abiertas = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.RevisionesAuditoria.AsNoTracking();

        if (empresaId is not null)
            query = query.Where(r => r.EmpresaId == empresaId);

        if (viajeId is not null)
            query = query.Where(r => r.ViajeId == viajeId);

        if (carroId is not null)
            query = query.Where(r => r.CarroId == carroId);

        if (estado is not null)
            query = query.Where(r => r.EstadoRevision == estado);

        if (abiertas is true)
            query = query.Where(r => r.FechaCierre == null);
        else if (abiertas is false)
            query = query.Where(r => r.FechaCierre != null);

        return await query
            .OrderByDescending(r => r.FechaRevision)
            .ThenByDescending(r => r.RevisionAuditoriaId)
            .ToListAsync(cancellationToken);
    }

    public Task<bool> ExistsByViajeAsync(Guid viajeId, CancellationToken cancellationToken = default)
        => _context.RevisionesAuditoria.AnyAsync(r => r.ViajeId == viajeId, cancellationToken);

    public Task<bool> ExistsByCarroAsync(Guid carroId, CancellationToken cancellationToken = default)
        => _context.RevisionesAuditoria.AnyAsync(r => r.CarroId == carroId, cancellationToken);

    public async Task AddAsync(RevisionAuditoria revision, CancellationToken cancellationToken = default)
        => await _context.RevisionesAuditoria.AddAsync(revision, cancellationToken);
}
