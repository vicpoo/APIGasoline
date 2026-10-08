namespace APIGasoline.Domain.Rutas;

/// <summary>Puntos que pasaron la limpieza (en orden cronológico) y cuántos se descartaron.</summary>
public sealed record ResultadoFiltro(IReadOnlyList<PuntoGps> Validos, int Descartados);

/// <summary>
/// Limpieza de los puntos crudos antes de calcular la ruta:
/// 1. Se ordenan por hora (los lotes offline pueden llegar en desorden).
/// 2. Se descartan los de mala precisión.
/// 3. Se descartan los duplicados (misma hora; se conserva el primero).
/// 4. Se descartan los "saltos imposibles": si para llegar desde el último punto válido
///    habría que ir a más de la velocidad máxima, el punto es un error del GPS.
/// </summary>
/// <remarks>
/// Limitación conocida (versión 1): cada punto se compara contra el último punto aceptado;
/// si el primer punto del viaje fuera un error, los siguientes se verían como saltos.
/// Un filtro de Kalman (versión 2) resuelve ese caso.
/// </remarks>
public static class FiltroPuntosRuta
{
    public static ResultadoFiltro Filtrar(IEnumerable<PuntoGps> puntos, ParametrosRuta parametros)
    {
        ArgumentNullException.ThrowIfNull(puntos);
        ArgumentNullException.ThrowIfNull(parametros);

        // OrderBy es estable: entre puntos con la misma hora se conserva el que llegó primero
        var ordenados = puntos.OrderBy(p => p.Timestamp).ToList();
        var validos = new List<PuntoGps>(ordenados.Count);

        foreach (var punto in ordenados)
        {
            if (punto.PrecisionMetros is { } precision && precision > parametros.PrecisionMaximaMetros)
                continue;

            if (validos.Count > 0)
            {
                var anterior = validos[^1];
                var segundos = (punto.Timestamp - anterior.Timestamp).TotalSeconds;

                // Misma hora que el anterior: duplicado
                if (segundos <= 0)
                    continue;

                var velocidadKmh = GeoMath.DistanciaMetros(anterior, punto) / segundos * 3.6;

                if (velocidadKmh > parametros.VelocidadMaximaKmh)
                    continue;
            }

            validos.Add(punto);
        }

        return new ResultadoFiltro(validos, ordenados.Count - validos.Count);
    }
}
