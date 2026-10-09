using APIGasoline.Application.Features.RutasProcesadas.Common;

namespace APIGasoline.Application.Tests.Features.RutasProcesadas.Common;

public class MapaRutaTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Coordenadas_SinPolyline_DevuelveListaVacia(string? polyline)
    {
        Assert.Empty(MapaRuta.Coordenadas(polyline));
    }

    [Fact]
    public void Coordenadas_EjemploDeGoogle_DecodificaLosTresPuntos()
    {
        var coordenadas = MapaRuta.Coordenadas("_p~iF~ps|U_ulLnnqC_mqNvxq`@");

        Assert.Equal(3, coordenadas.Count);
        Assert.Equal(38.5, coordenadas[0].Latitud, 5);
        Assert.Equal(-120.2, coordenadas[0].Longitud, 5);
        Assert.Equal(43.252, coordenadas[2].Latitud, 5);
        Assert.Equal(-126.453, coordenadas[2].Longitud, 5);
    }
}
