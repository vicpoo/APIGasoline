namespace APIGasoline.Domain.Enums;

/// <summary>Acción registrada en auditoria_evento.</summary>
public enum AccionAuditoria
{
    Crear = 0,
    Actualizar = 1,
    Eliminar = 2,
    Aprobar = 3,
    Rechazar = 4,
    Cancelar = 5,
    CambiarEstado = 6,
    Corregir = 7
}
