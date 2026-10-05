using System.ComponentModel.DataAnnotations;

namespace APIGasoline.Contracts.Viajes;

public class InterrumpirViajeRequest
{
    [Required]
    [StringLength(255)]
    public string Motivo { get; set; } = string.Empty;

    /// <summary>Si no se manda, se usa la hora actual del servidor.</summary>
    public DateTime? FechaFin { get; set; }

    public decimal? UbicacionFinLat { get; set; }
    public decimal? UbicacionFinLng { get; set; }
    public decimal? KilometrajeFinal { get; set; }
}
