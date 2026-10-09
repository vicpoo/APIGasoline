using APIGasoline.Domain.Entities;
using APIGasoline.Domain.Enums;

namespace APIGasoline.Domain.Interfaces;

public interface INotificacionRepository
{
    Task<Notificacion?> GetByIdAsync(Guid notificacionId, CancellationToken cancellationToken = default);

    /// <summary>Bandeja: de la más reciente a la más antigua.</summary>
    Task<IReadOnlyList<Notificacion>> GetAllAsync(
        Guid? empresaId = null,
        Guid? empleadoDestinatarioId = null,
        bool soloNoLeidas = false,
        TipoNotificacion? tipo = null,
        CancellationToken cancellationToken = default);

    Task<int> ContarNoLeidasAsync(Guid empleadoDestinatarioId, CancellationToken cancellationToken = default);

    /// <summary>Devuelve las no leídas del empleado (rastreadas) para marcarlas.</summary>
    Task<IReadOnlyList<Notificacion>> GetNoLeidasAsync(Guid empleadoDestinatarioId, CancellationToken cancellationToken = default);

    Task AddAsync(Notificacion notificacion, CancellationToken cancellationToken = default);
    Task AddRangeAsync(IEnumerable<Notificacion> notificaciones, CancellationToken cancellationToken = default);
    void Remove(Notificacion notificacion);
}
