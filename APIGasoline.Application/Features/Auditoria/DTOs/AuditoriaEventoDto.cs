using System.Text.Json;
using APIGasoline.Domain.Entities;
using APIGasoline.Domain.Enums;

namespace APIGasoline.Application.Features.Auditoria.DTOs;

/// <summary>Evento del historial de cambios. Los valores se devuelven como JSON (no como texto).</summary>
public record AuditoriaEventoDto(
    Guid AuditoriaEventoId,
    Guid? EmpresaId,
    Guid? UsuarioId,
    AccionAuditoria Accion,
    string Entidad,
    Guid EntidadId,
    JsonElement? ValoresAnteriores,
    JsonElement? ValoresNuevos,
    string? Motivo,
    OrigenAuditoria Origen,
    DateTime? FechaHora)
{
    public static AuditoriaEventoDto Desde(AuditoriaEvento evento)
        => new(
            evento.AuditoriaEventoId,
            evento.EmpresaId,
            evento.UsuarioId,
            evento.Accion,
            evento.Entidad,
            evento.EntidadId,
            LeerJson(evento.ValoresAnteriores),
            LeerJson(evento.ValoresNuevos),
            evento.Motivo,
            evento.Origen,
            evento.FechaHora);

    private static JsonElement? LeerJson(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return null;

        using var documento = JsonDocument.Parse(json);
        return documento.RootElement.Clone();
    }
}
