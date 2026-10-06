using System.ComponentModel.DataAnnotations;

namespace APIGasoline.Contracts.CargasCombustible;

/// <summary>
/// Se envía como multipart/form-data. "fotoComprobante" es opcional: si no se manda, se conserva el actual.
/// </summary>
public class UpdateCargaCombustibleRequest
{
    [Required]
    public decimal? Litros { get; set; }

    public decimal? Monto { get; set; }

    /// <summary>Nuevo comprobante (opcional).</summary>
    public IFormFile? FotoComprobante { get; set; }

    [Required]
    public DateTime? FechaCarga { get; set; }

    public decimal? UbicacionLat { get; set; }
    public decimal? UbicacionLng { get; set; }
}
