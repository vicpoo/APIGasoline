using System.ComponentModel.DataAnnotations;

namespace APIGasoline.Contracts.Roles;

public class CreateRolRequest
{
    /// <summary>null = rol global; con valor = rol exclusivo de esa empresa.</summary>
    [Range(1, int.MaxValue)]
    public int? EmpresaId { get; set; }

    [Required]
    [StringLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(255)]
    public string? Descripcion { get; set; }

    public bool Activo { get; set; } = true;
}
