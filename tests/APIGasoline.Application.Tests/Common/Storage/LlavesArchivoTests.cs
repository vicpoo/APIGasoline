using APIGasoline.Application.Common.Storage;

namespace APIGasoline.Application.Tests.Common.Storage;

/// <summary>
/// Formato de las llaves de MinIO (lo que se guarda en la BD): agrupadas por empresa y entidad,
/// con el id único de la versión del archivo y su extensión.
/// </summary>
public class LlavesArchivoTests
{
    private static readonly Guid Id = new("11111111-1111-1111-1111-111111111111");

    [Fact]
    public void FotoVehiculo_GeneraLlaveDelCarro()
    {
        var llave = LlavesArchivo.FotoVehiculo(1, 7, ".jpg", Id);

        Assert.Equal("empresas/1/carros/7/vehiculo-11111111111111111111111111111111.jpg", llave);
    }

    [Fact]
    public void FotoPerfilEmpleado_GeneraLlaveDelEmpleado()
    {
        var llave = LlavesArchivo.FotoPerfilEmpleado(1, 3, ".png", Id);

        Assert.Equal("empresas/1/empleados/3/perfil-11111111111111111111111111111111.png", llave);
    }

    [Fact]
    public void FotoLicenciaEmpleado_GeneraLlaveDelEmpleado()
    {
        var llave = LlavesArchivo.FotoLicenciaEmpleado(1, 3, ".pdf", Id);

        Assert.Equal("empresas/1/empleados/3/licencia-11111111111111111111111111111111.pdf", llave);
    }

    [Fact]
    public void ComprobanteCarga_SeAgrupaPorViaje()
    {
        var llave = LlavesArchivo.ComprobanteCarga(1, 12, ".jpg", Id);

        Assert.Equal("empresas/1/viajes/12/cargas/comprobante-11111111111111111111111111111111.jpg", llave);
    }

    [Fact]
    public void FotoVehiculo_IdsDistintos_GeneranLlavesDistintas()
    {
        var primera = LlavesArchivo.FotoVehiculo(1, 7, ".jpg", Id);
        var segunda = LlavesArchivo.FotoVehiculo(1, 7, ".jpg", new Guid("22222222-2222-2222-2222-222222222222"));

        Assert.NotEqual(primera, segunda);
    }

    [Fact]
    public void ComprobanteCarga_LlaveCabeEnLaColumnaDe255()
    {
        var llave = LlavesArchivo.ComprobanteCarga(int.MaxValue, int.MaxValue, ".webp", Id);

        Assert.True(llave.Length <= 255);
    }
}
