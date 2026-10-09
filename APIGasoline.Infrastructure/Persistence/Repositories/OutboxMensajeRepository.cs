using APIGasoline.Domain.Entities;
using APIGasoline.Domain.Enums;
using APIGasoline.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace APIGasoline.Infrastructure.Persistence.Repositories;

public class OutboxMensajeRepository : IOutboxMensajeRepository
{
    private readonly AppDbContext _context;

    public OutboxMensajeRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<OutboxMensaje?> GetByIdAsync(Guid outboxMensajeId, CancellationToken cancellationToken = default)
        => _context.OutboxMensajes.FirstOrDefaultAsync(o => o.OutboxMensajeId == outboxMensajeId, cancellationToken);

    public async Task<IReadOnlyList<OutboxMensaje>> GetAllAsync(
        Guid? empresaId = null,
        EstadoOutbox? estado = null,
        int limite = 100,
        CancellationToken cancellationToken = default)
    {
        var query = _context.OutboxMensajes.AsNoTracking();

        if (empresaId is not null)
            query = query.Where(o => o.EmpresaId == empresaId);

        if (estado is not null)
            query = query.Where(o => o.Estado == estado);

        return await query
            .OrderByDescending(o => o.FechaCreacion)
            .ThenByDescending(o => o.OutboxMensajeId)
            .Take(limite)
            .ToListAsync(cancellationToken);
    }

    // SKIP LOCKED: si otra instancia de la API ya apartó un mensaje, este worker lo salta en lugar de esperar
    public async Task<IReadOnlyList<OutboxMensaje>> TomarPendientesAsync(int cantidad, DateTime ahora, CancellationToken cancellationToken = default)
        => await _context.OutboxMensajes
            .FromSql($"""
                SELECT * FROM outbox_mensaje
                WHERE estado IN ('pendiente', 'procesando') AND proximo_intento <= {ahora}
                ORDER BY proximo_intento
                LIMIT {cantidad}
                FOR UPDATE SKIP LOCKED
                """)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(OutboxMensaje mensaje, CancellationToken cancellationToken = default)
        => await _context.OutboxMensajes.AddAsync(mensaje, cancellationToken);
}
