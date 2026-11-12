using System.ComponentModel.DataAnnotations;

namespace APIGasoline.Contracts.CentrosCosto;

public class UpdateCentroCostoRequest
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

    /// <summary>Opcional: corporativo de la misma empresa.</summary>
    [GuidNoVacio]
    public Guid? CorporativoId { get; set; }

    /// <summary>Opcional: división de la misma empresa (ligada al corporativo si vienen los dos).</summary>
    [GuidNoVacio]
    public Guid? DivisionId { get; set; }
}
