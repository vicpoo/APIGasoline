using APIGasoline.Application.Common.Security;

namespace APIGasoline.Application.Tests.Common.Security;

/// <summary>
/// Versión de privilegios para la app offline-first: SHA-256 de las claves ordenadas.
/// No depende del orden ni de repetidos, y cambia al agregar o quitar una clave.
/// </summary>
public class VersionPrivilegiosTests
{
    [Fact]
    public void Calcular_MismoConjuntoEnOtroOrden_DaLaMismaVersion()
    {
        var a = VersionPrivilegios.Calcular(["carro.crear", "carro.eliminar", "viaje.ver_propios"]);
        var b = VersionPrivilegios.Calcular(["viaje.ver_propios", "carro.crear", "carro.eliminar"]);

        Assert.Equal(a, b);
    }

    [Fact]
    public void Calcular_ConRepetidosYMayusculas_DaLaMismaVersion()
    {
        var a = VersionPrivilegios.Calcular(["carro.crear", "carro.eliminar"]);
        var b = VersionPrivilegios.Calcular(["Carro.Crear", "carro.eliminar", " carro.crear "]);

        Assert.Equal(a, b);
    }

    [Fact]
    public void Calcular_AlQuitarUnPrivilegio_CambiaLaVersion()
    {
        var antes = VersionPrivilegios.Calcular(["carro.crear", "carro.eliminar"]);
        var despues = VersionPrivilegios.Calcular(["carro.crear"]);

        Assert.NotEqual(antes, despues);
    }

    [Fact]
    public void Calcular_AlAgregarUnPrivilegio_CambiaLaVersion()
    {
        var antes = VersionPrivilegios.Calcular(["carro.crear"]);
        var despues = VersionPrivilegios.Calcular(["carro.crear", "carro.actualizar"]);

        Assert.NotEqual(antes, despues);
    }

    [Fact]
    public void Calcular_EsHexDe64CaracteresEnMinusculas()
    {
        var version = VersionPrivilegios.Calcular(["carro.crear"]);

        Assert.Equal(64, version.Length);
        Assert.Matches("^[0-9a-f]{64}$", version);
    }

    [Fact]
    public void Calcular_SinPrivilegios_EsElHashDeCadenaVacia()
    {
        // SHA-256("") conocido
        Assert.Equal(
            "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855",
            VersionPrivilegios.Calcular([]));
    }
}
