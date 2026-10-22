using APIGasoline.Domain.Enums;

namespace APIGasoline.Application.Features.SolicitudesReserva.DTOs;

/// <summary>
/// Solicitud para las listas (vista Solicitudes de la empresa y campana de "mis solicitudes").
/// Solicitante y Carro traen lo necesario para pintar la tarjeta sin otra llamada:
/// "Solicitud de {Solicitante.NombreCompleto}" con su foto y el carro solicitado.
/// </summary>
public record SolicitudReservaDto(
    Guid SolicitudReservaId,
    Guid EmpresaId,
    Guid CarroId,
    Guid EmpleadoSolicitanteId,
    /// <summary>v8. Para qué necesita el carro (lo escribe el solicitante).</summary>
    string? MotivoSolicitud,
    DateTime? FechaSolicitud,
    EstadoSolicitudReserva Estado,
    Guid? EmpleadoConfirmadorId,
    DateTime? FechaResolucion,
    string? MotivoResolucion,
    /// <summary>Cuándo se liberó el carro (HU-USO-04).</summary>
    DateTime? FechaLiberacion,
    string? ObservacionesLiberacion,
    /// <summary>Quién la pidió (nombre y foto). Lo llena el handler, no AutoMapper.</summary>
    EmpleadoResumenDto? Solicitante = null,
    /// <summary>Qué carro pidió. Lo llena el handler, no AutoMapper.</summary>
    CarroResumenDto? Carro = null);
