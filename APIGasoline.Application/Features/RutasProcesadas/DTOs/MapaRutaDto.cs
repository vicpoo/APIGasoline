namespace APIGasoline.Application.Features.RutasProcesadas.DTOs;

public record CoordenadaDto(double Latitud, double Longitud);

/// <summary>
/// La ruta simplificada ya decodificada, lista para dibujarla en un mapa
/// (por ejemplo, para revisar en Postman o en un cliente que no sepa leer polylines).
/// </summary>
public record MapaRutaDto(Guid ViajeId, string? Polyline, IReadOnlyList<CoordenadaDto> Coordenadas);
