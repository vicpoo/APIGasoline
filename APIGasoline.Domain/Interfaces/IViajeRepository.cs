using APIGasoline.Domain.Entities;
using APIGasoline.Domain.Enums;

namespace APIGasoline.Domain.Interfaces;

public interface IViajeRepository
{
    Task<Viaje?> GetByIdAsync(int viajeId, CancellationToken cancellationToken = default);

    /// <summary>Ordenados del más reciente al más antiguo.</summary>
    Task<IReadOnlyList<Viaje>> GetAllAsync(
        int? empresaId = null,
        int? carroId = null,
        int? empleadoOperadorId = null,
        int? solicitudReservaId = null,
        EstadoViaje? estado = null,
        CancellationToken cancellationToken = default);

    Task<bool> ExisteEnCursoPorCarroAsync(int carroId, CancellationToken cancellationToken = default);
    Task<bool> ExisteEnCursoPorSolicitudAsync(int solicitudReservaId, CancellationToken cancellationToken = default);
    Task<bool> ExistsByCarroAsync(int carroId, CancellationToken cancellationToken = default);
    Task<bool> ExistsBySolicitudAsync(int solicitudReservaId, CancellationToken cancellationToken = default);
    Task AddAsync(Viaje viaje, CancellationToken cancellationToken = default);
    void Remove(Viaje viaje);
}
