namespace APIGasoline.Application.Features.Divisiones.DTOs;

public record DivisionDto(Guid DivisionId, Guid EmpresaId, string Nombre, bool Activo, DateTime? FechaRegistro);
