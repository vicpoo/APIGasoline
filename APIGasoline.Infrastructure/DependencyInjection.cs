using APIGasoline.Application.Common.Interfaces;
using APIGasoline.Application.Common.Outbox;
using APIGasoline.Application.Common.Sesiones;
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
using APIGasoline.Application.Features.Reportes.Common;
using APIGasoline.Infrastructure.Reportes;

namespace APIGasoline.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString, JwtSettings jwtSettings, SesionesSettings sesionesSettings, MinioSettings minioSettings, OutboxSettings outboxSettings, PushSettings pushSettings)
    {
        // Auditoría: el interceptor agrega un auditoria_evento por cada cambio en el mismo SaveChanges.
        // Requiere que la API registre IUsuarioActual (usuario del token).
        services.AddScoped<IContextoAuditoria, ContextoAuditoria>();
        services.AddScoped<AuditoriaInterceptor>();

        // Aislamiento entre empresas: el filtro global de AppDbContext limita las lecturas y este
        // guardián las escrituras (va ANTES que la auditoría: si rechaza, no se registra nada).
        services.AddScoped<AislamientoEmpresaInterceptor>();

        // SQL Server. Los Guid se guardan como binary(16) en orden RFC 4122 (ver AppDbContext.ConfigureConventions).
        // Sin EnableRetryOnFailure: la API abre transacciones manuales (IUnitOfWork.BeginTransactionAsync)
        // y la estrategia de reintentos de EF no las permite.
        services.AddDbContext<AppDbContext>((serviceProvider, options) =>
            options
                .UseSqlServer(connectionString)
                .AddInterceptors(
                    serviceProvider.GetRequiredService<AislamientoEmpresaInterceptor>(),
                    serviceProvider.GetRequiredService<AuditoriaInterceptor>()));

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IMigradorBaseDatos, MigradorBaseDatos>();
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();

        // Seguridad
        jwtSettings.Validar();
        services.AddSingleton(jwtSettings);
        services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();
        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();

        // Sesiones largas con refresh tokens (rotación + revocación) y limpieza periódica de las viejas
        sesionesSettings.Validar();
        services.AddSingleton(sesionesSettings);
        services.AddHostedService<LimpiezaSesionesWorker>();

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
