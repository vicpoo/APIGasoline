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
    DateTime? FechaRecepcion);
