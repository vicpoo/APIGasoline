using System.ComponentModel.DataAnnotations;

namespace APIGasoline.Contracts.Viajes;

public class FinalizarViajeRequest
{
    [Required]
    public DateTime? FechaFin { get; set; }

    public decimal? UbicacionFinLat { get; set; }
    public decimal? UbicacionFinLng { get; set; }

    [Required]
    public decimal? KilometrajeFinal { get; set; }
}
