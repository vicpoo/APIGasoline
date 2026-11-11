using APIGasoline.Application.Common.Interfaces;
using APIGasoline.Infrastructure.Storage;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace APIGasoline.Api.IntegrationTests.Infraestructura;

/// <summary>
/// La API completa en memoria, conectada al SQL Server de Testcontainers.
/// - Ambiente "Pruebas": no carga appsettings.Development.json ni appsettings.Local.json.
/// - Migra y siembra la seguridad al arrancar (igual que en producción).
/// - MinIO se reemplaza por <see cref="AlmacenamientoFalso"/>; push en NoOp.
/// </summary>
public sealed class ApiFactory : WebApplicationFactory<Program>
{
    public const string AdminPlataformaEmail = "plataforma@pruebas.local";
    public const string AdminPlataformaPassword = "Plataforma#Pruebas2026";

    private readonly string _cadenaConexion;

    public ApiFactory(string cadenaConexion)
    {
        _cadenaConexion = cadenaConexion;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Pruebas");

        // UseSetting: Program.cs lee estos valores antes de Build(), por eso no basta ConfigureAppConfiguration
        builder.UseSetting("ConnectionStrings:DefaultConnection", _cadenaConexion);
        builder.UseSetting("Jwt:Key", "llave-solo-para-pruebas-de-integracion-0123456789-abcdef");
        builder.UseSetting("BaseDatos:MigrarAlIniciar", "true");
        builder.UseSetting("Seguridad:SembrarAlIniciar", "true");
        builder.UseSetting("Seguridad:AdminEmail", AdminPlataformaEmail);
        builder.UseSetting("Seguridad:AdminPassword", AdminPlataformaPassword);
        builder.UseSetting("Seguridad:SembrarDemo", "false"); // el escenario lo siembra a mano
        builder.UseSetting("LimiteAuth:Habilitado", "false");
        builder.UseSetting("Push:Proveedor", "NoOp");
        builder.UseSetting("Minio:Endpoint", "almacenamiento.falso:9000");
        builder.UseSetting("Minio:PublicEndpoint", "almacenamiento.falso:9000");
        builder.UseSetting("Minio:AccessKey", "falso");
        builder.UseSetting("Minio:SecretKey", "falso-falso");
        builder.UseSetting("Minio:Bucket", "pruebas");

        builder.ConfigureTestServices(servicios =>
        {
            servicios.QuitarTodos<IFileStorageService>();
            servicios.AddSingleton<IFileStorageService, AlmacenamientoFalso>();

            // Sin MinIO no hay bucket que crear (y esperaría 10 s su timeout)
            var inicializador = servicios.Where(s => s.ImplementationType == typeof(MinioBucketInitializer)).ToList();
            foreach (var descriptor in inicializador)
                servicios.Remove(descriptor);
        });
    }
}

internal static class ServiceCollectionExtensions
{
    public static void QuitarTodos<T>(this IServiceCollection servicios)
    {
        foreach (var descriptor in servicios.Where(s => s.ServiceType == typeof(T)).ToList())
            servicios.Remove(descriptor);
    }
}
