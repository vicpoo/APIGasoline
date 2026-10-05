namespace APIGasoline.Application.Features.Roles.DTOs;

/// <summary>EmpresaId null = rol global.</summary>
public record RolDto(int RolId, int? EmpresaId, string Nombre, string? Descripcion, bool Activo);
