using APIGasoline.Application.Common.Interfaces;
using APIGasoline.Application.Common.Outbox;
using APIGasoline.Application.Features.Auditoria.Common;
using APIGasoline.Domain.Interfaces;
using APIGasoline.Infrastructure.Auditoria;
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
using APIGasoline.Application.Features.Reportes.Common;
using APIGasoline.Infrastructure.Reportes;

namespace APIGasoline.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString, JwtSettings jwtSettings, MinioSettings minioSettings, OutboxSettings outboxSettings, PushSettings pushSettings)
    {
        // Los Guid se guardan como binary(16) en orden RFC 4122 (compatible con UUID_TO_BIN / BIN_TO_UUID).
        // Se fuerza aquí para no depender de que cada cadena de conexión lo traiga.
        var cadenaConexion = new MySqlConnectionStringBuilder(connectionString)
        {
            GuidFormat = MySqlGuidFormat.Binary16
        }.ConnectionString;

        // Auditoría: el interceptor agrega un auditoria_evento por cada cambio en el mismo SaveChanges.
        // Requiere que la API registre IUsuarioActual (usuario del token).
        services.AddScoped<IContextoAuditoria, ContextoAuditoria>();
        services.AddScoped<AuditoriaInterceptor>();

        services.AddDbContext<AppDbContext>((serviceProvider, options) =>
            options
                .UseMySql(cadenaConexion, new MySqlServerVersion(new Version(8, 0, 36)))
                .AddInterceptors(serviceProvider.GetRequiredService<AuditoriaInterceptor>()));

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IMigradorBaseDatos, MigradorBaseDatos>();
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();

        // Seguridad
        jwtSettings.Validar();
        services.AddSingleton(jwtSettings);
        services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();
        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();

        // Autorización basada en permisos: privilegios efectivos del usuario (BD + caché corta)
        services.AddMemoryCache();
        services.AddScoped<IConsultaPrivilegios, ConsultaPrivilegios>();

        // Almacenamiento de archivos (MinIO). En BD solo se guarda la llave del archivo.
        minioSettings.Validar();
        services.AddSingleton(minioSettings);
        services.AddSingleton<IFileStorageService, MinioFileStorageService>();
        services.AddHostedService<MinioBucketInitializer>();

        // Notificaciones push con patrón Outbox. El proveedor se elige en Push:Proveedor:
        // "NoOp" (simulado, para desarrollo y pruebas) o "Firebase" (envío real con FCM).
        outboxSettings.Validar();
        services.AddSingleton(outboxSettings);

        pushSettings.Validar();
        services.AddSingleton(pushSettings);

        if (pushSettings.UsaFirebase)
            services.AddSingleton<IPushNotificationService, FirebasePushService>();
        else
            services.AddSingleton<IPushNotificationService, NoOpPushService>();
        services.AddHostedService<OutboxWorker>();

        // Reportes de la flota (PDF y Excel). Sin estado: una instancia para toda la app
        services.AddSingleton<IReportePdfGenerator, ReportePdfGenerator>();
        services.AddSingleton<IReporteExcelGenerator, ReporteExcelGenerator>();

        return services;
    }
}
