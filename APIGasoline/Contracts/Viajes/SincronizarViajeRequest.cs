using System.ComponentModel.DataAnnotations;
using APIGasoline.Domain.Enums;

namespace APIGasoline.Contracts.Viajes;

/// <summary>
/// Viaje completo registrado sin conexión en la app.
/// </summary>
public class SincronizarViajeRequest
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

    public EstadoViaje Estado { get; set; } = EstadoViaje.Finalizado;

    public DateTime? FechaFin { get; set; }
    public decimal? UbicacionFinLat { get; set; }
    public decimal? UbicacionFinLng { get; set; }
    public decimal? KilometrajeFinal { get; set; }

    [StringLength(255)]
    public string? MotivoInterrupcion { get; set; }
}
