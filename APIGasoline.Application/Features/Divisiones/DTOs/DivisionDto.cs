namespace APIGasoline.Application.Features.Divisiones.DTOs;

public record DivisionDto(int DivisionId, int CorporativoId, string Nombre, bool Activo, DateTime? FechaRegistro);
