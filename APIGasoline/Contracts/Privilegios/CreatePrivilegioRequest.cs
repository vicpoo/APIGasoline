using System.ComponentModel.DataAnnotations;
using APIGasoline.Domain.Enums;

namespace APIGasoline.Contracts.Privilegios;

public class CreatePrivilegioRequest
{
    /// <summary>Código técnico inmutable con formato modulo.accion (ej. carro.crear, viaje.ver_todos).</summary>
    [Required]
    [StringLength(100)]
    [RegularExpression(@"^\s*[a-zA-Z][a-zA-Z0-9_]*(\.[a-zA-Z][a-zA-Z0-9_]*)+\s*$",
        ErrorMessage = "La clave debe tener el formato modulo.accion (ej. carro.crear).")]
    public string Clave { get; set; } = string.Empty;

    /// <summary>Opcional. Si no se manda, se toma el primer segmento de la clave.</summary>
    [StringLength(50)]
    public string? Modulo { get; set; }

    /// <summary>Texto visible; se puede cambiar después sin afectar permisos.</summary>
    [Required]
    [StringLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(255)]
    public string? Descripcion { get; set; }

    /// <summary>plataforma = solo para roles globales; empresa = para cualquier rol.</summary>
    public AmbitoPrivilegio Ambito { get; set; } = AmbitoPrivilegio.Empresa;

    public bool Activo { get; set; } = true;
}
