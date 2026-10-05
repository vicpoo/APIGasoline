using System.Security.Cryptography;
using APIGasoline.Application.Common.Interfaces;

namespace APIGasoline.Infrastructure.Security;

/// <summary>
/// Hash de contraseñas con PBKDF2-HMAC-SHA256 (incluido en .NET, sin paquetes extra).
/// Formato guardado: PBKDF2-SHA256$iteraciones$salt(base64)$hash(base64)
/// Cada contraseña lleva su propia sal aleatoria, así dos contraseñas iguales tienen hashes distintos.
/// </summary>
public sealed class Pbkdf2PasswordHasher : IPasswordHasher
{
    private const string Prefijo = "PBKDF2-SHA256";
    private const int SaltSize = 16;          // 128 bits
    private const int KeySize = 32;           // 256 bits
    private const int Iteraciones = 600_000;  // Recomendación OWASP para PBKDF2-HMAC-SHA256

    private static readonly HashAlgorithmName Algoritmo = HashAlgorithmName.SHA256;

    public string Hash(string password)
    {
        ArgumentException.ThrowIfNullOrEmpty(password);

        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var key = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iteraciones, Algoritmo, KeySize);

        return $"{Prefijo}${Iteraciones}${Convert.ToBase64String(salt)}${Convert.ToBase64String(key)}";
    }

    public bool Verify(string password, string passwordHash)
    {
        if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(passwordHash))
            return false;

        var partes = passwordHash.Split('$');

        if (partes.Length != 4 || partes[0] != Prefijo || !int.TryParse(partes[1], out var iteraciones) || iteraciones <= 0)
            return false;

        byte[] salt;
        byte[] esperado;

        try
        {
            salt = Convert.FromBase64String(partes[2]);
            esperado = Convert.FromBase64String(partes[3]);
        }
        catch (FormatException)
        {
            return false;
        }

        var calculado = Rfc2898DeriveBytes.Pbkdf2(password, salt, iteraciones, Algoritmo, esperado.Length);

        // Comparación en tiempo constante
        return CryptographicOperations.FixedTimeEquals(calculado, esperado);
    }
}
