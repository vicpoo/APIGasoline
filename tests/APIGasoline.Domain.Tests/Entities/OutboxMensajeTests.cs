using APIGasoline.Domain.Entities;
using APIGasoline.Domain.Enums;
using APIGasoline.Domain.Exceptions;
using APIGasoline.Domain.Outbox;

namespace APIGasoline.Domain.Tests.Entities;

public class OutboxMensajeTests
{
    private static readonly DateTime Ahora = new(2026, 10, 9, 12, 0, 0, DateTimeKind.Utc);
    private static readonly TimeSpan Bloqueo = TimeSpan.FromMinutes(2);
    private const string Payload = "{\"titulo\":\"Prueba\"}";

    // --- Crear ---

    [Fact]
    public void Crear_DatosValidos_QuedaPendienteYListoDeInmediato()
    {
        var mensaje = Construir();

        Assert.Equal(EstadoOutbox.Pendiente, mensaje.Estado);
        Assert.Equal(0, mensaje.Intentos);
        Assert.Equal(Ahora, mensaje.ProximoIntento);
        Assert.Equal(Ahora, mensaje.FechaCreacion);
        Assert.True(mensaje.EstaListo(Ahora));
        Assert.NotEqual(Guid.Empty, mensaje.OutboxMensajeId);
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(1, 0)]
    public void Crear_IdsInvalidos_LanzaDomainException(int empresaId, int agregadoId)
    {
        Assert.Throws<DomainException>(() =>
            OutboxMensaje.Crear(Id(empresaId), TiposOutbox.NotificacionPush, Id(agregadoId), Payload, Ahora));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Crear_SinTipo_LanzaDomainException(string tipo)
    {
        Assert.Throws<DomainException>(() => OutboxMensaje.Crear(Id(1), tipo, Id(2), Payload, Ahora));
    }

    [Fact]
    public void Crear_TipoMayorA100_LanzaDomainException()
    {
        Assert.Throws<DomainException>(() => OutboxMensaje.Crear(Id(1), new string('x', 101), Id(2), Payload, Ahora));
    }

    [Fact]
    public void Crear_SinPayload_LanzaDomainException()
    {
        Assert.Throws<DomainException>(() => OutboxMensaje.Crear(Id(1), TiposOutbox.NotificacionPush, Id(2), " ", Ahora));
    }

    // --- Procesar ---

    [Fact]
    public void MarcarProcesando_LoApartaHastaQueVenzaElBloqueo()
    {
        var mensaje = Construir();

        mensaje.MarcarProcesando(Ahora, Bloqueo);

        Assert.Equal(EstadoOutbox.Procesando, mensaje.Estado);
        Assert.False(mensaje.EstaListo(Ahora.AddMinutes(1)));
        Assert.True(mensaje.EstaListo(Ahora + Bloqueo));
    }

    [Fact]
    public void MarcarProcesando_AntesDeSuHora_LanzaDomainException()
    {
        var mensaje = Construir();
        mensaje.MarcarProcesando(Ahora, Bloqueo);

        Assert.Throws<DomainException>(() => mensaje.MarcarProcesando(Ahora.AddSeconds(30), Bloqueo));
    }

    [Fact]
    public void MarcarEnviado_TerminaYGuardaLaNota()
    {
        var mensaje = Construir();
        mensaje.MarcarProcesando(Ahora, Bloqueo);

        mensaje.MarcarEnviado(Ahora.AddSeconds(1), "  Sin dispositivos activos.  ");

        Assert.Equal(EstadoOutbox.Enviado, mensaje.Estado);
        Assert.Equal(1, mensaje.Intentos);
        Assert.Equal("Sin dispositivos activos.", mensaje.UltimoError);
        Assert.Equal(Ahora.AddSeconds(1), mensaje.FechaProcesado);
        Assert.False(mensaje.EstaListo(Ahora.AddDays(1)));
    }

    [Fact]
    public void MarcarEnviado_YaEnviado_LanzaDomainException()
    {
        var mensaje = Construir();
        mensaje.MarcarEnviado(Ahora);

        Assert.Throws<DomainException>(() => mensaje.MarcarEnviado(Ahora));
    }

    // --- Reintentos ---

    [Fact]
    public void RegistrarFallo_PrimerFallo_ReintentaEn30Segundos()
    {
        var mensaje = Construir();
        mensaje.MarcarProcesando(Ahora, Bloqueo);

        mensaje.RegistrarFallo("Firebase no responde", Ahora);

        Assert.Equal(EstadoOutbox.Pendiente, mensaje.Estado);
        Assert.Equal(1, mensaje.Intentos);
        Assert.Equal(Ahora.AddSeconds(30), mensaje.ProximoIntento);
        Assert.Equal("Firebase no responde", mensaje.UltimoError);
        Assert.Null(mensaje.FechaProcesado);
    }

    [Fact]
    public void RegistrarFallo_QuintoFallo_QuedaFallido()
    {
        var mensaje = Construir();

        for (var i = 0; i < PoliticaReintentos.MaxIntentos; i++)
            mensaje.RegistrarFallo($"Fallo {i + 1}", Ahora);

        Assert.Equal(EstadoOutbox.Fallido, mensaje.Estado);
        Assert.Equal(5, mensaje.Intentos);
        Assert.Equal("Fallo 5", mensaje.UltimoError);
        Assert.Equal(Ahora, mensaje.FechaProcesado);
    }

    [Fact]
    public void RegistrarFallo_ErrorMuyLargo_SeRecortaA500()
    {
        var mensaje = Construir();

        mensaje.RegistrarFallo(new string('x', 800), Ahora);

        Assert.Equal(OutboxMensaje.UltimoErrorMaxLength, mensaje.UltimoError!.Length);
        Assert.EndsWith("...", mensaje.UltimoError);
    }

    [Fact]
    public void Reintentar_Fallido_VuelveAPendienteDesdeCero()
    {
        var mensaje = Construir();
        for (var i = 0; i < PoliticaReintentos.MaxIntentos; i++)
            mensaje.RegistrarFallo("Fallo", Ahora);

        mensaje.Reintentar(Ahora.AddHours(1));

        Assert.Equal(EstadoOutbox.Pendiente, mensaje.Estado);
        Assert.Equal(0, mensaje.Intentos);
        Assert.Null(mensaje.FechaProcesado);
        Assert.True(mensaje.EstaListo(Ahora.AddHours(1)));
    }

    [Fact]
    public void Reintentar_NoFallido_LanzaDomainException()
    {
        var mensaje = Construir();

        Assert.Throws<DomainException>(() => mensaje.Reintentar(Ahora));
    }

    private static OutboxMensaje Construir()
        => OutboxMensaje.Crear(Id(1), TiposOutbox.NotificacionPush, Id(2), Payload, Ahora);
}
