namespace APIGasoline.Domain.Rutas;

/// <summary>Métricas de una ruta ya limpia.</summary>
/// <param name="VelocidadPromedioKmh">Promedio en movimiento; null si nunca se movió.</param>
/// <param name="VelocidadMaximaKmh">null si nunca se movió.</param>
public sealed record MetricasRuta(
    double DistanciaMetros,
    int TiempoMovimientoSeg,
    int TiempoDetenidoSeg,
    double? VelocidadPromedioKmh,
    double? VelocidadMaximaKmh);

/// <summary>
/// Calcula distancia, tiempo en movimiento / detenido y velocidades a partir de puntos ya filtrados
/// (en orden cronológico). Un tramo cuenta como "en movimiento" si su velocidad alcanza el umbral;
/// los tramos detenidos no suman distancia, porque el GPS "tiembla" aunque el carro esté parado.
/// </summary>
public static class CalculadoraRuta
{
    public static MetricasRuta Calcular(IReadOnlyList<PuntoGps> puntos, ParametrosRuta parametros)
    {
        ArgumentNullException.ThrowIfNull(puntos);
        ArgumentNullException.ThrowIfNull(parametros);

        double distancia = 0, segundosMovimiento = 0, segundosDetenido = 0;
        double? velocidadMaxima = null;

        for (var i = 1; i < puntos.Count; i++)
        {
            var segundos = (puntos[i].Timestamp - puntos[i - 1].Timestamp).TotalSeconds;
            if (segundos <= 0)
                continue;

            var metros = GeoMath.DistanciaMetros(puntos[i - 1], puntos[i]);
            var velocidadKmh = metros / segundos * 3.6;

            if (velocidadKmh >= parametros.VelocidadMovimientoKmh)
            {
                distancia += metros;
                segundosMovimiento += segundos;
                velocidadMaxima = Math.Max(velocidadMaxima ?? 0, velocidadKmh);
            }
            else
            {
                segundosDetenido += segundos;
            }
        }

        double? velocidadPromedio = segundosMovimiento > 0 ? distancia / segundosMovimiento * 3.6 : null;

        return new MetricasRuta(
            distancia,
            (int)Math.Round(segundosMovimiento),
            (int)Math.Round(segundosDetenido),
            velocidadPromedio,
            velocidadMaxima);
    }
}
