using System.ComponentModel.DataAnnotations;
using APIGasoline.Domain.Enums;

namespace APIGasoline.Contracts.Viajes;

/// <summary>
/// Viaje completo registrado sin conexión en la app.
/// </summary>
public class SincronizarViajeRequest
{
    /// <summary>
    /// UUID del viaje generado en el teléfono (offline-first, opcional). Si se manda, reintentar con el
    /// mismo id regresa el viaje ya creado en lugar de un 409.
    /// </summary>
    [GuidNoVacio]
    public Guid? ViajeId { get; set; }

    [GuidNoVacio]
    public Guid EmpresaId { get; set; }

    [GuidNoVacio]
    public Guid CarroId { get; set; }

    [GuidNoVacio]
    public Guid? SolicitudReservaId { get; set; }

    [GuidNoVacio]
    public Guid EmpleadoOperadorId { get; set; }

    /// <summary>Centro de costos del viaje (opcional, de la misma empresa).</summary>
    [GuidNoVacio]
    public Guid? CentroCostoId { get; set; }

    [Required]
    public DateTime? FechaInicio { get; set; }

    public decimal? UbicacionInicioLat { get; set; }
    public decimal? UbicacionInicioLng { get; set; }
    public decimal KilometrajeInicial { get; set; }

    public EstadoViaje Estado { get; set; } = EstadoViaje.Finalizado;

    public DateTime? FechaFin { get; set; }
    public decimal? UbicacionFinLat { get; set; }
    public decimal? UbicacionFinLng { get; set; }
    public decimal? KilometrajeFinal { get; set; }

    [StringLength(255)]
    public string? MotivoInterrupcion { get; set; }

    /// <summary>Pausas que se hicieron sin conexión (opcional).</summary>
    public List<PausaSincronizadaRequest>? Pausas { get; set; }
}
