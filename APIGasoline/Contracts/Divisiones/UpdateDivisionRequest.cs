using System.ComponentModel.DataAnnotations;

namespace APIGasoline.Contracts.Divisiones;

public class UpdateDivisionRequest
{
    [Required]
    [StringLength(100)]
    public string Nombre { get; set; } = string.Empty;

    public bool Activo { get; set; } = true;
}
