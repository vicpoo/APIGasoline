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
    DateTime? FechaFin,
    decimal? UbicacionFinLat,
    decimal? UbicacionFinLng,
    decimal? KilometrajeFinal,
    decimal? DistanciaRecorrida,
    EstadoViaje Estado,
    string? MotivoInterrupcion,
    bool Sincronizado);
