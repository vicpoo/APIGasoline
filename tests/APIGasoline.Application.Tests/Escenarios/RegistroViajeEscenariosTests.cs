using APIGasoline.Application.Features.CargasCombustible.Common;
using APIGasoline.Application.Features.PuntosRuta.Common;
using APIGasoline.Application.Features.Viajes.Common;
using APIGasoline.Domain.Entities;

namespace APIGasoline.Application.Tests.Escenarios;

/// <summary>
/// Escenarios de lo que registra la app móvil durante un viaje, componiendo los componentes reales
/// (PuntoRuta, CargaCombustible, VentanaViaje, DeduplicadorPuntosRuta, ReglaCapacidadTanque)
/// en el mismo orden que RegistrarLotePuntosRutaCommand y CargaCombustibleValidator.
/// Criterios: idempotencia de los lotes GPS y aceptación de registros offline dentro de la ventana.
/// </summary>
public class RegistroViajeEscenariosTests
{
    private static readonly DateTime Inicio = new(2026, 10, 5, 8, 30, 0, DateTimeKind.Utc);
    private static readonly DateTime Fin = new(2026, 10, 5, 12, 10, 0, DateTimeKind.Utc);
    private static readonly DateTime Ahora = new(2026, 10, 5, 9, 0, 0, DateTimeKind.Utc);

    // --- Idempotencia: la app reenvía el lote después de un corte de conexión ---

    [Fact]
    public void ReenvioTrasCorte_LoteConPuntosViejosYNuevos_SoloSeGuardanLosNuevos()
    {
        var primerLote = ConstruirLote(Inicio, cantidad: 3);
        var guardados = DeduplicadorPuntosRuta.FiltrarNuevos(primerLote, []);

        // La respuesta no llegó al teléfono: reenvía los 3 puntos y agrega 2 más
        var segundoLote = ConstruirLote(Inicio, cantidad: 5);
        var nuevos = DeduplicadorPuntosRuta.FiltrarNuevos(segundoLote, guardados.Select(p => p.TimestampCaptura));

        Assert.Equal(3, guardados.Count);
        Assert.Equal(2, nuevos.Count);
        Assert.Equal(Inicio.AddSeconds(90), nuevos[0].TimestampCaptura);
    }

    [Fact]
    public void ReenvioTrasCorte_MismoLoteTresVeces_LaRutaNoSeDuplica()
    {
        var lote = ConstruirLote(Inicio, cantidad: 4);
        var ruta = new List<PuntoRuta>();

        ruta.AddRange(DeduplicadorPuntosRuta.FiltrarNuevos(lote, ruta.Select(p => p.TimestampCaptura)));
        ruta.AddRange(DeduplicadorPuntosRuta.FiltrarNuevos(lote, ruta.Select(p => p.TimestampCaptura)));
        ruta.AddRange(DeduplicadorPuntosRuta.FiltrarNuevos(lote, ruta.Select(p => p.TimestampCaptura)));

        Assert.Equal(4, ruta.Count);
    }

    // --- Puntos GPS fuera de la ventana del viaje ---

    [Fact]
    public void PuntosDeUnViajeEnCurso_UnoConElRelojAdelantado_SeDetectaFueraDeLaVentana()
    {
        var puntos = new[]
        {
            ConstruirPunto(Inicio.AddMinutes(10)),
            ConstruirPunto(Ahora.AddMinutes(20)),  // el teléfono tiene el reloj 20 min adelantado
        };

        var fueraDeVentana = puntos.Where(p => !VentanaViaje.Contiene(p.TimestampCaptura, Inicio, null, Ahora));

        Assert.Single(fueraDeVentana);
    }

    // --- Carga registrada offline y subida después de finalizar el viaje ---

    [Fact]
    public void CargaOffline_SubidaDespuesDeFinalizar_DentroDeLaVentana_SeAcepta()
    {
        var carga = ConstruirCarga(litros: 45.30m, fecha: Fin.AddMinutes(3));
        var ahoraAlSincronizar = Fin.AddHours(5);

        var dentroDeVentana = VentanaViaje.Contiene(carga.FechaCarga, Inicio, Fin, ahoraAlSincronizar);
        var excedeTanque = ReglaCapacidadTanque.Excede(carga.Litros, 80m);

        Assert.True(dentroDeVentana);
        Assert.False(excedeTanque);
    }

    [Fact]
    public void CargaOffline_ConFechaPosteriorAlFinMasTolerancia_SeRechaza()
    {
        var carga = ConstruirCarga(fecha: Fin.AddMinutes(6));

        Assert.False(VentanaViaje.Contiene(carga.FechaCarga, Inicio, Fin, Fin.AddHours(5)));
    }

    [Fact]
    public void CargaValidaEnFecha_PeroMayorAlTanque_SeRechazaPorCapacidad()
    {
        var carga = ConstruirCarga(litros: 95m, fecha: Inicio.AddHours(1));

        Assert.True(VentanaViaje.Contiene(carga.FechaCarga, Inicio, Fin, Ahora));
        Assert.True(ReglaCapacidadTanque.Excede(carga.Litros, 80m));
    }

    // Puntos cada 30 segundos a partir de "desde"
    private static List<PuntoRuta> ConstruirLote(DateTime desde, int cantidad)
        => Enumerable.Range(0, cantidad).Select(i => ConstruirPunto(desde.AddSeconds(30 * i))).ToList();

    private static PuntoRuta ConstruirPunto(DateTime timestamp)
        => PuntoRuta.Crear(Id(1), Id(1), 16.753056m, -93.115556m, timestamp, false);

    private static CargaCombustible ConstruirCarga(decimal litros = 45.30m, DateTime? fecha = null)
        => CargaCombustible.Crear(Id(1), Id(1), litros, 1087.20m, "empresa/1/viaje/1/carga/1/comprobante.jpg", fecha ?? Inicio, null, null, true);
}
