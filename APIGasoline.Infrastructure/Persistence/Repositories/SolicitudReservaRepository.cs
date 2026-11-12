using APIGasoline.Domain.Common;
using APIGasoline.Domain.Entities;
using APIGasoline.Domain.Enums;
using APIGasoline.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace APIGasoline.Infrastructure.Persistence.Repositories;

public class SolicitudReservaRepository : ISolicitudReservaRepository
{
    private readonly AppDbContext _context;

    public SolicitudReservaRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<SolicitudReserva?> GetByIdAsync(Guid solicitudReservaId, CancellationToken cancellationToken = default)
        => _context.SolicitudesReserva.FirstOrDefaultAsync(s => s.SolicitudReservaId == solicitudReservaId, cancellationToken);

    public async Task<IReadOnlyList<SolicitudReserva>> GetAllAsync(Guid? empresaId = null, Guid? carroId = null, Guid? empleadoSolicitanteId = null, EstadoSolicitudReserva? estado = null, CancellationToken cancellationToken = default)
        => await Consultar(empresaId, carroId, empleadoSolicitanteId, estado).ToListAsync(cancellationToken);

    /// <summary>F011: como <see cref="GetAllAsync"/>, pero solo una página (y el total).</summary>
    public Task<PaginaDe<SolicitudReserva>> GetPaginaAsync(Paginacion pagina, Guid? empresaId = null, Guid? carroId = null, Guid? empleadoSolicitanteId = null, EstadoSolicitudReserva? estado = null, CancellationToken cancellationToken = default)
        => Consultar(empresaId, carroId, empleadoSolicitanteId, estado).APaginaAsync(pagina, cancellationToken);

    private IQueryable<SolicitudReserva> Consultar(Guid? empresaId, Guid? carroId, Guid? empleadoSolicitanteId, EstadoSolicitudReserva? estado)
    {
        var query = _context.SolicitudesReserva.AsNoTracking();

        if (empresaId is not null)
            query = query.Where(s => s.EmpresaId == empresaId);

        if (carroId is not null)
            query = query.Where(s => s.CarroId == carroId);

        if (empleadoSolicitanteId is not null)
            query = query.Where(s => s.EmpleadoSolicitanteId == empleadoSolicitanteId);

        if (estado is not null)
            query = query.Where(s => s.Estado == estado);

        // Cola de pendientes: la más antigua primero. Lo demás (historial), lo más reciente primero:
        // con paginación (F011) la primera página debe traer lo nuevo, no lo de hace meses.
        return estado == EstadoSolicitudReserva.Pendiente
            ? query.OrderBy(s => s.FechaSolicitud).ThenBy(s => s.SolicitudReservaId)
            : query.OrderByDescending(s => s.FechaSolicitud).ThenByDescending(s => s.SolicitudReservaId);
    }

    public async Task<IReadOnlyList<SolicitudReserva>> GetPendientesByCarroAsync(Guid carroId, Guid excludeSolicitudReservaId, CancellationToken cancellationToken = default)
        => await _context.SolicitudesReserva
            .Where(s => s.CarroId == carroId
                        && s.Estado == EstadoSolicitudReserva.Pendiente
                        && s.SolicitudReservaId != excludeSolicitudReservaId)
            .ToListAsync(cancellationToken);

    public Task<bool> ExistePendienteAsync(Guid empleadoSolicitanteId, Guid carroId, CancellationToken cancellationToken = default)
        => _context.SolicitudesReserva.AnyAsync(
            s => s.EmpleadoSolicitanteId == empleadoSolicitanteId
                 && s.CarroId == carroId
                 && s.Estado == EstadoSolicitudReserva.Pendiente,
            cancellationToken);

    public Task<bool> ExistsByCarroAsync(Guid carroId, CancellationToken cancellationToken = default)
        => _context.SolicitudesReserva.AnyAsync(s => s.CarroId == carroId, cancellationToken);

    public Task<SolicitudReserva?> GetAprobadaByCarroAsync(Guid carroId, CancellationToken cancellationToken = default)
        => _context.SolicitudesReserva
            .Where(s => s.CarroId == carroId && s.Estado == EstadoSolicitudReserva.Aprobada)
            .OrderByDescending(s => s.FechaResolucion)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task AddAsync(SolicitudReserva solicitudReserva, CancellationToken cancellationToken = default)
        => await _context.SolicitudesReserva.AddAsync(solicitudReserva, cancellationToken);

    public void Remove(SolicitudReserva solicitudReserva)
        => _context.SolicitudesReserva.Remove(solicitudReserva);
}
