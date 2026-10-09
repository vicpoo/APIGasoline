namespace APIGasoline.Application.Features.Corporativos.DTOs;

public record CorporativoDto(Guid CorporativoId, string Nombre, bool Activo, DateTime? FechaRegistro);
