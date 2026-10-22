namespace APIGasoline.Application.Features.Viajes.DTOs;

/// <summary>Una pausa del viaje. <see cref="FechaFin"/> null = sigue en pausa.</summary>
public record ViajePausaDto(
    Guid ViajePausaId,
    DateTime FechaInicio,
    DateTime? FechaFin,
    string? Motivo,
    decimal? UbicacionLat,
    decimal? UbicacionLng);
