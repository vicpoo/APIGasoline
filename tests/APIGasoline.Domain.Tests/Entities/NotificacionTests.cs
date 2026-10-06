using APIGasoline.Domain.Entities;
using APIGasoline.Domain.Enums;
using APIGasoline.Domain.Exceptions;

namespace APIGasoline.Domain.Tests.Entities;

/// <summary>
/// Reglas de la notificación: nace sin leer; el mensaje es informativo, así que si es muy largo
/// se recorta con "..." en lugar de fallar.
/// </summary>
public class NotificacionTests
{
    [Fact]
    public void Crear_DatosValidos_NaceSinLeer()
    {
        var notificacion = ConstruirNotificacion();

        Assert.False(notificacion.Leida);
        Assert.Equal(TipoNotificacion.NuevaSolicitud, notificacion.Tipo);
    }

    [Fact]
    public void MarcarLeida_SinLeer_QuedaLeida()
    {
        var notificacion = ConstruirNotificacion();

        notificacion.MarcarLeida();

        Assert.True(notificacion.Leida);
    }

    [Fact]
    public void Crear_MensajeMayorA255_SeRecortaA255ConPuntosSuspensivos()
    {
        var notificacion = ConstruirNotificacion(mensaje: new string('x', 300));

        Assert.Equal(255, notificacion.Mensaje!.Length);
        Assert.EndsWith("...", notificacion.Mensaje);
    }

    [Fact]
    public void Crear_MensajeEnBlanco_SeGuardaComoNull()
    {
        var notificacion = ConstruirNotificacion(mensaje: "   ");

        Assert.Null(notificacion.Mensaje);
    }

    [Fact]
    public void Crear_TipoFueraDelEnum_LanzaDomainException()
    {
        Assert.Throws<DomainException>(() => ConstruirNotificacion(tipo: (TipoNotificacion)99));
    }

    [Theory]
    [InlineData(0, 1, 1)]
    [InlineData(1, 0, 1)]
    [InlineData(1, 1, 0)]  // referencia con id inválido
    public void Crear_IdsInvalidos_LanzaDomainException(int empresaId, int destinatarioId, int? referenciaId)
    {
        Assert.Throws<DomainException>(() => ConstruirNotificacion(empresaId, destinatarioId, referenciaId: referenciaId));
    }

    private static Notificacion ConstruirNotificacion(
        int empresaId = 1,
        int destinatarioId = 2,
        TipoNotificacion tipo = TipoNotificacion.NuevaSolicitud,
        int? referenciaId = 1,
        string? mensaje = "Nueva solicitud de reserva")
        => Notificacion.Crear(empresaId, destinatarioId, tipo, referenciaId, mensaje);
}
