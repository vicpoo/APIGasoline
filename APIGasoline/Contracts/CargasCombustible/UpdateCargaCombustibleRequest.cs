using System.ComponentModel.DataAnnotations;

namespace APIGasoline.Contracts.CargasCombustible;

public class UpdateCargaCombustibleRequest
{
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
}
