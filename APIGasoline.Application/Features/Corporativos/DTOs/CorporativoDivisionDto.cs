namespace APIGasoline.Application.Features.Corporativos.DTOs;

/// <summary>Liga entre un corporativo y una división de la misma empresa.</summary>
public record CorporativoDivisionDto(Guid CorporativoDivisionId, Guid EmpresaId, Guid CorporativoId, Guid DivisionId, DateTime? FechaRegistro);
