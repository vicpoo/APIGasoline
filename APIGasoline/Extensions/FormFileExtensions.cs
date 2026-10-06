using APIGasoline.Application.Common.Models;

namespace APIGasoline.Extensions;

/// <summary>
/// Convierte el archivo HTTP (IFormFile) al modelo de Application, que no depende de ASP.NET Core.
/// </summary>
public static class FormFileExtensions
{
    /// <summary>
    /// Límite HTTP del cuerpo en los endpoints que reciben archivos (el límite de negocio, 5 MB,
    /// lo valida Application). Es un poco mayor para dejar espacio a los demás campos del formulario.
    /// </summary>
    public const long LimiteCuerpoBytes = 6 * 1024 * 1024;

    /// <param name="contenido">Stream abierto con <see cref="IFormFile.OpenReadStream"/>; lo cierra quien lo abrió.</param>
    public static ArchivoEntrada ToArchivoEntrada(this IFormFile archivo, Stream contenido)
        => new(contenido, archivo.FileName, archivo.ContentType, archivo.Length);
}
