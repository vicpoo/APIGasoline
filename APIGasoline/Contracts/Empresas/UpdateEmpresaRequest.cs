using System.ComponentModel.DataAnnotations;

namespace APIGasoline.Contracts.Empresas;

public class UpdateEmpresaRequest
{
    [Required]
    [StringLength(150)]
    public string Nombre { get; set; } = string.Empty;

    [GuidNoVacio]
    public Guid? CorporativoId { get; set; }

    [GuidNoVacio]
    public Guid? DivisionId { get; set; }
}
