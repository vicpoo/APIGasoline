using APIGasoline.Application.Features.Empleados.DTOs;
using APIGasoline.Application.Features.Usuarios.DTOs;

namespace APIGasoline.Application.Features.Auth.DTOs;

/// <summary>
/// Datos del usuario autenticado: su cuenta, su empleado (si tiene) y sus permisos.
/// Privilegios = claves (ej. carro.crear). El front decide qué mostrar con ellas, nunca con los roles.
/// VersionPrivilegios = hash de las claves; la app lo guarda para saber cuándo volver a descargarlas.
/// </summary>
public record PerfilUsuarioDto(
    UsuarioDto Usuario,
    EmpleadoDto? Empleado,
    IReadOnlyList<string> Roles,
    IReadOnlyList<string> Privilegios,
    string VersionPrivilegios);
