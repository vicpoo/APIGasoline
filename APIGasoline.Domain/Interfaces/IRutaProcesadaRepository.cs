using APIGasoline.Domain.Entities;

namespace APIGasoline.Domain.Interfaces;

public interface IRutaProcesadaRepository
{
    /// <summary>
    /// Ruta procesada del viaje (con seguimiento de cambios, para poder recalcularla).
    /// Un viaje tiene como máximo una.
    /// </summary>
    Task<RutaProcesada?> GetByViajeAsync(Guid viajeId, CancellationToken cancellationToken = default);

    /// <summary>Viajes cuyo GPS no cuadra con el odómetro, del más reciente al más antiguo.</summary>
    Task<IReadOnlyList<RutaProcesada>> GetConAlertaAsync(Guid? empresaId = null, CancellationToken cancellationToken = default);

    Task AddAsync(RutaProcesada rutaProcesada, CancellationToken cancellationToken = default);
    void Remove(RutaProcesada rutaProcesada);
}
