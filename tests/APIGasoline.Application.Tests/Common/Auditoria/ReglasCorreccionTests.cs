using APIGasoline.Application.Common.Auditoria;
using APIGasoline.Application.Common.Constants;
using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Common.Interfaces;
using APIGasoline.Domain.Entities;
using APIGasoline.Domain.Enums;

namespace APIGasoline.Application.Tests.Common.Auditoria;

/// <summary>
/// Quién puede corregir: el operador mientras el viaje está en curso;
/// con el viaje cerrado, solo un Confirmador o Gestor de activos. Siempre con motivo.
/// </summary>
public class ReglasCorreccionTests
{
    private static readonly DateTime Inicio = new(2026, 10, 5, 8, 30, 0, DateTimeKind.Utc);
    private static readonly Guid Empresa = Id(1);
    private static readonly Guid Operador = Id(3);

    [Fact]
    public void OperadorConViajeEnCurso_PuedeCorregir()
    {
        var motivo = ReglasCorreccion.Validar(Usuario(Operador), ViajeEnCurso(), "  Escribí mal el número  ");

        Assert.Equal("Escribí mal el número", motivo);
    }

    [Fact]
    public void OperadorConViajeCerrado_NoPuedeCorregir()
    {
        Assert.Throws<ForbiddenException>(() => ReglasCorreccion.Validar(Usuario(Operador), ViajeCerrado(), "Error"));
    }

    [Theory]
    [InlineData(NombresRol.Confirmador)]
    [InlineData(NombresRol.GestorActivos)]
    public void SupervisorConViajeCerrado_PuedeCorregir(string rol)
    {
        var motivo = ReglasCorreccion.Validar(Usuario(Id(9), rol), ViajeCerrado(), "Ticket mal capturado");

        Assert.Equal("Ticket mal capturado", motivo);
    }

    [Fact]
    public void OtroEmpleadoConViajeEnCurso_NoPuedeCorregir()
    {
        Assert.Throws<ForbiddenException>(() => ReglasCorreccion.Validar(Usuario(Id(9)), ViajeEnCurso(), "Error"));
    }

    [Fact]
    public void EmpleadoDeOtraEmpresa_NoPuedeCorregir()
    {
        var usuario = Usuario(Id(9), NombresRol.Confirmador) with { EmpresaId = Id(2) };

        Assert.Throws<ForbiddenException>(() => ReglasCorreccion.Validar(usuario, ViajeCerrado(), "Error"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("   ")]
    public void SinMotivo_LanzaBadRequest(string? motivo)
    {
        Assert.Throws<BadRequestException>(() => ReglasCorreccion.Validar(Usuario(Operador), ViajeEnCurso(), motivo));
    }

    [Fact]
    public void SinIniciarSesion_LanzaUnauthorized()
    {
        var anonimo = new UsuarioFalso(false, null, null, null, []);

        Assert.Throws<UnauthorizedException>(() => ReglasCorreccion.Validar(anonimo, ViajeEnCurso(), "Error"));
    }

    private static Viaje ViajeEnCurso()
        => Viaje.Iniciar(Empresa, Id(10), Id(20), Operador, Inicio, null, null, 15000m);

    private static Viaje ViajeCerrado()
    {
        var viaje = ViajeEnCurso();
        viaje.Finalizar(Inicio.AddHours(2), null, null, 15100m);
        return viaje;
    }

    private static UsuarioFalso Usuario(Guid empleadoId, params string[] roles)
        => new(true, Id(50), empleadoId, Empresa, roles);

    private sealed record UsuarioFalso(
        bool EstaAutenticado,
        Guid? UsuarioId,
        Guid? EmpleadoId,
        Guid? EmpresaId,
        IReadOnlyCollection<string> Roles) : IUsuarioActual
    {
        public OrigenAuditoria Origen => OrigenAuditoria.Web;

        public bool TieneRol(string rol) => Roles.Contains(rol);
    }
}
