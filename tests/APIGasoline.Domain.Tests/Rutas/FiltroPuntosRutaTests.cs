using APIGasoline.Domain.Rutas;

namespace APIGasoline.Domain.Tests.Rutas;

/// <summary>
/// Limpieza de puntos crudos: precisión mayor a 50 m, duplicados (misma hora) y saltos que implican
/// más de 200 km/h se descartan; el resultado queda en orden cronológico.
/// </summary>
public class FiltroPuntosRutaTests
{
    private static readonly ParametrosRuta Parametros = ParametrosRuta.PorDefecto;

    [Fact]
    public void Filtrar_RutaLimpia_NoDescartaNada()
    {
        var resultado = FiltroPuntosRuta.Filtrar(RutaDePrueba.LineaRecta(5), Parametros);

        Assert.Equal(5, resultado.Validos.Count);
        Assert.Equal(0, resultado.Descartados);
    }

    [Fact]
    public void Filtrar_SinPuntos_DevuelveVacio()
    {
        var resultado = FiltroPuntosRuta.Filtrar([], Parametros);

        Assert.Empty(resultado.Validos);
        Assert.Equal(0, resultado.Descartados);
    }

    // --- Precisión ---

    [Theory]
    [InlineData(80, 2)]    // peor que 50 m: se descarta
    [InlineData(50, 3)]    // exactamente 50 m: se acepta
    [InlineData(5, 3)]
    public void Filtrar_PrecisionDelPuntoIntermedio_DecideSiSeConserva(double precision, int validosEsperados)
    {
        var puntos = new List<PuntoGps>
        {
            RutaDePrueba.Punto(0),
            RutaDePrueba.Punto(1, precision: precision),
            RutaDePrueba.Punto(2)
        };

        var resultado = FiltroPuntosRuta.Filtrar(puntos, Parametros);

        Assert.Equal(validosEsperados, resultado.Validos.Count);
    }

    [Fact]
    public void Filtrar_SinPrecisionReportada_SeAcepta()
    {
        var resultado = FiltroPuntosRuta.Filtrar(RutaDePrueba.LineaRecta(3, precision: null), Parametros);

        Assert.Equal(3, resultado.Validos.Count);
    }

    // --- Saltos imposibles ---

    [Fact]
    public void Filtrar_SaltoImposible_SeDescartaYLaRutaContinua()
    {
        var puntos = RutaDePrueba.LineaRecta(5);
        // El punto 2 "brinca" 0.5° (~55 km) en 30 s: más de 6,000 km/h
        puntos[2] = puntos[2] with { Latitud = puntos[2].Latitud + 0.5 };

        var resultado = FiltroPuntosRuta.Filtrar(puntos, Parametros);

        Assert.Equal(1, resultado.Descartados);
        Assert.DoesNotContain(puntos[2], resultado.Validos);
        Assert.Contains(puntos[3], resultado.Validos);  // el siguiente punto normal sí se acepta
    }

    [Fact]
    public void Filtrar_VelocidadAltaPeroPosible_SeConserva()
    {
        // 1.5 km en 30 s = 180 km/h: rápido, pero por debajo del límite de 200 km/h
        var puntos = new List<PuntoGps>
        {
            RutaDePrueba.Punto(0),
            new(RutaDePrueba.LatitudInicio + 0.01349, RutaDePrueba.LongitudInicio, RutaDePrueba.Inicio.AddSeconds(30))
        };

        var resultado = FiltroPuntosRuta.Filtrar(puntos, Parametros);

        Assert.Equal(0, resultado.Descartados);
    }

    // --- Duplicados y orden ---

    [Fact]
    public void Filtrar_MismaHora_ConservaSoloElPrimero()
    {
        var original = RutaDePrueba.Punto(1);
        var repetido = original with { Longitud = original.Longitud + 0.0001 };
        var puntos = new List<PuntoGps> { RutaDePrueba.Punto(0), original, repetido };

        var resultado = FiltroPuntosRuta.Filtrar(puntos, Parametros);

        Assert.Equal(1, resultado.Descartados);
        Assert.Contains(original, resultado.Validos);
        Assert.DoesNotContain(repetido, resultado.Validos);
    }

    [Fact]
    public void Filtrar_PuntosDesordenados_LosDevuelveEnOrdenCronologico()
    {
        var puntos = new List<PuntoGps> { RutaDePrueba.Punto(2), RutaDePrueba.Punto(0), RutaDePrueba.Punto(1) };

        var resultado = FiltroPuntosRuta.Filtrar(puntos, Parametros);

        Assert.Equal(
            new[] { RutaDePrueba.Punto(0), RutaDePrueba.Punto(1), RutaDePrueba.Punto(2) },
            resultado.Validos);
    }

    [Fact]
    public void Filtrar_ParametrosMasEstrictos_DescartaMas()
    {
        var puntos = RutaDePrueba.LineaRecta(3, precision: 20);
        var estrictos = ParametrosRuta.PorDefecto with { PrecisionMaximaMetros = 10 };

        var resultado = FiltroPuntosRuta.Filtrar(puntos, estrictos);

        Assert.Equal(3, resultado.Descartados);
    }
}
