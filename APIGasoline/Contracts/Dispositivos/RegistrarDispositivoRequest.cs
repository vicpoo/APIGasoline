using System.ComponentModel.DataAnnotations;
using APIGasoline.Domain.Enums;

namespace APIGasoline.Contracts.Dispositivos;

public class RegistrarDispositivoRequest
{
    [GuidNoVacio]
    public Guid EmpleadoId { get; set; }

    [Required]
    [StringLength(512)]
    public string TokenFcm { get; set; } = string.Empty;

    /// <summary>"android", "ios" o "web".</summary>
    [Required]
    public PlataformaDispositivo? Plataforma { get; set; }
}
