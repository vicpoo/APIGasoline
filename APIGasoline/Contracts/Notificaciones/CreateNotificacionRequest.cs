using System.ComponentModel.DataAnnotations;
using APIGasoline.Domain.Enums;

namespace APIGasoline.Contracts.Notificaciones;

public class CreateNotificacionRequest
{
    [GuidNoVacio]
    public Guid EmpleadoDestinatarioId { get; set; }

    [Required]
    public TipoNotificacion? Tipo { get; set; }

    [GuidNoVacio]
    public Guid? ReferenciaId { get; set; }

    [StringLength(255)]
    public string? Mensaje { get; set; }
}
