using APIGasoline.Domain.Common;
using APIGasoline.Domain.Entities;

namespace APIGasoline.Domain.Interfaces;

public interface IPuntoRutaRepository
{
    Task<PuntoRuta?> GetByIdAsync(Guid puntoRutaId, CancellationToken cancellationToken = default);

    /// <summary>Puntos del viaje en orden de captura (para dibujar la ruta).</summary>
    Task<IReadOnlyList<PuntoRuta>> GetByViajeAsync(Guid viajeId, CancellationToken cancellationToken = default);

    /// <summary>Puntos de un viaje por páginas, en orden de captura (F011).</summary>
    Task<PaginaDe<PuntoRuta>> GetPaginaByViajeAsync(Guid viajeId, Paginacion pagina, CancellationToken cancellationToken = default);

    /// <summary>Timestamps ya registrados del viaje (para no duplicar al reintentar un lote).</summary>
    Task<HashSet<DateTime>> GetTimestampsByViajeAsync(Guid viajeId, CancellationToken cancellationToken = default);

    Task AddRangeAsync(IEnumerable<PuntoRuta> puntos, CancellationToken cancellationToken = default);
    void Remove(PuntoRuta puntoRuta);
}
