namespace APIGasoline.Application.Features.PuntosRuta.DTOs;

/// <summary>
/// Un punto dentro de un lote enviado por la app.
/// </summary>
public record PuntoRutaNuevoDto(decimal Latitud, decimal Longitud, DateTime TimestampCaptura);
