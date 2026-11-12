using APIGasoline.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace APIGasoline.Infrastructure.Persistence;

/// <summary>
/// Implementación con Entity Framework de <see cref="IMigradorBaseDatos"/>:
/// usa las migraciones de la carpeta Migrations (historial en la tabla __EFMigrationsHistory).
///
/// F013: si hay una cadena de conexión de migraciones, las migraciones corren con ESE login
/// (puede cambiar el esquema de la base) y la API sigue operando con el suyo, que solo lee y
/// escribe datos. Sin ella (desarrollo, pruebas) se usa la conexión normal.
/// </summary>
public class MigradorBaseDatos : IMigradorBaseDatos
{
    private readonly AppDbContext _context;
    private readonly string? _connectionStringMigraciones;

    public MigradorBaseDatos(AppDbContext context, string? connectionStringMigraciones = null)
    {
        _context = context;
        _connectionStringMigraciones = string.IsNullOrWhiteSpace(connectionStringMigraciones) ? null : connectionStringMigraciones;
    }

    public async Task<IReadOnlyList<string>> ObtenerPendientesAsync(CancellationToken cancellationToken = default)
    {
        await using var contexto = CrearContextoDeMigracion();
        return (await (contexto ?? _context).Database.GetPendingMigrationsAsync(cancellationToken)).ToList();
    }

    public async Task AplicarPendientesAsync(CancellationToken cancellationToken = default)
    {
        await using var contexto = CrearContextoDeMigracion();
        await (contexto ?? _context).Database.MigrateAsync(cancellationToken);
    }

    /// <summary>
    /// Contexto aparte, con el login de migraciones y sin interceptores (no hay usuario: el filtro por
    /// empresa no aplica, y las migraciones que tocan tablas con Row-Level Security fijan su propio
    /// SESSION_CONTEXT). null = usar el contexto normal.
    /// </summary>
    private AppDbContext? CrearContextoDeMigracion()
        => _connectionStringMigraciones is null
            ? null
            : new AppDbContext(new DbContextOptionsBuilder<AppDbContext>().UseSqlServer(_connectionStringMigraciones).Options);
}
