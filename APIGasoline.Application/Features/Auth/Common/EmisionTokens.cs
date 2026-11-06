using APIGasoline.Application.Common.Interfaces;
using APIGasoline.Domain.Entities;

namespace APIGasoline.Application.Features.Auth.Common;

/// <summary>Arma el JWT de acceso de una sesión (lo comparten el login y la renovación).</summary>
internal static class EmisionTokens
{
    public static TokenGenerado GenerarAcceso(
        IJwtTokenGenerator jwtTokenGenerator,
        Usuario usuario,
        Empleado? empleado,
        IReadOnlyList<string> roles,
        Guid sesionId)
        => jwtTokenGenerator.Generar(new TokenUsuario(
            usuario.UsuarioId,
            usuario.Email,
            empleado?.EmpleadoId,
            empleado?.EmpresaId,
            roles,
            sesionId));

    /// <summary>La BD guarda hora local del servidor; la app recibe UTC.</summary>
    public static DateTime AUtc(DateTime fechaLocal)
        => fechaLocal.Kind == DateTimeKind.Utc
            ? fechaLocal
            : DateTime.SpecifyKind(fechaLocal, DateTimeKind.Local).ToUniversalTime();
}
