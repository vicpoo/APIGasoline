using APIGasoline.Domain.Entities;
using APIGasoline.Domain.Enums;

namespace APIGasoline.Domain.Interfaces;

/// <summary>
/// Historial de cambios. Solo lectura e inserción: no existe Update ni Remove a propósito.
/// </summary>
public interface IAuditoriaEventoRepository
{
    Task<AuditoriaEvento?> GetByIdAsync(Guid auditoriaEventoId, CancellationToken cancellationToken = default);

    /// <summary>Ordenados del más reciente al más antiguo.</summary>
    Task<IReadOnlyList<AuditoriaEvento>> GetAllAsync(
        Guid? empresaId = null,
        string? entidad = null,
        Guid? entidadId = null,
        Guid? usuarioId = null,
        AccionAuditoria? accion = null,
        DateTime? desde = null,
        DateTime? hasta = null,
        int maximo = 500,
        CancellationToken cancellationToken = default);

    /// <summary>Para no borrar un usuario cuyo historial debe conservarse.</summary>
    Task<bool> ExistsByUsuarioAsync(Guid usuarioId, CancellationToken cancellationToken = default);

    Task AddAsync(AuditoriaEvento evento, CancellationToken cancellationToken = default);
}
