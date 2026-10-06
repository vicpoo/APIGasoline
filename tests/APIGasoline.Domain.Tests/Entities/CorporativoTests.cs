using APIGasoline.Domain.Entities;
using APIGasoline.Domain.Exceptions;

namespace APIGasoline.Domain.Tests.Entities;

/// <summary>
/// Reglas del corporativo: nombre obligatorio (máximo 150, se recorta) y activación.
/// </summary>
public class CorporativoTests
{
    [Fact]
    public void Crear_NombreConEspacios_SeRecorta()
    {
        var entidad = Corporativo.Crear("  Nombre de prueba  ");

        Assert.Equal("Nombre de prueba", entidad.Nombre);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Crear_SinNombre_LanzaDomainException(string nombre)
    {
        Assert.Throws<DomainException>(() => Corporativo.Crear(nombre));
    }

    [Fact]
    public void Crear_NombreMayorAlMaximo_LanzaDomainException()
    {
        Assert.Throws<DomainException>(() => Corporativo.Crear(new string('x', 151)));
    }

    [Fact]
    public void Desactivar_Activo_QuedaInactivo()
    {
        var entidad = Corporativo.Crear("Nombre");

        entidad.Desactivar();

        Assert.False(entidad.Activo);
    }
}
