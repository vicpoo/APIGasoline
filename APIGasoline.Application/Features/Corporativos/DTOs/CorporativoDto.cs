namespace APIGasoline.Application.Features.Corporativos.DTOs;

public record CorporativoDto(Guid CorporativoId, Guid EmpresaId, string Nombre, bool Activo, DateTime? FechaRegistro);
