using APIGasoline.Domain.Enums;

namespace APIGasoline.Application.Features.Notificaciones.DTOs;

public record NotificacionDto(
    Guid NotificacionId,
    Guid EmpresaId,
    Guid EmpleadoDestinatarioId,
    TipoNotificacion Tipo,
    Guid? ReferenciaId,
    string? Mensaje,
    bool Leida,
    DateTime? FechaEnvio);
