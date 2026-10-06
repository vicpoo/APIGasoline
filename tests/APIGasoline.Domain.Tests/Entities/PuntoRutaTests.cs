using APIGasoline.Domain.Entities;
using APIGasoline.Domain.Exceptions;

namespace APIGasoline.Domain.Tests.Entities;

/// <summary>
/// Reglas del punto GPS: coordenadas en rango, redondeo a 6 decimales (decimal(9,6), ~11 cm)
/// y timestamp truncado a segundos para detectar duplicados igual que la BD.
/// </summary>
public class PuntoRutaTests
{
    private static readonly DateTime Captura = new(2026, 10, 5, 8, 30, 0, DateTimeKind.Utc);

    [Fact]
    public void Crear_CoordenadasConMasDe6Decimales_SeRedondean()
    {
        var punto = ConstruirPunto(latitud: 16.7530567m, longitud: -93.1155564m);

        Assert.Equal(16.753057m, punto.Latitud);
        Assert.Equal(-93.115556m, punto.Longitud);
    }

    [Fact]
    public void Crear_TimestampConMilisegundos_SeTruncaASegundos()
    {
        var punto = ConstruirPunto(timestamp: Captura.AddMilliseconds(999));

        Assert.Equal(Captura, punto.TimestampCaptura);
    }

    [Theory]
    [InlineData(90.000001, 0)]
    [InlineData(-90.000001, 0)]
    [InlineData(0, 180.000001)]
    [InlineData(0, -180.000001)]
    public void Crear_CoordenadasFueraDeRango_LanzaDomainException(decimal latitud, decimal longitud)
    {
        Assert.Throws<DomainException>(() => ConstruirPunto(latitud: latitud, longitud: longitud));
    }

    [Theory]
    [InlineData(90, 180)]    // bordes exactos: permitidos
    [InlineData(-90, -180)]
    public void Crear_CoordenadasEnElBorde_SeAceptan(decimal latitud, decimal longitud)
    {
        var punto = ConstruirPunto(latitud: latitud, longitud: longitud);

        Assert.Equal(latitud, punto.Latitud);
    }

    [Fact]
    public void Crear_SinTimestamp_LanzaDomainException()
    {
        Assert.Throws<DomainException>(() => ConstruirPunto(timestamp: default(DateTime)));
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(1, 0)]
    public void Crear_IdsInvalidos_LanzaDomainException(int empresaId, int viajeId)
    {
        Assert.Throws<DomainException>(() => ConstruirPunto(empresaId: empresaId, viajeId: viajeId));
    }

    [Fact]
    public void TruncarASegundos_ConservaElKind()
    {
        var truncado = PuntoRuta.TruncarASegundos(new DateTime(2026, 2, 28, 23, 59, 59, 500, DateTimeKind.Utc));

        Assert.Equal(new DateTime(2026, 2, 28, 23, 59, 59, DateTimeKind.Utc), truncado);
        Assert.Equal(DateTimeKind.Utc, truncado.Kind);
    }

    private static PuntoRuta ConstruirPunto(
        int empresaId = 1,
        int viajeId = 1,
        decimal latitud = 16.753056m,
        decimal longitud = -93.115556m,
        DateTime? timestamp = null,
        bool sincronizado = false)
        => PuntoRuta.Crear(empresaId, viajeId, latitud, longitud, timestamp ?? Captura, sincronizado);
}
