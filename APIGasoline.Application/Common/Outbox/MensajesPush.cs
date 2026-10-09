using System.Text.Json;
using APIGasoline.Domain.Entities;
using APIGasoline.Domain.Enums;
using APIGasoline.Domain.Outbox;

namespace APIGasoline.Application.Common.Outbox;

/// <summary>Lo que se guarda en outbox_mensaje.payload para enviar el push sin volver a consultar la notificación.</summary>
public record PushNotificacionPayload(
    Guid NotificacionId,
    Guid EmpleadoDestinatarioId,
    TipoNotificacion Tipo,
    Guid? ReferenciaId,
    string Titulo,
    string Cuerpo);

public static class MensajesPush
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    public static string Titulo(TipoNotificacion tipo) => tipo switch
    {
        TipoNotificacion.NuevaSolicitud => "Nueva solicitud de carro",
        TipoNotificacion.ResultadoSolicitud => "Tu solicitud de reserva",
        TipoNotificacion.CambioEstatusForzado => "Cambio de estatus del carro",
        _ => "Notificación"
    };

    public static PushNotificacionPayload CrearPayload(Notificacion notificacion)
    {
        ArgumentNullException.ThrowIfNull(notificacion);

        var titulo = Titulo(notificacion.Tipo);

        return new PushNotificacionPayload(
            notificacion.NotificacionId,
            notificacion.EmpleadoDestinatarioId,
            notificacion.Tipo,
            notificacion.ReferenciaId,
            titulo,
            notificacion.Mensaje ?? titulo);
    }

    public static OutboxMensaje CrearMensaje(Notificacion notificacion, DateTime ahora)
    {
        var payload = CrearPayload(notificacion);

        return OutboxMensaje.Crear(
            notificacion.EmpresaId,
            TiposOutbox.NotificacionPush,
            notificacion.NotificacionId,
            JsonSerializer.Serialize(payload, Json),
            ahora);
    }

    public static PushNotificacionPayload Leer(string payload)
        => JsonSerializer.Deserialize<PushNotificacionPayload>(payload, Json)
           ?? throw new JsonException("El contenido del mensaje está vacío.");

    /// <summary>Datos extra que recibe la app para abrir la pantalla correcta al tocar el push.</summary>
    public static IReadOnlyDictionary<string, string> Datos(PushNotificacionPayload payload)
    {
        var datos = new Dictionary<string, string>
        {
            ["notificacionId"] = payload.NotificacionId.ToString(),
            ["tipo"] = payload.Tipo.ToString()
        };

        if (payload.ReferenciaId is not null)
            datos["referenciaId"] = payload.ReferenciaId.Value.ToString();

        return datos;
    }
}
