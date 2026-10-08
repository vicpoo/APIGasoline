namespace APIGasoline.Domain.Rutas;

/// <summary>
/// Cálculos geográficos básicos.
/// </summary>
public static class GeoMath
{
    /// <summary>Radio medio de la Tierra en metros (IUGG).</summary>
    public const double RadioTierraMetros = 6_371_008.8;

    /// <summary>
    /// Distancia sobre la superficie terrestre entre dos coordenadas (fórmula de Haversine), en metros.
    /// </summary>
    public static double DistanciaMetros(double latitud1, double longitud1, double latitud2, double longitud2)
    {
        var lat1 = ARadianes(latitud1);
        var lat2 = ARadianes(latitud2);
        var deltaLat = lat2 - lat1;
        var deltaLng = ARadianes(longitud2 - longitud1);

        var h = Math.Sin(deltaLat / 2) * Math.Sin(deltaLat / 2)
              + Math.Cos(lat1) * Math.Cos(lat2) * Math.Sin(deltaLng / 2) * Math.Sin(deltaLng / 2);

        // Math.Min evita un NaN por redondeo cuando h queda en 1.0000000001
        return 2 * RadioTierraMetros * Math.Asin(Math.Min(1, Math.Sqrt(h)));
    }

    public static double DistanciaMetros(PuntoGps a, PuntoGps b)
        => DistanciaMetros(a.Latitud, a.Longitud, b.Latitud, b.Longitud);

    public static double ARadianes(double grados) => grados * Math.PI / 180;
}
