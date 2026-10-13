using APIGasoline.Application.Common.Interfaces;
using APIGasoline.Domain.Enums;

namespace APIGasoline.Infrastructure.Auditoria;

/// <summary>Vive lo que dura la petición (scoped).</summary>
public sealed class ContextoAuditoria : IContextoAuditoria
{
    private readonly Dictionary<Guid, (AccionAuditoria? Accion, string? Motivo)> _registros = new();

    public void Registrar(Guid entidadId, AccionAuditoria? accion, string? motivo = null)
        => _registros[entidadId] = (accion, string.IsNullOrWhiteSpace(motivo) ? null : motivo.Trim());

    public AccionAuditoria? ObtenerAccion(Guid entidadId)
        => _registros.TryGetValue(entidadId, out var registro) ? registro.Accion : null;

    public string? ObtenerMotivo(Guid entidadId)
        => _registros.TryGetValue(entidadId, out var registro) ? registro.Motivo : null;
}
