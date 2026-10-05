using APIGasoline.Domain.Enums;

namespace APIGasoline.Application.Features.Notificaciones.DTOs;

public record NotificacionDto(
    int NotificacionId,
    int EmpresaId,
    int EmpleadoDestinatarioId,
    TipoNotificacion Tipo,
    int? ReferenciaId,
    string? Mensaje,
    bool Leida,
    DateTime? FechaEnvio);
