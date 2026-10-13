using System.ComponentModel.DataAnnotations;
using APIGasoline.Domain.Enums;

namespace APIGasoline.Contracts.Auditoria;

public class RegistrarResultadoRevisionRequest
{
    /// <summary>pendienteInformacion, revisado o conObservaciones.</summary>
    [Required]
    public EstadoRevision? EstadoRevision { get; set; }

    /// <summary>Obligatorias si el resultado es conObservaciones.</summary>
    [StringLength(1000)]
    public string? Observaciones { get; set; }

    /// <summary>true para concluir la revisión.</summary>
    public bool Cerrar { get; set; }
}
