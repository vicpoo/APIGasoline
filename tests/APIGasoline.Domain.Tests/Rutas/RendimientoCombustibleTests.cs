using APIGasoline.Domain.Rutas;

namespace APIGasoline.Domain.Tests.Rutas;

/// <summary>
/// Rendimiento del viaje: km (GPS) / litros cargados, con 2 decimales. Sin cargas o sin distancia no hay rendimiento.
/// </summary>
public class RendimientoCombustibleTests
{
    [Theory]
    [InlineData(300, 25, 12)]
    [InlineData(100, 3, 33.33)]   // 33.333... se redondea
    public void KmPorLitro_DistanciaYLitros_CalculaElRendimiento(decimal km, decimal litros, decimal esperado)
    {
        Assert.Equal(esperado, RendimientoCombustible.KmPorLitro(km, litros));
    }

    [Theory]
    [InlineData(300, 0)]   // el viaje no tuvo cargas
    [InlineData(0, 25)]    // el GPS no registró distancia
    public void KmPorLitro_SinLitrosOSinDistancia_EsNull(decimal km, decimal litros)
    {
        Assert.Null(RendimientoCombustible.KmPorLitro(km, litros));
    }

    [Fact]
    public void KmPorLitro_ValorFueraDeLaColumna_SeLimitaAlMaximo()
    {
        Assert.Equal(RendimientoCombustible.RendimientoMaximo, RendimientoCombustible.KmPorLitro(500m, 0.01m));
    }
}
