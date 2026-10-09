using APIGasoline.Application.Common.Interfaces;
using APIGasoline.Application.Common.Outbox;
using APIGasoline.Domain.Interfaces;
using APIGasoline.Infrastructure.Authentication;
using APIGasoline.Infrastructure.Outbox;
using APIGasoline.Infrastructure.Persistence;
using APIGasoline.Infrastructure.Push;
using APIGasoline.Infrastructure.Security;
using APIGasoline.Infrastructure.Services;
using APIGasoline.Infrastructure.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MySqlConnector;

namespace APIGasoline.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString, JwtSettings jwtSettings, MinioSettings minioSettings, OutboxSettings outboxSettings)
    {
        // Los Guid se guardan como binary(16) en orden RFC 4122 (compatible con UUID_TO_BIN / BIN_TO_UUID).
        // Se fuerza aquí para no depender de que cada cadena de conexión lo traiga.
        var cadenaConexion = new MySqlConnectionStringBuilder(connectionString)
        {
            GuidFormat = MySqlGuidFormat.Binary16
        }.ConnectionString;

        services.AddDbContext<AppDbContext>(options =>
            options.UseMySql(cadenaConexion, new MySqlServerVersion(new Version(8, 0, 36))));

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();

        // Seguridad
        jwtSettings.Validar();
        services.AddSingleton(jwtSettings);
        services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();
        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();

        // Almacenamiento de archivos (MinIO). En BD solo se guarda la llave del archivo.
        minioSettings.Validar();
        services.AddSingleton(minioSettings);
        services.AddSingleton<IFileStorageService, MinioFileStorageService>();
        services.AddHostedService<MinioBucketInitializer>();

        // Notificaciones push con patrón Outbox. Por ahora el envío es simulado (NoOp);
        // en el paso de Firebase se cambia solo esta línea por FirebasePushService.
        outboxSettings.Validar();
        services.AddSingleton(outboxSettings);
        services.AddSingleton<IPushNotificationService, NoOpPushService>();
        services.AddHostedService<OutboxWorker>();

        return services;
    }
}
