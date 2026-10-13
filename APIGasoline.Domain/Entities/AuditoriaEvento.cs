using APIGasoline.Domain.Enums;
using APIGasoline.Domain.Exceptions;

namespace APIGasoline.Domain.Entities;

/// <summary>
/// Historial de cambios (auditoria_evento). No duplica viajes ni cargas: guarda qué cambió,
/// quién, cuándo y por qué. Solo se inserta; nunca se edita ni se borra.
/// </summary>
public class AuditoriaEvento
{
    public const int EntidadMaxLength = 60;
    public const int MotivoMaxLength = 500;

    public Guid AuditoriaEventoId { get; private set; } = Guid.CreateVersion7();

    /// <summary>null para acciones del Administrador sobre la plataforma (ej. crear una empresa).</summary>
    public Guid? EmpresaId { get; private set; }

    /// <summary>Quién ejecutó la acción (del token). null si fue un proceso automático.</summary>
    public Guid? UsuarioId { get; private set; }

    public AccionAuditoria Accion { get; private set; }

    /// <summary>Tabla afectada: carro, viaje, carga_combustible, solicitud_reserva...</summary>
    public string Entidad { get; private set; } = string.Empty;

    public Guid EntidadId { get; private set; }

    /// <summary>JSON con los valores previos de los campos que cambiaron. null al crear.</summary>
    public string? ValoresAnteriores { get; private set; }

    /// <summary>JSON con los valores nuevos de los campos que cambiaron. null al eliminar.</summary>
    public string? ValoresNuevos { get; private set; }

    public string? Motivo { get; private set; }

    public OrigenAuditoria Origen { get; private set; }

    /// <summary>Hora del servidor (la pone la BD).</summary>
    public DateTime? FechaHora { get; private set; }

    // Requerido por Entity Framework
    private AuditoriaEvento()
    {
    }

    public static AuditoriaEvento Registrar(
        Guid? empresaId,
        Guid? usuarioId,
        AccionAuditoria accion,
        string entidad,
        Guid entidadId,
        string? valoresAnteriores,
        string? valoresNuevos,
        string? motivo,
        OrigenAuditoria origen)
    {
        if (!Enum.IsDefined(accion))
            throw new DomainException("La acción de auditoría no es válida.");

        if (!Enum.IsDefined(origen))
            throw new DomainException("El origen de auditoría no es válido.");

        var entidadNormalizada = entidad?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(entidadNormalizada))
            throw new DomainException("La entidad auditada es obligatoria.");

        if (entidadNormalizada.Length > EntidadMaxLength)
            throw new DomainException($"La entidad auditada no puede exceder {EntidadMaxLength} caracteres.");

        if (entidadId == Guid.Empty)
            throw new DomainException("El id del registro auditado es obligatorio.");

        var motivoNormalizado = string.IsNullOrWhiteSpace(motivo) ? null : motivo.Trim();

        // El motivo es informativo: si es muy largo se recorta en lugar de perder el evento
        if (motivoNormalizado is { Length: > MotivoMaxLength })
            motivoNormalizado = motivoNormalizado[..(MotivoMaxLength - 3)] + "...";

        return new AuditoriaEvento
        {
            EmpresaId = empresaId == Guid.Empty ? null : empresaId,
            UsuarioId = usuarioId == Guid.Empty ? null : usuarioId,
            Accion = accion,
            Entidad = entidadNormalizada,
            EntidadId = entidadId,
            ValoresAnteriores = valoresAnteriores,
            ValoresNuevos = valoresNuevos,
            Motivo = motivoNormalizado,
            Origen = origen
        };
    }
}
