using APIGasoline.Domain.Enums;
using APIGasoline.Domain.Exceptions;

namespace APIGasoline.Domain.Entities;

/// <summary>
/// Bandeja e historial de notificaciones dirigidas a empleados.
/// </summary>
public class Notificacion
{
    public const int MensajeMaxLength = 255;

    public int NotificacionId { get; private set; }
    public int EmpresaId { get; private set; }
    public int EmpleadoDestinatarioId { get; private set; }
    public TipoNotificacion Tipo { get; private set; }

    /// <summary>
    /// Id del registro relacionado según el tipo:
    /// NuevaSolicitud / ResultadoSolicitud → solicitud_reserva_id;
    /// CambioEstatusForzado → cambio_estatus_carro_id.
    /// </summary>
    public int? ReferenciaId { get; private set; }

    public string? Mensaje { get; private set; }
    public bool Leida { get; private set; }

    /// <summary>La asigna la base de datos al insertar (default now()).</summary>
    public DateTime? FechaEnvio { get; private set; }

    // Requerido por Entity Framework
    private Notificacion()
    {
    }

    public static Notificacion Crear(int empresaId, int empleadoDestinatarioId, TipoNotificacion tipo, int? referenciaId, string? mensaje)
    {
        if (empresaId <= 0)
            throw new DomainException("El id de la empresa debe ser mayor a cero.");

        if (empleadoDestinatarioId <= 0)
            throw new DomainException("El id del empleado destinatario debe ser mayor a cero.");

        if (!Enum.IsDefined(tipo))
            throw new DomainException("El tipo de notificación no es válido.");

        if (referenciaId is <= 0)
            throw new DomainException("El id de referencia debe ser mayor a cero.");

        var mensajeNormalizado = string.IsNullOrWhiteSpace(mensaje) ? null : mensaje.Trim();

        // El mensaje es informativo: si es muy largo se recorta en lugar de fallar
        if (mensajeNormalizado is { Length: > MensajeMaxLength })
            mensajeNormalizado = mensajeNormalizado[..(MensajeMaxLength - 3)] + "...";

        return new Notificacion
        {
            EmpresaId = empresaId,
            EmpleadoDestinatarioId = empleadoDestinatarioId,
            Tipo = tipo,
            ReferenciaId = referenciaId,
            Mensaje = mensajeNormalizado,
            Leida = false
        };
    }

    public void MarcarLeida() => Leida = true;
}
