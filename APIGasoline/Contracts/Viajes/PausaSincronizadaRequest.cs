using System.ComponentModel.DataAnnotations;

namespace APIGasoline.Contracts.Viajes;

/// <summary>Una pausa hecha sin conexión, dentro de <see cref="SincronizarViajeRequest.Pausas"/>.</summary>
public class PausaSincronizadaRequest
{
    [Required]
    public DateTime? FechaInicio { get; set; }

    /// <summary>null = la pausa sigue abierta (solo la última y solo si el viaje sigue en curso).</summary>
    public DateTime? FechaFin { get; set; }

    [StringLength(255)]
    public string? Motivo { get; set; }

    public decimal? UbicacionLat { get; set; }
    public decimal? UbicacionLng { get; set; }
}
