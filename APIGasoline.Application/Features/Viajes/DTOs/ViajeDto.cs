using APIGasoline.Domain.Enums;

namespace APIGasoline.Application.Features.Viajes.DTOs;

public record ViajeDto(
    int ViajeId,
    int EmpresaId,
    int CarroId,
    int? SolicitudReservaId,
    int EmpleadoOperadorId,
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
