using Microsoft.Data.SqlClient;

namespace APIGasoline.Infrastructure.Persistence;

/// <summary>
/// Detecta el error 1205 de SQL Server: dos transacciones se bloquearon entre sí y el motor
/// eligió esta como víctima (la deshizo completa). No es un error de los datos: repetir la
/// operación funciona. Pasa, por ejemplo, cuando el mismo lote GPS llega varias veces al mismo
/// tiempo e insertan en el índice único (viaje, timestamp) en distinto orden (N001).
/// </summary>
public static class Interbloqueo
{
    public const int NumeroSqlServer = 1205;

    /// <summary>true si la excepción (o alguna interna) es la víctima de un interbloqueo.</summary>
    public static bool Es(Exception? excepcion)
    {
        for (var actual = excepcion; actual is not null; actual = actual.InnerException)
        {
            if (actual is SqlException { Number: NumeroSqlServer })
                return true;
        }

        return false;
    }
}
