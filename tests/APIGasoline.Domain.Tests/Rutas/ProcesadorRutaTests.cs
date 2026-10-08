using APIGasoline.Domain.Rutas;

namespace APIGasoline.Domain.Tests.Rutas;

/// <summary>
/// Pipeline completo de una ruta: limpieza → métricas → odómetro → rendimiento → polyline.
/// </summary>
public class ProcesadorRutaTests
{
    private const decimal KmInicial = 15480m;

    [Fact]
    public void Procesar_RutaConBasura_CuentaRecibidosYDescartados()
    {
        var resultado = ProcesadorRuta.Procesar(ConstruirRutaConBasura(), KmInicial, null, 0m);

        Assert.Equal(12, resultado.PuntosRecibidos);
        Assert.Equal(2, resultado.PuntosDescartados);  // 1 salto imposible + 1 con mala precisión
    }

    [Fact]
    public void Procesar_RutaConBasura_LaDistanciaIgnoraLosPuntosDescartados()
    {
        var resultado = ProcesadorRuta.Procesar(ConstruirRutaConBasura(), KmInicial, null, 0m);

        // 10 puntos válidos en línea recta = 9 pasos de 277.99 m
        Assert.InRange(resultado.DistanciaGpsKm, 2.500m, 2.504m);
    }

    [Fact]
    public void Procesar_ConKilometrajeYLitros_CalculaOdometroYRendimiento()
    {
        var resultado = ProcesadorRuta.Procesar(RutaDePrueba.LineaRecta(10), KmInicial, KmInicial + 2.5m, 0.25m);

        Assert.False(resultado.Odometro.Alerta);
        Assert.InRange(resultado.RendimientoKmPorLitro!.Value, 10.0m, 10.02m);
        Assert.Equal(0.25m, resultado.LitrosCargados);
    }

    [Fact]
    public void Procesar_RutaRecta_LaPolylineTieneSoloInicioYFin()
    {
        var puntos = RutaDePrueba.LineaRecta(10);

        var resultado = ProcesadorRuta.Procesar(puntos, KmInicial, null, 0m);
        var mapa = CodificadorPolyline.Decodificar(resultado.Polyline);

        Assert.Equal(2, mapa.Count);
        Assert.Equal(Math.Round(puntos[0].Latitud, 5), mapa[0].Latitud);
        Assert.Equal(Math.Round(puntos[^1].Latitud, 5), mapa[^1].Latitud);
    }

    [Fact]
    public void Procesar_SinPuntos_ResultadoVacioSinErrores()
    {
        var resultado = ProcesadorRuta.Procesar([], KmInicial, null, 0m);

        Assert.Equal(0, resultado.PuntosRecibidos);
        Assert.Equal(0m, resultado.DistanciaGpsKm);
        Assert.Equal(string.Empty, resultado.Polyline);
        Assert.Null(resultado.RendimientoKmPorLitro);
    }

    [Fact]
    public void Procesar_IndicaLaVersionDelAlgoritmo()
    {
        var resultado = ProcesadorRuta.Procesar(RutaDePrueba.LineaRecta(3), KmInicial, null, 0m);

        Assert.Equal(ProcesadorRuta.VersionAlgoritmo, resultado.VersionAlgoritmo);
    }

    [Fact]
    public void Procesar_MismosPuntos_DaElMismoResultado()
    {
        var puntos = ConstruirRutaConBasura();

        var primero = ProcesadorRuta.Procesar(puntos, KmInicial, KmInicial + 3m, 1m);
        var segundo = ProcesadorRuta.Procesar(puntos, KmInicial, KmInicial + 3m, 1m);

        Assert.Equal(primero, segundo);
    }

    // 10 puntos válidos en línea recta + 1 salto imposible + 1 punto con precisión de 120 m
    private static List<PuntoGps> ConstruirRutaConBasura()
    {
        var puntos = RutaDePrueba.LineaRecta(10);
        puntos.Add(new PuntoGps(17.5, -93.11, RutaDePrueba.Inicio.AddSeconds(45)));
        puntos.Add(RutaDePrueba.Punto(4) with { Timestamp = RutaDePrueba.Inicio.AddSeconds(135), PrecisionMetros = 120 });
        return puntos;
    }
}
