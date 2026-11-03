using System.ComponentModel.DataAnnotations;

namespace APIGasoline.Contracts.Asignaciones;

public class DesvincularAsignacionRequest
{
    /// <summary>Por qué se le quita el carro al empleado (obligatorio; se le envía en la notificación).</summary>
    [Required]
    [StringLength(255, MinimumLength = 1)]
    public string Motivo { get; set; } = string.Empty;
}
