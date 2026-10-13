namespace APIGasoline.Domain.Enums;

public enum EstadoSolicitudReserva
{
    Pendiente = 0,
    Aprobada = 1,
    Rechazada = 2,
    Cancelada = 3,

    /// <summary>El operador liberó el carro al terminar de usarlo (HU-USO-04).</summary>
    Finalizada = 4
}
