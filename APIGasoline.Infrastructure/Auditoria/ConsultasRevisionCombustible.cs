using APIGasoline.Application.Features.Auditoria.Common;
using APIGasoline.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace APIGasoline.Infrastructure.Auditoria;

/// <summary>Consultas de solo lectura para la revisión de combustible.</summary>
public sealed class ConsultasRevisionCombustible : IConsultasRevisionCombustible
{
    private readonly AppDbContext _context;

    public ConsultasRevisionCombustible(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<ViajeRevision>> GetViajesAsync(
        Guid? empresaId,
        Guid? carroId,
        DateTime desde,
        DateTime hasta,
        CancellationToken cancellationToken = default)
    {
        var viajes = _context.Viajes.AsNoTracking()
            .Where(v => v.FechaInicio >= desde && v.FechaInicio <= hasta);

        if (empresaId is not null)
            viajes = viajes.Where(v => v.EmpresaId == empresaId);

        if (carroId is not null)
            viajes = viajes.Where(v => v.CarroId == carroId);

        // LEFT JOIN con ruta_procesada (puede no existir todavía)
        var consulta =
            from v in viajes
            join r in _context.RutasProcesadas.AsNoTracking() on v.ViajeId equals r.ViajeId into rutas
            from r in rutas.DefaultIfEmpty()
            orderby v.FechaInicio
            select new ViajeRevision(
                v.ViajeId,
                v.EmpresaId,
                v.CarroId,
                v.EmpleadoOperadorId,
                v.FechaInicio,
                v.FechaFin,
                v.KilometrajeInicial,
                v.KilometrajeFinal,
                v.Estado,
                v.MotivoInterrupcion,
                v.Corregido,
                r == null ? (bool?)null : r.AlertaKilometraje,
                r == null ? null : r.DiferenciaKm,
                r == null ? null : r.DiferenciaPorcentaje);

        return await consulta.ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<CargaRevision>> GetCargasAsync(IReadOnlyCollection<Guid> viajeIds, CancellationToken cancellationToken = default)
    {
        if (viajeIds.Count == 0)
            return [];

        var ids = viajeIds.ToList();

        return await _context.CargasCombustible.AsNoTracking()
            .Where(c => ids.Contains(c.ViajeId))
            .OrderBy(c => c.FechaCarga)
            .Select(c => new CargaRevision(
                c.CargaCombustibleId,
                c.ViajeId,
                c.Litros,
                c.Monto,
                c.FechaCarga,
                c.FechaSubidaComprobante,
                c.Corregido))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<CarroRevision>> GetCarrosAsync(IReadOnlyCollection<Guid> carroIds, CancellationToken cancellationToken = default)
    {
        if (carroIds.Count == 0)
            return [];

        var ids = carroIds.ToList();

        return await _context.Carros.AsNoTracking()
            .Where(c => ids.Contains(c.CarroId))
            .Select(c => new CarroRevision(c.CarroId, c.Placas, c.CapacidadTanque))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyDictionary<Guid, decimal>> GetUltimoKilometrajePorCarroAsync(
        IReadOnlyCollection<Guid> carroIds,
        DateTime antesDe,
        CancellationToken cancellationToken = default)
    {
        var resultado = new Dictionary<Guid, decimal>();

        // Una consulta por carro (son pocos por revisión) para no depender de GroupBy + First en MySQL
        foreach (var carroId in carroIds)
        {
            var kilometraje = await _context.Viajes.AsNoTracking()
                .Where(v => v.CarroId == carroId && v.FechaInicio < antesDe && v.KilometrajeFinal != null)
                .OrderByDescending(v => v.FechaInicio)
                .Select(v => v.KilometrajeFinal)
                .FirstOrDefaultAsync(cancellationToken);

            if (kilometraje is not null)
                resultado[carroId] = kilometraje.Value;
        }

        return resultado;
    }
}
