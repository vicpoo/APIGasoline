using APIGasoline.Application.Features.Reportes.Common;
using APIGasoline.Domain.Enums;
using APIGasoline.Domain.ReadModels;

namespace APIGasoline.Application.Tests.Features.Reportes;

/// <summary>Reporte de la flota: km, cargas, litros y dinero por carro, con sus totales.</summary>
public class ReporteFlotaCalculoTests
{
    private static readonly DateTime Ahora = new(2026, 10, 28, 12, 0, 0, DateTimeKind.Utc);
    private static readonly Guid Vocho = Id(1);
    private static readonly Guid Tsuru = Id(2);
    private static readonly Guid Baja = Id(3);

    private static CarroReporte Carro(Guid id, string placas, bool dadoDeBaja = false)
        => new(id, placas, "Nissan", "Tsuru", 2018, EstadoCarro.Disponible, dadoDeBaja, 50_000m);

    private static ReporteFlotaArmado Armar(
        IReadOnlyList<CarroReporte> carros,
        IReadOnlyList<ViajesPorCarro>? viajes = null,
        IReadOnlyList<CargasPorCarro>? cargas = null,
        IReadOnlyList<KmGpsPorCarro>? gps = null)
        => new(ReporteFlotaCalculo.Armar(Id(100), "Grupo de prueba", null, null, null, Ahora,
            carros, viajes ?? [], cargas ?? [], gps ?? []));

    /// <summary>Envoltorio para leer el resultado más corto en las pruebas.</summary>
    private sealed record ReporteFlotaArmado(APIGasoline.Application.Features.Reportes.DTOs.ReporteFlotaDto Reporte)
    {
        public APIGasoline.Application.Features.Reportes.DTOs.CarroReporteDto Fila(Guid carroId)
            => Reporte.Carros.Single(c => c.CarroId == carroId);
    }

    [Fact]
    public void PorCarro_SumaKmCargasLitrosYDinero_yCalculaRendimientoYCostoPorKm()
    {
        var r = Armar(
            [Carro(Vocho, "AAA-111")],
            viajes: [new ViajesPorCarro(Vocho, 3, 400m, Ahora.AddDays(-1))],
            cargas: [new CargasPorCarro(Vocho, 2, 40m, 1_000m)],
            gps: [new KmGpsPorCarro(Vocho, 390.5m)]);

        var fila = r.Fila(Vocho);
        Assert.Equal(3, fila.Viajes);
        Assert.Equal(400m, fila.KmRecorridos);
        Assert.Equal(390.5m, fila.KmGps);
        Assert.Equal(2, fila.Cargas);
        Assert.Equal(40m, fila.Litros);
        Assert.Equal(1_000m, fila.Gasto);
        Assert.Equal(10m, fila.RendimientoKmPorLitro); // 400 km / 40 L
        Assert.Equal(2.5m, fila.CostoPorKm);           // $1000 / 400 km
    }

    [Fact]
    public void CarroSinMovimiento_ApareceEnCeros_ySinRendimiento()
    {
        var r = Armar([Carro(Vocho, "AAA-111")]);

        var fila = r.Fila(Vocho);
        Assert.Equal(0, fila.Viajes);
        Assert.Equal(0m, fila.Gasto);
        Assert.Null(fila.RendimientoKmPorLitro);
        Assert.Null(fila.CostoPorKm);
        Assert.Equal(0, r.Reporte.Resumen.CarrosConActividad);
    }

    [Fact]
    public void DadoDeBajaSinMovimiento_NoAparece_peroConMovimientoSi()
    {
        var sinMovimiento = Armar([Carro(Vocho, "AAA-111"), Carro(Baja, "ZZZ-999", dadoDeBaja: true)]);
        Assert.DoesNotContain(sinMovimiento.Reporte.Carros, c => c.CarroId == Baja);

        var conMovimiento = Armar(
            [Carro(Vocho, "AAA-111"), Carro(Baja, "ZZZ-999", dadoDeBaja: true)],
            cargas: [new CargasPorCarro(Baja, 1, 30m, 700m)]);
        Assert.Contains(conMovimiento.Reporte.Carros, c => c.CarroId == Baja);
    }

    [Fact]
    public void Totales_SumanTodaLaFlota_yLosPromediosSeCalculanSobreElTotal()
    {
        var r = Armar(
            [Carro(Vocho, "AAA-111"), Carro(Tsuru, "BBB-222")],
            viajes: [new ViajesPorCarro(Vocho, 2, 300m, null), new ViajesPorCarro(Tsuru, 1, 100m, null)],
            cargas: [new CargasPorCarro(Vocho, 1, 30m, 750m), new CargasPorCarro(Tsuru, 1, 10m, 250m)]);

        var total = r.Reporte.Resumen;
        Assert.Equal(2, total.Carros);
        Assert.Equal(3, total.Viajes);
        Assert.Equal(400m, total.KmRecorridos);
        Assert.Equal(2, total.Cargas);
        Assert.Equal(40m, total.Litros);
        Assert.Equal(1_000m, total.Gasto);
        Assert.Equal(25m, total.PrecioPromedioLitro);   // $1000 / 40 L
        Assert.Equal(10m, total.RendimientoKmPorLitro); // 400 km / 40 L
        Assert.Equal(2.5m, total.CostoPorKm);
    }

    [Fact]
    public void Orden_DelQueMasGastoAlQueMenos()
    {
        var r = Armar(
            [Carro(Vocho, "AAA-111"), Carro(Tsuru, "BBB-222")],
            cargas: [new CargasPorCarro(Vocho, 1, 10m, 200m), new CargasPorCarro(Tsuru, 1, 30m, 900m)]);

        Assert.Equal(new[] { Tsuru, Vocho }, r.Reporte.Carros.Select(c => c.CarroId).ToArray());
    }

    [Fact]
    public void Redondeo_ADosDecimales()
    {
        var r = Armar(
            [Carro(Vocho, "AAA-111")],
            viajes: [new ViajesPorCarro(Vocho, 1, 100m, null)],
            cargas: [new CargasPorCarro(Vocho, 1, 3m, 100m)]);

        Assert.Equal(33.33m, r.Fila(Vocho).RendimientoKmPorLitro); // 100 / 3
        Assert.Equal(1m, r.Fila(Vocho).CostoPorKm);
    }
}
