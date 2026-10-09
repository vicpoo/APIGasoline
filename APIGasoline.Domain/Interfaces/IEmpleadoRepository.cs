using APIGasoline.Domain.Entities;

namespace APIGasoline.Domain.Interfaces;

/// <summary>
/// Las consultas solo devuelven empleados no eliminados (soft delete).
/// </summary>
public interface IEmpleadoRepository
{
    Task<Empleado?> GetByIdAsync(Guid empleadoId, CancellationToken cancellationToken = default);
    Task<Empleado?> GetByUsuarioIdAsync(Guid usuarioId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Empleado>> GetAllAsync(Guid? empresaId = null, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid empleadoId, CancellationToken cancellationToken = default);

    /// <summary>Incluye eliminados: el usuario_id es único en toda la tabla.</summary>
    Task<bool> UsuarioAsignadoAsync(Guid usuarioId, Guid? excludeEmpleadoId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Empleados (no eliminados) de la empresa cuyo usuario tiene el rol indicado y activo.
    /// </summary>
    Task<IReadOnlyList<Guid>> GetIdsConRolAsync(Guid empresaId, string nombreRol, CancellationToken cancellationToken = default);

    Task AddAsync(Empleado empleado, CancellationToken cancellationToken = default);
}
