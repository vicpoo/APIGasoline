using System.ComponentModel.DataAnnotations;

namespace APIGasoline.Contracts.Empresas;

public class CreateEmpresaRequest
{
    [Required]
    [StringLength(150)]
    public string Nombre { get; set; } = string.Empty;
}
