using APIGasoline.Application.Features.Asignaciones.Common;
using APIGasoline.Domain.Entities;
using APIGasoline.Domain.Enums;

namespace APIGasoline.Application.Tests.Features.Asignaciones.Common;

public class ArmadoAsignacionesTests
{
    private static readonly Guid Empresa = Id(1);
    private static readonly DateTime Hoy = new(2026, 11, 3, 9, 0, 0, DateTimeKind.Utc);

    private static Carro NuevoCarro(string placas, EstadoCarro estado = EstadoCarro.Disponible)
        => Carro.Crear(Empresa, "Nissan", "Versa", 2022, "Blanco", placas, $"carros/{placas}.jpg", 40m, 1000m, 2026, estado);

    private static Empleado NuevoEmpleado(string nombre, int usuario)
        => Empleado.Crear(Empresa, Id(usuario), nombre, fotoPerfil: $"perfiles/{usuario}.jpg");

    private static SolicitudReserva Aprobada(Carro carro, Empleado empleado, DateTime? fecha = null)
    {
        var reserva = SolicitudReserva.Crear(Empresa, carro.CarroId, empleado.EmpleadoId, "Visita a cliente");
        reserva.Aprobar(Id(99), fecha ?? Hoy);
        return reserva;
    }

    private static string? Url(string? llave) => llave is null ? null : $"https://cdn/{llave}";

    [Fact]
    public void Armar_CarroAsignado_TraeAlEmpleadoYElEmpleadoTraeElCarro()
    {
        var carro = NuevoCarro("ABC-123", EstadoCarro.Ocupado);
        var ana = NuevoEmpleado("Ana", 10);
        var reserva = Aprobada(carro, ana);

        var resultado = ArmadoAsignaciones.Armar([carro], [ana], [reserva], new HashSet<Guid>(), Url);

        var asignacion = Assert.Single(resultado.Carros).Asignacion;
        Assert.NotNull(asignacion);
        Assert.Equal(reserva.SolicitudReservaId, asignacion.SolicitudReservaId);
        Assert.Equal("Ana", asignacion.Empleado.NombreCompleto);
        Assert.Equal("https://cdn/perfiles/10.jpg", asignacion.Empleado.FotoPerfilUrl);
        Assert.False(asignacion.TieneViajeEnCurso);

        var carroDeAna = Assert.Single(Assert.Single(resultado.Empleados).Carros);
        Assert.Equal("ABC-123", carroDeAna.Carro.Placas);
        Assert.Equal(EstadoCarro.Ocupado, carroDeAna.EstadoCarro);
    }

    [Fact]
    public void Armar_CarroSinReserva_NoTieneAsignacion()
    {
        var resultado = ArmadoAsignaciones.Armar([NuevoCarro("ABC-123")], [NuevoEmpleado("Ana", 10)], [], new HashSet<Guid>(), Url);

        Assert.Null(Assert.Single(resultado.Carros).Asignacion);
        Assert.Empty(Assert.Single(resultado.Empleados).Carros);
    }

    [Fact]
    public void Armar_ReservaConViajeEnCurso_SeMarca()
    {
        var carro = NuevoCarro("ABC-123", EstadoCarro.Ocupado);
        var ana = NuevoEmpleado("Ana", 10);
        var reserva = Aprobada(carro, ana);

        var resultado = ArmadoAsignaciones.Armar([carro], [ana], [reserva], new HashSet<Guid> { reserva.SolicitudReservaId }, Url);

        Assert.True(Assert.Single(resultado.Carros).Asignacion!.TieneViajeEnCurso);
        Assert.True(Assert.Single(Assert.Single(resultado.Empleados).Carros).TieneViajeEnCurso);
    }

    [Fact]
    public void Armar_CarroDadoDeBaja_NoAparece()
    {
        var baja = NuevoCarro("BAJA-1");
        baja.DarDeBaja(Hoy, "Vendido");

        var resultado = ArmadoAsignaciones.Armar([baja, NuevoCarro("ABC-123")], [], [], new HashSet<Guid>(), Url);

        Assert.Equal("ABC-123", Assert.Single(resultado.Carros).Placas);
    }

    [Fact]
    public void Armar_DosReservasDelMismoCarro_SeTomaLaMasReciente()
    {
        var carro = NuevoCarro("ABC-123", EstadoCarro.Ocupado);
        var ana = NuevoEmpleado("Ana", 10);
        var beto = NuevoEmpleado("Beto", 11);
        var vieja = Aprobada(carro, ana, Hoy.AddDays(-2));
        var nueva = Aprobada(carro, beto, Hoy);

        var resultado = ArmadoAsignaciones.Armar([carro], [ana, beto], [vieja, nueva], new HashSet<Guid>(), Url);

        Assert.Equal("Beto", Assert.Single(resultado.Carros).Asignacion!.Empleado.NombreCompleto);
        Assert.Empty(resultado.Empleados.Single(e => e.NombreCompleto == "Ana").Carros);
    }

    [Fact]
    public void Armar_Empleados_PrimeroLosQueTienenCarroYLuegoPorNombre()
    {
        var carro = NuevoCarro("ABC-123", EstadoCarro.Ocupado);
        var ana = NuevoEmpleado("Ana", 10);
        var zoe = NuevoEmpleado("Zoe", 11);
        var beto = NuevoEmpleado("Beto", 12);

        var resultado = ArmadoAsignaciones.Armar([carro], [ana, zoe, beto], [Aprobada(carro, zoe)], new HashSet<Guid>(), Url);

        Assert.Equal(new[] { "Zoe", "Ana", "Beto" }, resultado.Empleados.Select(e => e.NombreCompleto));
    }
}
