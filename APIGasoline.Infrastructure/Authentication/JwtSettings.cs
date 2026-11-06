namespace APIGasoline.Infrastructure.Authentication;

/// <summary>
/// Sección "Jwt" de appsettings.
/// </summary>
public class JwtSettings
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;

    /// <summary>Clave secreta HMAC-SHA256. Mínimo 32 caracteres. No subirla al repositorio en producción.</summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Duración del token de ACCESO. Debe ser corta (15 min): la sesión larga la da el refresh token
    /// (sección "Sesiones"), que sí se puede revocar.
    /// </summary>
    public int ExpiracionMinutos { get; set; } = 15;

    public void Validar()
    {
        if (string.IsNullOrWhiteSpace(Issuer) || string.IsNullOrWhiteSpace(Audience))
            throw new InvalidOperationException("Faltan Jwt:Issuer o Jwt:Audience en la configuración.");

        if (string.IsNullOrWhiteSpace(Key) || Key.Length < 32)
            throw new InvalidOperationException("Jwt:Key debe tener al menos 32 caracteres.");

        if (ExpiracionMinutos is < 1 or > 1440)
            throw new InvalidOperationException("Jwt:ExpiracionMinutos debe estar entre 1 y 1440 (24 h).");
    }
}
