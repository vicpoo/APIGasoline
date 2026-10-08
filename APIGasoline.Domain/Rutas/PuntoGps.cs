namespace APIGasoline.Domain.Rutas;

/// <summary>
/// Punto GPS en el formato que usan los algoritmos de procesamiento de rutas.
/// Usa double porque son cálculos geométricos (trigonometría), no montos.
/// </summary>
/// <param name="PrecisionMetros">Radio de error del GPS; null si el teléfono no lo reportó.</param>
/// <param name="Velocidad">Velocidad instantánea en m/s; null si el teléfono no la reportó.</param>
public readonly record struct PuntoGps(
    double Latitud,
    double Longitud,
    DateTime Timestamp,
    double? PrecisionMetros = null,
    double? Velocidad = null);
