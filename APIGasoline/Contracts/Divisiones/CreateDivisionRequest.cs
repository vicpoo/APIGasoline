using System.ComponentModel.DataAnnotations;

namespace APIGasoline.Contracts.Divisiones;

public class CreateDivisionRequest
{
    [GuidNoVacio]
    public Guid CorporativoId { get; set; }

    [Required]
    [StringLength(100)]
    public string Nombre { get; set; } = string.Empty;

    public bool Activo { get; set; } = true;
}
