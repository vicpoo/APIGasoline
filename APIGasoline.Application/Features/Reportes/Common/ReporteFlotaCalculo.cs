using APIGasoline.Application.Features.Reportes.DTOs;
using APIGasoline.Domain.ReadModels;

namespace APIGasoline.Application.Features.Reportes.Common;

/// <summary>
/// Arma el reporte de la flota con los totales que ya agrupó la base de datos.
/// Es lógica pura (sin base de datos), por eso se prueba sola (ReporteFlotaCalculoTests).
/// </summary>
public static class ReporteFlotaCalculo
{
    public static ReporteFlotaDto Armar(
        Guid? empresaId,
        string empresa,
        Guid? carroId,
        DateTime? desde,
        DateTime? hasta,
        DateTime generadoEn,
        IReadOnlyList<CarroReporte> carros,
        IReadOnlyList<ViajesPorCarro> viajes,
        IReadOnlyList<CargasPorCarro> cargas,
        IReadOnlyList<KmGpsPorCarro> kmGps)
    {
        var viajesPorCarro = viajes.ToDictionary(v => v.CarroId);
        var cargasPorCarro = cargas.ToDictionary(c => c.CarroId);
        var gpsPorCarro = kmGps.ToDictionary(g => g.CarroId);

        var filas = carros
            .Select(carro =>
            {
                viajesPorCarro.TryGetValue(carro.CarroId, out var v);
                cargasPorCarro.TryGetValue(carro.CarroId, out var c);
                gpsPorCarro.TryGetValue(carro.CarroId, out var g);

                var km = v?.KmRecorridos ?? 0m;
                var litros = c?.Litros ?? 0m;
                var gasto = c?.Monto ?? 0m;

                return new CarroReporteDto(
                    carro.CarroId,
                    carro.Placas,
                    carro.Marca,
                    carro.Modelo,
                    carro.Anio,
                    carro.Estado,
                    carro.DadoDeBaja,
                    carro.KilometrajeActual,
                    Viajes: v?.Viajes ?? 0,
                    KmRecorridos: Redondear(km),
                    KmGps: Redondear(g?.KmGps ?? 0m),
                    Cargas: c?.Cargas ?? 0,
                    Litros: Redondear(litros),
                    Gasto: Redondear(gasto),
                    RendimientoKmPorLitro: Dividir(km, litros),
                    CostoPorKm: Dividir(gasto, km),
                    UltimoViaje: v?.UltimoViaje);
            })
            // Un carro dado de baja solo aparece si tuvo movimiento en el periodo
            .Where(f => !f.DadoDeBaja || TieneActividad(f))
            .OrderByDescending(f => f.Gasto)
            .ThenByDescending(f => f.KmRecorridos)
            .ThenBy(f => f.Placas, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var kmTotal = filas.Sum(f => f.KmRecorridos);
        var litrosTotal = filas.Sum(f => f.Litros);
        var gastoTotal = filas.Sum(f => f.Gasto);

        var resumen = new ResumenFlotaDto(
            Carros: filas.Count,
            CarrosConActividad: filas.Count(TieneActividad),
            Viajes: filas.Sum(f => f.Viajes),
            KmRecorridos: kmTotal,
            KmGps: filas.Sum(f => f.KmGps),
            Cargas: filas.Sum(f => f.Cargas),
            Litros: litrosTotal,
            Gasto: gastoTotal,
            PrecioPromedioLitro: Dividir(gastoTotal, litrosTotal),
            RendimientoKmPorLitro: Dividir(kmTotal, litrosTotal),
            CostoPorKm: Dividir(gastoTotal, kmTotal));

        return new ReporteFlotaDto(empresaId, empresa, carroId, desde, hasta, generadoEn, resumen, filas);
    }

    private static bool TieneActividad(CarroReporteDto fila) => fila.Viajes > 0 || fila.Cargas > 0;

    /// <summary>null cuando no hay con qué dividir (ej. sin km o sin litros): en el reporte se ve "—".</summary>
    private static decimal? Dividir(decimal numerador, decimal denominador)
        => numerador > 0m && denominador > 0m ? Redondear(numerador / denominador) : null;

    private static decimal Redondear(decimal valor) => Math.Round(valor, 2, MidpointRounding.AwayFromZero);
}
