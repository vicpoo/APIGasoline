using APIGasoline.Domain.Entities;
using APIGasoline.Domain.Enums;
using APIGasoline.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace APIGasoline.Infrastructure.Persistence.Repositories;

public class NotificacionRepository : INotificacionRepository
{
    private readonly AppDbContext _context;

    public NotificacionRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<Notificacion?> GetByIdAsync(int notificacionId, CancellationToken cancellationToken = default)
        => _context.Notificaciones.FirstOrDefaultAsync(n => n.NotificacionId == notificacionId, cancellationToken);

    public async Task<IReadOnlyList<Notificacion>> GetAllAsync(
        int? empresaId = null,
        int? empleadoDestinatarioId = null,
        bool soloNoLeidas = false,
        TipoNotificacion? tipo = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Notificaciones.AsNoTracking();

        if (empresaId is not null)
            query = query.Where(n => n.EmpresaId == empresaId);

        if (empleadoDestinatarioId is not null)
            query = query.Where(n => n.EmpleadoDestinatarioId == empleadoDestinatarioId);

        if (soloNoLeidas)
            query = query.Where(n => !n.Leida);

        if (tipo is not null)
            query = query.Where(n => n.Tipo == tipo);

        return await query
            .OrderByDescending(n => n.FechaEnvio)
            .ThenByDescending(n => n.NotificacionId)
            .ToListAsync(cancellationToken);
    }

    public Task<int> ContarNoLeidasAsync(int empleadoDestinatarioId, CancellationToken cancellationToken = default)
        => _context.Notificaciones.CountAsync(n => n.EmpleadoDestinatarioId == empleadoDestinatarioId && !n.Leida, cancellationToken);

    public async Task<IReadOnlyList<Notificacion>> GetNoLeidasAsync(int empleadoDestinatarioId, CancellationToken cancellationToken = default)
        => await _context.Notificaciones
            .Where(n => n.EmpleadoDestinatarioId == empleadoDestinatarioId && !n.Leida)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(Notificacion notificacion, CancellationToken cancellationToken = default)
        => await _context.Notificaciones.AddAsync(notificacion, cancellationToken);

    public Task AddRangeAsync(IEnumerable<Notificacion> notificaciones, CancellationToken cancellationToken = default)
        => _context.Notificaciones.AddRangeAsync(notificaciones, cancellationToken);

    public void Remove(Notificacion notificacion)
        => _context.Notificaciones.Remove(notificacion);
}
