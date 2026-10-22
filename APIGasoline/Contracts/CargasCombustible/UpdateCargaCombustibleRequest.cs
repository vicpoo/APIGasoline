using System.ComponentModel.DataAnnotations;

namespace APIGasoline.Contracts.CargasCombustible;

/// <summary>
/// Corrección de una carga. Se envía como multipart/form-data.
/// "fotoComprobante" es opcional: si no se manda, se conserva el actual. "motivo" es obligatorio.
/// Requiere token: el operador corrige mientras el viaje está en curso; después, un Confirmador o Gestor de activos.
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

    /// <summary>Centro de costos del gasto (opcional, de la misma empresa).</summary>
    [GuidNoVacio]
    public Guid? CentroCostoId { get; set; }

    /// <summary>Por qué se corrige (queda en el historial de auditoría).</summary>
    [Required]
    [StringLength(500)]
    public string? Motivo { get; set; }
}
