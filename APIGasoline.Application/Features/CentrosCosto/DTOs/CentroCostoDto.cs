namespace APIGasoline.Application.Features.CentrosCosto.DTOs;

public record CentroCostoDto(
    Guid CentroCostoId,
    Guid EmpresaId,
    Guid? CorporativoId,
    Guid? DivisionId,
    string Nombre,
    string Clave,
    bool Activo,
    DateTime? FechaRegistro);
