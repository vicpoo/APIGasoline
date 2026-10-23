using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text;
using APIGasoline.Application;
using APIGasoline.Application.Common.Interfaces;
using APIGasoline.Application.Common.Outbox;
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
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

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

builder.Services.AddInfrastructure(connectionString, jwtSettings, minioSettings, outboxSettings, pushSettings);

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

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi().AllowAnonymous();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
