using APIGasoline.Domain.Entities;
using APIGasoline.Domain.Enums;

namespace APIGasoline.Domain.Interfaces;

public interface IRevisionAuditoriaRepository
{
    Task<RevisionAuditoria?> GetByIdAsync(Guid revisionAuditoriaId, CancellationToken cancellationToken = default);

    /// <summary>Ordenadas de la más reciente a la más antigua.</summary>
    Task<IReadOnlyList<RevisionAuditoria>> GetAllAsync(
        Guid? empresaId = null,
        Guid? viajeId = null,
        Guid? carroId = null,
        EstadoRevision? estado = null,
        bool? abiertas = null,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsByViajeAsync(Guid viajeId, CancellationToken cancellationToken = default);

    Task<bool> ExistsByCarroAsync(Guid carroId, CancellationToken cancellationToken = default);

    Task AddAsync(RevisionAuditoria revision, CancellationToken cancellationToken = default);
}
