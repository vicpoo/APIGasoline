using APIGasoline.Domain.Entities;

namespace APIGasoline.Domain.Interfaces;

public interface ICambioEstatusCarroRepository
{
    Task<CambioEstatusCarro?> GetByIdAsync(Guid cambioEstatusCarroId, CancellationToken cancellationToken = default);

    /// <summary>Con seguimiento de cambios: para agregar la foto de evidencia.</summary>
    Task<CambioEstatusCarro?> GetByIdParaActualizarAsync(Guid cambioEstatusCarroId, CancellationToken cancellationToken = default);

    /// <summary>Ordenados del más reciente al más antiguo.</summary>
    Task<IReadOnlyList<CambioEstatusCarro>> GetAllAsync(
        Guid? empresaId = null,
        Guid? carroId = null,
        DateTime? desde = null,
        DateTime? hasta = null,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsByCarroAsync(Guid carroId, CancellationToken cancellationToken = default);
    Task<bool> ExistsByViajeAsync(Guid viajeId, CancellationToken cancellationToken = default);
    Task AddAsync(CambioEstatusCarro cambio, CancellationToken cancellationToken = default);
}
