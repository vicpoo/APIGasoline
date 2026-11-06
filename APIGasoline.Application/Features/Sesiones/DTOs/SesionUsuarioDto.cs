using APIGasoline.Domain.Enums;

namespace APIGasoline.Application.Features.Sesiones.DTOs;

/// <summary>
/// Una sesión abierta (dispositivo con refresh token vigente). Nunca incluye el token.
/// EsActual = es la sesión desde la que se hace la consulta.
/// </summary>
public record SesionUsuarioDto(
    Guid SesionId,
    PlataformaDispositivo? Plataforma,
    string? NombreDispositivo,
    string? Ip,
    string? UserAgent,
    DateTime FechaCreacion,
    DateTime FechaUltimoUso,
    DateTime ExpiraEn,
    bool EsActual);
