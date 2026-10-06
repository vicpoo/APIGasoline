using APIGasoline.Domain.Entities;
using APIGasoline.Domain.Exceptions;

namespace APIGasoline.Domain.Tests.Entities;

/// <summary>
/// Reglas del privilegio: nombre obligatorio (máximo 100, se recorta), descripción opcional y activación.
/// </summary>
public class PrivilegioTests
{
    [Fact]
    public void Crear_NombreConEspacios_SeRecorta()
    {
        var entidad = Privilegio.Crear("  Nombre de prueba  ", null);

        Assert.Equal("Nombre de prueba", entidad.Nombre);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Crear_SinNombre_LanzaDomainException(string nombre)
    {
        Assert.Throws<DomainException>(() => Privilegio.Crear(nombre, null));
    }

    [Fact]
    public void Crear_NombreMayorAlMaximo_LanzaDomainException()
    {
        Assert.Throws<DomainException>(() => Privilegio.Crear(new string('x', 101), null));
    }

    [Fact]
    public void Desactivar_Activo_QuedaInactivo()
    {
        var entidad = Privilegio.Crear("Nombre", null);

        entidad.Desactivar();

        Assert.False(entidad.Activo);
    }

    [Fact]
    public void Crear_DescripcionEnBlanco_SeGuardaComoNull()
    {
        var privilegio = Privilegio.Crear("reservar_carro", "   ");

        Assert.Null(privilegio.Descripcion);
    }

    [Fact]
    public void Crear_DescripcionMayorA255_LanzaDomainException()
    {
        Assert.Throws<DomainException>(() => Privilegio.Crear("reservar_carro", new string('x', 256)));
    }
}
