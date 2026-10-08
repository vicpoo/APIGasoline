namespace APIGasoline.Application.Features.PuntosRuta.DTOs;

public record PuntoRutaDto(
    int PuntoRutaId,
    int EmpresaId,
    int ViajeId,
    decimal Latitud,
    decimal Longitud,
    DateTime TimestampCaptura,
    bool Sincronizado,
    decimal? PrecisionMetros,
    decimal? Altitud,
    decimal? Velocidad);
