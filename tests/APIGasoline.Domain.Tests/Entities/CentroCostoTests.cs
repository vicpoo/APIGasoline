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
        var entidad = CentroCosto.Crear(Id(1), "  Nombre de prueba  ", "CC-LOG-01");

        Assert.Equal("Nombre de prueba", entidad.Nombre);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Crear_SinNombre_LanzaDomainException(string nombre)
    {
        Assert.Throws<DomainException>(() => CentroCosto.Crear(Id(1), nombre, "CC-LOG-01"));
    }

    [Fact]
    public void Crear_NombreMayorAlMaximo_LanzaDomainException()
    {
        Assert.Throws<DomainException>(() => CentroCosto.Crear(Id(1), new string('x', 151), "CC-LOG-01"));
    }

    [Fact]
    public void Desactivar_Activo_QuedaInactivo()
    {
        var entidad = CentroCosto.Crear(Id(1), "Nombre", "CC-LOG-01");

        entidad.Desactivar();

        Assert.False(entidad.Activo);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Crear_SinClave_LanzaDomainException(string clave)
    {
        Assert.Throws<DomainException>(() => CentroCosto.Crear(Id(1), "Logística", clave));
    }

    [Fact]
    public void Crear_ClaveMayorA100_LanzaDomainException()
    {
        Assert.Throws<DomainException>(() => CentroCosto.Crear(Id(1), "Logística", new string('x', 101)));
    }

    [Fact]
    public void Crear_EmpresaCero_LanzaDomainException()
    {
        Assert.Throws<DomainException>(() => CentroCosto.Crear(Guid.Empty, "Logística", "CC-LOG-01"));
    }

    // --- Corporativo y división (Fase 8) ---

    [Fact]
    public void Crear_SinOrganizacion_QuedanNulos()
    {
        var entidad = CentroCosto.Crear(Id(1), "Logística", "CC-LOG-01");

        Assert.Null(entidad.CorporativoId);
        Assert.Null(entidad.DivisionId);
    }

    [Fact]
    public void Crear_ConCorporativoYDivision_LosGuarda()
    {
        var entidad = CentroCosto.Crear(Id(1), "Logística", "CC-LOG-01", corporativoId: Id(2), divisionId: Id(3));

        Assert.Equal(Id(2), entidad.CorporativoId);
        Assert.Equal(Id(3), entidad.DivisionId);
    }

    [Theory]
    [InlineData(0, null)]
    [InlineData(null, 0)]
    public void Crear_IdsDeOrganizacionVacios_LanzaDomainException(int? corporativoId, int? divisionId)
    {
        Assert.Throws<DomainException>(() =>
            CentroCosto.Crear(Id(1), "Logística", "CC-LOG-01", corporativoId: Vacio(corporativoId), divisionId: Vacio(divisionId)));
    }

    [Fact]
    public void AsignarOrganizacion_ANulos_LaQuita()
    {
        var entidad = CentroCosto.Crear(Id(1), "Logística", "CC-LOG-01", corporativoId: Id(2), divisionId: Id(3));

        entidad.AsignarOrganizacion(null, null);

        Assert.Null(entidad.CorporativoId);
        Assert.Null(entidad.DivisionId);
    }

    // 0 = Guid.Empty (inválido); null = sin valor
    private static Guid? Vacio(int? n) => n is null ? null : n == 0 ? Guid.Empty : Id(n.Value);
}
