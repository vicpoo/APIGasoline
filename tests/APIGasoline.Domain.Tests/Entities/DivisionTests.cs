using APIGasoline.Domain.Entities;
using APIGasoline.Domain.Exceptions;

namespace APIGasoline.Domain.Tests.Entities;

/// <summary>
/// Reglas de la división: pertenece a un corporativo; nombre obligatorio (máximo 100, se recorta) y activación.
/// </summary>
public class DivisionTests
{
    [Fact]
    public void Crear_NombreConEspacios_SeRecorta()
    {
        var entidad = Division.Crear(1, "  Nombre de prueba  ");

        Assert.Equal("Nombre de prueba", entidad.Nombre);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Crear_SinNombre_LanzaDomainException(string nombre)
    {
        Assert.Throws<DomainException>(() => Division.Crear(1, nombre));
    }

    [Fact]
    public void Crear_NombreMayorAlMaximo_LanzaDomainException()
    {
        Assert.Throws<DomainException>(() => Division.Crear(1, new string('x', 101)));
    }

    [Fact]
    public void Desactivar_Activo_QuedaInactivo()
    {
        var entidad = Division.Crear(1, "Nombre");

        entidad.Desactivar();

        Assert.False(entidad.Activo);
    }

    [Fact]
    public void Crear_CorporativoCero_LanzaDomainException()
    {
        Assert.Throws<DomainException>(() => Division.Crear(0, "División Transporte"));
    }
}
