namespace APIGasoline.Application.Features.PuntosRuta.DTOs;

public record PuntoRutaDto(
    Guid PuntoRutaId,
    Guid EmpresaId,
    Guid ViajeId,
    decimal Latitud,
    decimal Longitud,
    DateTime TimestampCaptura,
    bool Sincronizado,
    decimal? PrecisionMetros,
    decimal? Altitud,
    decimal? Velocidad);
