namespace APIGasoline.Application.Features.PuntosRuta.DTOs;

/// <summary>
/// Resultado de registrar un lote: la app puede reintentar el mismo lote sin duplicar puntos.
/// </summary>
public record LotePuntosRutaResultadoDto(Guid ViajeId, int Recibidos, int Insertados, int Duplicados);
