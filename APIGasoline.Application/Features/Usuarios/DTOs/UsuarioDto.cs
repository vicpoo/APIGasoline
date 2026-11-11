using APIGasoline.Domain.Enums;

namespace APIGasoline.Application.Features.Usuarios.DTOs;

/// <summary>
/// Nunca incluye el hash de la contraseña. EmpresaId null: usuario de la plataforma o aún sin asignar.
/// </summary>
public record UsuarioDto(Guid UsuarioId, string Email, EstadoUsuario Estado, DateTime? FechaRegistro, Guid? EmpresaId);
