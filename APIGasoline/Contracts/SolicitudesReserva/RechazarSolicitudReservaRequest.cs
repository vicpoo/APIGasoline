using System.ComponentModel.DataAnnotations;

namespace APIGasoline.Contracts.SolicitudesReserva;

public class RechazarSolicitudReservaRequest
{
    [Range(1, int.MaxValue)]
    public int EmpleadoConfirmadorId { get; set; }

    [Required]
    [StringLength(255)]
    public string Motivo { get; set; } = string.Empty;
}
