namespace APIGasoline.Application.Features.Corporativos.DTOs;

public record CorporativoDto(int CorporativoId, string Nombre, bool Activo, DateTime? FechaRegistro);
