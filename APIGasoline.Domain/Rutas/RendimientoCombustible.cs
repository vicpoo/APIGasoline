namespace APIGasoline.Domain.Rutas;

/// <summary>
/// Rendimiento del viaje: kilómetros recorridos (según el GPS) por litro cargado.
/// </summary>
public static class RendimientoCombustible
{
    /// <summary>Límite de la columna decimal(6,2).</summary>
    public const decimal RendimientoMaximo = 9999.99m;

    /// <returns>km/L con 2 decimales; null si no hubo cargas o no hubo distancia.</returns>
    public static decimal? KmPorLitro(decimal distanciaKm, decimal litros)
    {
        if (litros <= 0 || distanciaKm <= 0)
            return null;

        return Math.Min(Math.Round(distanciaKm / litros, 2), RendimientoMaximo);
    }
}
