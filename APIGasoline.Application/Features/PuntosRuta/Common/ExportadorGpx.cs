using System.Globalization;
using System.Text;
using System.Xml;
using APIGasoline.Application.Features.PuntosRuta.DTOs;

namespace APIGasoline.Application.Features.PuntosRuta.Common;

/// <summary>
/// Convierte la ruta cruda de un viaje a GPX 1.1, para compararla con otra app (Strava, GPX Studio,
/// Google Earth) y ver tramo por tramo dónde se pierde o se gana distancia.
/// Es pura: la zona horaria entra como parámetro (GPX pide la hora en UTC y la BD la guarda en hora local).
/// </summary>
public static class ExportadorGpx
{
    private const string Namespace = "http://www.topografix.com/GPX/1/1";

    public static byte[] Generar(Guid viajeId, IEnumerable<PuntoRutaDto> puntos, TimeZoneInfo? zonaServidor = null)
    {
        ArgumentNullException.ThrowIfNull(puntos);
        var zona = zonaServidor ?? TimeZoneInfo.Local;

        var settings = new XmlWriterSettings
        {
            Encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
            Indent = true
        };

        using var stream = new MemoryStream();
        using (var xml = XmlWriter.Create(stream, settings))
        {
            xml.WriteStartDocument();
            xml.WriteStartElement("gpx", Namespace);
            xml.WriteAttributeString("version", "1.1");
            xml.WriteAttributeString("creator", "APIGasoline");

            xml.WriteStartElement("trk", Namespace);
            xml.WriteElementString("name", Namespace, $"Viaje {viajeId}");
            xml.WriteStartElement("trkseg", Namespace);

            foreach (var punto in puntos.OrderBy(p => p.TimestampCaptura))
            {
                xml.WriteStartElement("trkpt", Namespace);
                xml.WriteAttributeString("lat", punto.Latitud.ToString(CultureInfo.InvariantCulture));
                xml.WriteAttributeString("lon", punto.Longitud.ToString(CultureInfo.InvariantCulture));

                if (punto.Altitud is { } altitud)
                    xml.WriteElementString("ele", Namespace, altitud.ToString(CultureInfo.InvariantCulture));

                xml.WriteElementString("time", Namespace, AUtc(punto.TimestampCaptura, zona).ToString("yyyy-MM-dd'T'HH:mm:ss'Z'", CultureInfo.InvariantCulture));

                if (punto.PrecisionMetros is { } precision)
                    xml.WriteElementString("hdop", Namespace, (precision / 5m).ToString("0.##", CultureInfo.InvariantCulture));

                xml.WriteEndElement();
            }

            xml.WriteEndElement(); // trkseg
            xml.WriteEndElement(); // trk
            xml.WriteEndElement(); // gpx
            xml.WriteEndDocument();
        }

        return stream.ToArray();
    }

    /// <summary>La BD guarda la hora local del servidor (sin zona); GPX la pide en UTC.</summary>
    private static DateTime AUtc(DateTime fecha, TimeZoneInfo zona) => fecha.Kind switch
    {
        DateTimeKind.Utc => fecha,
        DateTimeKind.Local => fecha.ToUniversalTime(),
        _ => TimeZoneInfo.ConvertTimeToUtc(fecha, zona)
    };
}
