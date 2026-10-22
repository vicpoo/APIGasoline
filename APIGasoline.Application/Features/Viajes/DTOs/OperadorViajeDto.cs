namespace APIGasoline.Application.Features.Viajes.DTOs;

/// <summary>
/// Quién manejó el viaje (para la tarjeta del Historial: foto y nombre).
/// Lo llena el handler con OperadorViaje.ConOperadorAsync, no AutoMapper.
/// </summary>
public record OperadorViajeDto(
    Guid EmpleadoId,
    string NombreCompleto,
    string? FotoPerfilUrl);
