using APIGasoline.Domain.Enums;

namespace APIGasoline.Application.Features.Usuarios.DTOs;

/// <summary>
/// Nunca incluye el hash de la contraseña.
/// </summary>
public record UsuarioDto(Guid UsuarioId, string Email, EstadoUsuario Estado, DateTime? FechaRegistro);
