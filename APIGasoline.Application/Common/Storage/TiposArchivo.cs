namespace APIGasoline.Application.Common.Storage;

/// <summary>
/// Tipos de archivo permitidos por caso de uso (content type -> extensión).
/// </summary>
public static class TiposArchivo
{
    public static readonly IReadOnlyDictionary<string, string> Imagenes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        ["image/jpeg"] = ".jpg",
        ["image/jpg"] = ".jpg",   // algunos clientes móviles mandan este valor (no estándar)
        ["image/png"] = ".png",
        ["image/webp"] = ".webp"
    };

    /// <summary>La licencia puede ser una foto o un PDF escaneado.</summary>
    public static readonly IReadOnlyDictionary<string, string> ImagenesYPdf = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        ["image/jpeg"] = ".jpg",
        ["image/jpg"] = ".jpg",   // algunos clientes móviles mandan este valor (no estándar)
        ["image/png"] = ".png",
        ["image/webp"] = ".webp",
        ["application/pdf"] = ".pdf"
    };
}
