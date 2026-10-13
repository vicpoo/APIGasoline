using APIGasoline.Domain.Enums;

namespace APIGasoline.Application.Features.SolicitudesReserva.DTOs;

public record SolicitudReservaDto(
    Guid SolicitudReservaId,
    Guid EmpresaId,
    Guid CarroId,
    Guid EmpleadoSolicitanteId,
    DateTime? FechaSolicitud,
    EstadoSolicitudReserva Estado,
    Guid? EmpleadoConfirmadorId,
    DateTime? FechaResolucion,
    string? MotivoResolucion,
    /// <summary>Cuándo se liberó el carro (HU-USO-04).</summary>
    DateTime? FechaLiberacion,
    string? ObservacionesLiberacion);
