namespace APIGasoline.Configuration;

/// <summary>
/// Sección "Seguridad" de appsettings: siembra del catálogo de privilegios y del administrador inicial
/// (la ejecuta SembrarSeguridadCommand al arrancar la API).
/// </summary>
public class SeguridadSettings
{
    public const string SectionName = "Seguridad";

    /// <summary>Al iniciar la API: crea los privilegios del catálogo que falten y el rol Administrador.</summary>
    public bool SembrarAlIniciar { get; set; } = true;

    /// <summary>
    /// Usuario administrador inicial (opcional). Si se configura y no existe, se crea con este password
    /// y con el rol Administrador. Si ya existe, solo se le asegura el rol (su password NO cambia).
    /// No subir el password real al repositorio.
    /// </summary>
    public string? AdminEmail { get; set; }

    public string? AdminPassword { get; set; }
}
