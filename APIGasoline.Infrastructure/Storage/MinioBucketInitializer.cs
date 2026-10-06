using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Minio;
using Minio.DataModel.Args;

namespace APIGasoline.Infrastructure.Storage;

/// <summary>
/// Al arrancar la API crea el bucket si no existe (el bucket es privado: solo se accede con URLs prefirmadas).
/// Si MinIO no responde, solo se registra una advertencia: la API arranca igual.
/// </summary>
public class MinioBucketInitializer : IHostedService
{
    private readonly MinioSettings _settings;
    private readonly ILogger<MinioBucketInitializer> _logger;

    public MinioBucketInitializer(MinioSettings settings, ILogger<MinioBucketInitializer> logger)
    {
        _settings = settings;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(TimeSpan.FromSeconds(10));

            using var client = new MinioClient()
                .WithEndpoint(_settings.Endpoint)
                .WithCredentials(_settings.AccessKey, _settings.SecretKey)
                .WithRegion(_settings.Region)
                .WithSSL(_settings.UseSsl)
                .Build();

            var existe = await client.BucketExistsAsync(
                new BucketExistsArgs().WithBucket(_settings.Bucket), timeout.Token);

            if (!existe)
            {
                await client.MakeBucketAsync(
                    new MakeBucketArgs().WithBucket(_settings.Bucket).WithLocation(_settings.Region), timeout.Token);

                _logger.LogInformation("Bucket de MinIO creado: {Bucket}", _settings.Bucket);
            }
            else
            {
                _logger.LogInformation("Bucket de MinIO listo: {Bucket}", _settings.Bucket);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "No se pudo verificar el bucket {Bucket} en {Endpoint}. ¿Está levantado MinIO (docker compose up -d)?",
                _settings.Bucket, _settings.Endpoint);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
