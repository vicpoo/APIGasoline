using APIGasoline.Domain.Entities;
using APIGasoline.Domain.Enums;
using APIGasoline.Domain.Exceptions;

namespace APIGasoline.Domain.Tests.Entities;

/// <summary>
/// Historial de cambios: registra quién, qué, cuándo y por qué. Solo se crea; no tiene métodos para editarse.
/// </summary>
public class AuditoriaEventoTests
{
    [Fact]
    public void Registrar_DatosValidos_GuardaElEvento()
    {
        var evento = AuditoriaEvento.Registrar(
            Id(1), Id(2), AccionAuditoria.Corregir, " carga_combustible ", Id(3),
            "{\"litros\":35}", "{\"litros\":32}", "  Escribí mal los litros  ", OrigenAuditoria.Movil);

        Assert.Equal(Id(1), evento.EmpresaId);
        Assert.Equal(Id(2), evento.UsuarioId);
        Assert.Equal(AccionAuditoria.Corregir, evento.Accion);
        Assert.Equal("carga_combustible", evento.Entidad);
        Assert.Equal(Id(3), evento.EntidadId);
        Assert.Equal("{\"litros\":35}", evento.ValoresAnteriores);
        Assert.Equal("{\"litros\":32}", evento.ValoresNuevos);
        Assert.Equal("Escribí mal los litros", evento.Motivo);
        Assert.Equal(OrigenAuditoria.Movil, evento.Origen);
    }

    [Fact]
    public void Registrar_SinUsuarioNiEmpresa_EsUnProcesoDelSistema()
    {
        var evento = AuditoriaEvento.Registrar(null, null, AccionAuditoria.Crear, "empresa", Id(1), null, "{}", null, OrigenAuditoria.Sistema);

        Assert.Null(evento.EmpresaId);
        Assert.Null(evento.UsuarioId);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Registrar_SinEntidad_LanzaDomainException(string entidad)
    {
        Assert.Throws<DomainException>(() =>
            AuditoriaEvento.Registrar(Id(1), Id(1), AccionAuditoria.Actualizar, entidad, Id(1), null, null, null, OrigenAuditoria.Web));
    }

    [Fact]
    public void Registrar_SinIdDelRegistro_LanzaDomainException()
    {
        Assert.Throws<DomainException>(() =>
            AuditoriaEvento.Registrar(Id(1), Id(1), AccionAuditoria.Actualizar, "carro", Guid.Empty, null, null, null, OrigenAuditoria.Web));
    }

    [Fact]
    public void Registrar_MotivoMuyLargo_SeRecortaEnLugarDePerderElEvento()
    {
        var evento = AuditoriaEvento.Registrar(Id(1), Id(1), AccionAuditoria.Corregir, "viaje", Id(1), null, null, new string('x', 600), OrigenAuditoria.Web);

        Assert.Equal(AuditoriaEvento.MotivoMaxLength, evento.Motivo!.Length);
        Assert.EndsWith("...", evento.Motivo);
    }
}
