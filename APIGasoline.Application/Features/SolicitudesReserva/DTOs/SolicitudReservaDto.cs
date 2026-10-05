using APIGasoline.Domain.Enums;

namespace APIGasoline.Application.Features.SolicitudesReserva.DTOs;

public record SolicitudReservaDto(
    int SolicitudReservaId,
    int EmpresaId,
    int CarroId,
    int EmpleadoSolicitanteId,
    DateTime? FechaSolicitud,
    EstadoSolicitudReserva Estado,
    int? EmpleadoConfirmadorId,
    DateTime? FechaResolucion,
    string? MotivoResolucion);
