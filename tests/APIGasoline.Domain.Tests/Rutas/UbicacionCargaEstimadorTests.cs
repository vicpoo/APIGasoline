using APIGasoline.Domain.Entities;
using APIGasoline.Domain.Rutas;

namespace APIGasoline.Domain.Tests.Rutas;

/// <summary>
/// Una carga que llegó sin ubicación se ubica con el punto GPS del viaje más cercano a su hora
/// (dentro de 10 minutos), prefiriendo puntos con buena precisión.
/// </summary>
public class UbicacionCargaEstimadorTests
{
    private static readonly DateTime Carga = new(2026, 11, 4, 18, 18, 0);

    private static PuntoRuta Punto(decimal lat, decimal lng, int segundosDesdeLaCarga, decimal? precision = 5m)
        => PuntoRuta.Crear(Id(1), Id(2), lat, lng, Carga.AddSeconds(segundosDesdeLaCarga), precision);

    [Fact]
    public void Estimar_TomaElPuntoMasCercanoEnElTiempo()
    {
        var puntos = new[]
        {
            Punto(16.75m, -93.10m, -300),
            Punto(16.76m, -93.11m, -20),   // llegó a la gasolinera
            Punto(16.77m, -93.12m, 240),
        };

        var ubicacion = UbicacionCargaEstimador.Estimar(puntos, Carga);

        Assert.Equal((16.76m, -93.11m), ubicacion!.Value);
    }

    [Fact]
    public void Estimar_SinPuntosDentroDeLaTolerancia_RegresaNull()
    {
        var puntos = new[] { Punto(16.75m, -93.10m, -11 * 60), Punto(16.77m, -93.12m, 15 * 60) };

        Assert.Null(UbicacionCargaEstimador.Estimar(puntos, Carga));
    }

    [Fact]
    public void Estimar_SinPuntos_RegresaNull()
        => Assert.Null(UbicacionCargaEstimador.Estimar([], Carga));

    [Fact]
    public void Estimar_PrefierePuntosConBuenaPrecision()
    {
        var puntos = new[]
        {
            Punto(16.70m, -93.00m, 5, precision: 120m),   // más cerca en el tiempo pero impreciso
            Punto(16.76m, -93.11m, 90, precision: 8m),
        };

        Assert.Equal((16.76m, -93.11m), UbicacionCargaEstimador.Estimar(puntos, Carga)!.Value);
    }

    [Fact]
    public void Estimar_SoloHayPuntosImprecisos_UsaElMasCercano()
    {
        var puntos = new[]
        {
            Punto(16.70m, -93.00m, 5, precision: 120m),
            Punto(16.80m, -93.20m, 200, precision: 90m),
        };

        Assert.Equal((16.70m, -93.00m), UbicacionCargaEstimador.Estimar(puntos, Carga)!.Value);
    }

    [Fact]
    public void CompletarUbicacion_NoCambiaLaQueMandoElTelefono()
    {
        var carga = CargaCombustible.Crear(Id(1), Id(2), 40m, 900m, "comprobante.jpg", Carga, 16.1m, -93.1m);

        Assert.False(carga.CompletarUbicacion(16.9m, -93.9m));
        Assert.Equal(16.1m, carga.UbicacionLat);
        Assert.Equal(-93.1m, carga.UbicacionLng);
    }

    [Fact]
    public void CompletarUbicacion_SinUbicacion_LaAsigna()
    {
        var carga = CargaCombustible.Crear(Id(1), Id(2), 40m, 900m, "comprobante.jpg", Carga, null, null);

        Assert.True(carga.CompletarUbicacion(16.76m, -93.11m));
        Assert.Equal(16.76m, carga.UbicacionLat);
        Assert.Equal(-93.11m, carga.UbicacionLng);
    }
}
