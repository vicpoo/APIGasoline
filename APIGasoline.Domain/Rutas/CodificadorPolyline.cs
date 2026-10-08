using System.Text;

namespace APIGasoline.Domain.Rutas;

/// <summary>
/// Codifica / decodifica rutas en el formato "Encoded Polyline" de Google (precisión 5 decimales).
/// Es el formato estándar que leen Google Maps, Leaflet, Mapbox y la mayoría de librerías de mapas.
/// https://developers.google.com/maps/documentation/utilities/polylinealgorithm
/// </summary>
public static class CodificadorPolyline
{
    private const double Factor = 1e5;

    public static string Codificar(IEnumerable<(double Latitud, double Longitud)> puntos)
    {
        ArgumentNullException.ThrowIfNull(puntos);

        var resultado = new StringBuilder();
        long latitudAnterior = 0, longitudAnterior = 0;

        foreach (var (latitud, longitud) in puntos)
        {
            var lat = (long)Math.Round(latitud * Factor, MidpointRounding.AwayFromZero);
            var lng = (long)Math.Round(longitud * Factor, MidpointRounding.AwayFromZero);

            // Se codifica la diferencia contra el punto anterior (así la cadena es más corta)
            CodificarValor(lat - latitudAnterior, resultado);
            CodificarValor(lng - longitudAnterior, resultado);

            latitudAnterior = lat;
            longitudAnterior = lng;
        }

        return resultado.ToString();
    }

    public static IReadOnlyList<(double Latitud, double Longitud)> Decodificar(string polyline)
    {
        ArgumentNullException.ThrowIfNull(polyline);

        var puntos = new List<(double, double)>();
        var indice = 0;
        long lat = 0, lng = 0;

        while (indice < polyline.Length)
        {
            lat += DecodificarValor(polyline, ref indice);
            lng += DecodificarValor(polyline, ref indice);
            puntos.Add((lat / Factor, lng / Factor));
        }

        return puntos;
    }

    private static void CodificarValor(long valor, StringBuilder destino)
    {
        // Zigzag: el signo va en el bit menos significativo
        valor = valor < 0 ? ~(valor << 1) : valor << 1;

        // Bloques de 5 bits; 0x20 indica que sigue otro bloque
        while (valor >= 0x20)
        {
            destino.Append((char)((0x20 | (valor & 0x1f)) + 63));
            valor >>= 5;
        }

        destino.Append((char)(valor + 63));
    }

    private static long DecodificarValor(string polyline, ref int indice)
    {
        long resultado = 0;
        var desplazamiento = 0;
        long bloque;

        do
        {
            if (indice >= polyline.Length)
                throw new FormatException("La polyline está incompleta.");

            bloque = polyline[indice++] - 63;
            resultado |= (bloque & 0x1f) << desplazamiento;
            desplazamiento += 5;
        }
        while (bloque >= 0x20);

        return (resultado & 1) != 0 ? ~(resultado >> 1) : resultado >> 1;
    }
}
