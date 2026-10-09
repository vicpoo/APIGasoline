using APIGasoline.Domain.Entities;

namespace APIGasoline.Domain.Interfaces;

public interface IDispositivoEmpleadoRepository
{
    Task<DispositivoEmpleado?> GetByIdAsync(Guid dispositivoEmpleadoId, CancellationToken cancellationToken = default);
    Task<DispositivoEmpleado?> GetByTokenAsync(string tokenFcm, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DispositivoEmpleado>> GetByEmpleadoAsync(Guid empleadoId, CancellationToken cancellationToken = default);

    /// <summary>Con seguimiento de cambios: el envío puede desactivar los que tengan token inválido.</summary>
    Task<IReadOnlyList<DispositivoEmpleado>> GetActivosByEmpleadoAsync(Guid empleadoId, CancellationToken cancellationToken = default);

    Task AddAsync(DispositivoEmpleado dispositivo, CancellationToken cancellationToken = default);
}
