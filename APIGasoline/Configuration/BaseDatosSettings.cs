namespace APIGasoline.Configuration;

/// <summary>
/// Sección "BaseDatos" de appsettings (o variable de entorno BaseDatos__MigrarAlIniciar).
/// </summary>
public class BaseDatosSettings
{
    public const string SectionName = "BaseDatos";

    /// <summary>
    /// Al iniciar la API aplica las migraciones pendientes (AplicarMigracionesCommand).
    /// En producción (Docker/EC2) va en true; en desarrollo puedes seguir usando "dotnet ef database update".
    /// </summary>
    public bool MigrarAlIniciar { get; set; }
}
