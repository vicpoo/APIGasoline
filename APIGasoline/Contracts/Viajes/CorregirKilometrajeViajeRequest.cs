using System.ComponentModel.DataAnnotations;

namespace APIGasoline.Contracts.Viajes;

/// <summary>
/// Corrección del kilometraje. Requiere token: el operador mientras el viaje está en curso;
/// con el viaje cerrado, un Confirmador o Gestor de activos.
/// </summary>
public class CorregirKilometrajeViajeRequest
{
    [Required]
    public decimal? KilometrajeInicial { get; set; }

    /// <summary>Solo para viajes cerrados. Si el viaje ya tiene kilometraje final, es obligatorio.</summary>
    public decimal? KilometrajeFinal { get; set; }

    /// <summary>Por qué se corrige (queda en el historial de auditoría).</summary>
    [Required]
    [StringLength(500)]
    public string? Motivo { get; set; }
}
