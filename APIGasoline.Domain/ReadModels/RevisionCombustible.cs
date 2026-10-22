using APIGasoline.Domain.Enums;

namespace APIGasoline.Domain.ReadModels;

// Modelos de solo lectura (proyecciones) para la pantalla "Revisión de combustible".
// Viven en Domain para que el repositorio (Domain/Interfaces) pueda devolverlos.

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
