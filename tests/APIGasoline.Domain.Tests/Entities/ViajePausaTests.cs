using APIGasoline.Domain.Entities;
using APIGasoline.Domain.Enums;
using APIGasoline.Domain.Exceptions;

namespace APIGasoline.Domain.Tests.Entities;

/// <summary>
/// Pausar y reanudar un viaje: solo en curso, sin dos pausas abiertas, sin encimarse y,
/// al finalizar o interrumpir, la pausa abierta se cierra sola.
/// </summary>
public class ViajePausaTests
{
    private static readonly DateTime Inicio = new(2026, 10, 5, 8, 30, 0, DateTimeKind.Utc);

    private static Viaje ViajeEnCurso()
        => Viaje.Iniciar(Id(1), Id(2), Id(3), Id(4), Inicio, null, null, 1000m);

    // --- Pausar ---

    [Fact]
    public void Pausar_ViajeEnCurso_CreaPausaAbierta()
    {
        var viaje = ViajeEnCurso();

        var pausa = viaje.Pausar(null, Inicio.AddHours(1), "  Comida  ", 16.75m, -93.11m);

        Assert.True(pausa.EstaAbierta);
        Assert.Equal(viaje.ViajeId, pausa.ViajeId);
        Assert.Equal(viaje.EmpresaId, pausa.EmpresaId);
        Assert.Equal("Comida", pausa.Motivo);
    }

    [Fact]
    public void Pausar_YaEnPausa_LanzaDomainException()
    {
        var viaje = ViajeEnCurso();
        var pausa = viaje.Pausar(null, Inicio.AddHours(1), null, null, null);

        Assert.Throws<DomainException>(() => viaje.Pausar(pausa, Inicio.AddHours(2), null, null, null));
    }

    [Fact]
    public void Pausar_AntesDelInicioDelViaje_LanzaDomainException()
    {
        var viaje = ViajeEnCurso();

        Assert.Throws<DomainException>(() => viaje.Pausar(null, Inicio.AddMinutes(-1), null, null, null));
    }

    [Fact]
    public void Pausar_AntesDeQueTerminaraLaPausaAnterior_LanzaDomainException()
    {
        var viaje = ViajeEnCurso();
        var anterior = viaje.Pausar(null, Inicio.AddHours(1), null, null, null);
        viaje.Reanudar(anterior, Inicio.AddHours(2));

        Assert.Throws<DomainException>(() => viaje.Pausar(anterior, Inicio.AddMinutes(90), null, null, null));
    }

    [Fact]
    public void Pausar_ViajeFinalizado_LanzaDomainException()
    {
        var viaje = ViajeEnCurso();
        viaje.Finalizar(Inicio.AddHours(3), null, null, 1100m);

        Assert.Throws<DomainException>(() => viaje.Pausar(null, Inicio.AddHours(4), null, null, null));
    }

    [Fact]
    public void Pausar_PausaDeOtroViaje_LanzaDomainException()
    {
        var otro = ViajeEnCurso();
        var pausaDeOtro = otro.Pausar(null, Inicio.AddHours(1), null, null, null);
        otro.Reanudar(pausaDeOtro, Inicio.AddHours(2));

        Assert.Throws<DomainException>(() => ViajeEnCurso().Pausar(pausaDeOtro, Inicio.AddHours(3), null, null, null));
    }

    // --- Reanudar ---

    [Fact]
    public void Reanudar_CierraLaPausa_YCalculaDuracion()
    {
        var viaje = ViajeEnCurso();
        var pausa = viaje.Pausar(null, Inicio.AddHours(1), null, null, null);

        viaje.Reanudar(pausa, Inicio.AddHours(1).AddMinutes(25));

        Assert.False(pausa.EstaAbierta);
        Assert.Equal(TimeSpan.FromMinutes(25), pausa.Duracion(DateTime.UtcNow));
    }

    [Fact]
    public void Reanudar_SinPausaAbierta_LanzaDomainException()
    {
        var viaje = ViajeEnCurso();

        Assert.Throws<DomainException>(() => viaje.Reanudar(null, Inicio.AddHours(1)));
    }

    [Fact]
    public void Reanudar_AntesDeQueEmpezaraLaPausa_LanzaDomainException()
    {
        var viaje = ViajeEnCurso();
        var pausa = viaje.Pausar(null, Inicio.AddHours(1), null, null, null);

        Assert.Throws<DomainException>(() => viaje.Reanudar(pausa, Inicio.AddMinutes(30)));
    }

    // --- Cerrar el viaje estando en pausa ---

    [Fact]
    public void Finalizar_EnPausa_CierraLaPausaALaHoraDeFin()
    {
        var viaje = ViajeEnCurso();
        var pausa = viaje.Pausar(null, Inicio.AddHours(1), null, null, null);
        var fin = Inicio.AddHours(2);

        viaje.Finalizar(fin, null, null, 1100m, pausa);

        Assert.Equal(EstadoViaje.Finalizado, viaje.Estado);
        Assert.Equal(fin, pausa.FechaFin);
    }

    [Fact]
    public void Interrumpir_EnPausa_CierraLaPausa()
    {
        var viaje = ViajeEnCurso();
        var pausa = viaje.Pausar(null, Inicio.AddHours(1), null, null, null);

        viaje.Interrumpir("Falla mecánica", Inicio.AddHours(2), null, null, null, pausa);

        Assert.False(pausa.EstaAbierta);
    }

    [Fact]
    public void Finalizar_AntesDelInicioDeLaPausa_LanzaDomainException()
    {
        var viaje = ViajeEnCurso();
        var pausa = viaje.Pausar(null, Inicio.AddHours(2), null, null, null);

        Assert.Throws<DomainException>(() => viaje.Finalizar(Inicio.AddHours(1), null, null, 1100m, pausa));
    }

    // --- Contiene (puntos GPS en pausa) ---

    [Fact]
    public void Contiene_PuntoDentroYFuera()
    {
        var viaje = ViajeEnCurso();
        var pausa = viaje.Pausar(null, Inicio.AddHours(1), null, null, null);
        viaje.Reanudar(pausa, Inicio.AddHours(2));

        Assert.True(pausa.Contiene(Inicio.AddMinutes(90)));
        Assert.False(pausa.Contiene(Inicio.AddMinutes(30)));
        Assert.False(pausa.Contiene(Inicio.AddHours(3)));
    }

    // --- Sincronización sin conexión ---

    [Fact]
    public void Sincronizadas_Validas_SeCreanEnOrden()
    {
        var viaje = ViajeEnCurso();
        viaje.Finalizar(Inicio.AddHours(5), null, null, 1200m);

        var pausas = viaje.RegistrarPausasSincronizadas(
        [
            new DatosPausa(Inicio.AddHours(3), Inicio.AddHours(3.5), "Carga", null, null),
            new DatosPausa(Inicio.AddHours(1), Inicio.AddHours(1.5), "Comida", null, null)
        ]);

        Assert.Equal(2, pausas.Count);
        Assert.Equal("Comida", pausas[0].Motivo);
    }

    [Fact]
    public void Sincronizadas_ViajeCerradoConPausaSinFin_SeCierraALaHoraDeFin()
    {
        var viaje = ViajeEnCurso();
        var fin = Inicio.AddHours(5);
        viaje.Finalizar(fin, null, null, 1200m);

        var pausas = viaje.RegistrarPausasSincronizadas([new DatosPausa(Inicio.AddHours(4), null, null, null, null)]);

        Assert.Equal(fin, pausas[0].FechaFin);
    }

    [Fact]
    public void Sincronizadas_Encimadas_LanzaDomainException()
    {
        var viaje = ViajeEnCurso();

        Assert.Throws<DomainException>(() => viaje.RegistrarPausasSincronizadas(
        [
            new DatosPausa(Inicio.AddHours(1), Inicio.AddHours(2), null, null, null),
            new DatosPausa(Inicio.AddHours(1.5), Inicio.AddHours(3), null, null, null)
        ]));
    }

    [Fact]
    public void Sincronizadas_PausaAbiertaQueNoEsLaUltima_LanzaDomainException()
    {
        var viaje = ViajeEnCurso();

        Assert.Throws<DomainException>(() => viaje.RegistrarPausasSincronizadas(
        [
            new DatosPausa(Inicio.AddHours(1), null, null, null, null),
            new DatosPausa(Inicio.AddHours(2), Inicio.AddHours(3), null, null, null)
        ]));
    }

    [Fact]
    public void Sincronizadas_FueraDelViaje_LanzaDomainException()
    {
        var viaje = ViajeEnCurso();
        viaje.Finalizar(Inicio.AddHours(2), null, null, 1100m);

        Assert.Throws<DomainException>(() => viaje.RegistrarPausasSincronizadas(
            [new DatosPausa(Inicio.AddHours(1), Inicio.AddHours(3), null, null, null)]));
    }
}
