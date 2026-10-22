namespace APIGasoline.Infrastructure.Storage;

/// <summary>
/// Sección "Minio" de appsettings (o variables de entorno Minio__Endpoint, Minio__AccessKey, etc.).
/// </summary>
public class MinioSettings
{
    public const string SectionName = "Minio";

    /// <summary>Host:puerto con el que la API se conecta a MinIO (ej. "localhost:9000" o "minio:9000" dentro de Docker).</summary>
    public string Endpoint { get; set; } = string.Empty;

    /// <summary>
    /// Host:puerto con el que se firman las URLs que recibe el cliente (navegador / app móvil).
    /// Dentro de Docker la API usa "minio:9000", pero ese nombre no existe fuera de Docker,
    /// por eso las URLs se firman con este endpoint público. Si está vacío se usa <see cref="Endpoint"/>.
    /// Con "auto" se usa el host con el que el cliente llamó a la API y el puerto de <see cref="Endpoint"/>
    /// (localhost desde Postman, 10.0.2.2 desde el emulador, la IP de la PC desde un celular). Solo para desarrollo.
    /// </summary>
    public string PublicEndpoint { get; set; } = string.Empty;

    public const string PublicEndpointAuto = "auto";

    public bool PublicEndpointEsAuto => string.Equals(PublicEndpoint?.Trim(), PublicEndpointAuto, StringComparison.OrdinalIgnoreCase);

    /// <summary>Puerto de MinIO tomado de <see cref="Endpoint"/> (ej. "localhost:9000" -> "9000").</summary>
    public string? PuertoEndpoint
    {
        get
        {
            var separador = Endpoint.LastIndexOf(':');
            return separador > 0 && separador < Endpoint.Length - 1 ? Endpoint[(separador + 1)..] : null;
        }
    }

    public string AccessKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public string Bucket { get; set; } = string.Empty;
    public bool UseSsl { get; set; }

    /// <summary>MinIO usa "us-east-1" por defecto. Fijarla evita que el SDK consulte la región por red al firmar URLs.</summary>
    public string Region { get; set; } = "us-east-1";

    /// <summary>Vigencia de las URLs prefirmadas.</summary>
    public int UrlExpiracionMinutos { get; set; } = 60;

    /// <summary>Endpoint fijo para firmar URLs (en modo "auto" es el de respaldo fuera de una petición HTTP).</summary>
    public string EndpointPublicoEfectivo => string.IsNullOrWhiteSpace(PublicEndpoint) || PublicEndpointEsAuto ? Endpoint : PublicEndpoint;

    /// <summary>Endpoint público con el host de la petición (modo "auto").</summary>
    public string EndpointPublicoPara(string? hostPeticion)
    {
        if (!PublicEndpointEsAuto || string.IsNullOrWhiteSpace(hostPeticion))
            return EndpointPublicoEfectivo;

        return PuertoEndpoint is null ? hostPeticion : $"{hostPeticion}:{PuertoEndpoint}";
    }

    public void Validar()
    {
        if (string.IsNullOrWhiteSpace(Endpoint))
            throw new InvalidOperationException("Falta Minio:Endpoint en la configuración.");

        if (string.IsNullOrWhiteSpace(AccessKey) || string.IsNullOrWhiteSpace(SecretKey))
            throw new InvalidOperationException("Faltan Minio:AccessKey o Minio:SecretKey en la configuración.");

        if (string.IsNullOrWhiteSpace(Bucket))
            throw new InvalidOperationException("Falta Minio:Bucket en la configuración.");

        // Las URLs prefirmadas de S3 duran como máximo 7 días
        if (UrlExpiracionMinutos <= 0 || UrlExpiracionMinutos > 7 * 24 * 60)
            throw new InvalidOperationException("Minio:UrlExpiracionMinutos debe estar entre 1 y 10080 (7 días).");
    }
}
