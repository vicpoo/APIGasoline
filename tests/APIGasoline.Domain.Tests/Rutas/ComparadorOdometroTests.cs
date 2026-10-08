using APIGasoline.Domain.Rutas;

namespace APIGasoline.Domain.Tests.Rutas;

/// <summary>
/// GPS contra odómetro: hay alerta si |odómetro - GPS| supera el 10 % de la distancia GPS Y además 2 km.
/// La alerta aplica en ambos sentidos (km inflados o reducidos).
/// </summary>
public class ComparadorOdometroTests
{
    private const decimal KmInicial = 15000m;
    private static readonly ParametrosRuta Parametros = ParametrosRuta.PorDefecto;

    [Fact]
    public void Comparar_KilometrajeHonesto_SinAlerta()
    {
        var comparacion = Comparar(gpsKm: 100m, odometroKm: 102m);

        Assert.Equal(102m, comparacion.DistanciaOdometroKm);
        Assert.Equal(2m, comparacion.DiferenciaKm);
        Assert.Equal(2m, comparacion.DiferenciaPorcentaje);
        Assert.False(comparacion.Alerta);
    }

    [Fact]
    public void Comparar_KilometrajeInflado_GeneraAlerta()
    {
        var comparacion = Comparar(gpsKm: 120m, odometroKm: 300m);

        Assert.Equal(180m, comparacion.DiferenciaKm);
        Assert.Equal(150m, comparacion.DiferenciaPorcentaje);
        Assert.True(comparacion.Alerta);
    }

    [Fact]
    public void Comparar_KilometrajeReducido_TambienGeneraAlerta()
    {
        var comparacion = Comparar(gpsKm: 100m, odometroKm: 80m);

        Assert.Equal(-20m, comparacion.DiferenciaKm);
        Assert.True(comparacion.Alerta);
    }

    [Theory]
    [InlineData(5, 6.5)]      // 30 % pero solo 1.5 km: viaje corto, se ignora
    [InlineData(100, 105)]    // 5 km pero solo 5 %: dentro de la tolerancia
    [InlineData(100, 110)]    // exactamente 10 %: el límite no genera alerta
    public void Comparar_SoloSuperaUnUmbral_SinAlerta(decimal gpsKm, decimal odometroKm)
    {
        Assert.False(Comparar(gpsKm, odometroKm).Alerta);
    }

    [Fact]
    public void Comparar_SinKilometrajeFinal_NoHayComparacion()
    {
        var comparacion = ComparadorOdometro.Comparar(50m, KmInicial, null, Parametros);

        Assert.Null(comparacion.DistanciaOdometroKm);
        Assert.Null(comparacion.DiferenciaKm);
        Assert.Null(comparacion.DiferenciaPorcentaje);
        Assert.False(comparacion.Alerta);
    }

    [Fact]
    public void Comparar_GpsSinDistanciaYOdometroConKm_AlertaSinPorcentaje()
    {
        // El GPS no registró movimiento pero se declararon 50 km
        var comparacion = Comparar(gpsKm: 0m, odometroKm: 50m);

        Assert.Null(comparacion.DiferenciaPorcentaje);
        Assert.True(comparacion.Alerta);
    }

    [Fact]
    public void Comparar_PorcentajeGigante_SeLimitaAlMaximoDeLaColumna()
    {
        var comparacion = Comparar(gpsKm: 0.001m, odometroKm: 500m);

        Assert.Equal(ComparadorOdometro.PorcentajeMaximo, comparacion.DiferenciaPorcentaje);
    }

    private static ComparacionOdometro Comparar(decimal gpsKm, decimal odometroKm)
        => ComparadorOdometro.Comparar(gpsKm, KmInicial, KmInicial + odometroKm, Parametros);
}
