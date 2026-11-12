using APIGasoline.Domain.Entities;
using APIGasoline.Domain.Exceptions;

namespace APIGasoline.Domain.Tests.Entities;

/// <summary>
/// Reglas de la empresa: es la raíz de la organización (sus corporativos, divisiones y centros
/// apuntan a ella), nombre obligatorio y soft delete que no se repite.
/// </summary>
public class EmpresaTests
{
    [Fact]
    public void Crear_NombreConEspacios_SeRecorta()
    {
        var empresa = Empresa.Crear("  Gasolinera Tuxtla Centro  ");

        Assert.Equal("Gasolinera Tuxtla Centro", empresa.Nombre);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Crear_SinNombre_LanzaDomainException(string nombre)
    {
        Assert.Throws<DomainException>(() => Empresa.Crear(nombre));
    }

    [Fact]
    public void Crear_NombreMayorA150_LanzaDomainException()
    {
        Assert.Throws<DomainException>(() => Empresa.Crear(new string('x', 151)));
    }

    [Fact]
    public void Eliminar_YaEliminada_LanzaDomainException()
    {
        var empresa = Empresa.Crear("Gasolinera Tuxtla Centro");
        empresa.Eliminar();

        Assert.Throws<DomainException>(() => empresa.Eliminar());
    }

    // --- Activa / inactiva (HU-ADM-01) ---

    [Fact]
    public void Crear_QuedaActiva()
    {
        var empresa = Empresa.Crear("Gasolinera Tuxtla Centro");

        Assert.True(empresa.Activo);
    }

    [Fact]
    public void Desactivar_YActivar_CambiaElEstado()
    {
        var empresa = Empresa.Crear("Gasolinera Tuxtla Centro");

        empresa.Desactivar();
        Assert.False(empresa.Activo);

        empresa.Activar();
        Assert.True(empresa.Activo);
    }
}
