using APIGasoline.Application.Features.Viajes.Common;

namespace APIGasoline.Application.Tests.Features.Viajes.Common;

/// <summary>
/// Ventana de un viaje para aceptar cargas y puntos GPS: [inicio - 5 min, (fin o ahora) + 5 min].
/// Los bordes son inclusivos. La tolerancia cubre diferencias de reloj entre el teléfono y el servidor.
/// </summary>
public class VentanaViajeTests
{
    private static readonly DateTime Inicio = new(2026, 10, 5, 8, 30, 0, DateTimeKind.Utc);
    private static readonly DateTime Fin = new(2026, 10, 5, 12, 10, 0, DateTimeKind.Utc);
    private static readonly DateTime Ahora = new(2026, 10, 5, 9, 0, 0, DateTimeKind.Utc);

    // --- Viaje finalizado ---

    [Theory]
    [InlineData(0)]     // justo al inicio
    [InlineData(-5)]    // 5 min antes del inicio: borde inclusivo de la tolerancia
    [InlineData(100)]   // a mitad del viaje
    public void Contiene_ViajeFinalizado_DentroDelPeriodo_DevuelveTrue(int minutosDesdeInicio)
    {
        var fecha = Inicio.AddMinutes(minutosDesdeInicio);

        Assert.True(VentanaViaje.Contiene(fecha, Inicio, Fin, Ahora));
    }

    [Fact]
    public void Contiene_ViajeFinalizado_CincoMinutosDespuesDelFin_DevuelveTrue()
    {
        Assert.True(VentanaViaje.Contiene(Fin.AddMinutes(5), Inicio, Fin, Ahora));
    }

    [Fact]
    public void Contiene_UnSegundoAntesDeLaTolerancia_DevuelveFalse()
    {
        var fecha = Inicio.AddMinutes(-5).AddSeconds(-1);

        Assert.False(VentanaViaje.Contiene(fecha, Inicio, Fin, Ahora));
    }

    [Fact]
    public void Contiene_ViajeFinalizado_UnSegundoDespuesDeLaTolerancia_DevuelveFalse()
    {
        var fecha = Fin.AddMinutes(5).AddSeconds(1);

        Assert.False(VentanaViaje.Contiene(fecha, Inicio, Fin, Ahora));
    }

    [Fact]
    public void Contiene_ViajeFinalizado_NoDependeDeAhora()
    {
        // "ahora" es antes del fin; como el viaje ya tiene fecha de fin, se usa esa
        var fecha = Fin.AddMinutes(2);

        Assert.True(VentanaViaje.Contiene(fecha, Inicio, Fin, Ahora));
    }

    // --- Viaje en curso (sin fecha de fin) ---

    [Fact]
    public void Contiene_ViajeEnCurso_HastaAhoraMasTolerancia_DevuelveTrue()
    {
        Assert.True(VentanaViaje.Contiene(Ahora.AddMinutes(5), Inicio, null, Ahora));
    }

    [Fact]
    public void Contiene_ViajeEnCurso_DespuesDeAhoraMasTolerancia_DevuelveFalse()
    {
        // Un teléfono con el reloj adelantado más de 5 min no puede registrar puntos "en el futuro"
        Assert.False(VentanaViaje.Contiene(Ahora.AddMinutes(6), Inicio, null, Ahora));
    }

    // --- Calendario ---

    [Fact]
    public void Contiene_ViajeQueCruzaMedianocheYFinDeAnio_DevuelveTrue()
    {
        var inicio = new DateTime(2026, 12, 31, 23, 50, 0, DateTimeKind.Utc);
        var fin = new DateTime(2027, 1, 1, 0, 40, 0, DateTimeKind.Utc);

        Assert.True(VentanaViaje.Contiene(new DateTime(2027, 1, 1, 0, 0, 30, DateTimeKind.Utc), inicio, fin, fin));
    }

    [Fact]
    public void Tolerancia_EsDeCincoMinutos()
    {
        Assert.Equal(TimeSpan.FromMinutes(5), VentanaViaje.Tolerancia);
    }
}
