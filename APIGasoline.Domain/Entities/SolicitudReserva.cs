using APIGasoline.Domain.Enums;
using APIGasoline.Domain.Exceptions;

namespace APIGasoline.Domain.Entities;

/// Flujo: Pendiente → Aprobada | Rechazada | Cancelada.  Aprobada → Cancelada.
public class SolicitudReserva
{
    public const int MotivoMaxLength = 255;

    public Guid SolicitudReservaId { get; private set; } = Guid.CreateVersion7();
    public Guid EmpresaId { get; private set; }
    public Guid CarroId { get; private set; }
    public Guid EmpleadoSolicitanteId { get; private set; }

    /// La asigna la base de datos al insertar (default now()).
    public DateTime? FechaSolicitud { get; private set; }

    public EstadoSolicitudReserva Estado { get; private set; } = EstadoSolicitudReserva.Pendiente;
    public Guid? EmpleadoConfirmadorId { get; private set; }
    public DateTime? FechaResolucion { get; private set; }
    public string? MotivoResolucion { get; private set; }

    // Requerido por Entity Framework
    private SolicitudReserva()
    {
    }

    public static SolicitudReserva Crear(Guid empresaId, Guid carroId, Guid empleadoSolicitanteId)
    {
        if (empresaId == Guid.Empty)
            throw new DomainException("El id de la empresa es obligatorio.");

        if (carroId == Guid.Empty)
            throw new DomainException("El id del carro es obligatorio.");

        if (empleadoSolicitanteId == Guid.Empty)
            throw new DomainException("El id del empleado solicitante es obligatorio.");

        return new SolicitudReserva
        {
            EmpresaId = empresaId,
            CarroId = carroId,
            EmpleadoSolicitanteId = empleadoSolicitanteId,
            Estado = EstadoSolicitudReserva.Pendiente
        };
    }

    public bool EstaPendiente => Estado == EstadoSolicitudReserva.Pendiente;
    public bool EstaAprobada => Estado == EstadoSolicitudReserva.Aprobada;

    public void Aprobar(Guid empleadoConfirmadorId, DateTime fecha, string? motivo = null)
    {
        ValidarResolucion(empleadoConfirmadorId, "aprobar");

        Estado = EstadoSolicitudReserva.Aprobada;
        EmpleadoConfirmadorId = empleadoConfirmadorId;
        FechaResolucion = fecha;
        MotivoResolucion = NormalizarMotivo(motivo);
    }

    public void Rechazar(Guid empleadoConfirmadorId, DateTime fecha, string motivo)
    {
        ValidarResolucion(empleadoConfirmadorId, "rechazar");

        var motivoNormalizado = NormalizarMotivo(motivo)
            ?? throw new DomainException("El motivo es obligatorio para rechazar una solicitud.");

        Estado = EstadoSolicitudReserva.Rechazada;
        EmpleadoConfirmadorId = empleadoConfirmadorId;
        FechaResolucion = fecha;
        MotivoResolucion = motivoNormalizado;
    }

    /// Se puede cancelar una solicitud pendiente o una ya aprobada.
    /// Si estaba aprobada, quien la cancela debe liberar el carro.
    public void Cancelar(DateTime fecha, string? motivo = null)
    {
        if (Estado is not (EstadoSolicitudReserva.Pendiente or EstadoSolicitudReserva.Aprobada))
            throw new DomainException($"No se puede cancelar una solicitud en estado {Estado}.");

        Estado = EstadoSolicitudReserva.Cancelada;
        FechaResolucion = fecha;
        MotivoResolucion = NormalizarMotivo(motivo);
    }

    private void ValidarResolucion(Guid empleadoConfirmadorId, string accion)
    {
        if (!EstaPendiente)
            throw new DomainException($"Solo se puede {accion} una solicitud pendiente (estado actual: {Estado}).");

        if (empleadoConfirmadorId == Guid.Empty)
            throw new DomainException("El id del empleado confirmador es obligatorio.");

        if (empleadoConfirmadorId == EmpleadoSolicitanteId)
            throw new DomainException($"Un empleado no puede {accion} su propia solicitud.");
    }

    private static string? NormalizarMotivo(string? motivo)
    {
        var normalizado = string.IsNullOrWhiteSpace(motivo) ? null : motivo.Trim();

        if (normalizado is not null && normalizado.Length > MotivoMaxLength)
            throw new DomainException($"El motivo no puede exceder {MotivoMaxLength} caracteres.");

        return normalizado;
    }
}
