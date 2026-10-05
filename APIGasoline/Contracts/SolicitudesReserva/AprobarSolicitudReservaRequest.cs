using System.ComponentModel.DataAnnotations;

namespace APIGasoline.Contracts.SolicitudesReserva;

public class AprobarSolicitudReservaRequest
{
    [Range(1, int.MaxValue)]
    public int EmpleadoConfirmadorId { get; set; }

    [StringLength(255)]
    public string? Motivo { get; set; }
}
