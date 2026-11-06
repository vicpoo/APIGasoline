using APIGasoline.Domain.Entities;
using APIGasoline.Domain.Enums;
using APIGasoline.Domain.Exceptions;

namespace APIGasoline.Domain.Tests.Entities;

/// <summary>
/// Sesión de usuario (refresh token): rotación, expiración deslizante con tope absoluto,
/// detección de reuso y revocación.
/// </summary>
public class SesionUsuarioTests
{
    private static readonly DateTime Ahora = new(2026, 11, 6, 12, 0, 0);
    private static readonly TimeSpan Duracion = TimeSpan.FromDays(30);
    private static readonly TimeSpan DuracionMaxima = TimeSpan.FromDays(90);

    private static readonly string HashA = new('a', 64);
    private static readonly string HashB = new('b', 64);
    private static readonly string HashC = new('c', 64);

    [Fact]
    public void Iniciar_DatosValidos_QuedaActivaConExpiraciones()
    {
        var sesion = Construir();

        Assert.Equal(Id(1), sesion.UsuarioId);
        Assert.Equal(Id(2), sesion.EmpresaId);
        Assert.Equal(HashA, sesion.TokenHash);
        Assert.Null(sesion.TokenHashAnterior);
        Assert.Equal(PlataformaDispositivo.Android, sesion.Plataforma);
        Assert.Equal(Ahora, sesion.FechaCreacion);
        Assert.Equal(Ahora, sesion.FechaUltimoUso);
        Assert.Equal(Ahora + Duracion, sesion.ExpiraEn);
        Assert.Equal(Ahora + DuracionMaxima, sesion.ExpiraAbsolutaEn);
        Assert.True(sesion.EstaActiva(Ahora));
        Assert.False(sesion.EstaRevocada);
    }

    [Fact]
    public void Iniciar_DatosDelCliente_SeRecortanYSeLimpian()
    {
        var sesion = SesionUsuario.Iniciar(
            Id(1), null, HashA, null, "  " + new string('x', 150) + "  ", "   ", new string('u', 400),
            Ahora, Duracion, DuracionMaxima);

        Assert.Null(sesion.EmpresaId);
        Assert.Null(sesion.Plataforma);
        Assert.Equal(SesionUsuario.NombreDispositivoMaxLength, sesion.NombreDispositivo!.Length);
        Assert.Null(sesion.Ip);
        Assert.Equal(SesionUsuario.UserAgentMaxLength, sesion.UserAgent!.Length);
    }

    [Fact]
    public void Iniciar_SinUsuario_LanzaDomainException()
    {
        Assert.Throws<DomainException>(() => Construir(usuarioId: 0));
    }

    [Theory]
    [InlineData("")]
    [InlineData("abc")]
    public void Iniciar_HashInvalido_LanzaDomainException(string hash)
    {
        Assert.Throws<DomainException>(() => Construir(hash: hash));
    }

    [Fact]
    public void Iniciar_DuracionMaximaMenorALaDuracion_LanzaDomainException()
    {
        Assert.Throws<DomainException>(() =>
            SesionUsuario.Iniciar(Id(1), null, HashA, null, null, null, null, Ahora, Duracion, TimeSpan.FromDays(1)));
    }

    [Fact]
    public void EstaActiva_DespuesDeExpirar_EsFalse()
    {
        var sesion = Construir();

        Assert.True(sesion.EstaActiva(Ahora + Duracion - TimeSpan.FromSeconds(1)));
        Assert.False(sesion.EstaActiva(Ahora + Duracion));
    }

    [Fact]
    public void Rotar_ReemplazaElTokenYGuardaElAnterior()
    {
        var sesion = Construir();
        var despues = Ahora.AddDays(10);

        sesion.Rotar(HashB, despues, Duracion, "10.0.0.9", null);

        Assert.Equal(HashB, sesion.TokenHash);
        Assert.Equal(HashA, sesion.TokenHashAnterior);
        Assert.Equal(despues, sesion.FechaRotacion);
        Assert.Equal(despues, sesion.FechaUltimoUso);
        Assert.Equal("10.0.0.9", sesion.Ip);
        Assert.Equal("Agente", sesion.UserAgent); // sin dato nuevo se conserva el anterior
    }

    [Fact]
    public void Rotar_ExtiendeLaExpiracionDeslizante()
    {
        var sesion = Construir();
        var despues = Ahora.AddDays(20);

        sesion.Rotar(HashB, despues, Duracion, null, null);

        Assert.Equal(despues + Duracion, sesion.ExpiraEn);
    }

    [Fact]
    public void Rotar_NuncaPasaDelTopeAbsoluto()
    {
        var sesion = Construir();
        var casiAlTope = Ahora.AddDays(80);

        sesion.Rotar(HashB, Ahora.AddDays(29), Duracion, null, null);
        sesion.Rotar(HashC, Ahora.AddDays(58), Duracion, null, null);
        sesion.Rotar(HashA, casiAlTope, Duracion, null, null);

        Assert.Equal(sesion.ExpiraAbsolutaEn, sesion.ExpiraEn);
        Assert.False(sesion.EstaActiva(Ahora + DuracionMaxima));
    }

    [Fact]
    public void Rotar_SesionExpirada_LanzaDomainException()
    {
        var sesion = Construir();

        Assert.Throws<DomainException>(() => sesion.Rotar(HashB, Ahora + Duracion, Duracion, null, null));
    }

    [Fact]
    public void Rotar_SesionRevocada_LanzaDomainException()
    {
        var sesion = Construir();
        sesion.Revocar(MotivoRevocacionSesion.CierreSesion, Ahora);

        Assert.Throws<DomainException>(() => sesion.Rotar(HashB, Ahora.AddMinutes(1), Duracion, null, null));
    }

    [Fact]
    public void Rotar_MismoHash_LanzaDomainException()
    {
        var sesion = Construir();

        Assert.Throws<DomainException>(() => sesion.Rotar(HashA, Ahora.AddMinutes(1), Duracion, null, null));
    }

    [Fact]
    public void EsTokenAnterior_SoloElHashReemplazado()
    {
        var sesion = Construir();
        Assert.False(sesion.EsTokenAnterior(HashA)); // sin rotar no hay anterior

        sesion.Rotar(HashB, Ahora.AddMinutes(1), Duracion, null, null);

        Assert.True(sesion.EsTokenAnterior(HashA));
        Assert.False(sesion.EsTokenAnterior(HashB));
        Assert.False(sesion.EsTokenAnterior(HashC));
    }

    [Fact]
    public void RotadaRecientemente_DentroYFueraDeLaGracia()
    {
        var sesion = Construir();
        var gracia = TimeSpan.FromSeconds(30);
        Assert.False(sesion.RotadaRecientemente(Ahora, gracia));

        sesion.Rotar(HashB, Ahora, Duracion, null, null);

        Assert.True(sesion.RotadaRecientemente(Ahora.AddSeconds(30), gracia));
        Assert.False(sesion.RotadaRecientemente(Ahora.AddSeconds(31), gracia));
    }

    [Fact]
    public void Revocar_GuardaMotivoYQuien()
    {
        var sesion = Construir();

        sesion.Revocar(MotivoRevocacionSesion.CerradaPorAdministrador, Ahora.AddHours(1), Id(9));

        Assert.True(sesion.EstaRevocada);
        Assert.False(sesion.EstaActiva(Ahora.AddHours(1)));
        Assert.Equal(Ahora.AddHours(1), sesion.FechaRevocacion);
        Assert.Equal(MotivoRevocacionSesion.CerradaPorAdministrador, sesion.MotivoRevocacion);
        Assert.Equal(Id(9), sesion.RevocadaPorUsuarioId);
    }

    [Fact]
    public void Revocar_DosVeces_ConservaElPrimerMotivo()
    {
        var sesion = Construir();

        sesion.Revocar(MotivoRevocacionSesion.ReusoDetectado, Ahora);
        sesion.Revocar(MotivoRevocacionSesion.CierreSesion, Ahora.AddDays(1));

        Assert.Equal(MotivoRevocacionSesion.ReusoDetectado, sesion.MotivoRevocacion);
        Assert.Equal(Ahora, sesion.FechaRevocacion);
    }

    [Fact]
    public void Revocar_MotivoInvalido_LanzaDomainException()
    {
        var sesion = Construir();

        Assert.Throws<DomainException>(() => sesion.Revocar((MotivoRevocacionSesion)99, Ahora));
    }

    private static SesionUsuario Construir(int usuarioId = 1, string? hash = null)
        => SesionUsuario.Iniciar(
            Id(usuarioId), Id(2), hash ?? HashA, PlataformaDispositivo.Android, "Moto G84", "192.168.1.10", "Agente",
            Ahora, Duracion, DuracionMaxima);
}
