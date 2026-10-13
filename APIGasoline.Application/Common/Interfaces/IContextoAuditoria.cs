using APIGasoline.Domain.Enums;

namespace APIGasoline.Application.Common.Interfaces;

/// <summary>
/// Permite que un caso de uso indique la acción y el motivo del cambio que va a guardar
/// (por ejemplo, una corrección). El interceptor de auditoría lo usa al generar el evento.
/// Si no se indica acción, se deduce del cambio (crear, actualizar, eliminar, aprobar...).
/// </summary>
public interface IContextoAuditoria
{
    /// <param name="entidadId">Id del registro que se va a guardar.</param>
    /// <param name="accion">null = deducir la acción del cambio y solo agregar el motivo.</param>
    void Registrar(Guid entidadId, AccionAuditoria? accion, string? motivo = null);

    /// <summary>Acción indicada para el registro (null si se debe deducir).</summary>
    AccionAuditoria? ObtenerAccion(Guid entidadId);

    /// <summary>Motivo indicado para el registro (null si no hay).</summary>
    string? ObtenerMotivo(Guid entidadId);
}
