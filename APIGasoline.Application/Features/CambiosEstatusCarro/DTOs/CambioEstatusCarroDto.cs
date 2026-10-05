using APIGasoline.Domain.Enums;

namespace APIGasoline.Application.Features.CambiosEstatusCarro.DTOs;

public record CambioEstatusCarroDto(
    int CambioEstatusCarroId,
    int EmpresaId,
    int CarroId,
    int EmpleadoId,
    int? ViajeAfectadoId,
    EstadoCarro EstatusAnterior,
    EstadoCarro EstatusNuevo,
    string? Motivo,
    DateTime? FechaCambio,
    bool Sincronizado);
