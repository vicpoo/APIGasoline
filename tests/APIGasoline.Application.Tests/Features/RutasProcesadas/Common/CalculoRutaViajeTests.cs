using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Features.RutasProcesadas.Common;
using APIGasoline.Domain.Entities;
using APIGasoline.Domain.Rutas;

namespace APIGasoline.Application.Tests.Features.RutasProcesadas.Common;

/// <summary>
/// Orquestación del procesamiento de ruta: solo viajes cerrados,
/// el cálculo usa el odómetro del viaje y el resumen se crea una vez y luego se actualiza.
/// </summary>
public class CalculoRutaViajeTests
{
    private static readonly Guid EmpresaId = Id(1);
    private static readonly Guid ViajeId = Id(7);
    private static readonly DateTime Inicio = new(2026, 10, 7, 8, 30, 0, DateTimeKind.Utc);
    private static readonly DateTime FechaProceso = new(2026, 10, 7, 9, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void ValidarViajeCerrado_ViajeEnCurso_LanzaConflictException()
    {
        var viaje = IniciarViaje();

        Assert.Throws<ConflictException>(() => CalculoRutaViaje.ValidarViajeCerrado(viaje));
    }

    [Fact]
    public void ValidarViajeCerrado_ViajeFinalizado_NoLanza()
    {
        var viaje = IniciarViaje();
        viaje.Finalizar(Inicio.AddMinutes(5), null, null, 15002.80m);

        CalculoRutaViaje.ValidarViajeCerrado(viaje);
    }

    [Fact]
    public void ValidarViajeCerrado_ViajeInterrumpido_NoLanza()
    {
        var viaje = IniciarViaje();
        viaje.Interrumpir("Falla mecánica", Inicio.AddMinutes(5), null, null, null);

        CalculoRutaViaje.ValidarViajeCerrado(viaje);
    }

    [Fact]
    public void Calcular_ViajeFinalizado_UsaPuntosOdometroYLitros()
    {
        var viaje = IniciarViaje();
        viaje.Finalizar(Inicio.AddMinutes(5), null, null, 15002.80m);

        var resultado = CalculoRutaViaje.Calcular(viaje, Puntos(11), 0.278m);

        Assert.Equal(11, resultado.PuntosRecibidos);
        Assert.InRange(resultado.DistanciaGpsKm, 2.778m, 2.782m);
        Assert.Equal(2.80m, resultado.Odometro.DistanciaOdometroKm);
        Assert.False(resultado.Odometro.Alerta);
        Assert.Equal(0.278m, resultado.LitrosCargados);
        Assert.NotNull(resultado.RendimientoKmPorLitro);
    }

    [Fact]
    public void Calcular_ViajeInterrumpidoSinKilometraje_NoComparaOdometro()
    {
        var viaje = IniciarViaje();
        viaje.Interrumpir("Falla mecánica", Inicio.AddMinutes(5), null, null, null);

        var resultado = CalculoRutaViaje.Calcular(viaje, Puntos(6), 0m);

        Assert.Null(resultado.Odometro.DistanciaOdometroKm);
        Assert.False(resultado.Odometro.Alerta);
    }

    [Fact]
    public void CrearOActualizar_SinResumenPrevio_CreaUnoNuevoConLosIdsRecibidos()
    {
        var viaje = ViajeFinalizado();
        var resultado = CalculoRutaViaje.Calcular(viaje, Puntos(11), 0m);

        var ruta = CalculoRutaViaje.CrearOActualizar(null, EmpresaId, ViajeId, resultado, FechaProceso);

        Assert.Equal(EmpresaId, ruta.EmpresaId);
        Assert.Equal(ViajeId, ruta.ViajeId);
        Assert.Equal(resultado.DistanciaGpsKm, ruta.DistanciaGpsKm);
        Assert.Equal(FechaProceso, ruta.FechaProceso);
    }

    [Fact]
    public void CrearOActualizar_ConResumenPrevio_ActualizaLaMismaInstancia()
    {
        var viaje = ViajeFinalizado();
        var anterior = RutaProcesada.Crear(EmpresaId, ViajeId, CalculoRutaViaje.Calcular(viaje, Puntos(3), 0m), FechaProceso);
        var nuevo = CalculoRutaViaje.Calcular(viaje, Puntos(11), 0m);

        var ruta = CalculoRutaViaje.CrearOActualizar(anterior, EmpresaId, ViajeId, nuevo, FechaProceso.AddHours(1));

        Assert.Same(anterior, ruta);
        Assert.Equal(11, ruta.PuntosRecibidos);
        Assert.Equal(FechaProceso.AddHours(1), ruta.FechaProceso);
    }

    [Fact]
    public void Calcular_MismosDatos_EsIdempotente()
    {
        var viaje = ViajeFinalizado();

        var primero = CalculoRutaViaje.Calcular(viaje, Puntos(11), 0.278m);
        var segundo = CalculoRutaViaje.Calcular(viaje, Puntos(11), 0.278m);

        Assert.Equal(primero, segundo);
    }

    private static Viaje IniciarViaje()
        => Viaje.Iniciar(EmpresaId, carroId: Id(1), solicitudReservaId: null, empleadoOperadorId: Id(1), Inicio, null, null, 15000m);

    /// <summary>Viaje finalizado de prueba.</summary>
    private static Viaje ViajeFinalizado()
    {
        var viaje = IniciarViaje();
        viaje.Finalizar(Inicio.AddMinutes(5), null, null, 15002.80m);
        return viaje;
    }

    /// <summary>Puntos cada 30 s, ~278 m al norte, con buena precisión.</summary>
    private static List<PuntoRuta> Puntos(int cantidad)
        => Enumerable.Range(0, cantidad)
            .Select(i => PuntoRuta.Crear(EmpresaId, ViajeId, 16.75m + i * 0.0025m, -93.11m, Inicio.AddSeconds(30 * i),
                sincronizado: false, precisionMetros: 8m))
            .ToList();
}
