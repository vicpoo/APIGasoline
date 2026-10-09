using APIGasoline.Domain.Enums;

namespace APIGasoline.Application.Features.Dispositivos.DTOs;

public record DispositivoDto(
    Guid DispositivoEmpleadoId,
    Guid EmpresaId,
    Guid EmpleadoId,
    string TokenFcm,
    PlataformaDispositivo Plataforma,
    bool Activo,
    DateTime FechaRegistro,
    DateTime? FechaUltimoUso);
