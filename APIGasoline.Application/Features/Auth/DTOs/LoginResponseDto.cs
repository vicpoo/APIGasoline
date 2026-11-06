namespace APIGasoline.Application.Features.Auth.DTOs;

/// <summary>
/// Respuesta del login.
/// - Token / ExpiraEnUtc: JWT de acceso (corto, 15 min). Va en "Authorization: Bearer".
/// - RefreshToken / RefreshExpiraEnUtc: sirve SOLO para pedir tokens nuevos en POST /api/auth/refresh.
///   Guardarlo cifrado (Keystore / Keychain). Cada renovación devuelve uno nuevo y el anterior deja de servir.
/// </summary>
public record LoginResponseDto(
    string Token,
    string TipoToken,
    DateTime ExpiraEnUtc,
    string RefreshToken,
    DateTime RefreshExpiraEnUtc,
    Guid SesionId,
    PerfilUsuarioDto Perfil);
