using APIGasoline.Application.Features.SolicitudesReserva.DTOs;
using APIGasoline.Domain.Enums;

namespace APIGasoline.Application.Features.Asignaciones.DTOs;

/// <summary>
/// Vista "Asignaciones": solo lo vinculado, en dos listas para los dos segmentos de la app
/// (Carros | Empleados). Una asignación = una reserva aprobada vigente.
/// </summary>
public record AsignacionesDto(
    IReadOnlyList<CarroAsignacionDto> Carros,
    IReadOnlyList<EmpleadoAsignacionDto> Empleados);

/// <summary>Un carro vinculado a un empleado y su asignación.</summary>
public record CarroAsignacionDto(
    Guid CarroId,
    string Marca,
    string Modelo,
    int? Anio,
    string? Color,
    string Placas,
    string? FotoVehiculoUrl,
    EstadoCarro Estado,
    AsignacionCarroDto Asignacion);

/// <summary>Quién tiene el carro, desde cuándo y si hay un viaje en curso (no se puede desvincular).</summary>
public record AsignacionCarroDto(
    Guid SolicitudReservaId,
    EmpleadoResumenDto Empleado,
    DateTime? Desde,
    bool TieneViajeEnCurso);

/// <summary>Un empleado con al menos un carro y los carros que tiene asignados.</summary>
public record EmpleadoAsignacionDto(
    Guid EmpleadoId,
    string NombreCompleto,
    string? FotoPerfilUrl,
    IReadOnlyList<AsignacionEmpleadoDto> Carros);

/// <summary>Un carro asignado al empleado.</summary>
public record AsignacionEmpleadoDto(
    Guid SolicitudReservaId,
    CarroResumenDto Carro,
    EstadoCarro EstadoCarro,
    DateTime? Desde,
    bool TieneViajeEnCurso);
