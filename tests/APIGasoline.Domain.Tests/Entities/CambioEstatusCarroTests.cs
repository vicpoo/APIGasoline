using APIGasoline.Domain.Entities;
using APIGasoline.Domain.Enums;
using APIGasoline.Domain.Exceptions;

namespace APIGasoline.Domain.Tests.Entities;

/// <summary>
/// Bitácora de estatus del carro: cada registro aplica la transición sobre el carro y guarda el estatus anterior.
/// Mandar a mantenimiento exige motivo; no se registra un cambio al mismo estatus.
/// </summary>
public class CambioEstatusCarroTests
{
    private static readonly DateTime FechaCambio = new(2026, 10, 5, 13, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Registrar_MantenimientoConMotivo_CambiaElCarroYGuardaElAnterior()
    {
        var carro = ConstruirCarro();

        var cambio = CambioEstatusCarro.Registrar(carro, 2, EstadoCarro.EnMantenimiento, "  Servicio de los 20,000 km  ", fechaCambio: FechaCambio);

        Assert.Equal(EstadoCarro.EnMantenimiento, carro.Estado);
        Assert.Equal(EstadoCarro.Disponible, cambio.EstatusAnterior);
        Assert.Equal(EstadoCarro.EnMantenimiento, cambio.EstatusNuevo);
        Assert.Equal("Servicio de los 20,000 km", cambio.Motivo);
        Assert.Equal(carro.EmpresaId, cambio.EmpresaId);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("   ")]
    public void Registrar_MantenimientoSinMotivo_LanzaDomainException(string? motivo)
    {
        var carro = ConstruirCarro();

        Assert.Throws<DomainException>(() => CambioEstatusCarro.Registrar(carro, 2, EstadoCarro.EnMantenimiento, motivo));
    }

    [Fact]
    public void Registrar_DisponibleSinMotivo_SeAcepta()
    {
        var carro = ConstruirCarro(EstadoCarro.EnMantenimiento);

        var cambio = CambioEstatusCarro.Registrar(carro, 2, EstadoCarro.Disponible, null);

        Assert.Equal(EstadoCarro.Disponible, carro.Estado);
        Assert.Null(cambio.Motivo);
    }

    [Fact]
    public void Registrar_MismoEstatus_LanzaDomainException()
    {
        var carro = ConstruirCarro();

        Assert.Throws<DomainException>(() => CambioEstatusCarro.Registrar(carro, 2, EstadoCarro.Disponible, null));
    }

    [Fact]
    public void Registrar_OcupadoConCarroEnMantenimiento_LanzaDomainException()
    {
        var carro = ConstruirCarro(EstadoCarro.EnMantenimiento);

        Assert.Throws<DomainException>(() => CambioEstatusCarro.Registrar(carro, 2, EstadoCarro.Ocupado, null));
    }

    [Fact]
    public void Registrar_OcupadoConCarroDisponible_UsaLaReglaDeOcupar()
    {
        var carro = ConstruirCarro();

        CambioEstatusCarro.Registrar(carro, 2, EstadoCarro.Ocupado, null);

        Assert.Equal(EstadoCarro.Ocupado, carro.Estado);
    }

    [Fact]
    public void Registrar_ConViajeAfectado_LoGuarda()
    {
        var carro = ConstruirCarro(EstadoCarro.Ocupado);

        var cambio = CambioEstatusCarro.Registrar(carro, 1, EstadoCarro.EnMantenimiento, "Se sobrecalentó el motor", viajeAfectadoId: 7);

        Assert.Equal(7, cambio.ViajeAfectadoId);
    }

    [Theory]
    [InlineData(0, null)]
    [InlineData(2, 0)]  // viaje afectado con id inválido
    public void Registrar_IdsInvalidos_LanzaDomainException(int empleadoId, int? viajeAfectadoId)
    {
        var carro = ConstruirCarro();

        Assert.Throws<DomainException>(() => CambioEstatusCarro.Registrar(carro, empleadoId, EstadoCarro.EnMantenimiento, "Servicio", viajeAfectadoId));
    }

    [Fact]
    public void Registrar_MotivoMayorA255_LanzaDomainException()
    {
        var carro = ConstruirCarro();

        Assert.Throws<DomainException>(() => CambioEstatusCarro.Registrar(carro, 2, EstadoCarro.EnMantenimiento, new string('x', 256)));
    }

    [Fact]
    public void Registrar_SinCarro_LanzaArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => CambioEstatusCarro.Registrar(null!, 2, EstadoCarro.EnMantenimiento, "Servicio"));
    }

    private static Carro ConstruirCarro(EstadoCarro estado = EstadoCarro.Disponible)
        => Carro.Crear(1, "Nissan", "NP300", 2023, "Blanco", "DFG-123-A", null, 80m, 15000m, 2026, estado);
}
