using System.ComponentModel.DataAnnotations;

namespace APIGasoline.Contracts.CargasCombustible;

/// <summary>
/// Se envía como multipart/form-data: los datos de la carga + el archivo "fotoComprobante".
/// </summary>
public class CreateCargaCombustibleRequest
{
    /// <summary>
    /// UUID de la carga generado en el teléfono (offline-first, opcional). Si se manda, reintentar con el
    /// mismo id regresa la carga ya creada sin duplicarla ni volver a subir el ticket.
    /// </summary>
    [GuidNoVacio]
    public Guid? CargaCombustibleId { get; set; }

    [GuidNoVacio]
    public Guid ViajeId { get; set; }

    [Required]
    public decimal? Litros { get; set; }

    public decimal? Monto { get; set; }

    /// <summary>Foto del ticket (jpg, png o webp, máx. 5 MB).</summary>
    [Required]
    public IFormFile? FotoComprobante { get; set; }

    [Required]
    public DateTime? FechaCarga { get; set; }

    public decimal? UbicacionLat { get; set; }
    public decimal? UbicacionLng { get; set; }

    /// <summary>Centro de costos del gasto (opcional, de la misma empresa).</summary>
    [GuidNoVacio]
    public Guid? CentroCostoId { get; set; }
}
