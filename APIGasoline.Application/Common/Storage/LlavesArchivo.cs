namespace APIGasoline.Application.Common.Storage;

/// <summary>
/// Llaves (rutas dentro del bucket) de MinIO. Formato:
/// empresa/{empresaId}/{entidad}/{id}/{carpeta}/{tipo}-{version}{extension}
/// La versión es un Guid nuevo en cada subida: así se puede borrar el archivo anterior
/// sin afectar al nuevo y no hay problemas de caché.
/// </summary>
public static class LlavesArchivo
{
    public static string FotoVehiculo(Guid empresaId, Guid carroId, string extension, Guid version)
        => $"empresa/{empresaId}/carro/{carroId}/foto/vehiculo-{version:N}{extension}";

    public static string FotoPerfilEmpleado(Guid empresaId, Guid empleadoId, string extension, Guid version)
        => $"empresa/{empresaId}/empleado/{empleadoId}/perfil/perfil-{version:N}{extension}";

    public static string FotoLicenciaEmpleado(Guid empresaId, Guid empleadoId, string extension, Guid version)
        => $"empresa/{empresaId}/empleado/{empleadoId}/documento/licencia-{version:N}{extension}";

    public static string ComprobanteCarga(Guid empresaId, Guid viajeId, Guid cargaCombustibleId, string extension, Guid version)
        => $"empresa/{empresaId}/viaje/{viajeId}/carga/{cargaCombustibleId}/comprobante-{version:N}{extension}";

    /// <param name="momento">"inicial" o "final".</param>
    public static string FotoTablero(Guid empresaId, Guid viajeId, string momento, string extension, Guid version)
        => $"empresa/{empresaId}/viaje/{viajeId}/tablero/{momento}-{version:N}{extension}";

    public static string EvidenciaCambioEstatus(Guid empresaId, Guid carroId, Guid cambioEstatusCarroId, string extension, Guid version)
        => $"empresa/{empresaId}/carro/{carroId}/estatus/{cambioEstatusCarroId}/evidencia-{version:N}{extension}";
}
