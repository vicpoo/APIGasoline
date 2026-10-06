using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Common.Security;

namespace APIGasoline.Application.Tests.Common.Security;

/// <summary>
/// Política de contraseñas nuevas: obligatoria, entre 8 y 100 caracteres, con al menos una letra y un número.
/// </summary>
public class PasswordPolicyTests
{
    [Theory]
    [InlineData("Gasolina2026")]
    [InlineData("abcdefg1")]  // exactamente 8 caracteres
    public void Validar_ContrasenaValida_NoLanzaExcepcion(string password)
    {
        var excepcion = Record.Exception(() => PasswordPolicy.Validar(password));

        Assert.Null(excepcion);
    }

    [Fact]
    public void Validar_ContrasenaDe100Caracteres_NoLanzaExcepcion()
    {
        var excepcion = Record.Exception(() => PasswordPolicy.Validar(new string('a', 99) + "1"));

        Assert.Null(excepcion);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validar_SinContrasena_LanzaBadRequest(string? password)
    {
        var excepcion = Assert.Throws<BadRequestException>(() => PasswordPolicy.Validar(password));

        Assert.Contains("obligatoria", excepcion.Message);
    }

    [Theory]
    [InlineData("abcdef1")]  // 7 caracteres
    public void Validar_MenosDe8Caracteres_LanzaBadRequest(string password)
    {
        Assert.Throws<BadRequestException>(() => PasswordPolicy.Validar(password));
    }

    [Fact]
    public void Validar_MasDe100Caracteres_LanzaBadRequest()
    {
        Assert.Throws<BadRequestException>(() => PasswordPolicy.Validar(new string('a', 100) + "1"));
    }

    [Theory]
    [InlineData("SoloLetras")]
    [InlineData("1234567890")]
    public void Validar_SinLetraONumero_LanzaBadRequest(string password)
    {
        var excepcion = Assert.Throws<BadRequestException>(() => PasswordPolicy.Validar(password));

        Assert.Contains("letra y un número", excepcion.Message);
    }
}
