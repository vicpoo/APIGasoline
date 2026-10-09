namespace APIGasoline.Infrastructure.Push;

/// <summary>
/// Sección "Push" de appsettings (o variables de entorno Push__Proveedor, Push__Firebase__CredencialesPath).
/// </summary>
public class PushSettings
{
    public const string SectionName = "Push";
    public const string ProveedorNoOp = "NoOp";
    public const string ProveedorFirebase = "Firebase";

    /// <summary>"NoOp" (simulado, solo escribe en el log) o "Firebase" (envío real con FCM).</summary>
    public string Proveedor { get; set; } = ProveedorNoOp;

    public FirebasePushSettings Firebase { get; set; } = new();

    public bool UsaFirebase => string.Equals(Proveedor, ProveedorFirebase, StringComparison.OrdinalIgnoreCase);

    public void Validar()
    {
        if (!UsaFirebase && !string.Equals(Proveedor, ProveedorNoOp, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException($"Push:Proveedor debe ser \"{ProveedorNoOp}\" o \"{ProveedorFirebase}\".");

        if (!UsaFirebase)
            return;

        if (string.IsNullOrWhiteSpace(Firebase.CredencialesPath))
            throw new InvalidOperationException("Falta Push:Firebase:CredencialesPath (ruta del JSON de la service account).");

        if (!File.Exists(Firebase.CredencialesPath))
            throw new InvalidOperationException($"No se encontró el archivo de credenciales de Firebase: {Firebase.CredencialesPath}");
    }
}

public class FirebasePushSettings
{
    /// <summary>
    /// Ruta absoluta del JSON de la service account (Firebase > Configuración del proyecto > Cuentas de servicio).
    /// Es un secreto: debe estar fuera del repositorio.
    /// </summary>
    public string CredencialesPath { get; set; } = string.Empty;

    /// <summary>Opcional: si está vacío se toma del JSON de credenciales.</summary>
    public string ProjectId { get; set; } = string.Empty;
}
