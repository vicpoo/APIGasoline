using APIGasoline.Application.Features.CargasCombustible.Common;

namespace APIGasoline.Application.Tests.Features.CargasCombustible.Common;

/// <summary>
/// Una carga no puede superar la capacidad del tanque del carro; llenarlo exactamente sí se permite.
/// Si el carro no tiene capacidad registrada, la regla no aplica.
/// </summary>
public class ReglaCapacidadTanqueTests
{
    [Theory]
    [InlineData(45.30, 80)]
    [InlineData(80, 80)]       // tanque lleno exacto: permitido
    [InlineData(0.01, 80)]
    public void Excede_LitrosDentroDeLaCapacidad_DevuelveFalse(decimal litros, decimal capacidad)
    {
        Assert.False(ReglaCapacidadTanque.Excede(litros, capacidad));
    }

    [Theory]
    [InlineData(80.01, 80)]   // un centésimo arriba ya excede
    [InlineData(500, 80)]
    public void Excede_LitrosMayoresALaCapacidad_DevuelveTrue(decimal litros, decimal capacidad)
    {
        Assert.True(ReglaCapacidadTanque.Excede(litros, capacidad));
    }

    [Fact]
    public void Excede_CarroSinCapacidadRegistrada_DevuelveFalse()
    {
        Assert.False(ReglaCapacidadTanque.Excede(500m, null));
    }
}
