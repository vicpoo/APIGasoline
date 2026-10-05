using APIGasoline.Domain.Entities;
using APIGasoline.Domain.Enums;

namespace APIGasoline.Domain.Interfaces;

public interface ISolicitudReservaRepository
{
    Task<SolicitudReserva?> GetByIdAsync(int solicitudReservaId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Ordenadas de la más antigua a la más reciente (cola de confirmación).
    /// </summary>
    Task<IReadOnlyList<SolicitudReserva>> GetAllAsync(
        int? empresaId = null,
        int? carroId = null,
        int? empleadoSolicitanteId = null,
        EstadoSolicitudReserva? estado = null,
        CancellationToken cancellationToken = default);

    /// <summary>Pendientes del mismo carro (se rechazan al aprobar otra).</summary>
    Task<IReadOnlyList<SolicitudReserva>> GetPendientesByCarroAsync(int carroId, int excludeSolicitudReservaId, CancellationToken cancellationToken = default);

    Task<bool> ExistePendienteAsync(int empleadoSolicitanteId, int carroId, CancellationToken cancellationToken = default);
    Task<bool> ExistsByCarroAsync(int carroId, CancellationToken cancellationToken = default);
    /// <summary>La reserva aprobada vigente del carro (quien lo tiene asignado), si existe.</summary>
    Task<SolicitudReserva?> GetAprobadaByCarroAsync(int carroId, CancellationToken cancellationToken = default);

    Task AddAsync(SolicitudReserva solicitudReserva, CancellationToken cancellationToken = default);
    void Remove(SolicitudReserva solicitudReserva);
}
