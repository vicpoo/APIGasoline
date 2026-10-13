using APIGasoline.Domain.Entities;
using APIGasoline.Domain.Enums;
using APIGasoline.Domain.Exceptions;

namespace APIGasoline.Domain.Tests.Entities;

/// <summary>
/// Reglas del carro: placas en mayúsculas, rango del año (se permite el modelo del año siguiente),
/// capacidad del tanque, kilometraje que nunca baja y transiciones de estado (ocupar / liberar).
/// </summary>
public class CarroTests
{
    private const int AnioActual = 2026;

    // --- Placas ---

    [Fact]
    public void Crear_PlacasEnMinusculas_SeGuardanEnMayusculas()
    {
        var carro = ConstruirCarro(placas: " dfg-123-a ");

        Assert.Equal("DFG-123-A", carro.Placas);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("ABCDEFGHIJKLMNOPQRSTU")]  // 21 caracteres: excede el máximo de 20
    public void Crear_PlacasInvalidas_LanzaDomainException(string placas)
    {
        Assert.Throws<DomainException>(() => ConstruirCarro(placas: placas));
    }

    // --- Datos generales ---

    [Theory]
    [InlineData("", "NP300")]
    [InlineData("Nissan", " ")]
    public void Crear_MarcaOModeloVacio_LanzaDomainException(string marca, string modelo)
    {
        Assert.Throws<DomainException>(() => ConstruirCarro(marca: marca, modelo: modelo));
    }

    [Fact]
    public void Crear_ColorEnBlanco_SeGuardaComoNull()
    {
        var carro = ConstruirCarro(color: "   ");

        Assert.Null(carro.Color);
    }

    [Fact]
    public void Crear_EstadoPorDefecto_EsDisponible()
    {
        var carro = ConstruirCarro();

        Assert.Equal(EstadoCarro.Disponible, carro.Estado);
        Assert.True(carro.EstaDisponible);
    }

    // --- Año ---

    [Theory]
    [InlineData(1900)]
    [InlineData(2026)]
    [InlineData(2027)]  // modelo del año siguiente: permitido
    public void Crear_AnioDentroDelRango_SeAcepta(int anio)
    {
        var carro = ConstruirCarro(anio: anio);

        Assert.Equal(anio, carro.Anio);
    }

    [Theory]
    [InlineData(1899)]
    [InlineData(2028)]  // dos años adelante del actual
    public void Crear_AnioFueraDelRango_LanzaDomainException(int anio)
    {
        Assert.Throws<DomainException>(() => ConstruirCarro(anio: anio));
    }

    [Fact]
    public void CambiarDatos_AnioMaximoDependeDelAnioActualRecibido()
    {
        var carro = ConstruirCarro();

        carro.CambiarDatos("Nissan", "NP300", 2031, "Blanco", anioActual: 2030);

        Assert.Equal(2031, carro.Anio);
    }

    // --- Capacidad del tanque ---

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    [InlineData(10000)]  // máximo 9999.99 (decimal(6,2))
    public void Crear_CapacidadTanqueInvalida_LanzaDomainException(decimal capacidad)
    {
        Assert.Throws<DomainException>(() => ConstruirCarro(capacidadTanque: capacidad));
    }

    [Fact]
    public void Crear_SinCapacidadTanque_SeAcepta()
    {
        var carro = ConstruirCarro(capacidadTanque: null);

        Assert.Null(carro.CapacidadTanque);
    }

    // --- Kilometraje ---

    [Fact]
    public void Crear_KilometrajeNegativo_LanzaDomainException()
    {
        Assert.Throws<DomainException>(() => ConstruirCarro(kilometraje: -1m));
    }

    [Fact]
    public void ActualizarKilometraje_MenorAlActual_LanzaDomainException()
    {
        var carro = ConstruirCarro(kilometraje: 15000m);

        Assert.Throws<DomainException>(() => carro.ActualizarKilometraje(14999m));
    }

    [Theory]
    [InlineData(15000)]  // igual al actual: permitido
    [InlineData(15480)]
    public void ActualizarKilometraje_IgualOMayor_SeActualiza(decimal nuevo)
    {
        var carro = ConstruirCarro(kilometraje: 15000m);

        carro.ActualizarKilometraje(nuevo);

        Assert.Equal(nuevo, carro.KilometrajeActual);
    }

    [Fact]
    public void ActualizarKilometraje_MayorAlMaximo_LanzaDomainException()
    {
        var carro = ConstruirCarro();

        Assert.Throws<DomainException>(() => carro.ActualizarKilometraje(Carro.KilometrajeMaximo + 1));
    }

    [Fact]
    public void AvanzarKilometraje_ViajeOfflineConMenorKilometraje_ConservaElActual()
    {
        var carro = ConstruirCarro(kilometraje: 15300m);

        carro.AvanzarKilometraje(15100m);

        Assert.Equal(15300m, carro.KilometrajeActual);
    }

    [Fact]
    public void AvanzarKilometraje_Mayor_Avanza()
    {
        var carro = ConstruirCarro(kilometraje: 15300m);

        carro.AvanzarKilometraje(15775.40m);

        Assert.Equal(15775.40m, carro.KilometrajeActual);
    }

    // --- Estados ---

    [Fact]
    public void Ocupar_CarroDisponible_QuedaOcupado()
    {
        var carro = ConstruirCarro();

        carro.Ocupar();

        Assert.Equal(EstadoCarro.Ocupado, carro.Estado);
    }

    [Theory]
    [InlineData(EstadoCarro.Ocupado)]
    [InlineData(EstadoCarro.EnMantenimiento)]
    public void Ocupar_CarroNoDisponible_LanzaDomainException(EstadoCarro estado)
    {
        var carro = ConstruirCarro(estado: estado);

        Assert.Throws<DomainException>(() => carro.Ocupar());
    }

    [Fact]
    public void Liberar_CarroOcupado_QuedaDisponible()
    {
        var carro = ConstruirCarro(estado: EstadoCarro.Ocupado);

        carro.Liberar();

        Assert.Equal(EstadoCarro.Disponible, carro.Estado);
    }

    [Fact]
    public void Liberar_CarroEnMantenimiento_SigueEnMantenimiento()
    {
        var carro = ConstruirCarro(estado: EstadoCarro.EnMantenimiento);

        carro.Liberar();

        Assert.Equal(EstadoCarro.EnMantenimiento, carro.Estado);
    }

    [Fact]
    public void CambiarEstado_ValorFueraDelEnum_LanzaDomainException()
    {
        var carro = ConstruirCarro();

        Assert.Throws<DomainException>(() => carro.CambiarEstado((EstadoCarro)99));
    }

    // --- Foto ---

    [Fact]
    public void CambiarFotoVehiculo_LlaveMayorA255_LanzaDomainException()
    {
        var carro = ConstruirCarro();

        Assert.Throws<DomainException>(() => carro.CambiarFotoVehiculo(new string('x', 256)));
    }

    [Fact]
    public void CambiarFotoVehiculo_Null_QuitaLaFoto()
    {
        var carro = ConstruirCarro(foto: "empresa/1/carro/1/foto/vehiculo.jpg");

        carro.CambiarFotoVehiculo(null);

        Assert.Null(carro.FotoVehiculo);
    }

    // --- Baja (HU-AST-03) ---

    [Fact]
    public void DarDeBaja_CarroDisponible_QuedaDadoDeBajaYNoDisponible()
    {
        var carro = ConstruirCarro();
        var fecha = new DateTime(2026, 10, 13, 9, 0, 0, DateTimeKind.Utc);

        carro.DarDeBaja(fecha, "  Venta de la unidad  ");

        Assert.True(carro.DadoDeBaja);
        Assert.Equal(fecha, carro.FechaBaja);
        Assert.Equal("Venta de la unidad", carro.MotivoBaja);
        Assert.False(carro.EstaDisponible);
    }

    [Fact]
    public void DarDeBaja_CarroOcupado_LanzaDomainException()
    {
        var carro = ConstruirCarro(estado: EstadoCarro.Ocupado);

        Assert.Throws<DomainException>(() => carro.DarDeBaja(DateTime.UtcNow, "Venta"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("   ")]
    public void DarDeBaja_SinMotivo_LanzaDomainException(string? motivo)
    {
        var carro = ConstruirCarro();

        Assert.Throws<DomainException>(() => carro.DarDeBaja(DateTime.UtcNow, motivo!));
    }

    [Fact]
    public void DarDeBaja_DosVeces_LanzaDomainException()
    {
        var carro = ConstruirCarro();
        carro.DarDeBaja(DateTime.UtcNow, "Venta");

        Assert.Throws<DomainException>(() => carro.DarDeBaja(DateTime.UtcNow, "Venta"));
    }

    [Fact]
    public void Ocupar_CarroDadoDeBaja_LanzaDomainException()
    {
        var carro = ConstruirCarro();
        carro.DarDeBaja(DateTime.UtcNow, "Siniestro total");

        Assert.Throws<DomainException>(() => carro.Ocupar());
    }

    [Fact]
    public void CorregirKilometraje_PuedeBajar()
    {
        var carro = ConstruirCarro(kilometraje: 15800m);

        carro.CorregirKilometraje(15580m);

        Assert.Equal(15580m, carro.KilometrajeActual);
    }

    private static Carro ConstruirCarro(
        string marca = "Nissan",
        string modelo = "NP300",
        int? anio = 2023,
        string? color = "Blanco",
        string placas = "DFG-123-A",
        string? foto = null,
        decimal? capacidadTanque = 80m,
        decimal kilometraje = 15000m,
        EstadoCarro estado = EstadoCarro.Disponible)
        => Carro.Crear(Id(1), marca, modelo, anio, color, placas, foto, capacidadTanque, kilometraje, AnioActual, estado);
}
