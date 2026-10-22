using APIGasoline.Domain.ReadModels;

namespace APIGasoline.Domain.Interfaces;

/// <summary>
/// Lecturas para la pantalla "Revisión de combustible" (lado de lectura de CQRS, se expone en el
/// Unit of Work). No duplica información: lee viajes, cargas, carros y rutas procesadas tal como están.
/// </summary>
public interface IRevisionCombustibleRepository
{
    /// <summary>Viajes que iniciaron en el periodo (con el resultado GPS contra odómetro, si existe).</summary>
    Task<IReadOnlyList<ViajeRevision>> GetViajesAsync(Guid? empresaId, Guid? carroId, DateTime desde, DateTime hasta, CancellationToken cancellationToken = default);

    /// <summary>Cargas de los viajes indicados.</summary>
    Task<IReadOnlyList<CargaRevision>> GetCargasAsync(IReadOnlyCollection<Guid> viajeIds, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CarroRevision>> GetCarrosAsync(IReadOnlyCollection<Guid> carroIds, CancellationToken cancellationToken = default);

    /// <summary>Último kilometraje final de cada carro antes de <paramref name="antesDe"/> (para revisar la continuidad).</summary>
    Task<IReadOnlyDictionary<Guid, decimal>> GetUltimoKilometrajePorCarroAsync(IReadOnlyCollection<Guid> carroIds, DateTime antesDe, CancellationToken cancellationToken = default);
}
