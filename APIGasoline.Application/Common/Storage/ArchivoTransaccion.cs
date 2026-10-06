using APIGasoline.Application.Common.Interfaces;
using APIGasoline.Application.Common.Models;
using APIGasoline.Domain.Interfaces;

namespace APIGasoline.Application.Common.Storage;

/// <summary>
/// Coordina el almacenamiento de archivos con la transacción del Unit of Work.
///
/// MinIO no participa en la transacción de MySQL, por eso se usa compensación:
///   1. Se sube el archivo.
///   2. Se ejecutan los cambios en BD dentro de una transacción (Begin/Commit).
///   3. Si algo falla: Rollback y se elimina el archivo recién subido.
/// Así nunca queda en la BD una llave que apunte a un archivo inexistente.
/// </summary>
public static class ArchivoTransaccion
{
    public static async Task SubirYGuardarAsync(
        IUnitOfWork unitOfWork,
        IFileStorageService storage,
        ArchivoEntrada archivo,
        string llave,
        Func<CancellationToken, Task> cambiosEnBaseDeDatos,
        CancellationToken cancellationToken)
    {
        await storage.SubirAsync(archivo, llave, cancellationToken);

        try
        {
            await unitOfWork.BeginTransactionAsync(cancellationToken);
            await cambiosEnBaseDeDatos(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);
        }
        catch
        {
            // CancellationToken.None: la limpieza se hace aunque el cliente haya cancelado
            await unitOfWork.RollbackTransactionAsync(CancellationToken.None);
            await storage.IntentarEliminarAsync(llave);
            throw;
        }
    }
}
