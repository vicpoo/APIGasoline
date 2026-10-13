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
    /// <summary>Clasificación del cambio (falla_mecanica y choque_o_golpe = percances).</summary>
    TipoMotivoCambio? TipoMotivo,
    string? Motivo,
    /// <summary>Llave de la foto de evidencia (lo que se guarda en BD).</summary>
    string? FotoEvidencia,
    /// <summary>URL temporal de la foto (la llena el handler, no AutoMapper).</summary>
    string? FotoEvidenciaUrl,
    DateTime? FechaCambio,
    DateTime? FechaRecepcion);
