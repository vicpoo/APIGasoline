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

    // UPDLOCK + READPAST (equivalente a FOR UPDATE SKIP LOCKED de MySQL): aparta las filas hasta el
    // commit y, si otra instancia de la API ya apartó un mensaje, este worker lo salta en lugar de esperar.
    // ROWLOCK evita que SQL Server bloquee la página completa. Debe correr dentro de una transacción
    // (ProcesadorOutbox.TomarAsync la abre).
    public async Task<IReadOnlyList<OutboxMensaje>> TomarPendientesAsync(int cantidad, DateTime ahora, CancellationToken cancellationToken = default)
        => await _context.OutboxMensajes
            .FromSql($"""
                SELECT TOP ({cantidad}) * FROM outbox_mensaje WITH (UPDLOCK, READPAST, ROWLOCK)
                WHERE estado IN ('pendiente', 'procesando') AND proximo_intento <= {ahora}
                ORDER BY proximo_intento
                """)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(OutboxMensaje mensaje, CancellationToken cancellationToken = default)
        => await _context.OutboxMensajes.AddAsync(mensaje, cancellationToken);
}
