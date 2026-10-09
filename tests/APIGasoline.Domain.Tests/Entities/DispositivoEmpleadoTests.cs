using APIGasoline.Domain.Entities;
using APIGasoline.Domain.Enums;
using APIGasoline.Domain.Exceptions;

namespace APIGasoline.Domain.Tests.Entities;

public class DispositivoEmpleadoTests
{
    private static readonly DateTime Ahora = new(2026, 10, 9, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Registrar_DatosValidos_QuedaActivo()
    {
        var dispositivo = Construir(token: "  token-abc  ");

        Assert.True(dispositivo.Activo);
        Assert.Equal("token-abc", dispositivo.TokenFcm);
        Assert.Equal(PlataformaDispositivo.Android, dispositivo.Plataforma);
        Assert.Equal(Ahora, dispositivo.FechaRegistro);
        Assert.Equal(Ahora, dispositivo.FechaUltimoUso);
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(1, 0)]
    public void Registrar_IdsInvalidos_LanzaDomainException(int empresaId, int empleadoId)
    {
        Assert.Throws<DomainException>(() => Construir(empresaId, empleadoId));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Registrar_SinToken_LanzaDomainException(string token)
    {
        Assert.Throws<DomainException>(() => Construir(token: token));
    }

    [Fact]
    public void Registrar_TokenMayorA512_LanzaDomainException()
    {
        Assert.Throws<DomainException>(() => Construir(token: new string('x', 513)));
    }

    [Fact]
    public void Registrar_PlataformaInvalida_LanzaDomainException()
    {
        Assert.Throws<DomainException>(() =>
            DispositivoEmpleado.Registrar(Id(1), Id(1), "token", (PlataformaDispositivo)99, Ahora));
    }

    [Fact]
    public void Reasignar_OtroEmpleado_CambiaDeDuenoYSeReactiva()
    {
        var dispositivo = Construir();
        dispositivo.Desactivar();

        dispositivo.Reasignar(Id(1), Id(2), PlataformaDispositivo.Ios, Ahora.AddDays(1));

        Assert.True(dispositivo.Activo);
        Assert.Equal(Id(2), dispositivo.EmpleadoId);
        Assert.Equal(PlataformaDispositivo.Ios, dispositivo.Plataforma);
        Assert.Equal(Ahora, dispositivo.FechaRegistro);
        Assert.Equal(Ahora.AddDays(1), dispositivo.FechaUltimoUso);
    }

    [Fact]
    public void Desactivar_DejaDeEstarActivo()
    {
        var dispositivo = Construir();

        dispositivo.Desactivar();

        Assert.False(dispositivo.Activo);
    }

    private static DispositivoEmpleado Construir(int empresaId = 1, int empleadoId = 1, string token = "token-abc")
        => DispositivoEmpleado.Registrar(Id(empresaId), Id(empleadoId), token, PlataformaDispositivo.Android, Ahora);
}
