using APIGasoline.Domain.Entities;
using APIGasoline.Domain.Enums;
using APIGasoline.Domain.Exceptions;

namespace APIGasoline.Domain.Tests.Entities;

/// <summary>
/// Ciclo de vida del viaje: iniciar (en curso), finalizar e interrumpir.
/// Solo un viaje en curso se puede cerrar; la fecha y el kilometraje final no pueden ser menores a los iniciales.
/// </summary>
public class ViajeTests
{
    private static readonly DateTime Inicio = new(2026, 10, 5, 8, 30, 0, DateTimeKind.Utc);
    private static readonly DateTime Fin = new(2026, 10, 5, 12, 10, 0, DateTimeKind.Utc);

    // --- Iniciar ---

    [Fact]
    public void Iniciar_DatosValidos_QuedaEnCurso()
    {
        var viaje = ConstruirViaje();

        Assert.Equal(EstadoViaje.EnCurso, viaje.Estado);
        Assert.True(viaje.EstaEnCurso);
        Assert.Null(viaje.FechaFin);
    }

    [Fact]
    public void Iniciar_SinSolicitudDeReserva_SeAcepta()
    {
        var viaje = ConstruirViaje(solicitudReservaId: null);

        Assert.Null(viaje.SolicitudReservaId);
    }

    [Theory]
    [InlineData(0, 1, 1)]
    [InlineData(1, 0, 1)]
    [InlineData(1, 1, 0)]
    public void Iniciar_IdsInvalidos_LanzaDomainException(int empresaId, int carroId, int operadorId)
    {
        Assert.Throws<DomainException>(() => ConstruirViaje(empresaId: empresaId, carroId: carroId, operadorId: operadorId));
    }

    [Fact]
    public void Iniciar_SolicitudReservaCero_LanzaDomainException()
    {
        Assert.Throws<DomainException>(() => ConstruirViaje(solicitudReservaId: 0));
    }

    [Fact]
    public void Iniciar_SinFechaDeInicio_LanzaDomainException()
    {
        Assert.Throws<DomainException>(() => ConstruirViaje(fechaInicio: default(DateTime)));
    }

    [Fact]
    public void Iniciar_KilometrajeNegativo_LanzaDomainException()
    {
        Assert.Throws<DomainException>(() => ConstruirViaje(kilometrajeInicial: -1m));
    }

    [Theory]
    [InlineData(16.75, null)]   // latitud sin longitud
    [InlineData(91.0, -93.11)]  // latitud fuera de rango
    [InlineData(16.75, 181.0)]  // longitud fuera de rango
    public void Iniciar_UbicacionInvalida_LanzaDomainException(double lat, double? lng)
    {
        Assert.Throws<DomainException>(() => ConstruirViaje(lat: (decimal)lat, lng: (decimal?)lng));
    }

    // --- Finalizar ---

    [Fact]
    public void Finalizar_DatosValidos_CalculaDistanciaYQuedaFinalizado()
    {
        var viaje = ConstruirViaje(kilometrajeInicial: 15480m);

        viaje.Finalizar(Fin, 14.905556m, -92.263056m, 15775.40m);

        Assert.Equal(EstadoViaje.Finalizado, viaje.Estado);
        Assert.Equal(295.40m, viaje.DistanciaRecorrida);
        Assert.Equal(Fin, viaje.FechaFin);
    }

    [Fact]
    public void Finalizar_FechaFinAnteriorAlInicio_LanzaDomainException()
    {
        var viaje = ConstruirViaje();

        Assert.Throws<DomainException>(() => viaje.Finalizar(Inicio.AddMinutes(-1), null, null, 15500m));
    }

    [Fact]
    public void Finalizar_KilometrajeFinalMenorAlInicial_LanzaDomainException()
    {
        var viaje = ConstruirViaje(kilometrajeInicial: 15480m);

        Assert.Throws<DomainException>(() => viaje.Finalizar(Fin, null, null, 100m));
    }

    [Fact]
    public void Finalizar_ViajeYaFinalizado_LanzaDomainException()
    {
        var viaje = ConstruirViaje();
        viaje.Finalizar(Fin, null, null, 15500m);

        Assert.Throws<DomainException>(() => viaje.Finalizar(Fin, null, null, 15600m));
    }

    [Fact]
    public void Finalizar_ViajeInterrumpido_LanzaDomainException()
    {
        var viaje = ConstruirViaje();
        viaje.Interrumpir("Ponchadura", Fin, null, null, null);

        Assert.Throws<DomainException>(() => viaje.Finalizar(Fin, null, null, 15600m));
    }

    // --- Interrumpir ---

    [Fact]
    public void Interrumpir_ConMotivo_QuedaInterrumpidoConMotivoNormalizado()
    {
        var viaje = ConstruirViaje();

        viaje.Interrumpir("  Ponchadura de llanta  ", Fin, null, null, null);

        Assert.Equal(EstadoViaje.Interrumpido, viaje.Estado);
        Assert.Equal("Ponchadura de llanta", viaje.MotivoInterrupcion);
    }

    [Fact]
    public void Interrumpir_SinKilometrajeFinal_NoCalculaDistancia()
    {
        var viaje = ConstruirViaje();

        viaje.Interrumpir("Ponchadura", Fin, null, null, null);

        Assert.Null(viaje.DistanciaRecorrida);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Interrumpir_SinMotivo_LanzaDomainException(string motivo)
    {
        var viaje = ConstruirViaje();

        Assert.Throws<DomainException>(() => viaje.Interrumpir(motivo, Fin, null, null, null));
    }

    [Fact]
    public void Interrumpir_MotivoMayorA255_LanzaDomainException()
    {
        var viaje = ConstruirViaje();

        Assert.Throws<DomainException>(() => viaje.Interrumpir(new string('x', 256), Fin, null, null, null));
    }

    [Fact]
    public void MarcarSincronizado_ViajeOffline_QuedaSincronizado()
    {
        var viaje = ConstruirViaje();

        viaje.MarcarSincronizado();

        Assert.True(viaje.Sincronizado);
    }

    private static Viaje ConstruirViaje(
        int empresaId = 1,
        int carroId = 1,
        int? solicitudReservaId = 1,
        int operadorId = 1,
        DateTime? fechaInicio = null,
        decimal? lat = 16.753056m,
        decimal? lng = -93.115556m,
        decimal kilometrajeInicial = 15480m)
        => Viaje.Iniciar(Id(empresaId), Id(carroId), Id(solicitudReservaId), Id(operadorId), fechaInicio ?? Inicio, lat, lng, kilometrajeInicial);
}
