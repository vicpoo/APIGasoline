using System.ComponentModel.DataAnnotations;

namespace APIGasoline.Contracts.Viajes;

public class IniciarViajeRequest
{
    [GuidNoVacio]
    public Guid EmpresaId { get; set; }

    [GuidNoVacio]
    public Guid CarroId { get; set; }

    [GuidNoVacio]
    public Guid? SolicitudReservaId { get; set; }

    [GuidNoVacio]
    public Guid EmpleadoOperadorId { get; set; }

    [Required]
    public DateTime? FechaInicio { get; set; }

    public decimal? UbicacionInicioLat { get; set; }
    public decimal? UbicacionInicioLng { get; set; }

    public decimal KilometrajeInicial { get; set; }
}
