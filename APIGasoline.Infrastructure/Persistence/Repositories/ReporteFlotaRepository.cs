using APIGasoline.Domain.Entities;
using APIGasoline.Domain.Interfaces;
using APIGasoline.Domain.ReadModels;
using Microsoft.EntityFrameworkCore;

namespace APIGasoline.Infrastructure.Persistence.Repositories;

/// <summary>Consultas de solo lectura para los reportes de la flota (las sumas las hace MySQL).</summary>
public sealed class ReporteFlotaRepository : IReporteFlotaRepository
{
    private readonly AppDbContext _context;

    public ReporteFlotaRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<string?> GetNombreEmpresaAsync(Guid empresaId, CancellationToken cancellationToken = default)
        => await _context.Empresas.AsNoTracking()
            .Where(e => e.EmpresaId == empresaId)
            .Select(e => e.Nombre)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<IReadOnlyList<CarroReporte>> GetCarrosAsync(Guid? empresaId, Guid? carroId, CancellationToken cancellationToken = default)
    {
        var carros = _context.Carros.AsNoTracking();

        if (empresaId is not null)
            carros = carros.Where(c => c.EmpresaId == empresaId);

        if (carroId is not null)
            carros = carros.Where(c => c.CarroId == carroId);

        return await carros
            .OrderBy(c => c.Placas)
            .Select(c => new CarroReporte(c.CarroId, c.Placas, c.Marca, c.Modelo, c.Anio, c.Estado, c.DadoDeBaja, c.KilometrajeActual))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ViajesPorCarro>> GetViajesPorCarroAsync(
        Guid? empresaId,
        Guid? carroId,
        DateTime? desde,
        DateTime? hasta,
        CancellationToken cancellationToken = default)
        => await Viajes(empresaId, carroId, desde, hasta)
            .GroupBy(v => v.CarroId)
            .Select(g => new ViajesPorCarro(
                g.Key,
                g.Count(),
                // Solo cuentan los viajes cerrados con un km final coherente (no negativo)
                g.Sum(v => v.KilometrajeFinal != null && v.KilometrajeFinal >= v.KilometrajeInicial
                    ? v.KilometrajeFinal.Value - v.KilometrajeInicial
                    : 0m),
                g.Max(v => (DateTime?)v.FechaInicio)))
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<CargasPorCarro>> GetCargasPorCarroAsync(
        Guid? empresaId,
        Guid? carroId,
        DateTime? desde,
        DateTime? hasta,
        CancellationToken cancellationToken = default)
    {
        var cargas = _context.CargasCombustible.AsNoTracking();

        if (empresaId is not null)
            cargas = cargas.Where(c => c.EmpresaId == empresaId);

        if (desde is not null)
            cargas = cargas.Where(c => c.FechaCarga >= desde);

        if (hasta is not null)
            cargas = cargas.Where(c => c.FechaCarga <= hasta);

        // La carga no guarda el carro: se toma del viaje al que pertenece
        var consulta =
            from c in cargas
            join v in _context.Viajes.AsNoTracking() on c.ViajeId equals v.ViajeId
            select new { v.CarroId, c.Litros, c.Monto };

        if (carroId is not null)
            consulta = consulta.Where(x => x.CarroId == carroId);

        return await consulta
            .GroupBy(x => x.CarroId)
            .Select(g => new CargasPorCarro(g.Key, g.Count(), g.Sum(x => x.Litros), g.Sum(x => x.Monto ?? 0m)))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<CargaDetalle>> GetDetalleCargasAsync(
        Guid? empresaId,
        Guid? carroId,
        DateTime? desde,
        DateTime? hasta,
        CancellationToken cancellationToken = default)
    {
        var cargas = _context.CargasCombustible.AsNoTracking();

        if (empresaId is not null)
            cargas = cargas.Where(c => c.EmpresaId == empresaId);

        if (desde is not null)
            cargas = cargas.Where(c => c.FechaCarga >= desde);

        if (hasta is not null)
            cargas = cargas.Where(c => c.FechaCarga <= hasta);

        // El carro sale del viaje; el centro de costos es el de la carga o, si no tiene, el del viaje
        var consulta =
            from c in cargas
            join v in _context.Viajes.AsNoTracking() on c.ViajeId equals v.ViajeId
            select new
            {
                c.CargaCombustibleId,
                c.FechaCarga,
                v.CarroId,
                c.Litros,
                c.Monto,
                CentroCostoId = c.CentroCostoId ?? v.CentroCostoId,
                c.UbicacionLat,
                c.UbicacionLng,
            };

        if (carroId is not null)
            consulta = consulta.Where(x => x.CarroId == carroId);

        var filas = await consulta
            .OrderBy(x => x.FechaCarga)
            .ThenBy(x => x.CargaCombustibleId)
            .ToListAsync(cancellationToken);

        // Los centros de costo en una segunda consulta (pocos): evita un LEFT JOIN sobre un COALESCE
        var idsCentros = filas.Where(x => x.CentroCostoId is not null).Select(x => x.CentroCostoId!.Value).Distinct().ToList();
        var centros = idsCentros.Count == 0
            ? new Dictionary<Guid, (string Clave, string Nombre)>()
            : await _context.CentrosCosto.AsNoTracking()
                .Where(cc => idsCentros.Contains(cc.CentroCostoId))
                .ToDictionaryAsync(cc => cc.CentroCostoId, cc => (cc.Clave, cc.Nombre), cancellationToken);

        return filas
            .Select(x =>
            {
                (string Clave, string Nombre)? centro =
                    x.CentroCostoId is { } id && centros.TryGetValue(id, out var cc) ? cc : null;
                return new CargaDetalle(
                    x.CargaCombustibleId,
                    x.FechaCarga,
                    x.CarroId,
                    x.Litros,
                    x.Monto,
                    centro?.Clave,
                    centro?.Nombre,
                    x.UbicacionLat,
                    x.UbicacionLng);
            })
            .ToList();
    }

    public async Task<IReadOnlyList<KmGpsPorCarro>> GetKmGpsPorCarroAsync(
        Guid? empresaId,
        Guid? carroId,
        DateTime? desde,
        DateTime? hasta,
        CancellationToken cancellationToken = default)
    {
        var consulta =
            from r in _context.RutasProcesadas.AsNoTracking()
            join v in Viajes(empresaId, carroId, desde, hasta) on r.ViajeId equals v.ViajeId
            select new { v.CarroId, r.DistanciaGpsKm };

        return await consulta
            .GroupBy(x => x.CarroId)
            .Select(g => new KmGpsPorCarro(g.Key, g.Sum(x => x.DistanciaGpsKm)))
            .ToListAsync(cancellationToken);
    }

    /// <summary>Viajes de la empresa / carro que iniciaron en el periodo.</summary>
    private IQueryable<Viaje> Viajes(Guid? empresaId, Guid? carroId, DateTime? desde, DateTime? hasta)
    {
        var viajes = _context.Viajes.AsNoTracking();

        if (empresaId is not null)
            viajes = viajes.Where(v => v.EmpresaId == empresaId);

        if (carroId is not null)
            viajes = viajes.Where(v => v.CarroId == carroId);

        if (desde is not null)
            viajes = viajes.Where(v => v.FechaInicio >= desde);

        if (hasta is not null)
            viajes = viajes.Where(v => v.FechaInicio <= hasta);

        return viajes;
    }
}
