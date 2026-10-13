using APIGasoline.Domain.Entities;
using APIGasoline.Domain.Enums;
using APIGasoline.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace APIGasoline.Infrastructure.Persistence.Repositories;

public class AuditoriaEventoRepository : IAuditoriaEventoRepository
{
    private readonly AppDbContext _context;

    public AuditoriaEventoRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<AuditoriaEvento?> GetByIdAsync(Guid auditoriaEventoId, CancellationToken cancellationToken = default)
        => _context.AuditoriaEventos
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.AuditoriaEventoId == auditoriaEventoId, cancellationToken);

    public async Task<IReadOnlyList<AuditoriaEvento>> GetAllAsync(
        Guid? empresaId = null,
        string? entidad = null,
        Guid? entidadId = null,
        Guid? usuarioId = null,
        AccionAuditoria? accion = null,
        DateTime? desde = null,
        DateTime? hasta = null,
        int maximo = 500,
        CancellationToken cancellationToken = default)
    {
        var query = _context.AuditoriaEventos.AsNoTracking();

        if (empresaId is not null)
            query = query.Where(a => a.EmpresaId == empresaId);

        if (!string.IsNullOrWhiteSpace(entidad))
        {
            var entidadNormalizada = entidad.Trim();
            query = query.Where(a => a.Entidad == entidadNormalizada);
        }

        if (entidadId is not null)
            query = query.Where(a => a.EntidadId == entidadId);

        if (usuarioId is not null)
            query = query.Where(a => a.UsuarioId == usuarioId);

        if (accion is not null)
            query = query.Where(a => a.Accion == accion);

        if (desde is not null)
            query = query.Where(a => a.FechaHora >= desde);

        if (hasta is not null)
            query = query.Where(a => a.FechaHora <= hasta);

        return await query
            .OrderByDescending(a => a.FechaHora)
            .ThenByDescending(a => a.AuditoriaEventoId)
            .Take(maximo)
            .ToListAsync(cancellationToken);
    }

    public Task<bool> ExistsByUsuarioAsync(Guid usuarioId, CancellationToken cancellationToken = default)
        => _context.AuditoriaEventos.AnyAsync(a => a.UsuarioId == usuarioId, cancellationToken);

    public async Task AddAsync(AuditoriaEvento evento, CancellationToken cancellationToken = default)
        => await _context.AuditoriaEventos.AddAsync(evento, cancellationToken);
}
