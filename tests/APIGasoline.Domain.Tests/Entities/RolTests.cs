using APIGasoline.Domain.Entities;
using APIGasoline.Domain.Exceptions;

namespace APIGasoline.Domain.Tests.Entities;

/// <summary>
/// Reglas del rol: sin empresa es global (disponible para todas); con empresa solo está disponible para esa empresa.
/// </summary>
public class RolTests
{
    [Fact]
    public void Crear_SinEmpresa_EsGlobalYDisponibleParaCualquierEmpresa()
    {
        var rol = Rol.Crear("Confirmador", null);

        Assert.True(rol.EsGlobal);
        Assert.True(rol.DisponiblePara(Id(1)));
        Assert.True(rol.DisponiblePara(null));
    }

    [Theory]
    [InlineData(1, true)]
    [InlineData(2, false)]
    public void DisponiblePara_RolDeEmpresa_SoloParaSuEmpresa(int empresaId, bool esperado)
    {
        var rol = Rol.Crear("Gestor de activos", null, empresaId: Id(1));

        Assert.Equal(esperado, rol.DisponiblePara(Id(empresaId)));
    }

    [Fact]
    public void Crear_EmpresaCero_LanzaDomainException()
    {
        Assert.Throws<DomainException>(() => Rol.Crear("Operador", null, empresaId: Guid.Empty));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Crear_SinNombre_LanzaDomainException(string nombre)
    {
        Assert.Throws<DomainException>(() => Rol.Crear(nombre, null));
    }

    [Fact]
    public void Crear_NombreMayorA100_LanzaDomainException()
    {
        Assert.Throws<DomainException>(() => Rol.Crear(new string('x', 101), null));
    }

    [Fact]
    public void Crear_DescripcionEnBlanco_SeGuardaComoNull()
    {
        var rol = Rol.Crear("Operador", "   ");

        Assert.Null(rol.Descripcion);
    }

    [Fact]
    public void Crear_DescripcionMayorA255_LanzaDomainException()
    {
        Assert.Throws<DomainException>(() => Rol.Crear("Operador", new string('x', 256)));
    }

    [Fact]
    public void Desactivar_RolActivo_QuedaInactivo()
    {
        var rol = Rol.Crear("Operador", null);

        rol.Desactivar();

        Assert.False(rol.Activo);
    }
}
