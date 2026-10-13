using APIGasoline.Application.Features.Auditoria.Common;
using APIGasoline.Application.Features.Auditoria.DTOs;
using APIGasoline.Domain.Enums;

namespace APIGasoline.Application.Tests.Features.Auditoria.Common;

/// <summary>
/// Observaciones de la revisión de combustible. Se calculan al consultar; una alerta
/// no significa fraude, solo que alguien debería revisar el registro.
/// </summary>
public class DetectorAlertasRevisionTests
{
    private static readonly DateTime Dia1 = new(2026, 9, 1, 8, 0, 0, DateTimeKind.Utc);
    private static readonly Guid Carro = Id(10);

    private static readonly IReadOnlyDictionary<Guid, CarroRevision> Carros =
        new Dictionary<Guid, CarroRevision> { [Carro] = new(Carro, "DFG-123-A", 50m) };

    private static readonly IReadOnlyDictionary<Guid, decimal> SinAnterior = new Dictionary<Guid, decimal>();

    [Fact]
    public void DatosCompletosYCoherentes_NoHayAlertas()
    {
        var viajes = new[] { Viaje(1, Dia1, 15000m, 15100m), Viaje(2, Dia1.AddDays(1), 15100m, 15250m) };
        var cargas = new[] { Carga(1, 1, 40m, Dia1.AddHours(1)) };

        var alertas = DetectorAlertasRevision.Detectar(viajes, cargas, Carros, SinAnterior);

        Assert.Empty(alertas);
    }

    [Fact]
    public void CargaSinComprobante_AlertaComprobantePendiente()
    {
        var alertas = DetectorAlertasRevision.Detectar(
            [Viaje(1, Dia1, 15000m, 15100m)],
            [Carga(1, 1, 40m, Dia1.AddHours(1), comprobanteSubido: false)],
            Carros,
            SinAnterior);

        var alerta = Assert.Single(alertas);
        Assert.Equal(TipoAlertaRevision.ComprobantePendiente, alerta.Tipo);
        Assert.Equal("carga_combustible", alerta.Entidad);
        Assert.Equal("DFG-123-A", alerta.Placas);
    }

    [Fact]
    public void ViajeQueNoEmpiezaDondeTerminoElAnterior_AlertaKilometrajeInconsistente()
    {
        // El viaje 2 empieza en 15180 pero el 1 terminó en 15100: 80 km sin registrar
        var viajes = new[] { Viaje(1, Dia1, 15000m, 15100m), Viaje(2, Dia1.AddDays(1), 15180m, 15250m) };

        var alertas = DetectorAlertasRevision.Detectar(viajes, [], Carros, SinAnterior);

        var alerta = Assert.Single(alertas);
        Assert.Equal(TipoAlertaRevision.KilometrajeInconsistente, alerta.Tipo);
        Assert.Equal(Id(2), alerta.EntidadId);
    }

    [Fact]
    public void PrimerViajeDelPeriodo_SeComparaContraElUltimoAntesDelPeriodo()
    {
        var anterior = new Dictionary<Guid, decimal> { [Carro] = 14900m };

        var alertas = DetectorAlertasRevision.Detectar([Viaje(1, Dia1, 15000m, 15100m)], [], Carros, anterior);

        Assert.Equal(TipoAlertaRevision.KilometrajeInconsistente, Assert.Single(alertas).Tipo);
    }

    [Fact]
    public void ViajesQueLleganEnDesorden_SeOrdenanPorFechaAntesDeComparar()
    {
        var viajes = new[] { Viaje(2, Dia1.AddDays(1), 15100m, 15250m), Viaje(1, Dia1, 15000m, 15100m) };

        Assert.Empty(DetectorAlertasRevision.Detectar(viajes, [], Carros, SinAnterior));
    }

    [Fact]
    public void AlertaGps_CorreccionEInterrupcion_GeneranSusAlertas()
    {
        var viaje = Viaje(1, Dia1, 15000m, 15100m) with
        {
            AlertaKilometraje = true,
            DiferenciaKm = 40m,
            DiferenciaPorcentaje = 66.67m,
            Corregido = true,
            Estado = EstadoViaje.Interrumpido,
            MotivoInterrupcion = "Reporte de falla"
        };

        var tipos = DetectorAlertasRevision.Detectar([viaje], [], Carros, SinAnterior).Select(a => a.Tipo).ToList();

        Assert.Contains(TipoAlertaRevision.KilometrajeContraGps, tipos);
        Assert.Contains(TipoAlertaRevision.RegistroCorregido, tipos);
        Assert.Contains(TipoAlertaRevision.ViajeInterrumpido, tipos);
    }

    [Fact]
    public void CargaMayorAlTanque_AlertaCargaInusual()
    {
        var alertas = DetectorAlertasRevision.Detectar(
            [Viaje(1, Dia1, 15000m, 15100m)],
            [Carga(1, 1, 65m, Dia1.AddHours(1))], // el tanque es de 50 L
            Carros,
            SinAnterior);

        Assert.Equal(TipoAlertaRevision.CargaInusual, Assert.Single(alertas).Tipo);
    }

    [Fact]
    public void DosCargasIgualesConMinutosDeDiferencia_AlertaPosibleDuplicado()
    {
        var alertas = DetectorAlertasRevision.Detectar(
            [Viaje(1, Dia1, 15000m, 15100m)],
            [Carga(1, 1, 40m, Dia1.AddHours(1)), Carga(2, 1, 40m, Dia1.AddHours(1).AddMinutes(4))],
            Carros,
            SinAnterior);

        var alerta = Assert.Single(alertas);
        Assert.Equal(TipoAlertaRevision.PosibleDuplicado, alerta.Tipo);
        Assert.Equal(Id(2), alerta.EntidadId);
    }

    [Fact]
    public void DosCargasIgualesEnDiasDistintos_NoEsDuplicado()
    {
        var alertas = DetectorAlertasRevision.Detectar(
            [Viaje(1, Dia1, 15000m, 15100m)],
            [Carga(1, 1, 40m, Dia1.AddHours(1)), Carga(2, 1, 40m, Dia1.AddDays(1))],
            Carros,
            SinAnterior);

        Assert.Empty(alertas);
    }

    private static ViajeRevision Viaje(int id, DateTime inicio, decimal kmInicial, decimal? kmFinal)
        => new(Id(id), Id(1), Carro, Id(3), inicio, inicio.AddHours(2), kmInicial, kmFinal,
            EstadoViaje.Finalizado, null, false, false, null, null);

    private static CargaRevision Carga(int id, int viajeId, decimal litros, DateTime fecha, bool comprobanteSubido = true)
        => new(Id(100 + id), Id(viajeId), litros, 1000m, fecha, comprobanteSubido ? fecha : null, false);
}
