using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace APIGasoline.Infrastructure.Persistence;

/// <summary>
/// Traduce el error de SQL Server por llave o índice único duplicado (2601 / 2627) a un mensaje
/// para el cliente (409). Hace falta por Row-Level Security: si el dato repetido es de OTRA empresa,
/// la validación previa del handler no lo ve y el que lo detecta es el índice único.
/// El mensaje nunca incluye el valor repetido ni de quién es.
/// </summary>
public static class ViolacionUnicidad
{
    private static readonly (string Indice, string Mensaje)[] Mensajes =
    [
        ("uq_usuario_email", "Ya existe un usuario con ese correo."),
        ("uq_carro_placas", "Ya existe un carro con esas placas."),
        ("uq_dispositivo_empleado_token_fcm", "Este dispositivo ya está registrado a otro empleado."),
        ("uq_empleado_usuario_id", "El usuario ya está asignado a otro empleado."),
        ("uq_centro_costo_empresa_id_clave", "Ya existe un centro de costo con esa clave."),
        ("uq_rol_empresa_id_nombre", "Ya existe un rol con ese nombre."),
        ("uq_privilegio_clave", "Ya existe un privilegio con esa clave."),
        ("uq_usuario_rol_usuario_id_rol_id", "El usuario ya tiene ese rol."),
        ("uq_rol_privilegio_rol_id_privilegio_id", "El rol ya tiene ese privilegio."),
        ("uq_ruta_procesada_viaje_id", "El viaje ya tiene ruta procesada."),
    ];

    private const string MensajeGeneral = "Ya existe un registro con esos datos (o con ese id).";

    public static bool EsDuplicado(DbUpdateException excepcion, out string mensaje)
    {
        mensaje = string.Empty;

        if (excepcion.InnerException is not SqlException { Number: 2601 or 2627 } sql)
            return false;

        mensaje = Mensajes.FirstOrDefault(m => sql.Message.Contains($"'{m.Indice}'", StringComparison.OrdinalIgnoreCase)).Mensaje
                  ?? MensajeGeneral;
        return true;
    }
}
