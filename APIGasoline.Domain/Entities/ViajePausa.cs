using APIGasoline.Domain.Exceptions;

namespace APIGasoline.Domain.Entities;

/// <summary>
/// Pausa de un viaje en curso (comida, parada a cargar gasolina, espera...).
/// Mientras está abierta (<see cref="FechaFin"/> null) el viaje está en pausa: la app apaga el GPS y
/// la API descarta los puntos GPS que caigan dentro de ella.
/// Solo se crea y se cierra desde el viaje (<see cref="Viaje.Pausar"/>, <see cref="Viaje.Reanudar"/>,
/// <see cref="Viaje.RegistrarPausasSincronizadas"/>), así las reglas viven en un solo lugar.
/// </summary>
public class ViajePausa
{
    public const int MotivoMaxLength = 255;

    public Guid ViajePausaId { get; private set; } = Guid.CreateVersion7();
    public Guid EmpresaId { get; private set; }
    public Guid ViajeId { get; private set; }

    public DateTime FechaInicio { get; private set; }

    /// <summary>null = la pausa sigue abierta (el viaje está en pausa).</summary>
    public DateTime? FechaFin { get; private set; }

    public string? Motivo { get; private set; }
    public decimal? UbicacionLat { get; private set; }
    public decimal? UbicacionLng { get; private set; }

    // Requerido por Entity Framework
    private ViajePausa()
    {
    }

    internal static ViajePausa Crear(
        Guid empresaId,
        Guid viajeId,
        DateTime fechaInicio,
        DateTime? fechaFin,
        string? motivo,
        decimal? lat,
        decimal? lng)
    {
        if (fechaInicio == default)
            throw new DomainException("La fecha de inicio de la pausa es obligatoria.");

        if (fechaFin is not null && fechaFin < fechaInicio)
            throw new DomainException("La pausa no puede terminar antes de empezar.");

        var motivoNormalizado = string.IsNullOrWhiteSpace(motivo) ? null : motivo.Trim();

        if (motivoNormalizado is not null && motivoNormalizado.Length > MotivoMaxLength)
            throw new DomainException($"El motivo de la pausa no puede exceder {MotivoMaxLength} caracteres.");

        return new ViajePausa
        {
            EmpresaId = empresaId,
            ViajeId = viajeId,
            FechaInicio = fechaInicio,
            FechaFin = fechaFin,
            Motivo = motivoNormalizado,
            UbicacionLat = lat,
            UbicacionLng = lng
        };
    }

    public bool EstaAbierta => FechaFin is null;

    /// <summary>true si el instante cae dentro de la pausa (para descartar puntos GPS).</summary>
    public bool Contiene(DateTime instante)
        => instante >= FechaInicio && (FechaFin is null || instante <= FechaFin);

    /// <summary>Duración de la pausa; si sigue abierta, se mide hasta <paramref name="ahora"/>.</summary>
    public TimeSpan Duracion(DateTime ahora)
    {
        var fin = FechaFin ?? ahora;
        return fin > FechaInicio ? fin - FechaInicio : TimeSpan.Zero;
    }

    internal void Cerrar(DateTime fechaFin)
    {
        if (!EstaAbierta)
            throw new DomainException("La pausa ya estaba cerrada.");

        if (fechaFin < FechaInicio)
            throw new DomainException("No se puede reanudar antes de la hora en que empezó la pausa.");

        FechaFin = fechaFin;
    }
}

/// <summary>Pausa que la app registró sin conexión y llega con el viaje en /viajes/sincronizar.</summary>
public sealed record DatosPausa(DateTime FechaInicio, DateTime? FechaFin, string? Motivo, decimal? UbicacionLat, decimal? UbicacionLng);
