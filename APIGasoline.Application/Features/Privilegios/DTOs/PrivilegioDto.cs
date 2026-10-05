namespace APIGasoline.Application.Features.Privilegios.DTOs;

public record PrivilegioDto(int PrivilegioId, string Nombre, string? Descripcion, bool Activo);
