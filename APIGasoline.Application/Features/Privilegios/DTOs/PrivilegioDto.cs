namespace APIGasoline.Application.Features.Privilegios.DTOs;

public record PrivilegioDto(Guid PrivilegioId, string Nombre, string? Descripcion, bool Activo);
