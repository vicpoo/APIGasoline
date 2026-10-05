namespace APIGasoline.Application.Features.Auth.DTOs;

public record LoginResponseDto(
    string Token,
    string TipoToken,
    DateTime ExpiraEnUtc,
    PerfilUsuarioDto Perfil);
