using APIGasoline.Application.Common.Outbox;
using APIGasoline.Domain.Entities;
using APIGasoline.Domain.Enums;
using APIGasoline.Domain.Outbox;

namespace APIGasoline.Application.Tests.Common.Outbox;

public class MensajesPushTests
{
    private static readonly DateTime Ahora = new(2026, 10, 9, 12, 0, 0, DateTimeKind.Utc);

    [Theory]
    [InlineData(TipoNotificacion.NuevaSolicitud, "Nueva solicitud de carro")]
    [InlineData(TipoNotificacion.ResultadoSolicitud, "Tu solicitud de reserva")]
    [InlineData(TipoNotificacion.CambioEstatusForzado, "Cambio de estatus del carro")]
    public void Titulo_CadaTipoTieneSuTitulo(TipoNotificacion tipo, string esperado)
    {
        Assert.Equal(esperado, MensajesPush.Titulo(tipo));
    }

    [Fact]
    public void CrearMensaje_UsaLosDatosDeLaNotificacion()
    {
        var notificacion = Notificacion.Crear(Id(1), Id(5), TipoNotificacion.ResultadoSolicitud, Id(9), "Tu solicitud fue aprobada.");

        var mensaje = MensajesPush.CrearMensaje(notificacion, Ahora);

        Assert.Equal(Id(1), mensaje.EmpresaId);
        Assert.Equal(TiposOutbox.NotificacionPush, mensaje.Tipo);
        Assert.Equal(notificacion.NotificacionId, mensaje.AgregadoId);
        Assert.Equal(EstadoOutbox.Pendiente, mensaje.Estado);
        Assert.Equal(Ahora, mensaje.ProximoIntento);
    }

    [Fact]
    public void Leer_PayloadGuardado_RecuperaLosMismosDatos()
    {
        var notificacion = Notificacion.Crear(Id(1), Id(5), TipoNotificacion.NuevaSolicitud, Id(9), "Víctor solicitó el carro NP300.");
        var mensaje = MensajesPush.CrearMensaje(notificacion, Ahora);

        var payload = MensajesPush.Leer(mensaje.Payload);

        Assert.Equal(notificacion.NotificacionId, payload.NotificacionId);
        Assert.Equal(Id(5), payload.EmpleadoDestinatarioId);
        Assert.Equal(TipoNotificacion.NuevaSolicitud, payload.Tipo);
        Assert.Equal(Id(9), payload.ReferenciaId);
        Assert.Equal("Nueva solicitud de carro", payload.Titulo);
        Assert.Equal("Víctor solicitó el carro NP300.", payload.Cuerpo);
    }

    [Fact]
    public void CrearPayload_SinMensaje_UsaElTituloComoCuerpo()
    {
        var notificacion = Notificacion.Crear(Id(1), Id(5), TipoNotificacion.CambioEstatusForzado, null, null);

        var payload = MensajesPush.CrearPayload(notificacion);

        Assert.Equal("Cambio de estatus del carro", payload.Cuerpo);
    }

    [Fact]
    public void Datos_IncluyeNotificacionTipoYReferencia()
    {
        var notificacion = Notificacion.Crear(Id(1), Id(5), TipoNotificacion.NuevaSolicitud, Id(9), "x");

        var datos = MensajesPush.Datos(MensajesPush.CrearPayload(notificacion));

        Assert.Equal(notificacion.NotificacionId.ToString(), datos["notificacionId"]);
        Assert.Equal("NuevaSolicitud", datos["tipo"]);
        Assert.Equal(Id(9).ToString(), datos["referenciaId"]);
    }

    [Fact]
    public void Datos_SinReferencia_NoIncluyeLaLlave()
    {
        var notificacion = Notificacion.Crear(Id(1), Id(5), TipoNotificacion.CambioEstatusForzado, null, "x");

        var datos = MensajesPush.Datos(MensajesPush.CrearPayload(notificacion));

        Assert.False(datos.ContainsKey("referenciaId"));
    }
}
