namespace APIGasoline.Domain.Rutas;

/// <summary>Resultado de comparar el kilometraje declarado (odómetro) contra la distancia GPS.</summary>
/// <param name="DiferenciaKm">Odómetro - GPS. Positivo = se declararon más km de los recorridos.</param>
/// <param name="DiferenciaPorcentaje">Diferencia respecto a la distancia GPS; null si el GPS no registró distancia.</param>
public sealed record ComparacionOdometro(
    decimal? DistanciaOdometroKm,
    decimal? DiferenciaKm,
    decimal? DiferenciaPorcentaje,
    bool Alerta);

/// <summary>
/// El servidor no confía en el kilometraje que escribe el operador: lo compara contra lo que midió el GPS.
/// Hay alerta si la diferencia (en cualquier sentido) supera el porcentaje Y los km mínimos configurados.
/// Pedir ambos evita falsas alarmas en viajes cortos (1 km de diferencia en un viaje de 3 km es 33 %).
/// </summary>
public static class ComparadorOdometro
{
    /// <summary>Límite de la columna decimal(7,2).</summary>
    public const decimal PorcentajeMaximo = 99999.99m;

    public static ComparacionOdometro Comparar(
        decimal distanciaGpsKm,
        decimal kilometrajeInicial,
        decimal? kilometrajeFinal,
        ParametrosRuta parametros)
    {
        ArgumentNullException.ThrowIfNull(parametros);

        // Viaje cerrado sin kilometraje (por ejemplo, interrumpido): no hay contra qué comparar
        if (kilometrajeFinal is null)
            return new ComparacionOdometro(null, null, null, false);

        var odometro = kilometrajeFinal.Value - kilometrajeInicial;
        var diferencia = Math.Round(odometro - distanciaGpsKm, 2);

        decimal? porcentaje = distanciaGpsKm > 0
            ? Math.Clamp(Math.Round(diferencia / distanciaGpsKm * 100m, 2), -PorcentajeMaximo, PorcentajeMaximo)
            : null;

        // Sin distancia GPS cualquier diferencia relevante en km es sospechosa
        var alerta = Math.Abs(diferencia) > parametros.AlertaKmMinimos
                     && (porcentaje is null || Math.Abs(porcentaje.Value) > parametros.AlertaPorcentaje);

        return new ComparacionOdometro(odometro, diferencia, porcentaje, alerta);
    }
}
