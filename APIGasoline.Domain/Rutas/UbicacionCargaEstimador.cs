using APIGasoline.Domain.Entities;

namespace APIGasoline.Domain.Rutas;

/// <summary>
/// Estima dónde se hizo una carga de combustible que llegó sin ubicación, usando los puntos GPS
/// del mismo viaje: se toma el punto con la hora de captura más cercana a la fecha de la carga.
/// <para>
/// Sirve cuando el teléfono no tenía señal GPS al registrar la carga. Los puntos se guardan aunque
/// no haya internet, así que tarde o temprano llegan a la API y la carga se puede ubicar.
/// </para>
/// </summary>
public static class UbicacionCargaEstimador
{
    /// <summary>
    /// Máxima diferencia entre la hora de la carga y la del punto. Es amplia a propósito: en la
    /// gasolinera el carro está detenido (o el viaje en pausa) y la app no guarda puntos, así que el
    /// punto útil suele ser el de cuando llegó o el de cuando volvió a arrancar.
    /// </summary>
    public static readonly TimeSpan ToleranciaPorDefecto = TimeSpan.FromMinutes(10);

    /// <summary>Puntos con más error que esto se usan solo si no hay otro dentro de la tolerancia.</summary>
    public const decimal PrecisionPreferidaMetros = 50m;

    /// <returns>(latitud, longitud) del punto elegido, o null si no hay ninguno dentro de la tolerancia.</returns>
    public static (decimal Latitud, decimal Longitud)? Estimar(
        IReadOnlyList<PuntoRuta> puntos,
        DateTime fechaCarga,
        TimeSpan? tolerancia = null)
    {
        ArgumentNullException.ThrowIfNull(puntos);

        var maximo = tolerancia ?? ToleranciaPorDefecto;

        var candidatos = puntos
            .Select(p => new { Punto = p, Diferencia = (p.TimestampCaptura - fechaCarga).Duration() })
            .Where(x => x.Diferencia <= maximo)
            .ToList();

        if (candidatos.Count == 0)
            return null;

        // Primero los de buena precisión (o sin dato); entre ellos, el más cercano en el tiempo
        var elegido = candidatos
            .OrderBy(x => x.Punto.PrecisionMetros is > PrecisionPreferidaMetros ? 1 : 0)
            .ThenBy(x => x.Diferencia)
            .ThenBy(x => x.Punto.PrecisionMetros ?? 0m)
            .First()
            .Punto;

        return (elegido.Latitud, elegido.Longitud);
    }
}
