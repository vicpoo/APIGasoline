namespace APIGasoline.Application.Features.PuntosRuta.DTOs;

public record PuntoRutaDto(
    Guid PuntoRutaId,
    Guid EmpresaId,
    Guid ViajeId,
    decimal Latitud,
    decimal Longitud,
    DateTime TimestampCaptura,
    /// <summary>Hora del servidor en que llegó el punto.</summary>
    DateTime? FechaRecepcion,
    decimal? PrecisionMetros,
    decimal? Altitud,
    decimal? Velocidad);
