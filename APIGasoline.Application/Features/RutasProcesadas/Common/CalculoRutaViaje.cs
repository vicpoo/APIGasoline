using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Domain.Entities;
using APIGasoline.Domain.Rutas;

namespace APIGasoline.Application.Features.RutasProcesadas.Common;

/// <summary>
/// Reglas de orquestación del procesamiento de ruta (sin BD, para poder probarlas):
/// solo se procesan viajes cerrados y el resumen de un viaje se crea una vez y después se actualiza.
/// </summary>
public static class CalculoRutaViaje
{
    /// <summary>Un viaje en curso todavía recibe puntos: su ruta no se puede cerrar.</summary>
    public static void ValidarViajeCerrado(Viaje viaje)
    {
        ArgumentNullException.ThrowIfNull(viaje);

        if (viaje.EstaEnCurso)
            throw new ConflictException(
                $"El viaje {viaje.ViajeId} sigue en curso; la ruta se procesa al finalizarlo o interrumpirlo.");
    }

    /// <summary>Ejecuta el algoritmo de Domain con los puntos crudos, el odómetro del viaje y los litros cargados.</summary>
    public static ResultadoRuta Calcular(Viaje viaje, IEnumerable<PuntoRuta> puntos, decimal litrosCargados)
    {
        ArgumentNullException.ThrowIfNull(viaje);
        ArgumentNullException.ThrowIfNull(puntos);

        var crudos = puntos.Select(p => p.ToPuntoGps()).ToList();

        return ProcesadorRuta.Procesar(crudos, viaje.KilometrajeInicial, viaje.KilometrajeFinal, litrosCargados);
    }

    /// <summary>
    /// Si el viaje ya tenía resumen, se actualiza la misma fila (viaje_id es único);
    /// si no, se crea uno nuevo. Devuelve la entidad a guardar.
    /// </summary>
    public static RutaProcesada CrearOActualizar(
        RutaProcesada? existente, int empresaId, int viajeId, ResultadoRuta resultado, DateTime fechaProceso)
    {
        if (existente is null)
            return RutaProcesada.Crear(empresaId, viajeId, resultado, fechaProceso);

        existente.Actualizar(resultado, fechaProceso);
        return existente;
    }
}
