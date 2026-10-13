using System.ComponentModel.DataAnnotations;

namespace APIGasoline.Contracts.Auditoria;

/// <summary>
/// Alcance de la revisión: un viaje (viajeId), o un carro en un periodo (carroId + periodoDesde + periodoHasta).
/// El revisor es el empleado del token.
/// </summary>
public class CrearRevisionAuditoriaRequest
{
    [GuidNoVacio]
    public Guid? ViajeId { get; set; }

    [GuidNoVacio]
    public Guid? CarroId { get; set; }

    public DateOnly? PeriodoDesde { get; set; }

    public DateOnly? PeriodoHasta { get; set; }

    [StringLength(1000)]
    public string? Observaciones { get; set; }
}
