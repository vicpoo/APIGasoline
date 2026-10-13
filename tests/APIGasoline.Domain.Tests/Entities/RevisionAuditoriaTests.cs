using APIGasoline.Domain.Entities;
using APIGasoline.Domain.Enums;
using APIGasoline.Domain.Exceptions;

namespace APIGasoline.Domain.Tests.Entities;

/// <summary>
/// Revisión de auditoría: alcance de un viaje o de un carro en un periodo;
/// nace pendiente de información; no se cierra pendiente; cerrada ya no cambia.
/// </summary>
public class RevisionAuditoriaTests
{
    private static readonly DateOnly Desde = new(2026, 9, 1);
    private static readonly DateOnly Hasta = new(2026, 9, 30);
    private static readonly DateTime Ahora = new(2026, 10, 13, 9, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void DeViaje_QuedaPendienteDeInformacion()
    {
        var revision = RevisionAuditoria.DeViaje(Id(1), Id(5), Id(2), null);

        Assert.Equal(Id(5), revision.ViajeId);
        Assert.Null(revision.CarroId);
        Assert.Equal(EstadoRevision.PendienteInformacion, revision.EstadoRevision);
        Assert.False(revision.EstaCerrada);
    }

    [Fact]
    public void DeCarroEnPeriodo_GuardaElAlcance()
    {
        var revision = RevisionAuditoria.DeCarroEnPeriodo(Id(1), Id(7), Desde, Hasta, Id(2), "Gasto de septiembre");

        Assert.Equal(Id(7), revision.CarroId);
        Assert.Equal(Desde, revision.PeriodoDesde);
        Assert.Equal(Hasta, revision.PeriodoHasta);
        Assert.Null(revision.ViajeId);
    }

    [Fact]
    public void DeCarroEnPeriodo_PeriodoInvertido_LanzaDomainException()
    {
        Assert.Throws<DomainException>(() => RevisionAuditoria.DeCarroEnPeriodo(Id(1), Id(7), Hasta, Desde, Id(2), null));
    }

    [Fact]
    public void Registrar_ConObservacionesSinTexto_LanzaDomainException()
    {
        var revision = RevisionAuditoria.DeViaje(Id(1), Id(5), Id(2), null);

        Assert.Throws<DomainException>(() => revision.Registrar(EstadoRevision.ConObservaciones, "  "));
    }

    [Fact]
    public void Cerrar_PendienteDeInformacion_LanzaDomainException()
    {
        var revision = RevisionAuditoria.DeViaje(Id(1), Id(5), Id(2), null);

        Assert.Throws<DomainException>(() => revision.Cerrar(Ahora));
    }

    [Fact]
    public void Cerrar_Revisada_QuedaCerradaYYaNoCambia()
    {
        var revision = RevisionAuditoria.DeViaje(Id(1), Id(5), Id(2), null);
        revision.Registrar(EstadoRevision.Revisado, "Comprobantes completos");

        revision.Cerrar(Ahora);

        Assert.True(revision.EstaCerrada);
        Assert.Equal(Ahora, revision.FechaCierre);
        Assert.Throws<DomainException>(() => revision.Registrar(EstadoRevision.ConObservaciones, "Otra cosa"));
    }
}
