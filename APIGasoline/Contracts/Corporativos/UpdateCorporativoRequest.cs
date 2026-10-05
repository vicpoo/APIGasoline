using System.ComponentModel.DataAnnotations;

namespace APIGasoline.Contracts.Corporativos;

public class UpdateCorporativoRequest
{
    [Required]
    [StringLength(150)]
    public string Nombre { get; set; } = string.Empty;

    public bool Activo { get; set; } = true;
}
