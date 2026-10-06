using APIGasoline.Domain.Entities;
using APIGasoline.Domain.Exceptions;

namespace APIGasoline.Domain.Tests.Entities;

/// <summary>
/// Reglas del centro de costo: pertenece a una empresa; nombre (máximo 150) y clave (máximo 100) obligatorios.
/// </summary>
public class CentroCostoTests
{
    [Fact]
    public void Crear_NombreConEspacios_SeRecorta()
    {
        var entidad = CentroCosto.Crear(1, "  Nombre de prueba  ", "CC-LOG-01");

        Assert.Equal("Nombre de prueba", entidad.Nombre);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Crear_SinNombre_LanzaDomainException(string nombre)
    {
        Assert.Throws<DomainException>(() => CentroCosto.Crear(1, nombre, "CC-LOG-01"));
    }

    [Fact]
    public void Crear_NombreMayorAlMaximo_LanzaDomainException()
    {
        Assert.Throws<DomainException>(() => CentroCosto.Crear(1, new string('x', 151), "CC-LOG-01"));
    }

    [Fact]
    public void Desactivar_Activo_QuedaInactivo()
    {
        var entidad = CentroCosto.Crear(1, "Nombre", "CC-LOG-01");

        entidad.Desactivar();

        Assert.False(entidad.Activo);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Crear_SinClave_LanzaDomainException(string clave)
    {
        Assert.Throws<DomainException>(() => CentroCosto.Crear(1, "Logística", clave));
    }

    [Fact]
    public void Crear_ClaveMayorA100_LanzaDomainException()
    {
        Assert.Throws<DomainException>(() => CentroCosto.Crear(1, "Logística", new string('x', 101)));
    }

    [Fact]
    public void Crear_EmpresaCero_LanzaDomainException()
    {
        Assert.Throws<DomainException>(() => CentroCosto.Crear(0, "Logística", "CC-LOG-01"));
    }
}
