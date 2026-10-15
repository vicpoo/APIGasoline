using System.Security.Cryptography;
using System.Text;

namespace APIGasoline.Application.Common.Security;

/// <summary>
/// Versión de los privilegios efectivos de un usuario, para la app offline-first.
/// Es el SHA-256 (hex en minúsculas) de las claves ordenadas y sin repetir, unidas con '\n'.
/// Cambia cuando se agrega o se QUITA un privilegio (filas borradas de rol_privilegio o
/// usuario_rol), sin necesitar columnas de fecha en la BD.
/// </summary>
public static class VersionPrivilegios
{
    public static string Calcular(IEnumerable<string> claves)
    {
        var ordenadas = claves
            .Select(c => c.Trim().ToLowerInvariant())
            .Where(c => c.Length > 0)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal);

        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(string.Join('\n', ordenadas)));
        return Convert.ToHexStringLower(bytes);
    }
}
