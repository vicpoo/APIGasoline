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

    public Task<SolicitudReserva?> GetByIdAsync(int solicitudReservaId, CancellationToken cancellationToken = default)
        => _context.SolicitudesReserva.FirstOrDefaultAsync(s => s.SolicitudReservaId == solicitudReservaId, cancellationToken);

    public async Task<IReadOnlyList<SolicitudReserva>> GetAllAsync(
        int? empresaId = null,
        int? carroId = null,
        int? empleadoSolicitanteId = null,
        EstadoSolicitudReserva? estado = null,
        CancellationToken cancellationToken = default)
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

        // Cola: más antigua primero
        return await query
            .OrderBy(s => s.FechaSolicitud)
            .ThenBy(s => s.SolicitudReservaId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<SolicitudReserva>> GetPendientesByCarroAsync(int carroId, int excludeSolicitudReservaId, CancellationToken cancellationToken = default)
        => await _context.SolicitudesReserva
            .Where(s => s.CarroId == carroId
                        && s.Estado == EstadoSolicitudReserva.Pendiente
                        && s.SolicitudReservaId != excludeSolicitudReservaId)
            .ToListAsync(cancellationToken);

    public Task<bool> ExistePendienteAsync(int empleadoSolicitanteId, int carroId, CancellationToken cancellationToken = default)
        => _context.SolicitudesReserva.AnyAsync(
            s => s.EmpleadoSolicitanteId == empleadoSolicitanteId
                 && s.CarroId == carroId
                 && s.Estado == EstadoSolicitudReserva.Pendiente,
            cancellationToken);

    public Task<bool> ExistsByCarroAsync(int carroId, CancellationToken cancellationToken = default)
        => _context.SolicitudesReserva.AnyAsync(s => s.CarroId == carroId, cancellationToken);

    public Task<SolicitudReserva?> GetAprobadaByCarroAsync(int carroId, CancellationToken cancellationToken = default)
        => _context.SolicitudesReserva
            .Where(s => s.CarroId == carroId && s.Estado == EstadoSolicitudReserva.Aprobada)
            .OrderByDescending(s => s.FechaResolucion)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task AddAsync(SolicitudReserva solicitudReserva, CancellationToken cancellationToken = default)
        => await _context.SolicitudesReserva.AddAsync(solicitudReserva, cancellationToken);

    public void Remove(SolicitudReserva solicitudReserva)
        => _context.SolicitudesReserva.Remove(solicitudReserva);
}
