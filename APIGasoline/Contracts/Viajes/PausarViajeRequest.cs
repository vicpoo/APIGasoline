using System.ComponentModel.DataAnnotations;

namespace APIGasoline.Contracts.Viajes;

public class PausarViajeRequest
{
    /// <summary>Hora en que empezó la pausa. Si no se manda, se usa la hora actual del servidor.</summary>
    public DateTime? Fecha { get; set; }

    /// <summary>Opcional: comida, carga de gasolina, espera...</summary>
    [StringLength(255)]
    public string? Motivo { get; set; }

    public decimal? UbicacionLat { get; set; }
    public decimal? UbicacionLng { get; set; }
}
