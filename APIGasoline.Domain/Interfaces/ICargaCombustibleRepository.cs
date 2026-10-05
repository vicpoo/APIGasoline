using APIGasoline.Domain.Entities;

namespace APIGasoline.Domain.Interfaces;

public interface ICargaCombustibleRepository
{
    Task<CargaCombustible?> GetByIdAsync(int cargaCombustibleId, CancellationToken cancellationToken = default);

    /// <summary>Ordenadas de la más reciente a la más antigua.</summary>
    Task<IReadOnlyList<CargaCombustible>> GetAllAsync(
        int? empresaId = null,
        int? viajeId = null,
        DateTime? desde = null,
        DateTime? hasta = null,
        CancellationToken cancellationToken = default);

    /// <summary>Detecta reenvíos de la misma carga (mismo viaje y misma fecha).</summary>
    Task<bool> ExisteEnFechaAsync(int viajeId, DateTime fechaCarga, int? excludeCargaCombustibleId = null, CancellationToken cancellationToken = default);

    Task<bool> ExistsByViajeAsync(int viajeId, CancellationToken cancellationToken = default);
    Task AddAsync(CargaCombustible cargaCombustible, CancellationToken cancellationToken = default);
    void Remove(CargaCombustible cargaCombustible);
}
