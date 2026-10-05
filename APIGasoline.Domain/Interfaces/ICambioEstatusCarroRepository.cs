using APIGasoline.Domain.Entities;

namespace APIGasoline.Domain.Interfaces;

public interface ICambioEstatusCarroRepository
{
    Task<CambioEstatusCarro?> GetByIdAsync(int cambioEstatusCarroId, CancellationToken cancellationToken = default);

    /// <summary>Ordenados del más reciente al más antiguo.</summary>
    Task<IReadOnlyList<CambioEstatusCarro>> GetAllAsync(
        int? empresaId = null,
        int? carroId = null,
        DateTime? desde = null,
        DateTime? hasta = null,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsByCarroAsync(int carroId, CancellationToken cancellationToken = default);
    Task<bool> ExistsByViajeAsync(int viajeId, CancellationToken cancellationToken = default);
    Task AddAsync(CambioEstatusCarro cambio, CancellationToken cancellationToken = default);
}
