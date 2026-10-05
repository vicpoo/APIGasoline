using APIGasoline.Domain.Entities;
using APIGasoline.Domain.Enums;
using APIGasoline.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace APIGasoline.Infrastructure.Persistence.Repositories;

public class ViajeRepository : IViajeRepository
{
    private readonly AppDbContext _context;

    public ViajeRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<Viaje?> GetByIdAsync(int viajeId, CancellationToken cancellationToken = default)
        => _context.Viajes.FirstOrDefaultAsync(v => v.ViajeId == viajeId, cancellationToken);

    public async Task<IReadOnlyList<Viaje>> GetAllAsync(
        int? empresaId = null,
        int? carroId = null,
        int? empleadoOperadorId = null,
        int? solicitudReservaId = null,
        EstadoViaje? estado = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Viajes.AsNoTracking();

        if (empresaId is not null)
            query = query.Where(v => v.EmpresaId == empresaId);

        if (carroId is not null)
            query = query.Where(v => v.CarroId == carroId);

        if (empleadoOperadorId is not null)
            query = query.Where(v => v.EmpleadoOperadorId == empleadoOperadorId);

        if (solicitudReservaId is not null)
            query = query.Where(v => v.SolicitudReservaId == solicitudReservaId);

        if (estado is not null)
            query = query.Where(v => v.Estado == estado);

        return await query
            .OrderByDescending(v => v.FechaInicio)
            .ThenByDescending(v => v.ViajeId)
            .ToListAsync(cancellationToken);
    }

    public Task<bool> ExisteEnCursoPorCarroAsync(int carroId, CancellationToken cancellationToken = default)
        => _context.Viajes.AnyAsync(v => v.CarroId == carroId && v.Estado == EstadoViaje.EnCurso, cancellationToken);

    public Task<bool> ExisteEnCursoPorSolicitudAsync(int solicitudReservaId, CancellationToken cancellationToken = default)
        => _context.Viajes.AnyAsync(v => v.SolicitudReservaId == solicitudReservaId && v.Estado == EstadoViaje.EnCurso, cancellationToken);

    public Task<bool> ExistsByCarroAsync(int carroId, CancellationToken cancellationToken = default)
        => _context.Viajes.AnyAsync(v => v.CarroId == carroId, cancellationToken);

    public Task<bool> ExistsBySolicitudAsync(int solicitudReservaId, CancellationToken cancellationToken = default)
        => _context.Viajes.AnyAsync(v => v.SolicitudReservaId == solicitudReservaId, cancellationToken);

    public async Task AddAsync(Viaje viaje, CancellationToken cancellationToken = default)
        => await _context.Viajes.AddAsync(viaje, cancellationToken);

    public void Remove(Viaje viaje)
        => _context.Viajes.Remove(viaje);
}
