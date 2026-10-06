using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Common.Models;
using APIGasoline.Application.Common.Storage;

namespace APIGasoline.Application.Tests.Common.Storage;

/// <summary>
/// Reglas de los archivos que se suben a MinIO: obligatorio, máximo 5 MB, tipo permitido por caso de uso
/// y firma (primeros bytes) que coincida con el tipo, para no aceptar archivos renombrados.
/// </summary>
public class ArchivoValidatorTests
{
    private static readonly byte[] Jpeg = [0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46, 0x49, 0x46, 0x00, 0x01];
    private static readonly byte[] Png = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00, 0x00, 0x00, 0x0D];
    private static readonly byte[] Webp = [0x52, 0x49, 0x46, 0x46, 0x24, 0x00, 0x00, 0x00, 0x57, 0x45, 0x42, 0x50];
    private static readonly byte[] Pdf = [0x25, 0x50, 0x44, 0x46, 0x2D, 0x31, 0x2E, 0x34];      // "%PDF-1.4"
    private static readonly byte[] Texto = [0x68, 0x6F, 0x6C, 0x61, 0x20, 0x6D, 0x75, 0x6E, 0x64, 0x6F]; // "hola mundo"

    // --- Archivos válidos ---

    [Fact]
    public async Task ValidarAsync_Jpeg_DevuelveExtensionJpg()
    {
        var extension = await ValidarImagenAsync(ConstruirArchivo(Jpeg, "image/jpeg"));

        Assert.Equal(".jpg", extension);
    }

    [Fact]
    public async Task ValidarAsync_Png_DevuelveExtensionPng()
    {
        var extension = await ValidarImagenAsync(ConstruirArchivo(Png, "image/png"));

        Assert.Equal(".png", extension);
    }

    [Fact]
    public async Task ValidarAsync_Webp_DevuelveExtensionWebp()
    {
        var extension = await ValidarImagenAsync(ConstruirArchivo(Webp, "image/webp"));

        Assert.Equal(".webp", extension);
    }

    [Fact]
    public async Task ValidarAsync_ImageJpgNoEstandar_SeAceptaComoJpg()
    {
        var extension = await ValidarImagenAsync(ConstruirArchivo(Jpeg, "image/jpg"));

        Assert.Equal(".jpg", extension);
    }

    [Fact]
    public async Task ValidarAsync_ContentTypeConParametros_SeIgnoranLosParametros()
    {
        var extension = await ValidarImagenAsync(ConstruirArchivo(Png, "image/png; charset=binary"));

        Assert.Equal(".png", extension);
    }

    [Fact]
    public async Task ValidarAsync_PdfEnLicencia_DevuelveExtensionPdf()
    {
        var extension = await ArchivoValidator.ValidarAsync(
            ConstruirArchivo(Pdf, "application/pdf"), TiposArchivo.ImagenesYPdf, "la licencia", CancellationToken.None);

        Assert.Equal(".pdf", extension);
    }

    [Fact]
    public async Task ValidarAsync_DespuesDeRevisarLaFirma_ElStreamQuedaAlInicio()
    {
        var archivo = ConstruirArchivo(Jpeg, "image/jpeg");

        await ValidarImagenAsync(archivo);

        Assert.Equal(0, archivo.Contenido.Position);
    }

    // --- Obligatorio y tamaño ---

    [Fact]
    public async Task ValidarAsync_SinArchivo_LanzaBadRequest()
    {
        var excepcion = await Assert.ThrowsAsync<BadRequestException>(() => ValidarImagenAsync(null));

        Assert.Equal("Falta la foto de prueba.", excepcion.Message);
    }

    [Fact]
    public async Task ValidarAsync_ArchivoVacio_LanzaBadRequest()
    {
        await Assert.ThrowsAsync<BadRequestException>(() => ValidarImagenAsync(ConstruirArchivo([], "image/jpeg")));
    }

    [Fact]
    public async Task ValidarAsync_MayorA5MB_LanzaBadRequest()
    {
        var archivo = ConstruirArchivo(Jpeg, "image/jpeg", tamano: ArchivoValidator.TamanoMaximoBytes + 1);

        await Assert.ThrowsAsync<BadRequestException>(() => ValidarImagenAsync(archivo));
    }

    [Fact]
    public async Task ValidarAsync_Exactamente5MB_SeAcepta()
    {
        var archivo = ConstruirArchivo(Jpeg, "image/jpeg", tamano: ArchivoValidator.TamanoMaximoBytes);

        var extension = await ValidarImagenAsync(archivo);

        Assert.Equal(".jpg", extension);
    }

    // --- Tipo y firma ---

    [Theory]
    [InlineData("text/plain")]
    [InlineData("application/pdf")]  // las fotos no aceptan PDF (solo la licencia)
    [InlineData("")]
    public async Task ValidarAsync_TipoNoPermitido_LanzaBadRequest(string contentType)
    {
        await Assert.ThrowsAsync<BadRequestException>(() => ValidarImagenAsync(ConstruirArchivo(Pdf, contentType)));
    }

    [Fact]
    public async Task ValidarAsync_TextoRenombradoComoJpeg_LanzaBadRequest()
    {
        var excepcion = await Assert.ThrowsAsync<BadRequestException>(
            () => ValidarImagenAsync(ConstruirArchivo(Texto, "image/jpeg", "ticket.jpg")));

        Assert.Contains("no corresponde", excepcion.Message);
    }

    [Fact]
    public async Task ValidarAsync_PngDeclaradoComoJpeg_LanzaBadRequest()
    {
        await Assert.ThrowsAsync<BadRequestException>(() => ValidarImagenAsync(ConstruirArchivo(Png, "image/jpeg")));
    }

    [Fact]
    public async Task ValidarAsync_ArchivoMasCortoQueLaFirma_LanzaBadRequest()
    {
        await Assert.ThrowsAsync<BadRequestException>(() => ValidarImagenAsync(ConstruirArchivo([0xFF, 0xD8], "image/jpeg")));
    }

    private static Task<string> ValidarImagenAsync(ArchivoEntrada? archivo)
        => ArchivoValidator.ValidarAsync(archivo, TiposArchivo.Imagenes, "la foto de prueba", CancellationToken.None);

    private static ArchivoEntrada ConstruirArchivo(byte[] contenido, string contentType, string nombre = "archivo", long? tamano = null)
        => new(new MemoryStream(contenido), nombre, contentType, tamano ?? contenido.Length);
}
