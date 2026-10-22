using APIGasoline.Domain.Enums;

namespace APIGasoline.Application.Features.Viajes.DTOs;

public record ViajeDto(
    Guid ViajeId,
    Guid EmpresaId,
    Guid CarroId,
    Guid? SolicitudReservaId,
    Guid EmpleadoOperadorId,
    DateTime FechaInicio,
    decimal? UbicacionInicioLat,
    decimal? UbicacionInicioLng,
    decimal KilometrajeInicial,
    /// <summary>Llave de la foto del odómetro al iniciar (lo que se guarda en BD).</summary>
    string? FotoTableroInicial,
    /// <summary>URL temporal de la foto (la llena el handler, no AutoMapper).</summary>
    string? FotoTableroInicialUrl,
    DateTime? FechaFin,
    decimal? UbicacionFinLat,
    decimal? UbicacionFinLng,
    decimal? KilometrajeFinal,
    string? FotoTableroFinal,
    string? FotoTableroFinalUrl,
    decimal? DistanciaRecorrida,
    EstadoViaje Estado,
    string? MotivoInterrupcion,
    /// <summary>true si el kilometraje se corrigió después de capturarse (detalle en auditoría).</summary>
    bool Corregido,
    /// <summary>Hora del servidor en que llegó el viaje.</summary>
    DateTime? FechaRecepcion,
    /// <summary>Centro de costos del viaje (null = sin asignar).</summary>
    Guid? CentroCostoId = null,
    /// <summary>true si el viaje tiene una pausa abierta (lo llena el handler, no AutoMapper).</summary>
    bool EstaPausado = false,
    /// <summary>Tiempo total en pausa, en segundos (la duración real del viaje lo descuenta).</summary>
    long SegundosEnPausa = 0,
    /// <summary>Pausas del viaje en orden (las llena el handler).</summary>
    IReadOnlyList<ViajePausaDto>? Pausas = null,
    /// <summary>Quién manejó (nombre y foto). Lo llena el handler.</summary>
    OperadorViajeDto? Operador = null);
