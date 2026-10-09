using APIGasoline.Domain.Enums;
using APIGasoline.Domain.Exceptions;

namespace APIGasoline.Domain.Entities;


public class Notificacion
{
    public const int MensajeMaxLength = 255;

    public Guid NotificacionId { get; private set; } = Guid.CreateVersion7();
    public Guid EmpresaId { get; private set; }
    public Guid EmpleadoDestinatarioId { get; private set; }
    public TipoNotificacion Tipo { get; private set; }

  
    public Guid? ReferenciaId { get; private set; }

    public string? Mensaje { get; private set; }
    public bool Leida { get; private set; }

    public DateTime? FechaEnvio { get; private set; }

    // Requerido por Entity Framework
    private Notificacion()
    {
    }

    public static Notificacion Crear(Guid empresaId, Guid empleadoDestinatarioId, TipoNotificacion tipo, Guid? referenciaId, string? mensaje)
    {
        if (empresaId == Guid.Empty)
            throw new DomainException("El id de la empresa es obligatorio.");

        if (empleadoDestinatarioId == Guid.Empty)
            throw new DomainException("El id del empleado destinatario es obligatorio.");

        if (!Enum.IsDefined(tipo))
            throw new DomainException("El tipo de notificación no es válido.");

        if (referenciaId == Guid.Empty)
            throw new DomainException("El id de referencia es obligatorio.");

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
