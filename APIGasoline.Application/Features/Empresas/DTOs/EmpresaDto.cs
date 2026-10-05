namespace APIGasoline.Application.Features.Empresas.DTOs;

public record EmpresaDto(int EmpresaId, int? CorporativoId, int? DivisionId, string Nombre, DateTime? FechaRegistro);
