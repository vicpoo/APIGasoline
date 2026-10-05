using System.ComponentModel.DataAnnotations;
using APIGasoline.Domain.Enums;

namespace APIGasoline.Contracts.Carros;

public class UpdateCarroRequest
{
    [Range(1, int.MaxValue)]
    public int EmpresaId { get; set; }

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

    [StringLength(255)]
    public string? FotoVehiculo { get; set; }

    public decimal? CapacidadTanque { get; set; }

    public decimal KilometrajeActual { get; set; }
}
