using APIGasoline.Domain.Enums;

namespace APIGasoline.Application.Features.Auditoria.DTOs;

public record RevisionAuditoriaDto(
    Guid RevisionAuditoriaId,
    Guid EmpresaId,
    /// <summary>Alcance: un viaje...</summary>
    Guid? ViajeId,
    /// <summary>...o un carro en un periodo.</summary>
    Guid? CarroId,
    DateOnly? PeriodoDesde,
    DateOnly? PeriodoHasta,
    Guid EmpleadoRevisorId,
    DateTime? FechaRevision,
    EstadoRevision EstadoRevision,
    string? Observaciones,
    DateTime? FechaCierre,
    bool EstaCerrada);
