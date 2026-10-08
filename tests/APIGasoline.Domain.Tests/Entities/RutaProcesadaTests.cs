using APIGasoline.Domain.Entities;
using APIGasoline.Domain.Exceptions;
using APIGasoline.Domain.Rutas;
using APIGasoline.Domain.Tests.Rutas;

namespace APIGasoline.Domain.Tests.Entities;

/// <summary>
/// Reglas del resumen procesado de un viaje: copia fiel del resultado del algoritmo,
/// ids obligatorios y recalculo sobre la misma fila.
/// </summary>
public class RutaProcesadaTests
{
    private static readonly DateTime FechaProceso = new(2026, 10, 7, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Crear_ConResultado_CopiaTodasLasMetricas()
    {
        var resultado = ResultadoDePrueba();

        var ruta = RutaProcesada.Crear(1, 7, resultado, FechaProceso);

        Assert.Equal(1, ruta.EmpresaId);
        Assert.Equal(7, ruta.ViajeId);
        Assert.Equal(resultado.PuntosRecibidos, ruta.PuntosRecibidos);
        Assert.Equal(resultado.PuntosDescartados, ruta.PuntosDescartados);
        Assert.Equal(resultado.DistanciaGpsKm, ruta.DistanciaGpsKm);
        Assert.Equal(resultado.TiempoMovimientoSeg, ruta.TiempoMovimientoSeg);
        Assert.Equal(resultado.TiempoDetenidoSeg, ruta.TiempoDetenidoSeg);
        Assert.Equal(resultado.VelocidadPromedioKmh, ruta.VelocidadPromedioKmh);
        Assert.Equal(resultado.VelocidadMaximaKmh, ruta.VelocidadMaximaKmh);
        Assert.Equal(resultado.Odometro.DistanciaOdometroKm, ruta.DistanciaOdometroKm);
        Assert.Equal(resultado.Odometro.DiferenciaKm, ruta.DiferenciaKm);
        Assert.Equal(resultado.Odometro.DiferenciaPorcentaje, ruta.DiferenciaPorcentaje);
        Assert.Equal(resultado.Odometro.Alerta, ruta.AlertaKilometraje);
        Assert.Equal(resultado.LitrosCargados, ruta.LitrosCargados);
        Assert.Equal(resultado.RendimientoKmPorLitro, ruta.RendimientoKmPorLitro);
        Assert.Equal(resultado.Polyline, ruta.Polyline);
        Assert.Equal(ProcesadorRuta.VersionAlgoritmo, ruta.VersionAlgoritmo);
        Assert.Equal(FechaProceso, ruta.FechaProceso);
    }

    [Theory]
    [InlineData(0, 7)]
    [InlineData(1, 0)]
    [InlineData(-1, 7)]
    public void Crear_IdsInvalidos_LanzaDomainException(int empresaId, int viajeId)
    {
        Assert.Throws<DomainException>(() => RutaProcesada.Crear(empresaId, viajeId, ResultadoDePrueba(), FechaProceso));
    }

    [Fact]
    public void Crear_SinFechaDeProceso_LanzaDomainException()
    {
        Assert.Throws<DomainException>(() => RutaProcesada.Crear(1, 7, ResultadoDePrueba(), default));
    }

    [Fact]
    public void Crear_ResultadoNulo_LanzaArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => RutaProcesada.Crear(1, 7, null!, FechaProceso));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Crear_SinVersionDeAlgoritmo_LanzaDomainException(string version)
    {
        var resultado = ResultadoDePrueba() with { VersionAlgoritmo = version };

        Assert.Throws<DomainException>(() => RutaProcesada.Crear(1, 7, resultado, FechaProceso));
    }

    [Fact]
    public void Crear_RutaSinPuntos_GuardaPolylineNula()
    {
        var resultado = ProcesadorRuta.Procesar(Array.Empty<PuntoGps>(), 1000m, null, 0m);

        var ruta = RutaProcesada.Crear(1, 7, resultado, FechaProceso);

        Assert.Null(ruta.Polyline);
        Assert.Equal(0m, ruta.DistanciaGpsKm);
        Assert.False(ruta.AlertaKilometraje);
    }

    [Fact]
    public void Actualizar_NuevoResultado_ReemplazaMetricasYConservaIds()
    {
        var ruta = RutaProcesada.Crear(1, 7, ResultadoDePrueba(), FechaProceso);
        var nuevo = ProcesadorRuta.Procesar(RutaDePrueba.LineaRecta(5), 1000m, 1010m, 0m);
        var nuevaFecha = FechaProceso.AddHours(1);

        ruta.Actualizar(nuevo, nuevaFecha);

        Assert.Equal(1, ruta.EmpresaId);
        Assert.Equal(7, ruta.ViajeId);
        Assert.Equal(5, ruta.PuntosRecibidos);
        Assert.Equal(nuevo.DistanciaGpsKm, ruta.DistanciaGpsKm);
        Assert.True(ruta.AlertaKilometraje);
        Assert.Null(ruta.RendimientoKmPorLitro);
        Assert.Equal(nuevaFecha, ruta.FechaProceso);
    }

    [Fact]
    public void Actualizar_ResultadoInvalido_NoModificaLaRuta()
    {
        var ruta = RutaProcesada.Crear(1, 7, ResultadoDePrueba(), FechaProceso);
        var distanciaOriginal = ruta.DistanciaGpsKm;

        Assert.Throws<DomainException>(() => ruta.Actualizar(ResultadoDePrueba(), default));

        Assert.Equal(distanciaOriginal, ruta.DistanciaGpsKm);
        Assert.Equal(FechaProceso, ruta.FechaProceso);
    }

    /// <summary>11 puntos en línea recta (≈2.78 km), odómetro de 2.8 km y 0.278 L cargados.</summary>
    private static ResultadoRuta ResultadoDePrueba()
        => ProcesadorRuta.Procesar(RutaDePrueba.LineaRecta(11), 15000m, 15002.80m, 0.278m);
}
