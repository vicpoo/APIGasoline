using APIGasoline.Domain.Entities;
using APIGasoline.Domain.Rutas;

namespace APIGasoline.Domain.Tests.Escenarios;

/// <summary>
/// Flujo completo de un viaje con GPS: se inicia, se reciben puntos crudos,
/// se cierra y se procesa la ruta como lo hará el handler en Application.
/// </summary>
public class RutaEscenariosTests
{
    private const int EmpresaId = 1;
    private const int ViajeId = 7; // simula el id que asignaría la BD
    private const decimal KilometrajeInicial = 15000m;

    private static readonly DateTime Inicio = new(2026, 10, 7, 8, 30, 0, DateTimeKind.Utc);
    private static readonly DateTime FechaProceso = new(2026, 10, 7, 9, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void ViajeFinalizado_OdometroCoincideConGps_NoGeneraAlerta()
    {
        var viaje = IniciarViaje();
        var puntos = RecibirPuntos(11, precision: 8m);
        viaje.Finalizar(Inicio.AddMinutes(5), null, null, 15002.80m);

        var ruta = Procesar(viaje, puntos, litros: 0.278m);

        Assert.Equal(11, ruta.PuntosRecibidos);
        Assert.Equal(0, ruta.PuntosDescartados);
        Assert.InRange(ruta.DistanciaGpsKm, 2.778m, 2.782m);
        Assert.Equal(2.80m, ruta.DistanciaOdometroKm);
        Assert.False(ruta.AlertaKilometraje);
        Assert.NotNull(ruta.RendimientoKmPorLitro);
        Assert.False(string.IsNullOrEmpty(ruta.Polyline));
    }

    [Fact]
    public void ViajeFinalizado_OdometroInflado_GeneraAlerta()
    {
        var viaje = IniciarViaje();
        var puntos = RecibirPuntos(11, precision: 8m);
        viaje.Finalizar(Inicio.AddMinutes(5), null, null, 15010m);

        var ruta = Procesar(viaje, puntos, litros: 0m);

        Assert.True(ruta.AlertaKilometraje);
        Assert.True(ruta.DiferenciaKm > 7m);
        Assert.True(ruta.DiferenciaPorcentaje > 100m);
        Assert.Null(ruta.RendimientoKmPorLitro);
    }

    [Fact]
    public void ViajeInterrumpidoSinKilometraje_NoComparaOdometro()
    {
        var viaje = IniciarViaje();
        var puntos = RecibirPuntos(6, precision: 8m);
        viaje.Interrumpir("Falla mecánica", Inicio.AddMinutes(3), null, null, null);

        var ruta = Procesar(viaje, puntos, litros: 0m);

        Assert.Null(ruta.DistanciaOdometroKm);
        Assert.Null(ruta.DiferenciaKm);
        Assert.False(ruta.AlertaKilometraje);
        Assert.True(ruta.DistanciaGpsKm > 0m);
    }

    [Fact]
    public void Reprocesar_MismosPuntos_DaElMismoResultado()
    {
        var viaje = IniciarViaje();
        var puntos = RecibirPuntos(11, precision: 8m);
        viaje.Finalizar(Inicio.AddMinutes(5), null, null, 15002.80m);
        var ruta = Procesar(viaje, puntos, litros: 0.278m);
        var distancia = ruta.DistanciaGpsKm;
        var polyline = ruta.Polyline;

        var resultado = ProcesadorRuta.Procesar(
            puntos.Select(p => p.ToPuntoGps()).ToList(), viaje.KilometrajeInicial, viaje.KilometrajeFinal, 0.278m);
        ruta.Actualizar(resultado, FechaProceso.AddHours(1));

        Assert.Equal(distancia, ruta.DistanciaGpsKm);
        Assert.Equal(polyline, ruta.Polyline);
        Assert.Equal(FechaProceso.AddHours(1), ruta.FechaProceso);
    }

    [Fact]
    public void Reprocesar_ConParametrosMasEstrictos_CambiaLosDescartados()
    {
        var viaje = IniciarViaje();
        var puntos = RecibirPuntos(11, precision: 30m);
        viaje.Finalizar(Inicio.AddMinutes(5), null, null, 15002.80m);
        var crudos = puntos.Select(p => p.ToPuntoGps()).ToList();

        var normal = ProcesadorRuta.Procesar(crudos, viaje.KilometrajeInicial, viaje.KilometrajeFinal, 0m);
        var estricto = ProcesadorRuta.Procesar(crudos, viaje.KilometrajeInicial, viaje.KilometrajeFinal, 0m,
            new ParametrosRuta { PrecisionMaximaMetros = 20 });

        Assert.Equal(0, normal.PuntosDescartados);
        Assert.Equal(11, estricto.PuntosDescartados);
        Assert.Equal(0m, estricto.DistanciaGpsKm);
    }

    private static Viaje IniciarViaje()
        => Viaje.Iniciar(EmpresaId, carroId: 1, solicitudReservaId: null, empleadoOperadorId: 1, Inicio, null, null, KilometrajeInicial);

    /// <summary>Puntos crudos como llegarían del celular: uno cada 30 s, ~278 m al norte.</summary>
    private static List<PuntoRuta> RecibirPuntos(int cantidad, decimal precision)
        => Enumerable.Range(0, cantidad)
            .Select(i => PuntoRuta.Crear(EmpresaId, ViajeId,
                16.75m + i * 0.0025m, -93.11m, Inicio.AddSeconds(30 * i), sincronizado: false,
                precisionMetros: precision, altitud: 530m, velocidad: 9.27m))
            .ToList();

    private static RutaProcesada Procesar(Viaje viaje, List<PuntoRuta> puntos, decimal litros)
    {
        var resultado = ProcesadorRuta.Procesar(
            puntos.Select(p => p.ToPuntoGps()).ToList(), viaje.KilometrajeInicial, viaje.KilometrajeFinal, litros);

        return RutaProcesada.Crear(EmpresaId, ViajeId, resultado, FechaProceso);
    }
}
