using APIGasoline.Application.Common.Storage;

namespace APIGasoline.Application.Tests.Common.Storage;

/// <summary>
/// Formato de las llaves de MinIO (lo que se guarda en la BD): agrupadas por empresa y entidad,
/// con el id único de la versión del archivo y su extensión.
/// </summary>
public class LlavesArchivoTests
{
    private static readonly Guid Archivo = new("11111111-1111-1111-1111-111111111111");

    [Fact]
    public void FotoVehiculo_GeneraLlaveDelCarro()
    {
        var llave = LlavesArchivo.FotoVehiculo(Id(1), Id(7), ".jpg", Archivo);

        Assert.Equal($"empresas/{Id(1)}/carros/{Id(7)}/vehiculo-11111111111111111111111111111111.jpg", llave);
    }

    [Fact]
    public void FotoPerfilEmpleado_GeneraLlaveDelEmpleado()
    {
        var llave = LlavesArchivo.FotoPerfilEmpleado(Id(1), Id(3), ".png", Archivo);

        Assert.Equal($"empresas/{Id(1)}/empleados/{Id(3)}/perfil-11111111111111111111111111111111.png", llave);
    }

    [Fact]
    public void FotoLicenciaEmpleado_GeneraLlaveDelEmpleado()
    {
        var llave = LlavesArchivo.FotoLicenciaEmpleado(Id(1), Id(3), ".pdf", Archivo);

        Assert.Equal($"empresas/{Id(1)}/empleados/{Id(3)}/licencia-11111111111111111111111111111111.pdf", llave);
    }

    [Fact]
    public void ComprobanteCarga_SeAgrupaPorViaje()
    {
        var llave = LlavesArchivo.ComprobanteCarga(Id(1), Id(12), ".jpg", Archivo);

        Assert.Equal($"empresas/{Id(1)}/viajes/{Id(12)}/cargas/comprobante-11111111111111111111111111111111.jpg", llave);
    }

    [Fact]
    public void FotoVehiculo_IdsDistintos_GeneranLlavesDistintas()
    {
        var primera = LlavesArchivo.FotoVehiculo(Id(1), Id(7), ".jpg", Archivo);
        var segunda = LlavesArchivo.FotoVehiculo(Id(1), Id(7), ".jpg", new Guid("22222222-2222-2222-2222-222222222222"));

        Assert.NotEqual(primera, segunda);
    }

    [Fact]
    public void ComprobanteCarga_LlaveCabeEnLaColumnaDe255()
    {
        var llave = LlavesArchivo.ComprobanteCarga(Id(int.MaxValue), Id(int.MaxValue), ".webp", Archivo);

        Assert.True(llave.Length <= 255);
    }
}
