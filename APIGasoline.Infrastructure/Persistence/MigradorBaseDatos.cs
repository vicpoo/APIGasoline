using APIGasoline.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace APIGasoline.Infrastructure.Persistence;

/// <summary>
/// Implementación con Entity Framework de <see cref="IMigradorBaseDatos"/>:
/// usa las migraciones de la carpeta Migrations (historial en la tabla __EFMigrationsHistory).
/// </summary>
public class MigradorBaseDatos : IMigradorBaseDatos
{
    private readonly AppDbContext _context;

    public MigradorBaseDatos(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<string>> ObtenerPendientesAsync(CancellationToken cancellationToken = default)
        => (await _context.Database.GetPendingMigrationsAsync(cancellationToken)).ToList();

    public Task AplicarPendientesAsync(CancellationToken cancellationToken = default)
        => _context.Database.MigrateAsync(cancellationToken);
}
