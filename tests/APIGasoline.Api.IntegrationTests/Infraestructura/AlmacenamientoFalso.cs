using System.Collections.Concurrent;
using APIGasoline.Application.Common.Interfaces;
using APIGasoline.Application.Common.Models;

namespace APIGasoline.Api.IntegrationTests.Infraestructura;

/// <summary>
/// Reemplaza a MinIO en las pruebas: guarda las llaves en memoria y devuelve URLs fijas
/// (así los DTO con fotos son iguales en cada consulta y se pueden comparar).
/// </summary>
public sealed class AlmacenamientoFalso : IFileStorageService
{
    private readonly ConcurrentDictionary<string, byte> _llaves = new();

    public Task SubirAsync(ArchivoEntrada archivo, string llave, CancellationToken cancellationToken = default)
    {
        _llaves[llave] = 0;
        return Task.CompletedTask;
    }

    public Task EliminarAsync(string llave, CancellationToken cancellationToken = default)
    {
        _llaves.TryRemove(llave, out _);
        return Task.CompletedTask;
    }

    public Task<bool> IntentarEliminarAsync(string? llave)
        => Task.FromResult(llave is not null && _llaves.TryRemove(llave, out _));

    public Task<string?> ObtenerUrlAsync(string? llave, CancellationToken cancellationToken = default)
        => Task.FromResult(llave is null ? null : $"https://almacenamiento.falso/{llave}");
}
