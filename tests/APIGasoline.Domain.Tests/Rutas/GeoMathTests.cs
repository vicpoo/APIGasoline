using APIGasoline.Domain.Rutas;

namespace APIGasoline.Domain.Tests.Rutas;

/// <summary>
/// Distancia entre coordenadas con la fórmula de Haversine (radio medio de la Tierra 6,371,008.8 m).
/// </summary>
public class GeoMathTests
{
    [Fact]
    public void DistanciaMetros_MismoPunto_EsCero()
    {
        Assert.Equal(0, GeoMath.DistanciaMetros(16.753056, -93.115556, 16.753056, -93.115556));
    }

    [Fact]
    public void DistanciaMetros_UnGradoDeLongitudEnElEcuador_Mide111195Metros()
    {
        var metros = GeoMath.DistanciaMetros(0, 0, 0, 1);

        Assert.InRange(metros, 111_194, 111_196);
    }

    [Fact]
    public void DistanciaMetros_TuxtlaTapachula_MideUnos225KmEnLineaRecta()
    {
        var metros = GeoMath.DistanciaMetros(16.753056, -93.115556, 14.905556, -92.263056);

        Assert.InRange(metros, 224_700, 224_830);
    }

    [Fact]
    public void DistanciaMetros_EsSimetrica()
    {
        var ida = GeoMath.DistanciaMetros(16.75, -93.11, 14.90, -92.26);
        var vuelta = GeoMath.DistanciaMetros(14.90, -92.26, 16.75, -93.11);

        Assert.Equal(ida, vuelta, 6);
    }

    [Fact]
    public void DistanciaMetros_CruzandoElAntimeridiano_TomaElCaminoCorto()
    {
        // 179.5 E a 179.5 W son solo 1 grado de distancia, no 359
        var metros = GeoMath.DistanciaMetros(0, 179.5, 0, -179.5);

        Assert.InRange(metros, 111_194, 111_196);
    }

    [Fact]
    public void DistanciaMetros_ConPuntosGps_UsaSusCoordenadas()
    {
        var a = new PuntoGps(0, 0, new DateTime(2026, 10, 7, 8, 0, 0, DateTimeKind.Utc));
        var b = a with { Longitud = 1 };

        Assert.InRange(GeoMath.DistanciaMetros(a, b), 111_194, 111_196);
    }
}
