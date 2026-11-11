using System.Text.Json;
using System.Threading.RateLimiting;
using System.Text.Json.Serialization;
using System.Text;
using APIGasoline.Application;
using APIGasoline.Application.Common.Interfaces;
using APIGasoline.Application.Common.Outbox;
using APIGasoline.Application.Common.Sesiones;
using APIGasoline.Application.Features.Auth.Commands;
using APIGasoline.Application.Features.BaseDatos.Commands;
using APIGasoline.Authorization;
using APIGasoline.Configuration;
using APIGasoline.Infrastructure;
using APIGasoline.Infrastructure.Authentication;
using APIGasoline.Infrastructure.Push;
using APIGasoline.Infrastructure.Storage;
using APIGasoline.Middleware;
using APIGasoline.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Desarrollo: appsettings.Local.json (en .gitignore) guarda los valores personales de cada quien
// (llave JWT propia, Firebase, otra BD...). Se inserta justo después de appsettings.{Ambiente}.json
// para que las variables de entorno (Aspire, Docker) sigan teniendo prioridad sobre él.
if (builder.Environment.IsDevelopment())
    ConfiguracionLocal.Agregar(builder.Configuration, builder.Environment);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Falta la cadena de conexión 'DefaultConnection'.");

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
    });

builder.Services.AddOpenApi();

builder.Services.AddApplication();

// Usuario del token (lo usan la auditoría y las reglas de corrección)
builder.Services.AddHttpContextAccessor();
// Minio:PublicEndpoint = "auto": las URLs de las fotos se firman con el host con el que llamó el cliente
builder.Services.AddSingleton<IHostPeticionActual, HostPeticionActual>();
builder.Services.AddScoped<IUsuarioActual, UsuarioActual>();
var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
    ?? throw new InvalidOperationException("Falta la sección 'Jwt' en la configuración.");

// Desarrollo sin llave configurada (repo recién clonado): se genera una al azar en cada arranque.
// Los access tokens anteriores dejan de servir al reiniciar, pero los refresh tokens siguen
// funcionando (no son JWT). En cualquier otro ambiente la llave es obligatoria (Validar() falla).
var jwtKeyGenerada = false;
if (builder.Environment.IsDevelopment() && string.IsNullOrWhiteSpace(jwtSettings.Key))
{
    jwtSettings.Key = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(48));
    jwtKeyGenerada = true;
}

var sesionesSettings = builder.Configuration.GetSection(SesionesSettings.SectionName).Get<SesionesSettings>()
    ?? new SesionesSettings();

var limiteAuthSettings = builder.Configuration.GetSection(LimiteAuthSettings.SectionName).Get<LimiteAuthSettings>()
    ?? new LimiteAuthSettings();
limiteAuthSettings.Validar();

var minioSettings = builder.Configuration.GetSection(MinioSettings.SectionName).Get<MinioSettings>()
    ?? throw new InvalidOperationException("Falta la sección 'Minio' en la configuración.");

var outboxSettings = builder.Configuration.GetSection(OutboxSettings.SectionName).Get<OutboxSettings>()
    ?? new OutboxSettings();

var pushSettings = builder.Configuration.GetSection(PushSettings.SectionName).Get<PushSettings>()
    ?? new PushSettings();

var seguridadSettings = builder.Configuration.GetSection(SeguridadSettings.SectionName).Get<SeguridadSettings>()
    ?? new SeguridadSettings();

var baseDatosSettings = builder.Configuration.GetSection(BaseDatosSettings.SectionName).Get<BaseDatosSettings>()
    ?? new BaseDatosSettings();

builder.Services.AddInfrastructure(connectionString, jwtSettings, sesionesSettings, minioSettings, outboxSettings, pushSettings);

// Autenticación con JWT (el token lo emite POST /api/auth/login)
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Conserva los nombres de los claims tal cual ("sub", "role", "empresa_id"...)
        options.MapInboundClaims = false;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtSettings.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1),
            NameClaimType = "email",
            RoleClaimType = "role"
        };
    });

// Autorización basada en permisos (privilegios), nunca en el nombre del rol:
// - Todo endpoint pide sesión por defecto (FallbackPolicy); los públicos llevan [AllowAnonymous].
// - Cada acción pide su privilegio con [RequierePrivilegio("modulo.accion")].
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});
builder.Services.AddScoped<IAuthorizationHandler, PrivilegioAuthorizationHandler>();
builder.Services.AddSingleton<IAuthorizationMiddlewareResultHandler, RespuestaAutorizacionHandler>();

// Límite de peticiones por IP a login / refresh / logout (fuerza bruta). Ventana fija de 1 minuto.
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddPolicy(LimiteAuthSettings.PoliticaLogin, http =>
        LimitePorIp(http, limiteAuthSettings.Habilitado, limiteAuthSettings.LoginPorMinuto));

    options.AddPolicy(LimiteAuthSettings.PoliticaRefresh, http =>
        LimitePorIp(http, limiteAuthSettings.Habilitado, limiteAuthSettings.RefreshPorMinuto));

    options.OnRejected = async (context, cancellationToken) =>
    {
        var segundos = context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var espera)
            ? (int)Math.Ceiling(espera.TotalSeconds)
            : 60;

        context.HttpContext.Response.Headers.RetryAfter = segundos.ToString();

        await context.HttpContext.Response.WriteAsJsonAsync(
            new ProblemDetails
            {
                Status = StatusCodes.Status429TooManyRequests,
                Title = "Demasiadas solicitudes",
                Detail = $"Hiciste demasiados intentos. Espera {segundos} segundos e inténtalo de nuevo."
            },
            options: (JsonSerializerOptions?)null,
            contentType: "application/problem+json",
            cancellationToken: cancellationToken);
    };
});

var app = builder.Build();

// Esquema de la BD: aplica las migraciones pendientes (la lógica vive en Application/Infrastructure;
// aquí solo se manda por MediatR). Si falla, la API NO arranca: sin tablas no tiene caso seguir.
if (baseDatosSettings.MigrarAlIniciar)
{
    using var scope = app.Services.CreateScope();
    var sender = scope.ServiceProvider.GetRequiredService<MediatR.ISender>();
    await sender.Send(new AplicarMigracionesCommand());
}

// Catálogo de privilegios + rol Administrador (+ usuario admin inicial si se configuró).
// La lógica vive en Application (SembrarSeguridadCommand); aquí solo se manda por MediatR.
if (seguridadSettings.SembrarAlIniciar)
{
    try
    {
        using var scope = app.Services.CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<MediatR.ISender>();
        await sender.Send(new SembrarSeguridadCommand(seguridadSettings.AdminEmail, seguridadSettings.AdminPassword));
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "No se pudo sembrar la seguridad (¿la BD está arriba y con las migraciones aplicadas?).");
    }
}

// Empresas demo A y B (solo Development; ver SembrarDatosDemoCommand)
if (app.Environment.IsDevelopment() && seguridadSettings.SembrarAlIniciar && seguridadSettings.SembrarDemo)
{
    try
    {
        using var scope = app.Services.CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<MediatR.ISender>();
        await sender.Send(new SembrarDatosDemoCommand());
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "No se pudieron sembrar los datos demo.");
    }
}

if (jwtKeyGenerada)
{
    app.Logger.LogWarning(
        "Jwt:Key vacía: se generó una llave temporal para desarrollo. Los access tokens se invalidan al reiniciar. " +
        "Para fijarla, ponla en APIGasoline/appsettings.Local.json.");
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi().AllowAnonymous();
}

app.UseHttpsRedirection();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

// Partición por IP del cliente. Sin límite si LimiteAuth:Habilitado = false (pruebas de carga).
static RateLimitPartition<string> LimitePorIp(HttpContext http, bool habilitado, int porMinuto)
{
    var ip = http.Connection.RemoteIpAddress?.ToString() ?? "desconocida";

    if (!habilitado)
        return RateLimitPartition.GetNoLimiter(ip);

    return RateLimitPartition.GetFixedWindowLimiter(ip, _ => new FixedWindowRateLimiterOptions
    {
        PermitLimit = porMinuto,
        Window = TimeSpan.FromMinutes(1),
        QueueLimit = 0
    });
}
