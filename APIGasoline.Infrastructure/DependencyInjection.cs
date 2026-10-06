using APIGasoline.Application.Common.Interfaces;
using APIGasoline.Domain.Interfaces;
using APIGasoline.Infrastructure.Authentication;
using APIGasoline.Infrastructure.Persistence;
using APIGasoline.Infrastructure.Security;
using APIGasoline.Infrastructure.Services;
using APIGasoline.Infrastructure.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace APIGasoline.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString, JwtSettings jwtSettings, MinioSettings minioSettings)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 36))));

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

        return services;
    }
}
