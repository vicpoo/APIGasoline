using APIGasoline.Domain.Enums;

namespace APIGasoline.Application.Features.Outbox.DTOs;

public record OutboxMensajeDto(
    Guid OutboxMensajeId,
    Guid EmpresaId,
    string Tipo,
    Guid AgregadoId,
    string Payload,
    EstadoOutbox Estado,
    int Intentos,
    DateTime ProximoIntento,
    string? UltimoError,
    DateTime FechaCreacion,
    DateTime? FechaProcesado);
