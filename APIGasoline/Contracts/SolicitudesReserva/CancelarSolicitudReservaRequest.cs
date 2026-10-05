using System.ComponentModel.DataAnnotations;

namespace APIGasoline.Contracts.SolicitudesReserva;

public class CancelarSolicitudReservaRequest
{
    [StringLength(255)]
    public string? Motivo { get; set; }
}
