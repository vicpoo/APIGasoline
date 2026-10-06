namespace APIGasoline.Application.Common.Storage;

/// <summary>
/// Genera las llaves (rutas dentro del bucket) con un formato uniforme.
/// Cada versión del archivo lleva un id único (GUID) para que tenga una llave distinta
/// (así se puede borrar la anterior sin afectar a la nueva y no hay problemas de caché).
/// El id lo genera quien llama (Guid.NewGuid()), así esta clase es pura y se puede probar.
/// </summary>
public static class LlavesArchivo
{
    public static string FotoVehiculo(int empresaId, int carroId, string extension, Guid id)
        => $"empresas/{empresaId}/carros/{carroId}/vehiculo-{id:N}{extension}";

    public static string FotoPerfilEmpleado(int empresaId, int empleadoId, string extension, Guid id)
        => $"empresas/{empresaId}/empleados/{empleadoId}/perfil-{id:N}{extension}";

    public static string FotoLicenciaEmpleado(int empresaId, int empleadoId, string extension, Guid id)
        => $"empresas/{empresaId}/empleados/{empleadoId}/licencia-{id:N}{extension}";

    /// <summary>
    /// La carga todavía no tiene id cuando se sube el comprobante, por eso se agrupa por viaje.
    /// </summary>
    public static string ComprobanteCarga(int empresaId, int viajeId, string extension, Guid id)
        => $"empresas/{empresaId}/viajes/{viajeId}/cargas/comprobante-{id:N}{extension}";
}
