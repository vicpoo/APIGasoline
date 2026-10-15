using System.ComponentModel.DataAnnotations;
using APIGasoline.Domain.Enums;

namespace APIGasoline.Contracts.Privilegios;

/// <summary>La clave no se puede modificar, por eso no viene aquí.</summary>
public class UpdatePrivilegioRequest
{
    /// <summary>Opcional. Si no se manda, se toma el primer segmento de la clave.</summary>
    [StringLength(50)]
    public string? Modulo { get; set; }

    [Required]
    [StringLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(255)]
    public string? Descripcion { get; set; }

    public AmbitoPrivilegio Ambito { get; set; } = AmbitoPrivilegio.Empresa;

    public bool Activo { get; set; } = true;
}
