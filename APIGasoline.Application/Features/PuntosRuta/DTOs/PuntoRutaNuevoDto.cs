namespace APIGasoline.Application.Features.PuntosRuta.DTOs;

/// <summary>
/// Un punto dentro de un lote enviado por la app.
/// PrecisionMetros, Altitud (m) y Velocidad (m/s) son opcionales: los da el GPS del celular
/// y sirven para limpiar la ruta en el servidor.
/// </summary>
public record PuntoRutaNuevoDto(
    decimal Latitud,
    decimal Longitud,
    DateTime TimestampCaptura,
    decimal? PrecisionMetros = null,
    decimal? Altitud = null,
    decimal? Velocidad = null);
