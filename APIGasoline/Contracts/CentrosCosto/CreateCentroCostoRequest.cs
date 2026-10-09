using System.ComponentModel.DataAnnotations;

namespace APIGasoline.Contracts.CentrosCosto;

public class CreateCentroCostoRequest
{
    [GuidNoVacio]
    public Guid EmpresaId { get; set; }

    [Required]
    [StringLength(150)]
    public string Nombre { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Clave { get; set; } = string.Empty;

    public bool Activo { get; set; } = true;
}
