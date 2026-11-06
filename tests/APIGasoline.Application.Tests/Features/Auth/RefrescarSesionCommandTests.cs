using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Common.Sesiones;
using APIGasoline.Application.Features.Auth.Commands;
using APIGasoline.Domain.Entities;
using APIGasoline.Domain.Enums;
using Microsoft.Extensions.Logging.Abstractions;

namespace APIGasoline.Application.Tests.Features.Auth;

/// <summary>
/// POST /api/auth/refresh: rotación del refresh token, detección de reuso (robo), periodo de gracia
/// para peticiones simultáneas y nueva validación del usuario.
/// </summary>
public class RefrescarSesionCommandTests
{
    private static readonly DateTime Inicio = new(2026, 11, 6, 12, 0, 0, DateTimeKind.Local);
    private static readonly SesionesSettings Settings = new();

    private readonly RelojFijo _reloj = new(Inicio);
    private readonly JwtFalso _jwt = new();
    private readonly UnidadFalsa _uow;
    private readonly RefrescarSesionCommandHandler _handler;

    public RefrescarSesionCommandTests()
    {
        var (unitOfWork, estado) = UnidadFalsa.Crear();
        _uow = estado;
        _handler = new RefrescarSesionCommandHandler(unitOfWork, _jwt, _reloj, Settings, NullLogger<RefrescarSesionCommandHandler>.Instance);
    }

    [Fact]
    public async Task Refrescar_TokenVigente_RotaYDevuelveTokensNuevos()
    {
        var (sesion, token) = SesionDe(UsuarioActivo());
        _reloj.Now = Inicio.AddDays(5);

        var resultado = await Refrescar(token);

        Assert.NotEqual(token, resultado.RefreshToken);
        Assert.Equal(TokenRefresh.Hash(resultado.RefreshToken), sesion.TokenHash);
        Assert.Equal(TokenRefresh.Hash(token), sesion.TokenHashAnterior);
        Assert.Equal(sesion.SesionUsuarioId, resultado.SesionId);
        Assert.Equal(sesion.SesionUsuarioId, _jwt.Ultimo!.SesionId);
        Assert.Equal(Inicio.AddDays(5) + Settings.DuracionRefresh, sesion.ExpiraEn);
        Assert.Equal(1, _uow.Guardados);
    }

    [Fact]
    public async Task Refrescar_ConElTokenNuevo_VuelveARotar()
    {
        var (sesion, token) = SesionDe(UsuarioActivo());

        var primera = await Refrescar(token);
        _reloj.Now = Inicio.AddHours(1);
        var segunda = await Refrescar(primera.RefreshToken);

        Assert.Equal(TokenRefresh.Hash(segunda.RefreshToken), sesion.TokenHash);
        Assert.False(sesion.EstaRevocada);
    }

    [Fact]
    public async Task Refrescar_TokenDesconocido_Lanza401()
    {
        SesionDe(UsuarioActivo());

        await Assert.ThrowsAsync<UnauthorizedException>(() => Refrescar("token-que-no-existe"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Refrescar_TokenVacio_Lanza401(string token)
    {
        await Assert.ThrowsAsync<UnauthorizedException>(() => Refrescar(token));
    }

    [Fact]
    public async Task Refrescar_TokenAnteriorFueraDeLaGracia_CierraLaSesionCompleta()
    {
        var (sesion, robado) = SesionDe(UsuarioActivo());
        var renovado = await Refrescar(robado); // el dueño renueva
        _reloj.Now = Inicio.AddMinutes(10);     // el atacante usa el token viejo más tarde

        await Assert.ThrowsAsync<UnauthorizedException>(() => Refrescar(robado));

        Assert.True(sesion.EstaRevocada);
        Assert.Equal(MotivoRevocacionSesion.ReusoDetectado, sesion.MotivoRevocacion);

        // El token "bueno" también deja de servir: hay que volver a iniciar sesión
        await Assert.ThrowsAsync<UnauthorizedException>(() => Refrescar(renovado.RefreshToken));
    }

    [Fact]
    public async Task Refrescar_TokenAnteriorDentroDeLaGracia_RechazaSinCerrarLaSesion()
    {
        var (sesion, token) = SesionDe(UsuarioActivo());
        var renovado = await Refrescar(token);
        _reloj.Now = Inicio.AddSeconds(5); // segunda petición simultánea del mismo celular

        await Assert.ThrowsAsync<UnauthorizedException>(() => Refrescar(token));

        Assert.False(sesion.EstaRevocada);
        var otraVez = await Refrescar(renovado.RefreshToken);
        Assert.Equal(TokenRefresh.Hash(otraVez.RefreshToken), sesion.TokenHash);
    }

    [Fact]
    public async Task Refrescar_SesionExpirada_Lanza401()
    {
        var (_, token) = SesionDe(UsuarioActivo());
        _reloj.Now = Inicio + Settings.DuracionRefresh;

        await Assert.ThrowsAsync<UnauthorizedException>(() => Refrescar(token));
    }

    [Fact]
    public async Task Refrescar_SesionCerrada_Lanza401()
    {
        var (sesion, token) = SesionDe(UsuarioActivo());
        sesion.Revocar(MotivoRevocacionSesion.CierreSesion, Inicio);

        await Assert.ThrowsAsync<UnauthorizedException>(() => Refrescar(token));
    }

    [Fact]
    public async Task Refrescar_UsuarioDesactivado_CierraLaSesionYLanza401()
    {
        var usuario = UsuarioActivo();
        var (sesion, token) = SesionDe(usuario);
        usuario.Desactivar();

        await Assert.ThrowsAsync<UnauthorizedException>(() => Refrescar(token));

        Assert.Equal(MotivoRevocacionSesion.AccesoRevocado, sesion.MotivoRevocacion);
    }

    [Fact]
    public async Task Refrescar_UsuarioBorrado_CierraLaSesionYLanza401()
    {
        var usuario = UsuarioActivo();
        var (sesion, token) = SesionDe(usuario);
        _uow.Usuarios.Remove(usuario.UsuarioId);

        await Assert.ThrowsAsync<UnauthorizedException>(() => Refrescar(token));

        Assert.True(sesion.EstaRevocada);
    }

    [Fact]
    public async Task Refrescar_OtraPeticionGanoLaRotacion_Lanza401()
    {
        var (_, token) = SesionDe(UsuarioActivo());
        _uow.FallarAlGuardar = new DbUpdateConcurrencyException();

        await Assert.ThrowsAsync<UnauthorizedException>(() => Refrescar(token));
    }

    [Fact]
    public async Task Refrescar_LlegaAlTopeAbsoluto_ExpiraAunqueSeUse()
    {
        var (sesion, token) = SesionDe(UsuarioActivo());

        for (var dia = 25; dia < Settings.RefreshMaxDias; dia += 25)
        {
            _reloj.Now = Inicio.AddDays(dia);
            token = (await Refrescar(token)).RefreshToken;
        }

        Assert.Equal(sesion.ExpiraAbsolutaEn, sesion.ExpiraEn);
        _reloj.Now = sesion.ExpiraAbsolutaEn;
        await Assert.ThrowsAsync<UnauthorizedException>(() => Refrescar(token));
    }

    private Task<Application.Features.Auth.DTOs.SesionTokensDto> Refrescar(string token)
        => _handler.Handle(new RefrescarSesionCommand(token), CancellationToken.None);

    private Usuario UsuarioActivo()
    {
        var usuario = Usuario.Crear("chofer@empresa.com", "hash");
        _uow.Usuarios[usuario.UsuarioId] = usuario;
        return usuario;
    }

    private (SesionUsuario Sesion, string Token) SesionDe(Usuario usuario)
    {
        var token = TokenRefresh.Generar();
        var sesion = SesionUsuario.Iniciar(
            usuario.UsuarioId, null, TokenRefresh.Hash(token), PlataformaDispositivo.Android, null, null, null,
            Inicio, Settings.DuracionRefresh, Settings.DuracionMaxima);

        _uow.Sesiones.Sesiones.Add(sesion);
        return (sesion, token);
    }
}
