namespace APIGasoline.Application.Features.CentrosCosto.DTOs;

public record CentroCostoDto(Guid CentroCostoId, Guid EmpresaId, string Nombre, string Clave, bool Activo, DateTime? FechaRegistro);
