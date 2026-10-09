using APIGasoline.Domain.Entities;
using APIGasoline.Domain.Enums;

namespace APIGasoline.Domain.Interfaces;

public interface IOutboxMensajeRepository
{
    Task<OutboxMensaje?> GetByIdAsync(Guid outboxMensajeId, CancellationToken cancellationToken = default);

    /// <summary>Del más reciente al más antiguo.</summary>
    Task<IReadOnlyList<OutboxMensaje>> GetAllAsync(
        Guid? empresaId = null,
        EstadoOutbox? estado = null,
        int limite = 100,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Mensajes listos para enviarse, bloqueados para este worker (FOR UPDATE SKIP LOCKED).
    /// Debe llamarse dentro de una transacción.
    /// </summary>
    Task<IReadOnlyList<OutboxMensaje>> TomarPendientesAsync(int cantidad, DateTime ahora, CancellationToken cancellationToken = default);

    Task AddAsync(OutboxMensaje mensaje, CancellationToken cancellationToken = default);
}
