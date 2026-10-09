using APIGasoline.Domain.Entities;
using APIGasoline.Domain.Enums;

namespace APIGasoline.Domain.Interfaces;

public interface IViajeRepository
{
    Task<Viaje?> GetByIdAsync(Guid viajeId, CancellationToken cancellationToken = default);

    /// <summary>Ordenados del más reciente al más antiguo.</summary>
    Task<IReadOnlyList<Viaje>> GetAllAsync(
        Guid? empresaId = null,
        Guid? carroId = null,
        Guid? empleadoOperadorId = null,
        Guid? solicitudReservaId = null,
        EstadoViaje? estado = null,
        CancellationToken cancellationToken = default);

    Task<bool> ExisteEnCursoPorCarroAsync(Guid carroId, CancellationToken cancellationToken = default);
    Task<bool> ExisteEnCursoPorSolicitudAsync(Guid solicitudReservaId, CancellationToken cancellationToken = default);
    Task<bool> ExistsByCarroAsync(Guid carroId, CancellationToken cancellationToken = default);
    Task<bool> ExistsBySolicitudAsync(Guid solicitudReservaId, CancellationToken cancellationToken = default);
    Task AddAsync(Viaje viaje, CancellationToken cancellationToken = default);
    void Remove(Viaje viaje);
}
