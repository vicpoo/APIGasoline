using System.ComponentModel.DataAnnotations;

namespace APIGasoline.Contracts.Roles;

public class UpdateRolRequest
{
    [Required]
    [StringLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(255)]
    public string? Descripcion { get; set; }

    public bool Activo { get; set; } = true;
}
