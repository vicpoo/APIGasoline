namespace APIGasoline.Application.Common.Interfaces;

/// <summary>
/// Abstrae la aplicación del esquema de la base de datos (migraciones del ORM).
/// La implementación vive en Infrastructure (Entity Framework); Application no conoce EF.
/// </summary>
public interface IMigradorBaseDatos
{
    /// <summary>Nombres de las migraciones que todavía no se aplican en la base de datos.</summary>
    Task<IReadOnlyList<string>> ObtenerPendientesAsync(CancellationToken cancellationToken = default);

    /// <summary>Crea la base de datos si no existe y aplica las migraciones pendientes.</summary>
    Task AplicarPendientesAsync(CancellationToken cancellationToken = default);
}
