using APIGasoline.Application.Common.Storage;

namespace APIGasoline.Application.Tests.Common.Storage;

/// <summary>
/// Tipos de archivo permitidos por caso de uso: las fotos solo aceptan imágenes;
/// la licencia acepta imágenes y PDF.
/// (Fase 0: prueba mínima para verificar que el proyecto corre; la Fase 2 agrega ArchivoValidatorTests.)
/// </summary>
public class TiposArchivoTests
{
    [Theory]
    [InlineData("image/jpeg", ".jpg")]
    [InlineData("image/jpg", ".jpg")]   // valor no estándar que mandan algunos clientes móviles
    [InlineData("image/png", ".png")]
    [InlineData("image/webp", ".webp")]
    [InlineData("IMAGE/PNG", ".png")]   // el content type no distingue mayúsculas
    public void Imagenes_ContentTypePermitido_DevuelveExtension(string contentType, string extension)
    {
        Assert.Equal(extension, TiposArchivo.Imagenes[contentType]);
    }

    [Fact]
    public void Imagenes_Pdf_NoEstaPermitido()
    {
        Assert.False(TiposArchivo.Imagenes.ContainsKey("application/pdf"));
    }

    [Fact]
    public void ImagenesYPdf_Pdf_DevuelveExtensionPdf()
    {
        Assert.Equal(".pdf", TiposArchivo.ImagenesYPdf["application/pdf"]);
    }
}
