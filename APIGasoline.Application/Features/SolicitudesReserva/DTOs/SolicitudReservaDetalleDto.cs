using APIGasoline.Domain.Enums;

namespace APIGasoline.Application.Features.SolicitudesReserva.DTOs;

/// <summary>
/// Detalle de una solicitud (al tocar la tarjeta en la vista Solicitudes): el motivo, quién la pide
/// con su foto y su licencia, qué carro y, si ya se resolvió, quién la aceptó o rechazó.
/// </summary>
public record SolicitudReservaDetalleDto(
    Guid SolicitudReservaId,
    Guid EmpresaId,
    Guid CarroId,
    Guid EmpleadoSolicitanteId,
    string? MotivoSolicitud,
    DateTime? FechaSolicitud,
    EstadoSolicitudReserva Estado,
    SolicitanteDetalleDto? Solicitante,
    CarroDetalleDto? Carro,
    Guid? EmpleadoConfirmadorId,
    EmpleadoResumenDto? Confirmador,
    DateTime? FechaResolucion,
    string? MotivoResolucion,
    DateTime? FechaLiberacion,
    string? ObservacionesLiberacion);

/// <summary>Empleado en una tarjeta: nombre y foto de perfil.</summary>
public record EmpleadoResumenDto(
    Guid EmpleadoId,
    string NombreCompleto,
    string? FotoPerfilUrl);

/// <summary>Solicitante en el detalle: además de nombre y foto, su licencia para que el confirmador la revise.</summary>
public record SolicitanteDetalleDto(
    Guid EmpleadoId,
    string NombreCompleto,
    string? FotoPerfilUrl,
    bool TieneLicencia,
    string? FotoLicenciaUrl,
    DateTime? FechaCargaLicencia);

/// <summary>Carro en una tarjeta de solicitud.</summary>
public record CarroResumenDto(
    Guid CarroId,
    string Marca,
    string Modelo,
    int? Anio,
    string? Color,
    string Placas,
    string? FotoVehiculoUrl);

/// <summary>Carro en el detalle de la solicitud.</summary>
public record CarroDetalleDto(
    Guid CarroId,
    string Marca,
    string Modelo,
    int? Anio,
    string? Color,
    string Placas,
    decimal KilometrajeActual,
    EstadoCarro Estado,
    bool DadoDeBaja,
    string? FotoVehiculoUrl);
