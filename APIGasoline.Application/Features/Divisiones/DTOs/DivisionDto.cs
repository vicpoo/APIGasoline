namespace APIGasoline.Application.Features.Divisiones.DTOs;

public record DivisionDto(Guid DivisionId, Guid CorporativoId, string Nombre, bool Activo, DateTime? FechaRegistro);
