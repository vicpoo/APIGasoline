namespace APIGasoline.Application.Features.Auth.DTOs;

/// <summary>
/// Respuesta de POST /api/auth/refresh: JWT de acceso nuevo + refresh token nuevo (rotación).
/// El refresh token anterior deja de servir en cuanto se emite este.
/// VersionPrivilegios permite a la app saber si debe volver a descargar sus privilegios.
/// </summary>
public record SesionTokensDto(
    string Token,
    string TipoToken,
    DateTime ExpiraEnUtc,
    string RefreshToken,
    DateTime RefreshExpiraEnUtc,
    Guid SesionId,
    string VersionPrivilegios);
