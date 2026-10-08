namespace APIGasoline.Application.Features.RutasProcesadas.DTOs;

/// <summary>
/// Resumen de la ruta de un viaje calculado en el servidor (estilo Strava).
/// Polyline: ruta simplificada en formato Google Encoded Polyline, lista para dibujar en el mapa.
/// </summary>
public record RutaProcesadaDto(
    int RutaProcesadaId,
    int EmpresaId,
    int ViajeId,
    int PuntosRecibidos,
    int PuntosDescartados,
    decimal DistanciaGpsKm,
    int TiempoMovimientoSeg,
    int TiempoDetenidoSeg,
    decimal? VelocidadPromedioKmh,
    decimal? VelocidadMaximaKmh,
    decimal? DistanciaOdometroKm,
    decimal? DiferenciaKm,
    decimal? DiferenciaPorcentaje,
    bool AlertaKilometraje,
    decimal LitrosCargados,
    decimal? RendimientoKmPorLitro,
    string? Polyline,
    string VersionAlgoritmo,
    DateTime FechaProceso);
