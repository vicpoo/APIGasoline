using APIGasoline.Application.Common.Models;

namespace APIGasoline.Application.Common.Interfaces;

/// <summary>
/// Almacenamiento de archivos (imágenes, PDFs). La implementación vive en Infrastructure (MinIO),
/// Application solo conoce esta abstracción. En la base de datos se guarda únicamente la llave.
/// </summary>
public interface IFileStorageService
{
    /// <summary>Sube el archivo con la llave indicada (ruta dentro del bucket).</summary>
    Task SubirAsync(ArchivoEntrada archivo, string llave, CancellationToken cancellationToken = default);

    /// <summary>Elimina el archivo. Lanza excepción si el almacenamiento falla.</summary>
    Task EliminarAsync(string llave, CancellationToken cancellationToken = default);

    /// <summary>
    /// Elimina el archivo sin lanzar excepción (solo registra el error en el log).
    /// Se usa para compensaciones y limpieza después de confirmar la transacción.
    /// </summary>
    Task<bool> IntentarEliminarAsync(string? llave);

    /// <summary>
    /// URL temporal (prefirmada) para descargar/ver el archivo. Devuelve null si la llave es null.
    /// </summary>
    Task<string?> ObtenerUrlAsync(string? llave, CancellationToken cancellationToken = default);
}
