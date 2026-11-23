using System.ComponentModel.DataAnnotations;
using APIGasoline.Domain.Enums;

namespace APIGasoline.Contracts.Carros;

public class UpdateCarroRequest
{
    [GuidNoVacio]
    public Guid EmpresaId { get; set; }

    [Required]
    [StringLength(80)]
    public string Marca { get; set; } = string.Empty;

    [Required]
    [StringLength(80)]
    public string Modelo { get; set; } = string.Empty;

    public int? Anio { get; set; }

    [StringLength(50)]
    public string? Color { get; set; }

    [Required]
    [StringLength(20)]
    public string Placas { get; set; } = string.Empty;

    public decimal? CapacidadTanque { get; set; }

    public decimal KilometrajeActual { get; set; }

    /// <summary>
    /// "carro", "moto", "camioneta" o "camion". Opcional: si no se manda se conserva el actual
    /// (así las versiones viejas de la app no lo regresan a "carro").
    /// </summary>
    public TipoVehiculo? TipoVehiculo { get; set; }

    /// <summary>
    /// Cuándo se hizo el cambio en el celular (hora con zona, ej. "2026-11-23T10:00:00-06:00"). Opcional.
    /// Si otro celular ya guardó un cambio más reciente del carro, este no se aplica (409 CAMBIO_MAS_RECIENTE).
    /// Sin ella se usa la hora del servidor.
    /// </summary>
    public DateTime? FechaEdicion { get; set; }
}
