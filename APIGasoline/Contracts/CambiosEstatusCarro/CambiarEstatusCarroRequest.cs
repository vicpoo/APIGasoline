using System.ComponentModel.DataAnnotations;
using APIGasoline.Domain.Enums;

namespace APIGasoline.Contracts.CambiosEstatusCarro;

public class CambiarEstatusCarroRequest
{
    [Range(1, int.MaxValue)]
    public int CarroId { get; set; }

    /// <summary>Empleado que realiza el cambio.</summary>
    [Range(1, int.MaxValue)]
    public int EmpleadoId { get; set; }

    [Required]
    public EstadoCarro? EstatusNuevo { get; set; }

    /// <summary>Obligatorio para mandar a mantenimiento.</summary>
    [StringLength(255)]
    public string? Motivo { get; set; }

    /// <summary>Si viene y el viaje está en curso, se interrumpe.</summary>
    [Range(1, int.MaxValue)]
    public int? ViajeAfectadoId { get; set; }

    /// <summary>Solo para registros offline; si no viene, se usa la hora del servidor.</summary>
    public DateTime? FechaCambio { get; set; }

    public bool Sincronizado { get; set; }
}
