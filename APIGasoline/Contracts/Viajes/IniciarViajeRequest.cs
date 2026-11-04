using System.ComponentModel.DataAnnotations;

namespace APIGasoline.Contracts.Viajes;

public class IniciarViajeRequest
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
}
