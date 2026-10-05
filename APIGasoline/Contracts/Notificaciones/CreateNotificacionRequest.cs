using System.ComponentModel.DataAnnotations;
using APIGasoline.Domain.Enums;

namespace APIGasoline.Contracts.Notificaciones;

public class CreateNotificacionRequest
{
    [Range(1, int.MaxValue)]
    public int EmpleadoDestinatarioId { get; set; }

    [Required]
    public TipoNotificacion? Tipo { get; set; }

    [Range(1, int.MaxValue)]
    public int? ReferenciaId { get; set; }

    [StringLength(255)]
    public string? Mensaje { get; set; }
}
