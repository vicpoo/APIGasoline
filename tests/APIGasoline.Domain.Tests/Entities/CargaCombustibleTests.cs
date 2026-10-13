using APIGasoline.Domain.Entities;
using APIGasoline.Domain.Exceptions;

namespace APIGasoline.Domain.Tests.Entities;

/// <summary>
/// Reglas de la carga de combustible: litros y monto en rango, comprobante obligatorio (llave de MinIO),
/// ubicación completa, fecha truncada a segundos (como la guarda la BD) y precio por litro calculado.
/// </summary>
public class CargaCombustibleTests
{
    private static readonly DateTime FechaCarga = new(2026, 10, 5, 10, 5, 0, DateTimeKind.Utc);
    private const string Llave = "empresa/1/viaje/1/carga/1/comprobante-11111111111111111111111111111111.jpg";

    [Fact]
    public void Crear_DatosValidos_GuardaLosDatos()
    {
        var carga = ConstruirCarga();

        Assert.Equal(45.30m, carga.Litros);
        Assert.Equal(Llave, carga.FotoComprobante);
        Assert.Equal(Id(1), carga.ViajeId);
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(1, 0)]
    public void Crear_IdsInvalidos_LanzaDomainException(int empresaId, int viajeId)
    {
        Assert.Throws<DomainException>(() => ConstruirCarga(empresaId: empresaId, viajeId: viajeId));
    }

    [Fact]
    public void Crear_ConIdGeneradoAntes_UsaEseId()
    {
        var carga = CargaCombustible.Crear(Id(1), Id(1), 45.30m, null, Llave, FechaCarga, null, null, cargaCombustibleId: Id(40));

        Assert.Equal(Id(40), carga.CargaCombustibleId);
    }

    [Fact]
    public void Crear_SinId_GeneraUnoNuevo()
    {
        var primera = ConstruirCarga();
        var segunda = ConstruirCarga();

        Assert.NotEqual(Guid.Empty, primera.CargaCombustibleId);
        Assert.NotEqual(primera.CargaCombustibleId, segunda.CargaCombustibleId);
    }

    [Fact]
    public void Crear_ConIdVacio_LanzaDomainException()
    {
        Assert.Throws<DomainException>(() =>
            CargaCombustible.Crear(Id(1), Id(1), 45.30m, null, Llave, FechaCarga, null, null, cargaCombustibleId: Guid.Empty));
    }

    // --- Litros y monto ---

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(1000000)]  // máximo 999999.99 (decimal(8,2))
    public void Crear_LitrosFueraDeRango_LanzaDomainException(decimal litros)
    {
        Assert.Throws<DomainException>(() => ConstruirCarga(litros: litros));
    }

    [Fact]
    public void Crear_MontoNegativo_LanzaDomainException()
    {
        Assert.Throws<DomainException>(() => ConstruirCarga(monto: -0.01m));
    }

    // --- Precio por litro ---

    [Theory]
    [InlineData(45.30, 1087.20, 24.00)]
    [InlineData(3, 100, 33.33)]   // 33.333... se redondea a 2 decimales
    [InlineData(3, 200, 66.67)]   // 66.666... se redondea hacia arriba
    public void PrecioPorLitro_SeCalculaConDosDecimales(decimal litros, decimal monto, decimal esperado)
    {
        var carga = ConstruirCarga(litros: litros, monto: monto);

        Assert.Equal(esperado, carga.PrecioPorLitro);
    }

    [Fact]
    public void PrecioPorLitro_SinMonto_EsNull()
    {
        var carga = ConstruirCarga(monto: null);

        Assert.Null(carga.PrecioPorLitro);
    }

    // --- Comprobante ---

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Crear_SinComprobante_LanzaDomainException(string llave)
    {
        Assert.Throws<DomainException>(() => ConstruirCarga(foto: llave));
    }

    [Fact]
    public void Crear_LlaveMayorA255_LanzaDomainException()
    {
        Assert.Throws<DomainException>(() => ConstruirCarga(foto: new string('x', 256)));
    }

    [Fact]
    public void Crear_LlaveConEspacios_SeRecorta()
    {
        var carga = ConstruirCarga(foto: "  " + Llave + "  ");

        Assert.Equal(Llave, carga.FotoComprobante);
    }

    // --- Fecha ---

    [Fact]
    public void Crear_SinFecha_LanzaDomainException()
    {
        Assert.Throws<DomainException>(() => ConstruirCarga(fecha: default(DateTime)));
    }

    [Fact]
    public void Crear_FechaConMilisegundos_SeTruncaASegundosYConservaElKind()
    {
        var fecha = new DateTime(2026, 10, 5, 10, 5, 30, 789, DateTimeKind.Utc);

        var carga = ConstruirCarga(fecha: fecha);

        Assert.Equal(new DateTime(2026, 10, 5, 10, 5, 30, DateTimeKind.Utc), carga.FechaCarga);
        Assert.Equal(DateTimeKind.Utc, carga.FechaCarga.Kind);
    }

    // --- Ubicación ---

    [Fact]
    public void Crear_LatitudSinLongitud_LanzaDomainException()
    {
        Assert.Throws<DomainException>(() => ConstruirCarga(lat: 16.75m, lng: null));
    }

    [Fact]
    public void Crear_SinUbicacion_SeAcepta()
    {
        var carga = ConstruirCarga(lat: null, lng: null);

        Assert.Null(carga.UbicacionLat);
        Assert.Null(carga.UbicacionLng);
    }

    [Theory]
    [InlineData(-90.01, 0)]
    [InlineData(0, 180.01)]
    public void Crear_UbicacionFueraDeRango_LanzaDomainException(decimal lat, decimal lng)
    {
        Assert.Throws<DomainException>(() => ConstruirCarga(lat: lat, lng: lng));
    }

    // --- Auditoría ---

    [Fact]
    public void Crear_SinMarcarSubida_ComprobantePendienteYNoCorregida()
    {
        var carga = ConstruirCarga();

        Assert.Null(carga.FechaSubidaComprobante);
        Assert.False(carga.Corregido);
    }

    [Fact]
    public void MarcarComprobanteSubido_GuardaLaFecha()
    {
        var carga = ConstruirCarga();
        var fecha = FechaCarga.AddMinutes(3);

        carga.MarcarComprobanteSubido(fecha);

        Assert.Equal(fecha, carga.FechaSubidaComprobante);
    }

    [Fact]
    public void MarcarCorregida_QuedaCorregida()
    {
        var carga = ConstruirCarga();

        carga.MarcarCorregida();

        Assert.True(carga.Corregido);
    }

    private static CargaCombustible ConstruirCarga(
        int empresaId = 1,
        int viajeId = 1,
        decimal litros = 45.30m,
        decimal? monto = 1087.20m,
        string foto = Llave,
        DateTime? fecha = null,
        decimal? lat = 16.750120m,
        decimal? lng = -93.100340m)
        => CargaCombustible.Crear(Id(empresaId), Id(viajeId), litros, monto, foto, fecha ?? FechaCarga, lat, lng);
}
