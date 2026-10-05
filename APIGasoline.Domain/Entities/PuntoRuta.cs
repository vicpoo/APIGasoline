using APIGasoline.Domain.Exceptions;

namespace APIGasoline.Domain.Entities;

/// <summary>
/// Punto GPS capturado durante un viaje para reconstruir la ruta.
/// Es inmutable: una vez registrado no se modifica.
/// </summary>
public class PuntoRuta
{
    public int PuntoRutaId { get; private set; }
    public int EmpresaId { get; private set; }
    public int ViajeId { get; private set; }
    public decimal Latitud { get; private set; }
    public decimal Longitud { get; private set; }
    public DateTime TimestampCaptura { get; private set; }

    /// <summary>
    /// true cuando el punto se capturó sin conexión y se subió después.
    /// </summary>
    public bool Sincronizado { get; private set; }

    // Requerido por Entity Framework
    private PuntoRuta()
    {
    }

    public static PuntoRuta Crear(int empresaId, int viajeId, decimal latitud, decimal longitud, DateTime timestampCaptura, bool sincronizado)
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

        return new PuntoRuta
        {
            EmpresaId = empresaId,
            ViajeId = viajeId,
            // decimal(9,6): ~11 cm de precisión
            Latitud = Math.Round(latitud, 6),
            Longitud = Math.Round(longitud, 6),
            TimestampCaptura = TruncarASegundos(timestampCaptura),
            Sincronizado = sincronizado
        };
    }

    /// <summary>
    /// La columna timestamp de MySQL guarda segundos; se trunca para que
    /// la detección de puntos duplicados compare el mismo valor que está en la BD.
    /// </summary>
    public static DateTime TruncarASegundos(DateTime valor)
        => new(valor.Ticks - (valor.Ticks % TimeSpan.TicksPerSecond), valor.Kind);
}
