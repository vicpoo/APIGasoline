namespace APIGasoline.Infrastructure.Storage;

/// <summary>
/// Host (sin puerto) con el que el cliente llamó a la API en la petición actual
/// (ej. "localhost" desde Postman, "10.0.2.2" desde el emulador de Android, "192.168.1.50" desde un celular).
/// Lo implementa la capa HTTP; fuera de una petición (procesos en segundo plano) devuelve null.
/// </summary>
public interface IHostPeticionActual
{
    string? Host { get; }
}
