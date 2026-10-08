using APIGasoline.Domain.Rutas;

namespace APIGasoline.Domain.Tests.Rutas;

/// <summary>
/// Simplificación Douglas-Peucker para el mapa: los puntos que se desvían menos que la tolerancia
/// se eliminan; el primero, el último y las curvas se conservan.
/// </summary>
public class SimplificadorRutaTests
{
    private const double Tolerancia = 5;

    [Fact]
    public void Simplificar_LineaRecta_QuedaSoloInicioYFin()
    {
        var puntos = RutaDePrueba.LineaRecta(10);

        var simplificada = SimplificadorRuta.Simplificar(puntos, Tolerancia);

        Assert.Equal(new[] { puntos[0], puntos[9] }, simplificada);
    }

    [Fact]
    public void Simplificar_RutaEnL_ConservaLaEsquina()
    {
        // 4 puntos al norte y luego 4 al este
        var norte = RutaDePrueba.LineaRecta(5);
        var esquina = norte[^1];
        var este = Enumerable.Range(1, 4)
            .Select(i => esquina with { Longitud = esquina.Longitud + i * 0.0025, Timestamp = esquina.Timestamp.AddSeconds(30 * i) });
        var puntos = norte.Concat(este).ToList();

        var simplificada = SimplificadorRuta.Simplificar(puntos, Tolerancia);

        Assert.Equal(new[] { puntos[0], esquina, puntos[^1] }, simplificada);
    }

    [Fact]
    public void Simplificar_DesviacionMenorALaTolerancia_SeElimina()
    {
        // El punto de en medio se desvía ~1 m de la recta
        var puntos = new List<PuntoGps> { RutaDePrueba.Punto(0), RutaDePrueba.Punto(1, desplazamientoLongitud: 0.00001), RutaDePrueba.Punto(2) };

        var simplificada = SimplificadorRuta.Simplificar(puntos, Tolerancia);

        Assert.Equal(2, simplificada.Count);
    }

    [Fact]
    public void Simplificar_DesviacionMayorALaTolerancia_SeConserva()
    {
        // El punto de en medio se desvía ~21 m de la recta
        var puntos = new List<PuntoGps> { RutaDePrueba.Punto(0), RutaDePrueba.Punto(1, desplazamientoLongitud: 0.0002), RutaDePrueba.Punto(2) };

        var simplificada = SimplificadorRuta.Simplificar(puntos, Tolerancia);

        Assert.Equal(3, simplificada.Count);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void Simplificar_MenosDeTresPuntos_DevuelveLosMismos(int cantidad)
    {
        var simplificada = SimplificadorRuta.Simplificar(RutaDePrueba.LineaRecta(cantidad), Tolerancia);

        Assert.Equal(cantidad, simplificada.Count);
    }

    [Fact]
    public void Simplificar_RutaLarga_NoDesbordaLaPila()
    {
        // 20,000 puntos en zigzag: la versión iterativa no debe fallar
        var puntos = Enumerable.Range(0, 20_000)
            .Select(i => RutaDePrueba.Punto(i, desplazamientoLongitud: i % 2 == 0 ? 0 : 0.001))
            .ToList();

        var simplificada = SimplificadorRuta.Simplificar(puntos, Tolerancia);

        Assert.Equal(puntos.Count, simplificada.Count);
    }

    [Fact]
    public void Simplificar_ToleranciaNegativa_LanzaExcepcion()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => SimplificadorRuta.Simplificar(RutaDePrueba.LineaRecta(3), -1));
    }
}
