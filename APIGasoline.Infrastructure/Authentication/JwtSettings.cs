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

    public int ExpiracionMinutos { get; set; } = 480;

    public void Validar()
    {
        if (string.IsNullOrWhiteSpace(Issuer) || string.IsNullOrWhiteSpace(Audience))
            throw new InvalidOperationException("Faltan Jwt:Issuer o Jwt:Audience en la configuración.");

        if (string.IsNullOrWhiteSpace(Key) || Key.Length < 32)
            throw new InvalidOperationException("Jwt:Key debe tener al menos 32 caracteres.");

        if (ExpiracionMinutos <= 0)
            throw new InvalidOperationException("Jwt:ExpiracionMinutos debe ser mayor a cero.");
    }
}
