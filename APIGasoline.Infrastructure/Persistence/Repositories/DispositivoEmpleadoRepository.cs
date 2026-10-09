using APIGasoline.Domain.Entities;
using APIGasoline.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace APIGasoline.Infrastructure.Persistence.Repositories;

public class DispositivoEmpleadoRepository : IDispositivoEmpleadoRepository
{
    private readonly AppDbContext _context;

    public DispositivoEmpleadoRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<DispositivoEmpleado?> GetByIdAsync(Guid dispositivoEmpleadoId, CancellationToken cancellationToken = default)
        => _context.Dispositivos.FirstOrDefaultAsync(d => d.DispositivoEmpleadoId == dispositivoEmpleadoId, cancellationToken);

    public Task<DispositivoEmpleado?> GetByTokenAsync(string tokenFcm, CancellationToken cancellationToken = default)
        => _context.Dispositivos.FirstOrDefaultAsync(d => d.TokenFcm == tokenFcm, cancellationToken);

    public async Task<IReadOnlyList<DispositivoEmpleado>> GetByEmpleadoAsync(Guid empleadoId, CancellationToken cancellationToken = default)
        => await _context.Dispositivos
            .AsNoTracking()
            .Where(d => d.EmpleadoId == empleadoId)
            .OrderByDescending(d => d.Activo)
            .ThenByDescending(d => d.FechaUltimoUso)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<DispositivoEmpleado>> GetActivosByEmpleadoAsync(Guid empleadoId, CancellationToken cancellationToken = default)
        => await _context.Dispositivos
            .Where(d => d.EmpleadoId == empleadoId && d.Activo)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(DispositivoEmpleado dispositivo, CancellationToken cancellationToken = default)
        => await _context.Dispositivos.AddAsync(dispositivo, cancellationToken);
}
