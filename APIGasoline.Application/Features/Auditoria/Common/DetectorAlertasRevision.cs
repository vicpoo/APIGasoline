using APIGasoline.Application.Features.Auditoria.DTOs;
using APIGasoline.Domain.Enums;

namespace APIGasoline.Application.Features.Auditoria.Common;

/// <summary>
/// Calcula las observaciones de la revisión de combustible a partir de los datos existentes.
/// No guarda nada: las alertas se calculan cada vez que se consulta.
/// </summary>
public static class DetectorAlertasRevision
{
    /// <summary>Dos cargas iguales separadas por menos de esto se consideran posible duplicado.</summary>
    public static readonly TimeSpan VentanaDuplicado = TimeSpan.FromMinutes(10);

    private const string EntidadViaje = "viaje";
    private const string EntidadCarga = "carga_combustible";

    public static IReadOnlyList<AlertaRevisionDto> Detectar(
        IReadOnlyList<ViajeRevision> viajes,
        IReadOnlyList<CargaRevision> cargas,
        IReadOnlyDictionary<Guid, CarroRevision> carros,
        IReadOnlyDictionary<Guid, decimal> kilometrajeAnteriorPorCarro)
    {
        var alertas = new List<AlertaRevisionDto>();
        var viajesPorId = viajes.ToDictionary(v => v.ViajeId);

        string? Placas(Guid carroId) => carros.TryGetValue(carroId, out var carro) ? carro.Placas : null;

        // ---------- Viajes ----------
        foreach (var grupo in viajes.GroupBy(v => v.CarroId))
        {
            decimal? kilometrajeAnterior = kilometrajeAnteriorPorCarro.TryGetValue(grupo.Key, out var km) ? km : null;

            foreach (var viaje in grupo.OrderBy(v => v.FechaInicio))
            {
                AlertaRevisionDto AlertaViaje(TipoAlertaRevision tipo, string descripcion)
                    => new(tipo, descripcion, EntidadViaje, viaje.ViajeId, viaje.ViajeId, viaje.CarroId, Placas(viaje.CarroId), viaje.FechaInicio);

                if (viaje.KilometrajeFinal is { } final && final < viaje.KilometrajeInicial)
                {
                    alertas.Add(AlertaViaje(TipoAlertaRevision.KilometrajeInconsistente,
                        $"El kilometraje final ({final:N2}) es menor que el inicial ({viaje.KilometrajeInicial:N2})."));
                }

                if (kilometrajeAnterior is { } anterior && viaje.KilometrajeInicial != anterior)
                {
                    alertas.Add(AlertaViaje(TipoAlertaRevision.KilometrajeInconsistente,
                        $"El viaje empieza con {viaje.KilometrajeInicial:N2} km, pero el viaje anterior del carro terminó con {anterior:N2} km."));
                }

                if (viaje.AlertaKilometraje is true)
                {
                    alertas.Add(AlertaViaje(TipoAlertaRevision.KilometrajeContraGps,
                        $"El odómetro difiere de la ruta GPS por {viaje.DiferenciaKm:N2} km ({viaje.DiferenciaPorcentaje:N2} %)."));
                }

                if (viaje.Corregido)
                    alertas.Add(AlertaViaje(TipoAlertaRevision.RegistroCorregido, "El kilometraje del viaje se corrigió después de registrarse."));

                if (viaje.Estado == EstadoViaje.Interrumpido)
                {
                    alertas.Add(AlertaViaje(TipoAlertaRevision.ViajeInterrumpido,
                        $"El viaje se interrumpió. Motivo: {viaje.MotivoInterrupcion ?? "sin motivo"}."));
                }

                // Si el viaje no tiene kilometraje final, la continuidad se revisa contra el último conocido
                if (viaje.KilometrajeFinal is not null)
                    kilometrajeAnterior = viaje.KilometrajeFinal;
            }
        }

        // ---------- Cargas ----------
        foreach (var carga in cargas)
        {
            if (!viajesPorId.TryGetValue(carga.ViajeId, out var viaje))
                continue;

            AlertaRevisionDto AlertaCarga(TipoAlertaRevision tipo, string descripcion)
                => new(tipo, descripcion, EntidadCarga, carga.CargaCombustibleId, carga.ViajeId, viaje.CarroId, Placas(viaje.CarroId), carga.FechaCarga);

            if (carga.FechaSubidaComprobante is null)
                alertas.Add(AlertaCarga(TipoAlertaRevision.ComprobantePendiente, "La carga no tiene comprobante disponible en el servidor."));

            if (carga.Corregido)
                alertas.Add(AlertaCarga(TipoAlertaRevision.RegistroCorregido, "La carga se corrigió después de registrarse."));

            if (carros.TryGetValue(viaje.CarroId, out var carro) && carro.CapacidadTanque is { } capacidad && carga.Litros > capacidad)
            {
                alertas.Add(AlertaCarga(TipoAlertaRevision.CargaInusual,
                    $"Se registraron {carga.Litros:N2} L, pero el tanque es de {capacidad:N2} L."));
            }
        }

        // Posibles duplicados: mismo viaje, mismos litros y monto, con pocos minutos de diferencia
        foreach (var grupo in cargas.GroupBy(c => (c.ViajeId, c.Litros, c.Monto)))
        {
            var ordenadas = grupo.OrderBy(c => c.FechaCarga).ToList();

            for (var i = 1; i < ordenadas.Count; i++)
            {
                var carga = ordenadas[i];

                if (carga.FechaCarga - ordenadas[i - 1].FechaCarga > VentanaDuplicado)
                    continue;

                if (!viajesPorId.TryGetValue(carga.ViajeId, out var viaje))
                    continue;

                alertas.Add(new AlertaRevisionDto(
                    TipoAlertaRevision.PosibleDuplicado,
                    $"Hay otra carga de {carga.Litros:N2} L en el mismo viaje con menos de {VentanaDuplicado.TotalMinutes:N0} minutos de diferencia.",
                    EntidadCarga,
                    carga.CargaCombustibleId,
                    carga.ViajeId,
                    viaje.CarroId,
                    Placas(viaje.CarroId),
                    carga.FechaCarga));
            }
        }

        return alertas
            .OrderByDescending(a => a.Fecha)
            .ThenBy(a => a.Tipo)
            .ToList();
    }
}
