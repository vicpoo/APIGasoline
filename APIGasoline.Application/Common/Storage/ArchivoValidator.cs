using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Common.Models;

namespace APIGasoline.Application.Common.Storage;

/// <summary>
/// Reglas de negocio de los archivos: obligatorio, tamaño máximo y tipo permitido.
/// Además revisa la "firma" (primeros bytes) para que no se acepte un archivo renombrado.
/// </summary>
public static class ArchivoValidator
{
    public const long TamanoMaximoBytes = 5 * 1024 * 1024; // 5 MB

    /// <summary>
    /// Valida el archivo y devuelve la extensión que se usará en la llave (ej. ".jpg").
    /// </summary>
    public static async Task<string> ValidarAsync(
        ArchivoEntrada? archivo,
        IReadOnlyDictionary<string, string> tiposPermitidos,
        string campo,
        CancellationToken cancellationToken)
    {
        if (archivo is null || archivo.Tamano <= 0)
            throw new BadRequestException($"Falta {campo}.");

        if (archivo.Tamano > TamanoMaximoBytes)
            throw new BadRequestException($"El archivo ({campo}) no puede pesar más de {TamanoMaximoBytes / (1024 * 1024)} MB.");

        var contentType = archivo.ContentType?.Split(';')[0].Trim() ?? string.Empty;

        if (!tiposPermitidos.TryGetValue(contentType, out var extension))
            throw new BadRequestException(
                $"El archivo ({campo}) debe ser de tipo: {string.Join(", ", tiposPermitidos.Keys)}.");

        if (!await FirmaCoincideAsync(archivo.Contenido, contentType, cancellationToken))
            throw new BadRequestException($"El contenido del archivo ({campo}) no corresponde a un {contentType} válido.");

        return extension;
    }

    private static async Task<bool> FirmaCoincideAsync(Stream contenido, string contentType, CancellationToken cancellationToken)
    {
        // Si el stream no permite regresar al inicio, no se puede revisar sin consumirlo
        if (!contenido.CanSeek)
            return true;

        var posicion = contenido.Position;
        var cabecera = new byte[12];
        var leidos = 0;

        try
        {
            int n;
            while (leidos < cabecera.Length
                   && (n = await contenido.ReadAsync(cabecera.AsMemory(leidos, cabecera.Length - leidos), cancellationToken)) > 0)
                leidos += n;
        }
        finally
        {
            contenido.Position = posicion;
        }

        return contentType.ToLowerInvariant() switch
        {
            "image/jpeg" or "image/jpg" => leidos >= 3 && cabecera[0] == 0xFF && cabecera[1] == 0xD8 && cabecera[2] == 0xFF,
            "image/png" => leidos >= 8 && cabecera[0] == 0x89 && cabecera[1] == 0x50 && cabecera[2] == 0x4E && cabecera[3] == 0x47
                           && cabecera[4] == 0x0D && cabecera[5] == 0x0A && cabecera[6] == 0x1A && cabecera[7] == 0x0A,
            "image/webp" => leidos >= 12 && cabecera[0] == 0x52 && cabecera[1] == 0x49 && cabecera[2] == 0x46 && cabecera[3] == 0x46
                            && cabecera[8] == 0x57 && cabecera[9] == 0x45 && cabecera[10] == 0x42 && cabecera[11] == 0x50,
            "application/pdf" => leidos >= 4 && cabecera[0] == 0x25 && cabecera[1] == 0x50 && cabecera[2] == 0x44 && cabecera[3] == 0x46,
            _ => false
        };
    }
}
