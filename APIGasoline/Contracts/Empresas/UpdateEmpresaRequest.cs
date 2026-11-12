using System.ComponentModel.DataAnnotations;

namespace APIGasoline.Contracts.Empresas;

public class UpdateEmpresaRequest
{
    [Required]
    [StringLength(150)]
    public string Nombre { get; set; } = string.Empty;

    /// <summary>HU-ADM-01: empresa activa o inactiva. Si no viene, no cambia.</summary>
    public bool? Activo { get; set; }
}
