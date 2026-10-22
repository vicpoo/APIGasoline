using APIGasoline.Application.Common.Auditoria;
using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Common.Interfaces;
using APIGasoline.Application.Common.Security;
using APIGasoline.Domain.Entities;
using APIGasoline.Domain.Enums;

namespace APIGasoline.Application.Tests.Common.Auditoria;

/// <summary>
/// Quién puede corregir: el operador mientras el viaje está en curso;
/// con el viaje cerrado, solo quien tenga el privilegio viaje.corregir_cerrado
/// (sin importar cómo se llame su rol). Siempre con motivo.
/// </summary>
public class ReglasCorreccionTests
{
    private static readonly DateTime Inicio = new(2026, 10, 5, 8, 30, 0, DateTimeKind.Utc);
    private static readonly Guid Empresa = Id(1);
    private static readonly Guid Operador = Id(3);

    [Fact]
    public async Task OperadorConViajeEnCurso_PuedeCorregir()
    {
        var motivo = await ReglasCorreccion.ValidarAsync(Usuario(Operador), ViajeEnCurso(), "  Escribí mal el número  ");

        Assert.Equal("Escribí mal el número", motivo);
    }

    [Fact]
    public async Task OperadorConViajeCerrado_NoPuedeCorregir()
    {
        await Assert.ThrowsAsync<ForbiddenException>(() => ReglasCorreccion.ValidarAsync(Usuario(Operador), ViajeCerrado(), "Error"));
    }

    [Theory]
    [InlineData("Confirmador")]
    [InlineData("Jefe de flota")]
    [InlineData("Supervisor de ruta")]
    public async Task ConPrivilegioCorregirCerrado_PuedeCorregir_SinImportarElNombreDelRol(string rol)
    {
        var usuario = Usuario(Id(9), Permisos.ViajeCorregirCerrado) with { Roles = [rol] };

        var motivo = await ReglasCorreccion.ValidarAsync(usuario, ViajeCerrado(), "Ticket mal capturado");

        Assert.Equal("Ticket mal capturado", motivo);
    }

    [Fact]
    public async Task RolLlamadoConfirmadorSinElPrivilegio_NoPuedeCorregirCerrado()
    {
        var usuario = Usuario(Id(9)) with { Roles = ["Confirmador"] };

        await Assert.ThrowsAsync<ForbiddenException>(() => ReglasCorreccion.ValidarAsync(usuario, ViajeCerrado(), "Error"));
    }

    [Fact]
    public async Task OtroEmpleadoConViajeEnCurso_NoPuedeCorregir()
    {
        await Assert.ThrowsAsync<ForbiddenException>(() => ReglasCorreccion.ValidarAsync(Usuario(Id(9)), ViajeEnCurso(), "Error"));
    }

    [Fact]
    public async Task EmpleadoDeOtraEmpresa_NoPuedeCorregir()
    {
        var usuario = Usuario(Id(9), Permisos.ViajeCorregirCerrado) with { EmpresaId = Id(2) };

        await Assert.ThrowsAsync<ForbiddenException>(() => ReglasCorreccion.ValidarAsync(usuario, ViajeCerrado(), "Error"));
    }

    [Fact]
    public async Task ConMultiempresa_PuedeCorregirDeOtraEmpresa()
    {
        var usuario = Usuario(Id(9), Permisos.ViajeCorregirCerrado, Permisos.PlataformaMultiempresa) with { EmpresaId = null };

        var motivo = await ReglasCorreccion.ValidarAsync(usuario, ViajeCerrado(), "Ajuste de soporte");

        Assert.Equal("Ajuste de soporte", motivo);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("   ")]
    public async Task SinMotivo_LanzaBadRequest(string? motivo)
    {
        await Assert.ThrowsAsync<BadRequestException>(() => ReglasCorreccion.ValidarAsync(Usuario(Operador), ViajeEnCurso(), motivo));
    }

    [Fact]
    public async Task SinIniciarSesion_LanzaUnauthorized()
    {
        var anonimo = new UsuarioFalso(false, null, null, null, [], new HashSet<string>());

        await Assert.ThrowsAsync<UnauthorizedException>(() => ReglasCorreccion.ValidarAsync(anonimo, ViajeEnCurso(), "Error"));
    }

    private static Viaje ViajeEnCurso()
        => Viaje.Iniciar(Empresa, Id(10), Id(20), Operador, Inicio, null, null, 15000m);

    private static Viaje ViajeCerrado()
    {
        var viaje = ViajeEnCurso();
        viaje.Finalizar(Inicio.AddHours(2), null, null, 15100m);
        return viaje;
    }

    private static UsuarioFalso Usuario(Guid empleadoId, params string[] privilegios)
        => new(true, Id(50), empleadoId, Empresa, [], new HashSet<string>(privilegios));

    private sealed record UsuarioFalso(
        bool EstaAutenticado,
        Guid? UsuarioId,
        Guid? EmpleadoId,
        Guid? EmpresaId,
        IReadOnlyCollection<string> Roles,
        IReadOnlySet<string> Privilegios) : IUsuarioActual
    {
        public OrigenAuditoria Origen => OrigenAuditoria.Web;

        public Task<IReadOnlySet<string>> GetPrivilegiosAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(Privilegios);

        public Task<bool> TienePrivilegioAsync(string clave, CancellationToken cancellationToken = default)
            => Task.FromResult(Privilegios.Contains(clave));
    }
}
