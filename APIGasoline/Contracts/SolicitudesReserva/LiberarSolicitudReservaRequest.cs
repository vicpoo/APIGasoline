using System.ComponentModel.DataAnnotations;

namespace APIGasoline.Contracts.SolicitudesReserva;

public class LiberarSolicitudReservaRequest
{
    /// <summary>Hora del celular (puede registrarse offline). Si no viene, se usa la del servidor.</summary>
    public DateTime? FechaLiberacion { get; set; }

    /// <summary>Observaciones opcionales al devolver el carro (HU-USO-04).</summary>
    [StringLength(500)]
    public string? Observaciones { get; set; }
}
