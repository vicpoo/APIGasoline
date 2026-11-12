using APIGasoline.Domain.Common;
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

    public Task<Viaje?> GetByIdAsync(Guid viajeId, CancellationToken cancellationToken = default)
        => _context.Viajes.FirstOrDefaultAsync(v => v.ViajeId == viajeId, cancellationToken);

    public async Task<IReadOnlyList<Viaje>> GetAllAsync(Guid? empresaId = null, Guid? carroId = null, Guid? empleadoOperadorId = null, Guid? solicitudReservaId = null, EstadoViaje? estado = null, CancellationToken cancellationToken = default)
        => await Consultar(empresaId, carroId, empleadoOperadorId, solicitudReservaId, estado).ToListAsync(cancellationToken);

    /// <summary>F011: como <see cref="GetAllAsync"/>, pero solo una página (y el total).</summary>
    public Task<PaginaDe<Viaje>> GetPaginaAsync(Paginacion pagina, Guid? empresaId = null, Guid? carroId = null, Guid? empleadoOperadorId = null, Guid? solicitudReservaId = null, EstadoViaje? estado = null, CancellationToken cancellationToken = default)
        => Consultar(empresaId, carroId, empleadoOperadorId, solicitudReservaId, estado).APaginaAsync(pagina, cancellationToken);

    private IQueryable<Viaje> Consultar(Guid? empresaId, Guid? carroId, Guid? empleadoOperadorId, Guid? solicitudReservaId, EstadoViaje? estado)
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

        return query
            .OrderByDescending(v => v.FechaInicio)
            .ThenByDescending(v => v.ViajeId);
    }

    public Task<bool> ExisteEnCursoPorCarroAsync(Guid carroId, CancellationToken cancellationToken = default)
        => _context.Viajes.AnyAsync(v => v.CarroId == carroId && v.Estado == EstadoViaje.EnCurso, cancellationToken);

    public Task<Viaje?> GetEnCursoPorCarroAsync(Guid carroId, CancellationToken cancellationToken = default)
        => _context.Viajes
            .AsNoTracking()
            .Where(v => v.CarroId == carroId && v.Estado == EstadoViaje.EnCurso)
            .OrderByDescending(v => v.FechaInicio)
            .FirstOrDefaultAsync(cancellationToken);

    public Task<bool> ExisteEnCursoPorSolicitudAsync(Guid solicitudReservaId, CancellationToken cancellationToken = default)
        => _context.Viajes.AnyAsync(v => v.SolicitudReservaId == solicitudReservaId && v.Estado == EstadoViaje.EnCurso, cancellationToken);

    public Task<bool> ExistsByCarroAsync(Guid carroId, CancellationToken cancellationToken = default)
        => _context.Viajes.AnyAsync(v => v.CarroId == carroId, cancellationToken);

    public Task<bool> ExistsByCentroCostoAsync(Guid centroCostoId, CancellationToken cancellationToken = default)
        => _context.Viajes.AnyAsync(v => v.CentroCostoId == centroCostoId, cancellationToken);

    public Task<bool> ExistsBySolicitudAsync(Guid solicitudReservaId, CancellationToken cancellationToken = default)
        => _context.Viajes.AnyAsync(v => v.SolicitudReservaId == solicitudReservaId, cancellationToken);

    public async Task AddAsync(Viaje viaje, CancellationToken cancellationToken = default)
        => await _context.Viajes.AddAsync(viaje, cancellationToken);

    public void Remove(Viaje viaje)
        => _context.Viajes.Remove(viaje);
}
