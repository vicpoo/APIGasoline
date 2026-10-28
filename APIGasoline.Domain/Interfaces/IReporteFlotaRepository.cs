using APIGasoline.Domain.ReadModels;

namespace APIGasoline.Domain.Interfaces;

/// <summary>
/// Lecturas para los reportes de la flota (lado de lectura de CQRS, se expone en el Unit of Work).
/// Cada consulta regresa los totales ya agrupados por carro. <c>desde</c> / <c>hasta</c> en null = sin límite.
/// </summary>
public interface IReporteFlotaRepository
{
    Task<string?> GetNombreEmpresaAsync(Guid empresaId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CarroReporte>> GetCarrosAsync(Guid? empresaId, Guid? carroId, CancellationToken cancellationToken = default);

    /// <summary>Viajes que iniciaron en el periodo.</summary>
    Task<IReadOnlyList<ViajesPorCarro>> GetViajesPorCarroAsync(Guid? empresaId, Guid? carroId, DateTime? desde, DateTime? hasta, CancellationToken cancellationToken = default);

    /// <summary>Cargas cuya fecha de carga cae en el periodo.</summary>
    Task<IReadOnlyList<CargasPorCarro>> GetCargasPorCarroAsync(Guid? empresaId, Guid? carroId, DateTime? desde, DateTime? hasta, CancellationToken cancellationToken = default);

    /// <summary>Km GPS de los viajes que iniciaron en el periodo y ya tienen ruta procesada.</summary>
    Task<IReadOnlyList<KmGpsPorCarro>> GetKmGpsPorCarroAsync(Guid? empresaId, Guid? carroId, DateTime? desde, DateTime? hasta, CancellationToken cancellationToken = default);
}
