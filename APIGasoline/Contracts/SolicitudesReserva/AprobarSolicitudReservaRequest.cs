using System.ComponentModel.DataAnnotations;

namespace APIGasoline.Contracts.SolicitudesReserva;

public class AprobarSolicitudReservaRequest
{
    /// <summary>
    /// OBSOLETO (F008): se ignora. El confirmador es siempre el empleado del token.
    /// Se acepta para no romper las apps que todavía lo mandan.
    /// </summary>
    [Obsolete("Se ignora: el confirmador es el empleado del token.")]
    public Guid? EmpleadoConfirmadorId { get; set; }

    [StringLength(255)]
    public string? Motivo { get; set; }
}
