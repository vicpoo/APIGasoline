using APIGasoline.Domain.Rutas;

namespace APIGasoline.Domain.Tests.Rutas;

/// <summary>
/// Datos de prueba compartidos por las pruebas de rutas.
/// Un "paso" de 0.0025° de latitud mide 277.99 m; cada 30 s equivale a 33.36 km/h.
/// </summary>
internal static class RutaDePrueba
{
    public static readonly DateTime Inicio = new(2026, 10, 7, 8, 30, 0, DateTimeKind.Utc);
    public const double LatitudInicio = 16.75;
    public const double LongitudInicio = -93.11;
    public const double Paso = 0.0025;
    public const double MetrosPorPaso = 277.99;

    /// <summary>Ruta en línea recta hacia el norte: un punto cada 30 s.</summary>
    public static List<PuntoGps> LineaRecta(int cantidad, double? precision = null)
        => Enumerable.Range(0, cantidad)
            .Select(i => Punto(i, precision: precision))
            .ToList();

    /// <summary>Punto n de la línea recta (n pasos al norte, n * 30 s después del inicio).</summary>
    public static PuntoGps Punto(int n, double desplazamientoLongitud = 0, double? precision = null)
        => new(LatitudInicio + n * Paso, LongitudInicio + desplazamientoLongitud, Inicio.AddSeconds(30 * n), precision);
}
