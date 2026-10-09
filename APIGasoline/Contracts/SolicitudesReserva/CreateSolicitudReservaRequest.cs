using System.ComponentModel.DataAnnotations;

namespace APIGasoline.Contracts.SolicitudesReserva;

public class CreateSolicitudReservaRequest
{
    [GuidNoVacio]
    public Guid EmpresaId { get; set; }

    [GuidNoVacio]
    public Guid CarroId { get; set; }

    [GuidNoVacio]
    public Guid EmpleadoSolicitanteId { get; set; }
}
