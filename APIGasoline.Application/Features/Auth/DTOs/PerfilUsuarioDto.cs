using APIGasoline.Application.Features.Empleados.DTOs;
using APIGasoline.Application.Features.Usuarios.DTOs;

namespace APIGasoline.Application.Features.Auth.DTOs;

/// <summary>
/// Datos del usuario autenticado: su cuenta, su empleado (si tiene) y sus permisos.
/// </summary>
public record PerfilUsuarioDto(
    UsuarioDto Usuario,
    EmpleadoDto? Empleado,
    IReadOnlyList<string> Roles,
    IReadOnlyList<string> Privilegios);
