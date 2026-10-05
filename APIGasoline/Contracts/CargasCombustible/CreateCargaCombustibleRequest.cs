using System.ComponentModel.DataAnnotations;

namespace APIGasoline.Contracts.CargasCombustible;

public class CreateCargaCombustibleRequest
{
    [Range(1, int.MaxValue)]
    public int ViajeId { get; set; }

    [Required]
    public decimal? Litros { get; set; }

    public decimal? Monto { get; set; }

    [Required]
    [StringLength(255)]
    public string FotoComprobante { get; set; } = string.Empty;

    [Required]
    public DateTime? FechaCarga { get; set; }

    public decimal? UbicacionLat { get; set; }
    public decimal? UbicacionLng { get; set; }

    /// <summary>true si la carga se registró sin conexión.</summary>
    public bool Sincronizado { get; set; }
}
