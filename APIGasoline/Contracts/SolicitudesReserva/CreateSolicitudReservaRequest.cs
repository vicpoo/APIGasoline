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

    /// <summary>v8. Para qué necesita el carro (se ve en el detalle de la solicitud).</summary>
    [Required(AllowEmptyStrings = false)]
    [StringLength(255)]
    public string Motivo { get; set; } = string.Empty;
}
