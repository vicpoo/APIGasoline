using APIGasoline.Domain.Entities;

namespace APIGasoline.Application.Features.PuntosRuta.Common;

/// <summary>
/// Hace idempotente el registro de puntos GPS: un punto cuyo timestamp ya existe para el viaje
/// (en la BD o antes en el mismo lote) se descarta. Así la app puede reenviar un lote completo
/// si se cortó la conexión sin duplicar la ruta.
/// </summary>
public static class DeduplicadorPuntosRuta
{
    /// <returns>Los puntos nuevos, en el mismo orden en que llegaron.</returns>
    public static IReadOnlyList<PuntoRuta> FiltrarNuevos(IEnumerable<PuntoRuta> puntos, IEnumerable<DateTime> timestampsExistentes)
    {
        var vistos = new HashSet<DateTime>(timestampsExistentes);
        var nuevos = new List<PuntoRuta>();

        foreach (var punto in puntos)
        {
            // Add devuelve false si ya estaba: cubre duplicados en la BD y dentro del mismo lote
            if (vistos.Add(punto.TimestampCaptura))
                nuevos.Add(punto);
        }

        return nuevos;
    }
}
