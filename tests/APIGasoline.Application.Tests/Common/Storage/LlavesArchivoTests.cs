using APIGasoline.Application.Common.Storage;

namespace APIGasoline.Application.Tests.Common.Storage;

public class LlavesArchivoTests
{
    private static readonly Guid Version = new("11111111-1111-1111-1111-111111111111");
    private const string VersionN = "11111111111111111111111111111111";

    [Fact]
    public void FotoVehiculo_GeneraLlaveEnLaCarpetaFotoDelCarro()
    {
        var llave = LlavesArchivo.FotoVehiculo(Id(1), Id(7), ".jpg", Version);

        Assert.Equal($"empresa/{Id(1)}/carro/{Id(7)}/foto/vehiculo-{VersionN}.jpg", llave);
    }

    [Fact]
    public void FotoPerfilEmpleado_GeneraLlaveEnLaCarpetaPerfil()
    {
        var llave = LlavesArchivo.FotoPerfilEmpleado(Id(1), Id(3), ".png", Version);

        Assert.Equal($"empresa/{Id(1)}/empleado/{Id(3)}/perfil/perfil-{VersionN}.png", llave);
    }

    [Fact]
    public void FotoLicenciaEmpleado_GeneraLlaveEnLaCarpetaDocumento()
    {
        var llave = LlavesArchivo.FotoLicenciaEmpleado(Id(1), Id(3), ".pdf", Version);

        Assert.Equal($"empresa/{Id(1)}/empleado/{Id(3)}/documento/licencia-{VersionN}.pdf", llave);
    }

    [Fact]
    public void ComprobanteCarga_SeGuardaEnLaCarpetaDeSuCarga()
    {
        var llave = LlavesArchivo.ComprobanteCarga(Id(1), Id(12), Id(40), ".jpg", Version);

        Assert.Equal($"empresa/{Id(1)}/viaje/{Id(12)}/carga/{Id(40)}/comprobante-{VersionN}.jpg", llave);
    }

    [Fact]
    public void FotoVehiculo_VersionesDistintas_GeneranLlavesDistintas()
    {
        var primera = LlavesArchivo.FotoVehiculo(Id(1), Id(7), ".jpg", Version);
        var segunda = LlavesArchivo.FotoVehiculo(Id(1), Id(7), ".jpg", new Guid("22222222-2222-2222-2222-222222222222"));

        Assert.NotEqual(primera, segunda);
    }

    [Fact]
    public void LlavesDelEmpleado_CompartenLaCarpetaDelEmpleado()
    {
        var perfil = LlavesArchivo.FotoPerfilEmpleado(Id(1), Id(3), ".png", Version);
        var licencia = LlavesArchivo.FotoLicenciaEmpleado(Id(1), Id(3), ".pdf", Version);
        var prefijo = $"empresa/{Id(1)}/empleado/{Id(3)}/";

        Assert.StartsWith(prefijo, perfil);
        Assert.StartsWith(prefijo, licencia);
    }

    [Fact]
    public void ComprobanteCarga_ConUuidsReales_CabeEnLaColumnaDe255()
    {
        var llave = LlavesArchivo.ComprobanteCarga(Guid.CreateVersion7(), Guid.CreateVersion7(), Guid.CreateVersion7(), ".webp", Guid.NewGuid());

        Assert.True(llave.Length <= 255);
    }
}
