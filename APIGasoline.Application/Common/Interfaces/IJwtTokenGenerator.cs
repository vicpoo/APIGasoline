namespace APIGasoline.Application.Common.Interfaces;

/// <summary>
/// Datos que viajan como claims dentro del token.
/// SesionId = sesión (refresh token) que emitió este JWT; va en el claim "sid".
/// </summary>
public record TokenUsuario(Guid UsuarioId, string Email, Guid? EmpleadoId, Guid? EmpresaId, IReadOnlyList<string> Roles, Guid? SesionId = null);

public record TokenGenerado(string Token, DateTime ExpiraEnUtc);

/// <summary>
/// Genera el JWT de acceso. La implementación vive en Infrastructure.
/// </summary>
public interface IJwtTokenGenerator
{
    TokenGenerado Generar(TokenUsuario usuario);
}
