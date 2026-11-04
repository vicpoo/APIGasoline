using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Features.Viajes.Common;
using APIGasoline.Domain.Entities;

namespace APIGasoline.Application.Tests.Features.Viajes.Common;

/// <summary>
/// Offline-first: la app reenvía el viaje o la carga con el mismo UUID cuando no le llegó la respuesta.
/// Un reintento del mismo registro se acepta (se regresa el existente); un UUID ocupado por otro, no.
/// </summary>
public class ReintentoRegistroTests
{
    private static readonly DateTime Inicio = new(2026, 11, 4, 17, 40, 0, DateTimeKind.Utc);
    private static readonly Guid IdTelefono = Guid.Parse("3f2b8c1e-9d4a-4e6b-8f1a-2c7d5e9b0a11");

    // --- Viaje ---

    [Fact]
    public void Viaje_MismoUuidMismosDatos_EsReintento()
    {
        var existente = ConstruirViaje(empresa: 1, carro: 2, operador: 3);

        var ex = Record.Exception(() => ReintentoRegistro.ValidarViaje(existente, Id(1), Id(2), Id(3)));

        Assert.Null(ex);
    }

    [Theory]
    [InlineData(9, 2, 3)] // otra empresa
    [InlineData(1, 9, 3)] // otro carro
    [InlineData(1, 2, 9)] // otro operador
    public void Viaje_MismoUuidOtrosDatos_LanzaIdDuplicado(int empresa, int carro, int operador)
    {
        var existente = ConstruirViaje(empresa: 1, carro: 2, operador: 3);

        var ex = Assert.Throws<ConflictException>(() =>
            ReintentoRegistro.ValidarViaje(existente, Id(empresa), Id(carro), Id(operador)));

        Assert.Equal(CodigosConflicto.IdDuplicado, ex.Codigo);
        Assert.Equal(IdTelefono, ex.Datos!["id"]);
    }

    // --- Carga ---

    [Fact]
    public void Carga_MismoUuidMismoViaje_EsReintento()
    {
        var existente = ConstruirCarga(viaje: 5);

        var ex = Record.Exception(() => ReintentoRegistro.ValidarCarga(existente, Id(5)));

        Assert.Null(ex);
    }

    [Fact]
    public void Carga_MismoUuidOtroViaje_LanzaIdDuplicado()
    {
        var existente = ConstruirCarga(viaje: 5);

        var ex = Assert.Throws<ConflictException>(() => ReintentoRegistro.ValidarCarga(existente, Id(6)));

        Assert.Equal(CodigosConflicto.IdDuplicado, ex.Codigo);
    }

    private static Viaje ConstruirViaje(int empresa, int carro, int operador)
        => Viaje.Iniciar(Id(empresa), Id(carro), null, Id(operador), Inicio, null, null, 15480m, viajeId: IdTelefono);

    private static CargaCombustible ConstruirCarga(int viaje)
        => CargaCombustible.Crear(Id(1), Id(viaje), 40m, 980m, "empresa/1/viaje/5/carga/1/comprobante.jpg",
            Inicio.AddMinutes(20), null, null, cargaCombustibleId: IdTelefono);
}
