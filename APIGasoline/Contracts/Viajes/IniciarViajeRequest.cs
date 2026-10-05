using System.ComponentModel.DataAnnotations;

namespace APIGasoline.Contracts.Viajes;

public class IniciarViajeRequest
{
    [Range(1, int.MaxValue)]
    public int EmpresaId { get; set; }

    [Range(1, int.MaxValue)]
    public int CarroId { get; set; }

    [Range(1, int.MaxValue)]
    public int? SolicitudReservaId { get; set; }

    [Range(1, int.MaxValue)]
    public int EmpleadoOperadorId { get; set; }

    [Required]
    public DateTime? FechaInicio { get; set; }

    public decimal? UbicacionInicioLat { get; set; }
    public decimal? UbicacionInicioLng { get; set; }

    public decimal KilometrajeInicial { get; set; }
}
