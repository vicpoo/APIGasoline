using APIGasoline.Application.Features.PuntosRuta.Common;
using APIGasoline.Domain.Entities;

namespace APIGasoline.Application.Tests.Features.PuntosRuta.Common;

/// <summary>
/// Idempotencia del registro de puntos GPS: los timestamps que ya existen para el viaje
/// (en la BD o repetidos dentro del mismo lote) se descartan; los nuevos conservan su orden.
/// </summary>
public class DeduplicadorPuntosRutaTests
{
    private static readonly DateTime T0 = new(2026, 10, 5, 8, 30, 0, DateTimeKind.Utc);
    private static readonly DateTime T1 = T0.AddSeconds(30);
    private static readonly DateTime T2 = T0.AddSeconds(60);

    [Fact]
    public void FiltrarNuevos_SinExistentes_DevuelveTodos()
    {
        var puntos = new[] { ConstruirPunto(T0), ConstruirPunto(T1), ConstruirPunto(T2) };

        var nuevos = DeduplicadorPuntosRuta.FiltrarNuevos(puntos, []);

        Assert.Equal(3, nuevos.Count);
    }

    [Fact]
    public void FiltrarNuevos_ReenvioDelMismoLote_NoDevuelveNinguno()
    {
        var puntos = new[] { ConstruirPunto(T0), ConstruirPunto(T1), ConstruirPunto(T2) };

        var nuevos = DeduplicadorPuntosRuta.FiltrarNuevos(puntos, [T0, T1, T2]);

        Assert.Empty(nuevos);
    }

    [Fact]
    public void FiltrarNuevos_LoteParcialmenteRegistrado_SoloDevuelveLosNuevos()
    {
        var puntos = new[] { ConstruirPunto(T0), ConstruirPunto(T1), ConstruirPunto(T2) };

        var nuevos = DeduplicadorPuntosRuta.FiltrarNuevos(puntos, [T0]);

        Assert.Equal(new[] { T1, T2 }, nuevos.Select(p => p.TimestampCaptura));
    }

    [Fact]
    public void FiltrarNuevos_TimestampRepetidoDentroDelLote_SeQuedaElPrimero()
    {
        var primero = ConstruirPunto(T0, latitud: 16.75m);
        var repetido = ConstruirPunto(T0, latitud: 16.80m);

        var nuevos = DeduplicadorPuntosRuta.FiltrarNuevos([primero, repetido], []);

        Assert.Equal(16.75m, Assert.Single(nuevos).Latitud);
    }

    [Fact]
    public void FiltrarNuevos_MilisegundosDistintosEnElMismoSegundo_SeConsideranDuplicados()
    {
        // El dominio trunca a segundos, igual que la columna de la BD
        var a = ConstruirPunto(T0.AddMilliseconds(100));
        var b = ConstruirPunto(T0.AddMilliseconds(900));

        var nuevos = DeduplicadorPuntosRuta.FiltrarNuevos([a, b], []);

        Assert.Single(nuevos);
    }

    [Fact]
    public void FiltrarNuevos_ConservaElOrdenDeLlegada()
    {
        var puntos = new[] { ConstruirPunto(T2), ConstruirPunto(T0), ConstruirPunto(T1) };

        var nuevos = DeduplicadorPuntosRuta.FiltrarNuevos(puntos, []);

        Assert.Equal(new[] { T2, T0, T1 }, nuevos.Select(p => p.TimestampCaptura));
    }

    [Fact]
    public void FiltrarNuevos_NoModificaLaColeccionDeExistentes()
    {
        var existentes = new HashSet<DateTime> { T0 };

        DeduplicadorPuntosRuta.FiltrarNuevos([ConstruirPunto(T1)], existentes);

        Assert.Equal(new[] { T0 }, existentes);
    }

    private static PuntoRuta ConstruirPunto(DateTime timestamp, decimal latitud = 16.753056m, decimal longitud = -93.115556m)
        => PuntoRuta.Crear(Id(1), Id(1), latitud, longitud, timestamp);
}
