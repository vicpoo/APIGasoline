namespace APIGasoline.Application.Features.PuntosRuta.DTOs;

/// <summary>
/// Resultado de registrar un lote: la app puede reintentar el mismo lote sin duplicar puntos.
/// </summary>
/// <param name="EnPausa">Puntos descartados porque caían dentro de una pausa del viaje.</param>
public record LotePuntosRutaResultadoDto(Guid ViajeId, int Recibidos, int Insertados, int Duplicados, int EnPausa = 0);
