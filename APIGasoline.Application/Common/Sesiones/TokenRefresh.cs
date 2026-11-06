using System.Buffers.Text;
using System.Security.Cryptography;
using System.Text;

namespace APIGasoline.Application.Common.Sesiones;

/// <summary>
/// Refresh token: 32 bytes aleatorios (256 bits) en Base64Url. No es un JWT: no lleva datos y no se
/// puede validar sin la BD, que es justo lo que permite revocarlo.
/// En la BD solo se guarda su hash SHA-256: si alguien lee la tabla, no obtiene tokens utilizables.
/// (Con 256 bits aleatorios no hace falta sal ni un hash lento como el de las contraseñas.)
/// </summary>
public static class TokenRefresh
{
    public const int BytesAleatorios = 32;

    /// <summary>Largo máximo aceptado al recibirlo (evita hashear cuerpos gigantes).</summary>
    public const int LargoMaximo = 200;

    public static string Generar()
        => Base64Url.EncodeToString(RandomNumberGenerator.GetBytes(BytesAleatorios));

    /// <summary>SHA-256 en hexadecimal en minúsculas (64 caracteres).</summary>
    public static string Hash(string token)
        => Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(token)));

    public static bool FormatoValido(string? token)
        => !string.IsNullOrWhiteSpace(token) && token.Length <= LargoMaximo;
}
