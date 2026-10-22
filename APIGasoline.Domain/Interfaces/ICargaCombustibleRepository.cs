using APIGasoline.Domain.Entities;

namespace APIGasoline.Domain.Interfaces;

public interface ICargaCombustibleRepository
{
    Task<CargaCombustible?> GetByIdAsync(Guid cargaCombustibleId, CancellationToken cancellationToken = default);

    /// <summary>Ordenadas de la más reciente a la más antigua.</summary>
    Task<IReadOnlyList<CargaCombustible>> GetAllAsync(
        Guid? empresaId = null,
        Guid? viajeId = null,
        DateTime? desde = null,
        DateTime? hasta = null,
        CancellationToken cancellationToken = default);

    /// <summary>Detecta reenvíos de la misma carga (mismo viaje y misma fecha).</summary>
    Task<bool> ExisteEnFechaAsync(Guid viajeId, DateTime fechaCarga, Guid? excludeCargaCombustibleId = null, CancellationToken cancellationToken = default);

    Task<bool> ExistsByViajeAsync(Guid viajeId, CancellationToken cancellationToken = default);
    Task<bool> ExistsByCentroCostoAsync(Guid centroCostoId, CancellationToken cancellationToken = default);

    /// <summary>Total de litros cargados en el viaje (0 si no hubo cargas).</summary>
    Task<decimal> SumLitrosByViajeAsync(Guid viajeId, CancellationToken cancellationToken = default);

    Task AddAsync(CargaCombustible cargaCombustible, CancellationToken cancellationToken = default);
    void Remove(CargaCombustible cargaCombustible);
}
