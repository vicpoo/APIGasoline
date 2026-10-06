using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Common.Interfaces;
using APIGasoline.Application.Common.Models;
using Microsoft.Extensions.Logging;
using Minio;
using Minio.DataModel.Args;

namespace APIGasoline.Infrastructure.Storage;

/// <summary>
/// Implementación de <see cref="IFileStorageService"/> con MinIO (compatible con S3).
/// Es la única clase del proyecto que conoce el SDK de MinIO.
/// </summary>
public class MinioFileStorageService : IFileStorageService
{
    private readonly IMinioClient _client;        // operaciones (subir, borrar) -> Endpoint interno
    private readonly IMinioClient _clientPublico; // solo firma URLs -> PublicEndpoint (no hace llamadas de red)
    private readonly MinioSettings _settings;
    private readonly ILogger<MinioFileStorageService> _logger;

    public MinioFileStorageService(MinioSettings settings, ILogger<MinioFileStorageService> logger)
    {
        _settings = settings;
        _logger = logger;
        _client = CrearCliente(settings.Endpoint);
        _clientPublico = CrearCliente(settings.EndpointPublicoEfectivo);
    }

    public async Task SubirAsync(ArchivoEntrada archivo, string llave, CancellationToken cancellationToken = default)
    {
        if (archivo.Contenido.CanSeek)
            archivo.Contenido.Position = 0;

        var args = new PutObjectArgs()
            .WithBucket(_settings.Bucket)
            .WithObject(llave)
            .WithStreamData(archivo.Contenido)
            .WithObjectSize(archivo.Tamano)
            .WithContentType(archivo.ContentType);

        try
        {
            await _client.PutObjectAsync(args, cancellationToken);
            _logger.LogInformation("Archivo subido a MinIO: {Llave} ({Tamano} bytes)", llave, archivo.Tamano);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "No se pudo subir el archivo {Llave} a MinIO.", llave);
            throw new AlmacenamientoException("No se pudo guardar el archivo. Intenta de nuevo más tarde.", ex);
        }
    }

    public async Task EliminarAsync(string llave, CancellationToken cancellationToken = default)
    {
        var args = new RemoveObjectArgs()
            .WithBucket(_settings.Bucket)
            .WithObject(llave);

        try
        {
            await _client.RemoveObjectAsync(args, cancellationToken);
            _logger.LogInformation("Archivo eliminado de MinIO: {Llave}", llave);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "No se pudo eliminar el archivo {Llave} de MinIO.", llave);
            throw new AlmacenamientoException("No se pudo eliminar el archivo. Intenta de nuevo más tarde.", ex);
        }
    }

    public async Task<bool> IntentarEliminarAsync(string? llave)
    {
        if (string.IsNullOrWhiteSpace(llave))
            return false;

        try
        {
            await EliminarAsync(llave, CancellationToken.None);
            return true;
        }
        catch (Exception ex)
        {
            // Queda un archivo huérfano en el bucket, pero la BD está consistente
            _logger.LogWarning(ex, "Archivo huérfano en MinIO (no se pudo limpiar): {Llave}", llave);
            return false;
        }
    }

    public async Task<string?> ObtenerUrlAsync(string? llave, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(llave))
            return null;

        var args = new PresignedGetObjectArgs()
            .WithBucket(_settings.Bucket)
            .WithObject(llave)
            .WithExpiry(_settings.UrlExpiracionMinutos * 60);

        try
        {
            return await _clientPublico.PresignedGetObjectAsync(args);
        }
        catch (Exception ex)
        {
            // Una URL que no se pudo firmar no debe tumbar la consulta completa
            _logger.LogWarning(ex, "No se pudo generar la URL del archivo {Llave}.", llave);
            return null;
        }
    }

    private IMinioClient CrearCliente(string endpoint)
        => new MinioClient()
            .WithEndpoint(endpoint)
            .WithCredentials(_settings.AccessKey, _settings.SecretKey)
            .WithRegion(_settings.Region)
            .WithSSL(_settings.UseSsl)
            .Build();
}
