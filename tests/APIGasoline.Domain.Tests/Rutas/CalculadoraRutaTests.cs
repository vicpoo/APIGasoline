using APIGasoline.Domain.Rutas;

namespace APIGasoline.Domain.Tests.Rutas;

/// <summary>
/// Métricas de una ruta limpia: un tramo cuenta como movimiento a partir de 3 km/h; los tramos detenidos
/// suman tiempo detenido pero no distancia (el GPS "tiembla" aunque el carro esté parado).
/// </summary>
public class CalculadoraRutaTests
{
    private static readonly ParametrosRuta Parametros = ParametrosRuta.PorDefecto;

    [Fact]
    public void Calcular_LineaRectaDeCincoPuntos_SumaCuatroTramos()
    {
        var metricas = CalculadoraRuta.Calcular(RutaDePrueba.LineaRecta(5), Parametros);

        Assert.InRange(metricas.DistanciaMetros, 4 * RutaDePrueba.MetrosPorPaso - 1, 4 * RutaDePrueba.MetrosPorPaso + 1);
        Assert.Equal(120, metricas.TiempoMovimientoSeg);
        Assert.Equal(0, metricas.TiempoDetenidoSeg);
    }

    [Fact]
    public void Calcular_VelocidadConstante_PromedioYMaximaIguales()
    {
        var metricas = CalculadoraRuta.Calcular(RutaDePrueba.LineaRecta(5), Parametros);

        Assert.InRange(metricas.VelocidadPromedioKmh!.Value, 33.3, 33.4);
        Assert.InRange(metricas.VelocidadMaximaKmh!.Value, 33.3, 33.4);
    }

    [Fact]
    public void Calcular_CarroDetenido_SumaTiempoDetenidoSinDistancia()
    {
        var parado = RutaDePrueba.Punto(0);
        var puntos = new List<PuntoGps> { parado, parado with { Timestamp = parado.Timestamp.AddSeconds(60) } };

        var metricas = CalculadoraRuta.Calcular(puntos, Parametros);

        Assert.Equal(0, metricas.DistanciaMetros);
        Assert.Equal(60, metricas.TiempoDetenidoSeg);
        Assert.Equal(0, metricas.TiempoMovimientoSeg);
    }

    [Fact]
    public void Calcular_GpsTemblandoConElCarroParado_NoSumaDistancia()
    {
        // 5 m en 60 s = 0.3 km/h: es ruido del GPS, no movimiento
        var a = RutaDePrueba.Punto(0);
        var b = a with { Longitud = a.Longitud + 0.000047, Timestamp = a.Timestamp.AddSeconds(60) };

        var metricas = CalculadoraRuta.Calcular([a, b], Parametros);

        Assert.Equal(0, metricas.DistanciaMetros);
        Assert.Equal(60, metricas.TiempoDetenidoSeg);
    }

    [Fact]
    public void Calcular_ViajeConUnaParada_SeparaMovimientoYDetenido()
    {
        var puntos = RutaDePrueba.LineaRecta(3);
        var ultimo = puntos[^1];
        puntos.Add(ultimo with { Timestamp = ultimo.Timestamp.AddMinutes(5) });  // 5 min parado en un semáforo largo

        var metricas = CalculadoraRuta.Calcular(puntos, Parametros);

        Assert.Equal(60, metricas.TiempoMovimientoSeg);
        Assert.Equal(300, metricas.TiempoDetenidoSeg);
    }

    [Fact]
    public void Calcular_TramosConDistintaVelocidad_TomaLaMaxima()
    {
        var puntos = new List<PuntoGps>
        {
            RutaDePrueba.Punto(0),
            RutaDePrueba.Punto(1),                                                    // 33 km/h
            RutaDePrueba.Punto(3) with { Timestamp = RutaDePrueba.Inicio.AddSeconds(60) }  // 2 pasos en 30 s = 67 km/h
        };

        var metricas = CalculadoraRuta.Calcular(puntos, Parametros);

        Assert.InRange(metricas.VelocidadMaximaKmh!.Value, 66.6, 66.8);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public void Calcular_MenosDeDosPuntos_TodoEnCeroYSinVelocidades(int cantidad)
    {
        var metricas = CalculadoraRuta.Calcular(RutaDePrueba.LineaRecta(cantidad), Parametros);

        Assert.Equal(0, metricas.DistanciaMetros);
        Assert.Null(metricas.VelocidadPromedioKmh);
        Assert.Null(metricas.VelocidadMaximaKmh);
    }
}
