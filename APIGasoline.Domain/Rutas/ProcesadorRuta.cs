namespace APIGasoline.Domain.Rutas;

/// <summary>Resumen calculado de la ruta de un viaje (lo que se guarda en ruta_procesada).</summary>
public sealed record ResultadoRuta(
    int PuntosRecibidos,
    int PuntosDescartados,
    decimal DistanciaGpsKm,
    int TiempoMovimientoSeg,
    int TiempoDetenidoSeg,
    decimal? VelocidadPromedioKmh,
    decimal? VelocidadMaximaKmh,
    ComparacionOdometro Odometro,
    decimal LitrosCargados,
    decimal? RendimientoKmPorLitro,
    string Polyline,
    string VersionAlgoritmo);

/// <summary>
/// Pipeline completo: puntos crudos → limpieza → métricas → comparación con odómetro
/// → rendimiento → ruta simplificada y codificada para el mapa.
/// Es puro: recibe datos y devuelve el resultado, sin BD ni red.
/// </summary>
public static class ProcesadorRuta
{
    /// <summary>
    /// Cambiarla cada vez que cambie el algoritmo o sus umbrales, para saber qué viajes
    /// conviene reprocesar.
    /// </summary>
    public const string VersionAlgoritmo = "1.0";

    public static ResultadoRuta Procesar(
        IReadOnlyCollection<PuntoGps> puntosCrudos,
        decimal kilometrajeInicial,
        decimal? kilometrajeFinal,
        decimal litrosCargados,
        ParametrosRuta? parametros = null)
    {
        ArgumentNullException.ThrowIfNull(puntosCrudos);
        parametros ??= ParametrosRuta.PorDefecto;

        var filtro = FiltroPuntosRuta.Filtrar(puntosCrudos, parametros);
        var metricas = CalculadoraRuta.Calcular(filtro.Validos, parametros);

        var distanciaKm = Math.Round((decimal)(metricas.DistanciaMetros / 1000), 3);
        var odometro = ComparadorOdometro.Comparar(distanciaKm, kilometrajeInicial, kilometrajeFinal, parametros);

        var rutaMapa = SimplificadorRuta.Simplificar(filtro.Validos, parametros.ToleranciaSimplificacionMetros);
        var polyline = CodificadorPolyline.Codificar(rutaMapa.Select(p => (p.Latitud, p.Longitud)));

        return new ResultadoRuta(
            puntosCrudos.Count,
            filtro.Descartados,
            distanciaKm,
            metricas.TiempoMovimientoSeg,
            metricas.TiempoDetenidoSeg,
            Redondear(metricas.VelocidadPromedioKmh),
            Redondear(metricas.VelocidadMaximaKmh),
            odometro,
            litrosCargados,
            RendimientoCombustible.KmPorLitro(distanciaKm, litrosCargados),
            polyline,
            VersionAlgoritmo);
    }

    private static decimal? Redondear(double? valor)
        => valor is null ? null : Math.Round((decimal)valor.Value, 2);
}
