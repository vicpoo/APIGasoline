namespace APIGasoline.Application.Common.Models;

/// <summary>
/// Archivo recibido por la API, independiente de HTTP (Application no conoce IFormFile).
/// El stream lo abre y lo cierra la capa API.
/// </summary>
public record ArchivoEntrada(
    Stream Contenido,
    string NombreOriginal,
    string ContentType,
    long Tamano);
