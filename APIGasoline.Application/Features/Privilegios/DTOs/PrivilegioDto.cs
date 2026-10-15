using APIGasoline.Domain.Enums;

namespace APIGasoline.Application.Features.Privilegios.DTOs;

public record PrivilegioDto(
    Guid PrivilegioId,
    string Clave,
    string Modulo,
    string Nombre,
    string? Descripcion,
    AmbitoPrivilegio Ambito,
    bool Activo);
