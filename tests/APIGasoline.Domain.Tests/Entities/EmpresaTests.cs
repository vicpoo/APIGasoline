using APIGasoline.Domain.Entities;
using APIGasoline.Domain.Exceptions;

namespace APIGasoline.Domain.Tests.Entities;

/// <summary>
/// Reglas de la empresa: puede ser independiente, pertenecer solo a un corporativo o a corporativo + división;
/// nunca a una división sin corporativo. Soft delete que no se repite.
/// </summary>
public class EmpresaTests
{
    [Fact]
    public void Crear_ConCorporativoYDivision_SeAsignaLaOrganizacion()
    {
        var empresa = Empresa.Crear("Gasolinera Tuxtla Centro", 1, 1);

        Assert.Equal(1, empresa.CorporativoId);
        Assert.Equal(1, empresa.DivisionId);
    }

    [Fact]
    public void Crear_SoloConCorporativo_SeAcepta()
    {
        var empresa = Empresa.Crear("Transportes del Sur", 1, null);

        Assert.Null(empresa.DivisionId);
    }

    [Fact]
    public void Crear_Independiente_SeAcepta()
    {
        var empresa = Empresa.Crear("Empresa Independiente", null, null);

        Assert.Null(empresa.CorporativoId);
    }

    [Fact]
    public void Crear_DivisionSinCorporativo_LanzaDomainException()
    {
        Assert.Throws<DomainException>(() => Empresa.Crear("Empresa X", null, 1));
    }

    [Theory]
    [InlineData(0, null)]
    [InlineData(1, 0)]
    public void Crear_IdsDeOrganizacionInvalidos_LanzaDomainException(int? corporativoId, int? divisionId)
    {
        Assert.Throws<DomainException>(() => Empresa.Crear("Empresa X", corporativoId, divisionId));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Crear_SinNombre_LanzaDomainException(string nombre)
    {
        Assert.Throws<DomainException>(() => Empresa.Crear(nombre, null, null));
    }

    [Fact]
    public void Crear_NombreMayorA150_LanzaDomainException()
    {
        Assert.Throws<DomainException>(() => Empresa.Crear(new string('x', 151), null, null));
    }

    [Fact]
    public void Eliminar_YaEliminada_LanzaDomainException()
    {
        var empresa = Empresa.Crear("Gasolinera Tuxtla Centro", null, null);
        empresa.Eliminar();

        Assert.Throws<DomainException>(() => empresa.Eliminar());
    }
}
