using APIGasoline.Domain.Enums;

namespace APIGasoline.Application.Features.CambiosEstatusCarro.DTOs;

public record CambioEstatusCarroDto(
    Guid CambioEstatusCarroId,
    Guid EmpresaId,
    Guid CarroId,
    Guid EmpleadoId,
    Guid? ViajeAfectadoId,
    EstadoCarro EstatusAnterior,
    EstadoCarro EstatusNuevo,
    string? Motivo,
    DateTime? FechaCambio,
    bool Sincronizado);
