using APIGasoline.Domain.Enums;

namespace APIGasoline.Application.Features.Auditoria.Common;

/// <summary>
/// Lecturas para la pantalla "Revisión de combustible". No duplica información:
/// lee viajes, cargas, carros y rutas procesadas tal como están.
/// </summary>
public interface IConsultasRevisionCombustible
{
    /// <summary>Viajes que iniciaron en el periodo (con el resultado GPS contra odómetro, si existe).</summary>
    Task<IReadOnlyList<ViajeRevision>> GetViajesAsync(Guid? empresaId, Guid? carroId, DateTime desde, DateTime hasta, CancellationToken cancellationToken = default);

    /// <summary>Cargas de los viajes indicados.</summary>
    Task<IReadOnlyList<CargaRevision>> GetCargasAsync(IReadOnlyCollection<Guid> viajeIds, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CarroRevision>> GetCarrosAsync(IReadOnlyCollection<Guid> carroIds, CancellationToken cancellationToken = default);

    /// <summary>Último kilometraje final de cada carro antes de <paramref name="antesDe"/> (para revisar la continuidad).</summary>
    Task<IReadOnlyDictionary<Guid, decimal>> GetUltimoKilometrajePorCarroAsync(IReadOnlyCollection<Guid> carroIds, DateTime antesDe, CancellationToken cancellationToken = default);
}

public record ViajeRevision(
    Guid ViajeId,
    Guid EmpresaId,
    Guid CarroId,
    Guid EmpleadoOperadorId,
    DateTime FechaInicio,
    DateTime? FechaFin,
    decimal KilometrajeInicial,
    decimal? KilometrajeFinal,
    EstadoViaje Estado,
    string? MotivoInterrupcion,
    bool Corregido,
    bool? AlertaKilometraje,
    decimal? DiferenciaKm,
    decimal? DiferenciaPorcentaje);

public record CargaRevision(
    Guid CargaCombustibleId,
    Guid ViajeId,
    decimal Litros,
    decimal? Monto,
    DateTime FechaCarga,
    DateTime? FechaSubidaComprobante,
    bool Corregido);

public record CarroRevision(Guid CarroId, string Placas, decimal? CapacidadTanque);
