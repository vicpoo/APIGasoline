namespace APIGasoline.Application.Features.Empresas.DTOs;

public record EmpresaDto(Guid EmpresaId, Guid? CorporativoId, Guid? DivisionId, string Nombre, DateTime? FechaRegistro);
