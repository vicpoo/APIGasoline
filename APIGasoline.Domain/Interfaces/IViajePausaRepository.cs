using APIGasoline.Domain.Entities;

namespace APIGasoline.Domain.Interfaces;

public interface IViajePausaRepository
{
    /// <summary>La pausa más reciente del viaje (abierta o cerrada), con seguimiento de cambios.</summary>
    Task<ViajePausa?> GetUltimaByViajeAsync(Guid viajeId, CancellationToken cancellationToken = default);

    /// <summary>La pausa abierta del viaje (si está en pausa), con seguimiento de cambios.</summary>
    Task<ViajePausa?> GetAbiertaByViajeAsync(Guid viajeId, CancellationToken cancellationToken = default);

    /// <summary>Pausas de varios viajes, ordenadas por fecha de inicio (solo lectura).</summary>
    Task<IReadOnlyList<ViajePausa>> GetByViajesAsync(IReadOnlyCollection<Guid> viajeIds, CancellationToken cancellationToken = default);

    Task AddAsync(ViajePausa pausa, CancellationToken cancellationToken = default);
    Task AddRangeAsync(IEnumerable<ViajePausa> pausas, CancellationToken cancellationToken = default);
}
