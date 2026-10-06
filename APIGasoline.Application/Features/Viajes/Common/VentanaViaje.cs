namespace APIGasoline.Application.Features.Viajes.Common;

/// <summary>
/// Periodo en el que se aceptan registros de un viaje (cargas de combustible y puntos GPS):
/// desde el inicio hasta el fin del viaje (o hasta "ahora" si sigue en curso),
/// con una tolerancia por diferencias de reloj entre el teléfono y el servidor.
/// Es pura: "ahora" entra como parámetro.
/// </summary>
public static class VentanaViaje
{
    public static readonly TimeSpan Tolerancia = TimeSpan.FromMinutes(5);

    public static bool Contiene(DateTime fecha, DateTime fechaInicio, DateTime? fechaFin, DateTime ahora)
    {
        var desde = fechaInicio - Tolerancia;
        var hasta = (fechaFin ?? ahora) + Tolerancia;

        return fecha >= desde && fecha <= hasta;
    }
}
