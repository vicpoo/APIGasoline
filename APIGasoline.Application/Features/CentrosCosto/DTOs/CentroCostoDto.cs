namespace APIGasoline.Application.Features.CentrosCosto.DTOs;

public record CentroCostoDto(int CentroCostoId, int EmpresaId, string Nombre, string Clave, bool Activo, DateTime? FechaRegistro);
