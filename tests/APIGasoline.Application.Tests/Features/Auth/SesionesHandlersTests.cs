using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Common.Sesiones;
using APIGasoline.Application.Features.Auth.Commands;
using APIGasoline.Application.Features.Sesiones.Commands;
using APIGasoline.Application.Features.Sesiones.Queries;
using APIGasoline.Domain.Entities;
using APIGasoline.Domain.Enums;

namespace APIGasoline.Application.Tests.Features.Auth;

/// <summary>Logout, lista de sesiones y cierre de sesiones (propias o por un administrador).</summary>
public class SesionesHandlersTests
{
    private static readonly DateTime Ahora = new(2026, 11, 6, 12, 0, 0, DateTimeKind.Local);
    private static readonly SesionesSettings Settings = new();

    private readonly RelojFijo _reloj = new(Ahora);
    private readonly Domain.Interfaces.IUnitOfWork _unitOfWork;
    private readonly UnidadFalsa _uow;

    public SesionesHandlersTests()
    {
        (_unitOfWork, _uow) = UnidadFalsa.Crear();
    }

    [Fact]
    public async Task Logout_CierraLaSesionDelToken()
    {
        var (sesion, token) = Sesion(Id(1));

        await new CerrarSesionCommandHandler(_unitOfWork, _reloj).Handle(new CerrarSesionCommand(token), CancellationToken.None);

        Assert.Equal(MotivoRevocacionSesion.CierreSesion, sesion.MotivoRevocacion);
    }

    [Theory]
    [InlineData("")]
    [InlineData("no-existe")]
    public async Task Logout_TokenInvalido_NoFallaNiGuarda(string token)
    {
        Sesion(Id(1));

        await new CerrarSesionCommandHandler(_unitOfWork, _reloj).Handle(new CerrarSesionCommand(token), CancellationToken.None);

        Assert.Equal(0, _uow.Guardados);
    }

    [Fact]
    public async Task GetSesiones_SoloActivasYMarcaLaActual()
    {
        var (actual, _) = Sesion(Id(1));
        var (otra, _) = Sesion(Id(1));
        var (cerrada, _) = Sesion(Id(1));
        cerrada.Revocar(MotivoRevocacionSesion.CierreSesion, Ahora);
        Sesion(Id(2));

        var handler = new GetSesionesUsuarioQueryHandler(_unitOfWork, new UsuarioActualFalso(Id(1), actual.SesionUsuarioId), _reloj);
        var sesiones = await handler.Handle(new GetSesionesUsuarioQuery(Id(1)), CancellationToken.None);

        Assert.Equal(2, sesiones.Count);
        Assert.True(sesiones.Single(s => s.SesionId == actual.SesionUsuarioId).EsActual);
        Assert.False(sesiones.Single(s => s.SesionId == otra.SesionUsuarioId).EsActual);
    }

    [Fact]
    public async Task CerrarUna_Propia_MotivoCerradaPorUsuario()
    {
        var (sesion, _) = Sesion(Id(1));
        var handler = new CerrarSesionUsuarioCommandHandler(_unitOfWork, new UsuarioActualFalso(Id(1), null), _reloj);

        await handler.Handle(new CerrarSesionUsuarioCommand(Id(1), sesion.SesionUsuarioId), CancellationToken.None);

        Assert.Equal(MotivoRevocacionSesion.CerradaPorUsuario, sesion.MotivoRevocacion);
        Assert.Equal(Id(1), sesion.RevocadaPorUsuarioId);
    }

    [Fact]
    public async Task CerrarUna_PorAdministrador_MotivoCerradaPorAdministrador()
    {
        UsuarioVisible(Id(1));
        var (sesion, _) = Sesion(Id(1));
        var handler = new CerrarSesionUsuarioCommandHandler(_unitOfWork, new UsuarioActualFalso(Id(9), null), _reloj);

        await handler.Handle(new CerrarSesionUsuarioCommand(Id(1), sesion.SesionUsuarioId), CancellationToken.None);

        Assert.Equal(MotivoRevocacionSesion.CerradaPorAdministrador, sesion.MotivoRevocacion);
        Assert.Equal(Id(9), sesion.RevocadaPorUsuarioId);
    }

    [Fact]
    public async Task CerrarUna_SesionDeOtroUsuario_Lanza404()
    {
        var (ajena, _) = Sesion(Id(2));
        var handler = new CerrarSesionUsuarioCommandHandler(_unitOfWork, new UsuarioActualFalso(Id(1), null), _reloj);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(new CerrarSesionUsuarioCommand(Id(1), ajena.SesionUsuarioId), CancellationToken.None));

        Assert.False(ajena.EstaRevocada);
    }

    [Fact]
    public async Task CerrarOtras_ConservaLaSesionActual()
    {
        var (actual, _) = Sesion(Id(1));
        var (otra1, _) = Sesion(Id(1));
        var (otra2, _) = Sesion(Id(1));
        var handler = new CerrarSesionesUsuarioCommandHandler(_unitOfWork, new UsuarioActualFalso(Id(1), actual.SesionUsuarioId), _reloj);

        var cerradas = await handler.Handle(new CerrarSesionesUsuarioCommand(Id(1), ConservarActual: true), CancellationToken.None);

        Assert.Equal(2, cerradas);
        Assert.False(actual.EstaRevocada);
        Assert.True(otra1.EstaRevocada);
        Assert.True(otra2.EstaRevocada);
    }

    [Fact]
    public async Task CerrarTodas_PorAdministrador_CierraTodas()
    {
        UsuarioVisible(Id(1));
        var (s1, _) = Sesion(Id(1));
        var (s2, _) = Sesion(Id(1));
        var (deOtro, _) = Sesion(Id(2));
        var handler = new CerrarSesionesUsuarioCommandHandler(_unitOfWork, new UsuarioActualFalso(Id(9), Guid.NewGuid()), _reloj);

        var cerradas = await handler.Handle(new CerrarSesionesUsuarioCommand(Id(1), ConservarActual: true), CancellationToken.None);

        Assert.Equal(2, cerradas);
        Assert.Equal(MotivoRevocacionSesion.CerradaPorAdministrador, s1.MotivoRevocacion);
        Assert.True(s2.EstaRevocada);
        Assert.False(deOtro.EstaRevocada);
    }

    [Fact]
    public async Task RevocarDeEmpresa_SoloLasDeEsaEmpresa()
    {
        var (deEmpresa, _) = Sesion(Id(1), empresaId: Id(50));
        var (deOtra, _) = Sesion(Id(2), empresaId: Id(60));

        var cerradas = await _unitOfWork.RevocarDeEmpresaAsync(Id(50), MotivoRevocacionSesion.EmpresaEliminada, Ahora);

        Assert.Equal(1, cerradas);
        Assert.Equal(MotivoRevocacionSesion.EmpresaEliminada, deEmpresa.MotivoRevocacion);
        Assert.False(deOtra.EstaRevocada);
    }

    // El usuario de otra empresa no se ve (filtro por empresa): sus sesiones "no existen"
    [Fact]
    public async Task GetSesiones_DeUnUsuarioQueNoSeVe_Lanza404()
    {
        Sesion(Id(1));
        var handler = new GetSesionesUsuarioQueryHandler(_unitOfWork, new UsuarioActualFalso(Id(9), null), _reloj);

        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(new GetSesionesUsuarioQuery(Id(1)), CancellationToken.None));
    }

    [Fact]
    public async Task CerrarUna_DeUnUsuarioQueNoSeVe_Lanza404YNoLaCierra()
    {
        var (sesion, _) = Sesion(Id(1));
        var handler = new CerrarSesionUsuarioCommandHandler(_unitOfWork, new UsuarioActualFalso(Id(9), null), _reloj);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(new CerrarSesionUsuarioCommand(Id(1), sesion.SesionUsuarioId), CancellationToken.None));

        Assert.False(sesion.EstaRevocada);
    }

    [Fact]
    public async Task CerrarTodas_DeUnUsuarioQueNoSeVe_Lanza404YNoCierraNada()
    {
        var (sesion, _) = Sesion(Id(1));
        var handler = new CerrarSesionesUsuarioCommandHandler(_unitOfWork, new UsuarioActualFalso(Id(9), null), _reloj);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(new CerrarSesionesUsuarioCommand(Id(1), ConservarActual: false), CancellationToken.None));

        Assert.False(sesion.EstaRevocada);
    }

    private void UsuarioVisible(Guid usuarioId)
        => _uow.Usuarios[usuarioId] = Usuario.Crear($"u{usuarioId:N}@pruebas.local", "hash");

    private (SesionUsuario Sesion, string Token) Sesion(Guid usuarioId, Guid? empresaId = null)
    {
        var token = TokenRefresh.Generar();
        var sesion = SesionUsuario.Iniciar(
            usuarioId, empresaId, TokenRefresh.Hash(token), null, null, null, null,
            Ahora.AddMinutes(-_uow.Sesiones.Sesiones.Count), Settings.DuracionRefresh, Settings.DuracionMaxima);

        _uow.Sesiones.Sesiones.Add(sesion);
        return (sesion, token);
    }
}
