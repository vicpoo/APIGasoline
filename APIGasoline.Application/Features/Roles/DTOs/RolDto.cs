namespace APIGasoline.Application.Features.Roles.DTOs;

/// <summary>EmpresaId null = rol global.</summary>
public record RolDto(Guid RolId, Guid? EmpresaId, string Nombre, string? Descripcion, bool Activo);
