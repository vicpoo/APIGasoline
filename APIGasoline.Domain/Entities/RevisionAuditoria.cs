using APIGasoline.Domain.Enums;
using APIGasoline.Domain.Exceptions;

namespace APIGasoline.Domain.Entities;

/// <summary>
/// Revisión posterior de un viaje, o de las cargas de un carro en un periodo.
/// NO es una aprobación operativa (eso es SolicitudReserva). Sus cambios quedan en auditoria_evento.
/// </summary>
public class RevisionAuditoria
{
    public const int ObservacionesMaxLength = 1000;

    public Guid RevisionAuditoriaId { get; private set; } = Guid.CreateVersion7();
    public Guid EmpresaId { get; private set; }

    // --- Alcance: un viaje, o un carro en un periodo ---
    public Guid? ViajeId { get; private set; }
    public Guid? CarroId { get; private set; }
    public DateOnly? PeriodoDesde { get; private set; }
    public DateOnly? PeriodoHasta { get; private set; }

    // --- Resultado ---
    public Guid EmpleadoRevisorId { get; private set; }

    /// <summary>La asigna la base de datos al insertar (default now()).</summary>
    public DateTime? FechaRevision { get; private set; }

    public EstadoRevision EstadoRevision { get; private set; } = EstadoRevision.PendienteInformacion;
    public string? Observaciones { get; private set; }

    /// <summary>Cuándo se concluyó. null = sigue abierta.</summary>
    public DateTime? FechaCierre { get; private set; }

    // Requerido por Entity Framework
    private RevisionAuditoria()
    {
    }

    public bool EstaCerrada => FechaCierre is not null;

    public static RevisionAuditoria DeViaje(Guid empresaId, Guid viajeId, Guid empleadoRevisorId, string? observaciones)
    {
        if (viajeId == Guid.Empty)
            throw new DomainException("El id del viaje es obligatorio.");

        var revision = Nueva(empresaId, empleadoRevisorId, observaciones);
        revision.ViajeId = viajeId;
        return revision;
    }

    public static RevisionAuditoria DeCarroEnPeriodo(
        Guid empresaId,
        Guid carroId,
        DateOnly periodoDesde,
        DateOnly periodoHasta,
        Guid empleadoRevisorId,
        string? observaciones)
    {
        if (carroId == Guid.Empty)
            throw new DomainException("El id del carro es obligatorio.");

        if (periodoHasta < periodoDesde)
            throw new DomainException("El fin del periodo no puede ser anterior al inicio.");

        var revision = Nueva(empresaId, empleadoRevisorId, observaciones);
        revision.CarroId = carroId;
        revision.PeriodoDesde = periodoDesde;
        revision.PeriodoHasta = periodoHasta;
        return revision;
    }

    /// <summary>Actualiza el resultado de una revisión abierta.</summary>
    public void Registrar(EstadoRevision estado, string? observaciones)
    {
        ValidarAbierta();

        if (!Enum.IsDefined(estado))
            throw new DomainException("El estado de la revisión no es válido.");

        var observacionesNormalizadas = NormalizarObservaciones(observaciones);

        if (estado == EstadoRevision.ConObservaciones && observacionesNormalizadas is null)
            throw new DomainException("Las observaciones son obligatorias cuando la revisión tiene observaciones.");

        EstadoRevision = estado;
        Observaciones = observacionesNormalizadas;
    }

    /// <summary>Concluye la revisión. No se puede cerrar si sigue pendiente de información.</summary>
    public void Cerrar(DateTime fecha)
    {
        ValidarAbierta();

        if (EstadoRevision == EstadoRevision.PendienteInformacion)
            throw new DomainException("No se puede cerrar una revisión pendiente de información. Indica si quedó revisada o con observaciones.");

        if (fecha == default)
            throw new DomainException("La fecha de cierre es obligatoria.");

        FechaCierre = fecha;
    }

    private static RevisionAuditoria Nueva(Guid empresaId, Guid empleadoRevisorId, string? observaciones)
    {
        if (empresaId == Guid.Empty)
            throw new DomainException("El id de la empresa es obligatorio.");

        if (empleadoRevisorId == Guid.Empty)
            throw new DomainException("El id del empleado revisor es obligatorio.");

        return new RevisionAuditoria
        {
            EmpresaId = empresaId,
            EmpleadoRevisorId = empleadoRevisorId,
            EstadoRevision = EstadoRevision.PendienteInformacion,
            Observaciones = NormalizarObservaciones(observaciones)
        };
    }

    private void ValidarAbierta()
    {
        if (EstaCerrada)
            throw new DomainException("La revisión ya está cerrada.");
    }

    private static string? NormalizarObservaciones(string? observaciones)
    {
        var normalizadas = string.IsNullOrWhiteSpace(observaciones) ? null : observaciones.Trim();

        if (normalizadas is not null && normalizadas.Length > ObservacionesMaxLength)
            throw new DomainException($"Las observaciones no pueden exceder {ObservacionesMaxLength} caracteres.");

        return normalizadas;
    }
}
