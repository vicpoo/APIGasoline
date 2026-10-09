using APIGasoline.Domain.Entities;
using APIGasoline.Domain.Enums;

namespace APIGasoline.Domain.Interfaces;

public interface ISolicitudReservaRepository
{
    Task<SolicitudReserva?> GetByIdAsync(Guid solicitudReservaId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Ordenadas de la más antigua a la más reciente (cola de confirmación).
    /// </summary>
    Task<IReadOnlyList<SolicitudReserva>> GetAllAsync(
        Guid? empresaId = null,
        Guid? carroId = null,
        Guid? empleadoSolicitanteId = null,
        EstadoSolicitudReserva? estado = null,
        CancellationToken cancellationToken = default);

    /// <summary>Pendientes del mismo carro (se rechazan al aprobar otra).</summary>
    Task<IReadOnlyList<SolicitudReserva>> GetPendientesByCarroAsync(Guid carroId, Guid excludeSolicitudReservaId, CancellationToken cancellationToken = default);

    Task<bool> ExistePendienteAsync(Guid empleadoSolicitanteId, Guid carroId, CancellationToken cancellationToken = default);
    Task<bool> ExistsByCarroAsync(Guid carroId, CancellationToken cancellationToken = default);
    /// <summary>La reserva aprobada vigente del carro (quien lo tiene asignado), si existe.</summary>
    Task<SolicitudReserva?> GetAprobadaByCarroAsync(Guid carroId, CancellationToken cancellationToken = default);

    Task AddAsync(SolicitudReserva solicitudReserva, CancellationToken cancellationToken = default);
    void Remove(SolicitudReserva solicitudReserva);
}
