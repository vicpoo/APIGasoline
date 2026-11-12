namespace APIGasoline.Application.Features.Empresas.DTOs;

/// <summary>La empresa es la raíz: sus corporativos, divisiones y centros de costo apuntan a ella.</summary>
public record EmpresaDto(Guid EmpresaId, string Nombre, bool Activo, DateTime? FechaRegistro);
