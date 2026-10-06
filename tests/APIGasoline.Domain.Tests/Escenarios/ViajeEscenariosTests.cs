using APIGasoline.Domain.Entities;
using APIGasoline.Domain.Enums;

namespace APIGasoline.Domain.Tests.Escenarios;

/// <summary>
/// Escenarios de punta a punta de un viaje y el kilometraje del carro, componiendo las entidades reales
/// como lo hacen los handlers de Finalizar, Interrumpir, Sincronizar y CambiarEstatusCarro.
/// Criterio clave (recencia): el kilometraje del carro solo avanza, aunque los viajes offline lleguen en desorden.
/// </summary>
public class ViajeEscenariosTests
{
    private static readonly DateTime Dia4 = new(2026, 10, 4, 9, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime Dia5 = new(2026, 10, 5, 8, 30, 0, DateTimeKind.Utc);

    // --- Viaje normal: iniciar → finalizar → el carro avanza su kilometraje ---

    [Fact]
    public void ViajeCompleto_Finalizar_ElCarroQuedaConElKilometrajeFinal()
    {
        var carro = ConstruirCarro(kilometraje: 15480m);
        var viaje = Viaje.Iniciar(1, 1, 1, 1, Dia5, null, null, carro.KilometrajeActual);

        viaje.Finalizar(Dia5.AddHours(3), null, null, 15775.40m);
        carro.AvanzarKilometraje(viaje.KilometrajeFinal!.Value);

        Assert.Equal(EstadoViaje.Finalizado, viaje.Estado);
        Assert.Equal(295.40m, viaje.DistanciaRecorrida);
        Assert.Equal(15775.40m, carro.KilometrajeActual);
    }

    // --- Recencia: viajes offline sincronizados en desorden ---

    [Fact]
    public void ViajesOffline_ElMasRecienteLlegaPrimero_ElKilometrajeNoRetrocede()
    {
        var carro = ConstruirCarro(kilometraje: 15000m);
        var viajeDia4 = Viaje.Iniciar(1, 1, 1, 1, Dia4, null, null, 15000m);
        var viajeDia5 = Viaje.Iniciar(1, 1, 1, 1, Dia5, null, null, 15300m);

        // El teléfono sube primero el viaje del día 5 y después el del día 4
        viajeDia5.Finalizar(Dia5.AddHours(2), null, null, 15500m);
        viajeDia5.MarcarSincronizado();
        carro.AvanzarKilometraje(viajeDia5.KilometrajeFinal!.Value);

        viajeDia4.Finalizar(Dia4.AddHours(1), null, null, 15300m);
        viajeDia4.MarcarSincronizado();
        carro.AvanzarKilometraje(viajeDia4.KilometrajeFinal!.Value);

        Assert.Equal(15500m, carro.KilometrajeActual);
        Assert.True(viajeDia4.Sincronizado);
        Assert.Equal(300m, viajeDia4.DistanciaRecorrida);
    }

    [Fact]
    public void ViajesOffline_LleganEnOrden_ElKilometrajeTerminaIgual()
    {
        var carro = ConstruirCarro(kilometraje: 15000m);
        var viajeDia4 = Viaje.Iniciar(1, 1, 1, 1, Dia4, null, null, 15000m);
        var viajeDia5 = Viaje.Iniciar(1, 1, 1, 1, Dia5, null, null, 15300m);

        viajeDia4.Finalizar(Dia4.AddHours(1), null, null, 15300m);
        carro.AvanzarKilometraje(viajeDia4.KilometrajeFinal!.Value);
        viajeDia5.Finalizar(Dia5.AddHours(2), null, null, 15500m);
        carro.AvanzarKilometraje(viajeDia5.KilometrajeFinal!.Value);

        Assert.Equal(15500m, carro.KilometrajeActual);
    }

    // --- Falla en carretera: mantenimiento interrumpe el viaje en curso ---

    [Fact]
    public void FallaDuranteElViaje_MantenimientoInterrumpeElViajeYDejaBitacora()
    {
        var carro = ConstruirCarro(EstadoCarro.Ocupado, kilometraje: 15480m);
        var viaje = Viaje.Iniciar(1, 1, 1, 1, Dia5, null, null, 15480m);

        var cambio = CambioEstatusCarro.Registrar(carro, 1, EstadoCarro.EnMantenimiento, "Se sobrecalentó el motor", viajeAfectadoId: 10);
        viaje.Interrumpir("Se sobrecalentó el motor", Dia5.AddHours(1), 16.5m, -93.0m, 15520m);
        carro.AvanzarKilometraje(viaje.KilometrajeFinal!.Value);

        Assert.Equal(EstadoViaje.Interrumpido, viaje.Estado);
        Assert.Equal(EstadoCarro.EnMantenimiento, carro.Estado);
        Assert.Equal(EstadoCarro.Ocupado, cambio.EstatusAnterior);
        Assert.Equal(15520m, carro.KilometrajeActual);
    }

    [Fact]
    public void FallaDuranteElViaje_TerminaElMantenimiento_ElCarroVuelveADisponible()
    {
        var carro = ConstruirCarro(EstadoCarro.Ocupado);
        CambioEstatusCarro.Registrar(carro, 1, EstadoCarro.EnMantenimiento, "Ponchadura");

        CambioEstatusCarro.Registrar(carro, 2, EstadoCarro.Disponible, "Mantenimiento terminado");

        Assert.True(carro.EstaDisponible);
    }

    // --- Interrupción sin kilometraje: el carro conserva el que tenía ---

    [Fact]
    public void InterrumpirSinKilometraje_ElCarroConservaSuKilometraje()
    {
        var carro = ConstruirCarro(kilometraje: 15480m);
        var viaje = Viaje.Iniciar(1, 1, 1, 1, Dia5, null, null, 15480m);

        viaje.Interrumpir("Cancelado por el cliente", Dia5.AddMinutes(10), null, null, null);

        Assert.Null(viaje.KilometrajeFinal);
        Assert.Equal(15480m, carro.KilometrajeActual);
    }

    private static Carro ConstruirCarro(EstadoCarro estado = EstadoCarro.Disponible, decimal kilometraje = 15000m)
        => Carro.Crear(1, "Nissan", "NP300", 2023, "Blanco", "DFG-123-A", null, 80m, kilometraje, 2026, estado);
}
