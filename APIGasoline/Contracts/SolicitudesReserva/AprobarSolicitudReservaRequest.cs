using System.ComponentModel.DataAnnotations;

namespace APIGasoline.Contracts.SolicitudesReserva;

public class AprobarSolicitudReservaRequest
{
    [GuidNoVacio]
    public Guid EmpleadoConfirmadorId { get; set; }

    [StringLength(255)]
    public string? Motivo { get; set; }
}
