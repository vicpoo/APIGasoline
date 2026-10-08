using APIGasoline.Domain.Exceptions;
using APIGasoline.Domain.Rutas;

namespace APIGasoline.Domain.Entities;

/// <summary>
/// Punto GPS CRUDO capturado durante un viaje. Es la fuente de verdad de la ruta:
/// una vez registrado no se modifica. El resumen calculado se guarda en RutaProcesada.
/// </summary>
public class PuntoRuta
{
    public const decimal PrecisionMaxima = 9999.99m;   // decimal(6,2)
    public const decimal AltitudMinima = -500m;        // Mar Muerto ≈ -430 m
    public const decimal AltitudMaxima = 9000m;        // Everest ≈ 8849 m
    public const decimal VelocidadMaxima = 9999.99m;   // decimal(6,2), en m/s

    public int PuntoRutaId { get; private set; }
    public int EmpresaId { get; private set; }
    public int ViajeId { get; private set; }
    public decimal Latitud { get; private set; }
    public decimal Longitud { get; private set; }

    /// <summary>Radio de error reportado por el GPS (metros). Opcional.</summary>
    public decimal? PrecisionMetros { get; private set; }

    /// <summary>Metros sobre el nivel del mar reportados por el GPS (sin corregir). Opcional.</summary>
    public decimal? Altitud { get; private set; }

    /// <summary>Velocidad instantánea reportada por el GPS, en m/s. Opcional.</summary>
    public decimal? Velocidad { get; private set; }

    public DateTime TimestampCaptura { get; private set; }

    /// <summary>
    /// true cuando el punto se capturó sin conexión y se subió después.
    /// </summary>
    public bool Sincronizado { get; private set; }

    // Requerido por Entity Framework
    private PuntoRuta()
    {
    }

    /// <remarks>
    /// precisión, altitud y velocidad son opcionales para que la app anterior
    /// (que solo manda latitud, longitud y hora) siga funcionando.
    /// </remarks>
    public static PuntoRuta Crear(
        int empresaId,
        int viajeId,
        decimal latitud,
        decimal longitud,
        DateTime timestampCaptura,
        bool sincronizado,
        decimal? precisionMetros = null,
        decimal? altitud = null,
        decimal? velocidad = null)
    {
        if (empresaId <= 0)
            throw new DomainException("El id de la empresa debe ser mayor a cero.");

        if (viajeId <= 0)
            throw new DomainException("El id del viaje debe ser mayor a cero.");

        if (latitud is < -90m or > 90m)
            throw new DomainException($"La latitud {latitud} debe estar entre -90 y 90.");

        if (longitud is < -180m or > 180m)
            throw new DomainException($"La longitud {longitud} debe estar entre -180 y 180.");

        if (timestampCaptura == default)
            throw new DomainException("El timestamp de captura es obligatorio.");

        if (precisionMetros is < 0m or > PrecisionMaxima)
            throw new DomainException($"La precisión debe estar entre 0 y {PrecisionMaxima} metros.");

        if (altitud is < AltitudMinima or > AltitudMaxima)
            throw new DomainException($"La altitud debe estar entre {AltitudMinima} y {AltitudMaxima} metros.");

        if (velocidad is < 0m or > VelocidadMaxima)
            throw new DomainException($"La velocidad debe estar entre 0 y {VelocidadMaxima} m/s.");

        return new PuntoRuta
        {
            EmpresaId = empresaId,
            ViajeId = viajeId,
            // decimal(9,6): ~11 cm de precisión
            Latitud = Math.Round(latitud, 6),
            Longitud = Math.Round(longitud, 6),
            // decimal(6,2) / decimal(7,2)
            PrecisionMetros = precisionMetros is null ? null : Math.Round(precisionMetros.Value, 2),
            Altitud = altitud is null ? null : Math.Round(altitud.Value, 2),
            Velocidad = velocidad is null ? null : Math.Round(velocidad.Value, 2),
            TimestampCaptura = TruncarASegundos(timestampCaptura),
            Sincronizado = sincronizado
        };
    }

    /// <summary>
    /// Convierte el punto al formato que usan los algoritmos de Domain/Rutas.
    /// </summary>
    public PuntoGps ToPuntoGps()
        => new(
            (double)Latitud,
            (double)Longitud,
            TimestampCaptura,
            PrecisionMetros is null ? null : (double)PrecisionMetros.Value,
            Velocidad is null ? null : (double)Velocidad.Value);

    /// <summary>
    /// La columna timestamp de MySQL guarda segundos; se trunca para que
    /// la detección de puntos duplicados compare el mismo valor que está en la BD.
    /// </summary>
    public static DateTime TruncarASegundos(DateTime valor)
        => new(valor.Ticks - (valor.Ticks % TimeSpan.TicksPerSecond), valor.Kind);
}
