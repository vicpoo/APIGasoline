// ============================================================
//  APIGasoline - Orquestador de desarrollo (Aspire)
//
//  dotnet run --project APIGasoline.AppHost
//
//  Levanta, en este orden:
//    1. SQL Server 2022 (contenedor, datos en el volumen "apigasoline-sql")
//    2. MinIO (contenedor, datos en el volumen "apigasoline-minio", puertos 9000 y 9001)
//    3. La API, que al arrancar crea la BD, aplica migraciones y siembra seguridad + datos demo.
//
//  El panel de Aspire (la URL sale en la consola) muestra logs, variables y estado de cada uno.
//  Requiere Docker Desktop abierto. Si tienes corriendo "docker compose up", detenlo antes
//  (docker compose down): usan los mismos puertos de MinIO.
// ============================================================

var builder = DistributedApplication.CreateBuilder(args);

// ---------- SQL Server ----------
// La contraseña de sa la genera Aspire la primera vez y la guarda en los user-secrets del AppHost.
// Persistent: el contenedor sigue vivo entre ejecuciones (arranca más rápido y no pierde datos).
var sql = builder.AddSqlServer("sql")
    .WithImageTag("2022-latest")
    .WithEnvironment("TZ", "America/Mexico_City")
    .WithDataVolume("apigasoline-sql")
    .WithLifetime(ContainerLifetime.Persistent);

// La BD NO se crea con AddDatabase: así la crea EF al migrar, con la intercalación
// Modern_Spanish_CI_AI del modelo (AddDatabase la crearía con la intercalación por defecto).
var cadenaConexion = ReferenceExpression.Create(
    $"{sql.Resource.ConnectionStringExpression};Database=apigasoline");

// ---------- MinIO ----------
// Se usa como contenedor genérico con el fork pgsty/minio (el mismo de docker-compose):
// MinIO retiró sus imágenes de Docker Hub y la integración de Aspire para MinIO está obsoleta.
var minioUsuario = builder.AddParameter("minio-usuario", "minioadmin");
var minioPassword = builder.AddParameter("minio-password", "minioadmin123", secret: true);

var minio = builder.AddContainer("minio", "pgsty/minio", "RELEASE.2026-08-04T00-00-00Z")
    .WithArgs("server", "/data", "--console-address", ":9001")
    .WithEnvironment("MINIO_ROOT_USER", minioUsuario)
    .WithEnvironment("MINIO_ROOT_PASSWORD", minioPassword)
    .WithEnvironment("TZ", "America/Mexico_City")
    // Puertos fijos: la app móvil y el emulador (10.0.2.2:9000) los esperan así
    .WithHttpEndpoint(port: 9000, targetPort: 9000, name: "api")
    .WithHttpEndpoint(port: 9001, targetPort: 9001, name: "consola")
    .WithHttpHealthCheck("/minio/health/live", endpointName: "api")
    .WithVolume("apigasoline-minio", "/data")
    .WithLifetime(ContainerLifetime.Persistent);

var minioApi = minio.GetEndpoint("api");

// ---------- API ----------
builder.AddProject<Projects.APIGasoline>("api")
    .WithEnvironment("ConnectionStrings__DefaultConnection", cadenaConexion)
    .WithEnvironment("Minio__Endpoint", ReferenceExpression.Create($"{minioApi.Property(EndpointProperty.HostAndPort)}"))
    .WithEnvironment("Minio__AccessKey", minioUsuario)
    .WithEnvironment("Minio__SecretKey", minioPassword)
    .WithEnvironment("BaseDatos__MigrarAlIniciar", "true")
    .WithEnvironment("Seguridad__SembrarAlIniciar", "true")
    .WithEnvironment("Seguridad__SembrarDemo", "true")
    .WaitFor(sql)
    .WaitFor(minio);

builder.Build().Run();
