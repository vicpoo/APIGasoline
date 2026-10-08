namespace APIGasoline.Domain.Rutas;

/// <summary>
/// Umbrales del procesamiento de rutas. Se concentran aquí para poder ajustarlos
/// (y probarlos) sin tocar los algoritmos.
/// </summary>
public sealed record ParametrosRuta
{
    /// <summary>Valores usados por la API (versión 1 del algoritmo).</summary>
    public static ParametrosRuta PorDefecto { get; } = new();

    /// <summary>Puntos con un radio de error mayor se descartan.</summary>
    public double PrecisionMaximaMetros { get; init; } = 50;

    /// <summary>Un salto entre dos puntos que implique más de esta velocidad es imposible para un carro.</summary>
    public double VelocidadMaximaKmh { get; init; } = 200;

    /// <summary>Por debajo de esta velocidad el carro se considera detenido (el GPS "tiembla" aunque no se mueva).</summary>
    public double VelocidadMovimientoKmh { get; init; } = 3;

    /// <summary>Tolerancia de Douglas-Peucker para simplificar la ruta del mapa.</summary>
    public double ToleranciaSimplificacionMetros { get; init; } = 5;

    /// <summary>Alerta de odómetro: la diferencia debe superar este porcentaje...</summary>
    public decimal AlertaPorcentaje { get; init; } = 10m;

    /// <summary>...y además estos kilómetros (evita falsas alarmas en viajes muy cortos).</summary>
    public decimal AlertaKmMinimos { get; init; } = 2m;
}
