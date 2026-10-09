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

    // --- Campos opcionales del GPS (precisión, altitud y velocidad) ---

    [Fact]
    public void Crear_SinCamposOpcionales_QuedanNulos()
    {
        var punto = ConstruirPunto();

        Assert.Null(punto.PrecisionMetros);
        Assert.Null(punto.Altitud);
        Assert.Null(punto.Velocidad);
    }

    [Fact]
    public void Crear_ConCamposOpcionales_SeRedondeanA2Decimales()
    {
        var punto = PuntoRuta.Crear(Id(1), Id(1), 16.753056m, -93.115556m, Captura, false,
            precisionMetros: 4.567m, altitud: 532.126m, velocidad: 9.999m);

        Assert.Equal(4.57m, punto.PrecisionMetros);
        Assert.Equal(532.13m, punto.Altitud);
        Assert.Equal(10.00m, punto.Velocidad);
    }

    [Theory]
    [InlineData(-0.01)]
    [InlineData(10000)]
    public void Crear_PrecisionFueraDeRango_LanzaDomainException(decimal precision)
    {
        Assert.Throws<DomainException>(() => PuntoRuta.Crear(Id(1), Id(1), 16.75m, -93.11m, Captura, false, precisionMetros: precision));
    }

    [Theory]
    [InlineData(-500.01)]
    [InlineData(9000.01)]
    public void Crear_AltitudFueraDeRango_LanzaDomainException(decimal altitud)
    {
        Assert.Throws<DomainException>(() => PuntoRuta.Crear(Id(1), Id(1), 16.75m, -93.11m, Captura, false, altitud: altitud));
    }

    [Theory]
    [InlineData(-500)]
    [InlineData(0)]
    [InlineData(9000)]
    public void Crear_AltitudEnElBorde_SeAcepta(decimal altitud)
    {
        var punto = PuntoRuta.Crear(Id(1), Id(1), 16.75m, -93.11m, Captura, false, altitud: altitud);

        Assert.Equal(altitud, punto.Altitud);
    }

    [Fact]
    public void Crear_VelocidadNegativa_LanzaDomainException()
    {
        Assert.Throws<DomainException>(() => PuntoRuta.Crear(Id(1), Id(1), 16.75m, -93.11m, Captura, false, velocidad: -1m));
    }

    [Fact]
    public void ToPuntoGps_ConvierteCoordenadasYCamposOpcionales()
    {
        var punto = PuntoRuta.Crear(Id(1), Id(1), 16.753056m, -93.115556m, Captura, false,
            precisionMetros: 8.5m, altitud: 530m, velocidad: 12.25m);

        var gps = punto.ToPuntoGps();

        Assert.Equal(16.753056, gps.Latitud, 6);
        Assert.Equal(-93.115556, gps.Longitud, 6);
        Assert.Equal(Captura, gps.Timestamp);
        Assert.Equal(8.5, gps.PrecisionMetros);
        Assert.Equal(12.25, gps.Velocidad);
    }

    [Fact]
    public void ToPuntoGps_SinCamposOpcionales_QuedanNulos()
    {
        var gps = ConstruirPunto().ToPuntoGps();

        Assert.Null(gps.PrecisionMetros);
        Assert.Null(gps.Velocidad);
    }

    private static PuntoRuta ConstruirPunto(
        int empresaId = 1,
        int viajeId = 1,
        decimal latitud = 16.753056m,
        decimal longitud = -93.115556m,
        DateTime? timestamp = null,
        bool sincronizado = false)
        => PuntoRuta.Crear(Id(empresaId), Id(viajeId), latitud, longitud, timestamp ?? Captura, sincronizado);
}
