using System.Text;
using System.Xml.Linq;
using APIGasoline.Application.Features.PuntosRuta.Common;
using APIGasoline.Application.Features.PuntosRuta.DTOs;

namespace APIGasoline.Application.Tests.Features.PuntosRuta;

/// <summary>GPX de la ruta para compararla con Strava u otra app.</summary>
public class ExportadorGpxTests
{
    private static readonly XNamespace Gpx = "http://www.topografix.com/GPX/1/1";
    private static readonly TimeZoneInfo Mexico = TimeZoneInfo.CreateCustomTimeZone("CST", TimeSpan.FromHours(-6), "CST", "CST");

    [Fact]
    public void Generar_PuntosDesordenados_SalenEnOrdenConHoraUtc()
    {
        var puntos = new[]
        {
            Punto(16.7530m, -93.1150m, new DateTime(2026, 11, 4, 17, 40, 5), altitud: 530.5m),
            Punto(16.7528m, -93.1152m, new DateTime(2026, 11, 4, 17, 40, 0), altitud: 530m),
        };

        var doc = XDocument.Parse(Encoding.UTF8.GetString(ExportadorGpx.Generar(Id(1), puntos, Mexico)));
        var trkpts = doc.Descendants(Gpx + "trkpt").ToList();

        Assert.Equal(2, trkpts.Count);
        Assert.Equal("16.7528", trkpts[0].Attribute("lat")!.Value);
        Assert.Equal("-93.1152", trkpts[0].Attribute("lon")!.Value);
        Assert.Equal("2026-11-04T23:40:00Z", trkpts[0].Element(Gpx + "time")!.Value);
        Assert.Equal("530.5", trkpts[1].Element(Gpx + "ele")!.Value);
    }

    [Fact]
    public void Generar_SinPuntos_EsUnGpxValidoVacio()
    {
        var doc = XDocument.Parse(Encoding.UTF8.GetString(ExportadorGpx.Generar(Id(1), [], Mexico)));

        Assert.Equal("1.1", doc.Root!.Attribute("version")!.Value);
        Assert.Empty(doc.Descendants(Gpx + "trkpt"));
    }

    private static PuntoRutaDto Punto(decimal lat, decimal lng, DateTime hora, decimal? altitud = null)
        => new(Guid.NewGuid(), Id(1), Id(1), lat, lng, hora, null, 5m, altitud, null);
}
