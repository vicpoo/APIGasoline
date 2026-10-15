namespace APIGasoline.Application.Features.Auth.DTOs;

/// <summary>
/// Privilegios efectivos del usuario autenticado (unión de los de todos sus roles activos).
/// Version = SHA-256 de las claves ordenadas: si no cambió, la app conserva su copia local.
/// </summary>
public record PrivilegiosUsuarioDto(IReadOnlyList<string> Claves, string Version);
