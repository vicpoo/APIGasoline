using APIGasoline.Domain.Entities;
using APIGasoline.Domain.Exceptions;

namespace APIGasoline.Domain.Tests.Entities;

/// <summary>
/// Reglas del empleado: nombre obligatorio, llaves de archivos (perfil y licencia) opcionales,
/// TieneLicencia como requisito para pedir un carro y soft delete que no se repite.
/// </summary>
public class EmpleadoTests
{
    [Fact]
    public void Crear_DatosValidos_SinFotosYSinLicencia()
    {
        var empleado = ConstruirEmpleado();

        Assert.Null(empleado.FotoPerfil);
        Assert.Null(empleado.FotoLicencia);
        Assert.False(empleado.TieneLicencia);
        Assert.False(empleado.Eliminado);
    }

    [Fact]
    public void Crear_NombreConEspacios_SeRecorta()
    {
        var empleado = ConstruirEmpleado(nombre: "  Víctor Alejandro  ");

        Assert.Equal("Víctor Alejandro", empleado.NombreCompleto);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Crear_SinNombre_LanzaDomainException(string nombre)
    {
        Assert.Throws<DomainException>(() => ConstruirEmpleado(nombre: nombre));
    }

    [Fact]
    public void Crear_NombreMayorA150_LanzaDomainException()
    {
        Assert.Throws<DomainException>(() => ConstruirEmpleado(nombre: new string('x', 151)));
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(1, 0)]
    public void Crear_IdsInvalidos_LanzaDomainException(int empresaId, int usuarioId)
    {
        Assert.Throws<DomainException>(() => ConstruirEmpleado(empresaId: empresaId, usuarioId: usuarioId));
    }

    // --- Licencia y fotos ---

    [Fact]
    public void CambiarFotoLicencia_ConLlave_TieneLicencia()
    {
        var empleado = ConstruirEmpleado();

        empleado.CambiarFotoLicencia("empresa/1/empleado/1/documento/licencia.pdf");

        Assert.True(empleado.TieneLicencia);
    }

    [Fact]
    public void CambiarFotoLicencia_Null_YaNoTieneLicencia()
    {
        var empleado = ConstruirEmpleado();
        empleado.CambiarFotoLicencia("empresa/1/empleado/1/documento/licencia.pdf");

        empleado.CambiarFotoLicencia(null);

        Assert.False(empleado.TieneLicencia);
    }

    [Fact]
    public void CambiarFotoPerfil_EnBlanco_SeGuardaComoNull()
    {
        var empleado = ConstruirEmpleado();

        empleado.CambiarFotoPerfil("   ");

        Assert.Null(empleado.FotoPerfil);
    }

    [Fact]
    public void CambiarFotoPerfil_LlaveMayorA255_LanzaDomainException()
    {
        var empleado = ConstruirEmpleado();

        Assert.Throws<DomainException>(() => empleado.CambiarFotoPerfil(new string('x', 256)));
    }

    // --- Soft delete ---

    [Fact]
    public void Eliminar_Activo_QuedaEliminado()
    {
        var empleado = ConstruirEmpleado();

        empleado.Eliminar();

        Assert.True(empleado.Eliminado);
    }

    [Fact]
    public void Eliminar_YaEliminado_LanzaDomainException()
    {
        var empleado = ConstruirEmpleado();
        empleado.Eliminar();

        Assert.Throws<DomainException>(() => empleado.Eliminar());
    }

    private static Empleado ConstruirEmpleado(int empresaId = 1, int usuarioId = 1, string nombre = "Víctor Alejandro Pérez López")
        => Empleado.Crear(Id(empresaId), Id(usuarioId), nombre);
}
