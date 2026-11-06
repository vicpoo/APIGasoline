namespace APIGasoline.Configuration;

/// <summary>
/// Sección "LimiteAuth" de appsettings: límite de peticiones por IP a los endpoints públicos de
/// autenticación (frena ataques de fuerza bruta a contraseñas y a refresh tokens).
/// Al pasarse, la API responde 429 Too Many Requests con el header Retry-After.
/// </summary>
public class LimiteAuthSettings
{
    public const string SectionName = "LimiteAuth";

    /// <summary>Nombre de la política para POST /api/auth/login.</summary>
    public const string PoliticaLogin = "auth-login";

    /// <summary>Nombre de la política para POST /api/auth/refresh y /api/auth/logout.</summary>
    public const string PoliticaRefresh = "auth-refresh";

    public bool Habilitado { get; set; } = true;

    /// <summary>Intentos de login por IP por minuto.</summary>
    public int LoginPorMinuto { get; set; } = 10;

    /// <summary>Renovaciones / logouts por IP por minuto (varios celulares pueden compartir IP del operador).</summary>
    public int RefreshPorMinuto { get; set; } = 60;

    public void Validar()
    {
        if (LoginPorMinuto is < 1 or > 1000)
            throw new InvalidOperationException("LimiteAuth:LoginPorMinuto debe estar entre 1 y 1000.");

        if (RefreshPorMinuto is < 1 or > 10000)
            throw new InvalidOperationException("LimiteAuth:RefreshPorMinuto debe estar entre 1 y 10000.");
    }
}
