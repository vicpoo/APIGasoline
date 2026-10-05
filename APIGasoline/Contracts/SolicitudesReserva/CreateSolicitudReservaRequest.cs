using System.ComponentModel.DataAnnotations;

namespace APIGasoline.Contracts.SolicitudesReserva;

public class CreateSolicitudReservaRequest
{
    [Range(1, int.MaxValue)]
    public int EmpresaId { get; set; }

    [Range(1, int.MaxValue)]
    public int CarroId { get; set; }

    [Range(1, int.MaxValue)]
    public int EmpleadoSolicitanteId { get; set; }
}
