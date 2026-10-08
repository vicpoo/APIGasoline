namespace APIGasoline.Domain.Rutas;

/// <summary>
/// Simplifica una ruta con el algoritmo de Douglas-Peucker: elimina los puntos que se desvían
/// menos que la tolerancia de la línea entre sus vecinos. Un tramo recto de cientos de puntos
/// queda en 2, y las curvas se conservan. Se usa solo para el mapa (la polyline), no para medir.
/// </summary>
public static class SimplificadorRuta
{
    public static IReadOnlyList<PuntoGps> Simplificar(IReadOnlyList<PuntoGps> puntos, double toleranciaMetros)
    {
        ArgumentNullException.ThrowIfNull(puntos);
        ArgumentOutOfRangeException.ThrowIfNegative(toleranciaMetros);

        if (puntos.Count < 3)
            return puntos.ToList();

        // Proyección local a metros (equirectangular): suficiente para distancias de un viaje
        var latitudReferencia = GeoMath.ARadianes(puntos.Average(p => p.Latitud));
        var cosLat = Math.Cos(latitudReferencia);
        var xy = puntos
            .Select(p => (
                X: GeoMath.ARadianes(p.Longitud) * cosLat * GeoMath.RadioTierraMetros,
                Y: GeoMath.ARadianes(p.Latitud) * GeoMath.RadioTierraMetros))
            .ToArray();

        var conservar = new bool[puntos.Count];
        conservar[0] = true;
        conservar[^1] = true;

        // Versión iterativa (con pila) para no desbordar la recursión con rutas de miles de puntos
        var pendientes = new Stack<(int Inicio, int Fin)>();
        pendientes.Push((0, puntos.Count - 1));

        while (pendientes.Count > 0)
        {
            var (inicio, fin) = pendientes.Pop();
            var distanciaMaxima = 0.0;
            var indiceMaximo = -1;

            for (var k = inicio + 1; k < fin; k++)
            {
                var distancia = DistanciaAlSegmento(xy[k], xy[inicio], xy[fin]);
                if (distancia > distanciaMaxima)
                {
                    distanciaMaxima = distancia;
                    indiceMaximo = k;
                }
            }

            if (indiceMaximo != -1 && distanciaMaxima > toleranciaMetros)
            {
                conservar[indiceMaximo] = true;
                pendientes.Push((inicio, indiceMaximo));
                pendientes.Push((indiceMaximo, fin));
            }
        }

        return puntos.Where((_, i) => conservar[i]).ToList();
    }

    private static double DistanciaAlSegmento((double X, double Y) p, (double X, double Y) a, (double X, double Y) b)
    {
        var dx = b.X - a.X;
        var dy = b.Y - a.Y;
        var longitud2 = dx * dx + dy * dy;

        if (longitud2 == 0)
            return Math.Sqrt((p.X - a.X) * (p.X - a.X) + (p.Y - a.Y) * (p.Y - a.Y));

        var t = Math.Clamp(((p.X - a.X) * dx + (p.Y - a.Y) * dy) / longitud2, 0, 1);
        var proyX = a.X + t * dx;
        var proyY = a.Y + t * dy;

        return Math.Sqrt((p.X - proyX) * (p.X - proyX) + (p.Y - proyY) * (p.Y - proyY));
    }
}
