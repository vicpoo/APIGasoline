namespace APIGasoline.Domain.Enums;

public enum TipoNotificacion
{
    /// <summary>Al confirmador: alguien solicitó un carro.</summary>
    NuevaSolicitud = 0,

    /// <summary>Al solicitante: su solicitud fue aprobada o rechazada.</summary>
    ResultadoSolicitud = 1,

    /// <summary>Al empleado que tiene el carro: su estatus se cambió (ej. a mantenimiento).</summary>
    CambioEstatusForzado = 2
}
