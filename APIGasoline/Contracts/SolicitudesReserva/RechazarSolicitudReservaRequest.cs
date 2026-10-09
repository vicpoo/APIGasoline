using System.ComponentModel.DataAnnotations;

namespace APIGasoline.Contracts.SolicitudesReserva;

public class RechazarSolicitudReservaRequest
{
    [GuidNoVacio]
    public Guid EmpleadoConfirmadorId { get; set; }

    [Required]
    [StringLength(255)]
    public string Motivo { get; set; } = string.Empty;
}
