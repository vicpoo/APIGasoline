using System.ComponentModel.DataAnnotations;

namespace APIGasoline.Contracts.Carros;

public class DarDeBajaCarroRequest
{
    /// <summary>Si no viene, se usa la hora del servidor.</summary>
    public DateTime? FechaBaja { get; set; }

    [Required]
    [StringLength(255)]
    public string? Motivo { get; set; }
}
