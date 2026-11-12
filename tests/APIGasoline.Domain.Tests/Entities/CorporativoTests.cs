using APIGasoline.Domain.Entities;
using APIGasoline.Domain.Exceptions;

namespace APIGasoline.Domain.Tests.Entities;

/// <summary>
/// Reglas del corporativo: pertenece a una empresa, nombre obligatorio (máximo 150, se recorta) y activación.
/// </summary>
public class CorporativoTests
{
    [Fact]
    public void Crear_NombreConEspacios_SeRecorta()
    {
        var entidad = Corporativo.Crear(Id(1), "  Nombre de prueba  ");

        Assert.Equal("Nombre de prueba", entidad.Nombre);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Crear_SinNombre_LanzaDomainException(string nombre)
    {
        Assert.Throws<DomainException>(() => Corporativo.Crear(Id(1), nombre));
    }

    [Fact]
    public void Crear_NombreMayorAlMaximo_LanzaDomainException()
    {
        Assert.Throws<DomainException>(() => Corporativo.Crear(Id(1), new string('x', 151)));
    }

    [Fact]
    public void Desactivar_Activo_QuedaInactivo()
    {
        var entidad = Corporativo.Crear(Id(1), "Nombre");

        entidad.Desactivar();

        Assert.False(entidad.Activo);
    }

    [Fact]
    public void Crear_GuardaLaEmpresa()
    {
        var entidad = Corporativo.Crear(Id(7), "Corporativo Sur");

        Assert.Equal(Id(7), entidad.EmpresaId);
    }

    [Fact]
    public void Crear_EmpresaCero_LanzaDomainException()
    {
        Assert.Throws<DomainException>(() => Corporativo.Crear(Guid.Empty, "Corporativo Sur"));
    }
}
