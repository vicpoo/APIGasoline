namespace APIGasoline.Domain.Enums;

public enum EstadoOutbox
{
    Pendiente = 0,
    Procesando = 1,
    Enviado = 2,
    Fallido = 3
}
