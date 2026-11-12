using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Features.Reportes.Common;
using APIGasoline.Domain.Common;

namespace APIGasoline.Application.Tests.Common;

/// <summary>F011 (páginas acotadas) y F012 (periodo de los reportes).</summary>
public class PaginacionYPeriodoTests
{
    [Fact]
    public void Paginacion_SinDatos_PrimeraPaginaDe50()
    {
        var p = new Paginacion();

        Assert.Equal(1, p.Pagina);
        Assert.Equal(50, p.TamanoPagina);
        Assert.Equal(0, p.Saltar);
    }

    [Theory]
    [InlineData(0, 10, 1, 10)]
    [InlineData(-3, 10, 1, 10)]
    [InlineData(3, 500, 3, 200)]
    [InlineData(2, 0, 2, 1)]
    public void Paginacion_SeAcotaASusLimites(int pagina, int tamano, int paginaEsperada, int tamanoEsperado)
    {
        var p = new Paginacion(pagina, tamano);

        Assert.Equal(paginaEsperada, p.Pagina);
        Assert.Equal(tamanoEsperado, p.TamanoPagina);
    }

    [Fact]
    public void Paginacion_SaltaLasPaginasAnteriores()
        => Assert.Equal(40, new Paginacion(3, 20).Saltar);

    [Fact]
    public void Paginacion_ConLimitesPropios()
        => Assert.Equal(5000, new Paginacion(1, 9999, porDefecto: 1000, maximo: 5000).TamanoPagina);

    private static readonly DateTime Ahora = new(2026, 11, 12, 10, 0, 0);

    [Fact]
    public void Periodo_SinFechas_EsElUltimoMes()
    {
        var (desde, hasta) = PeriodoReporte.Resolver(null, null, Ahora);

        Assert.Equal(Ahora, hasta);
        Assert.Equal(Ahora.AddMonths(-1), desde);
    }

    [Fact]
    public void Periodo_SoloDesde_LlegaHastaAhora()
        => Assert.Equal(Ahora, PeriodoReporte.Resolver(Ahora.AddDays(-10), null, Ahora).Hasta);

    [Fact]
    public void Periodo_DeUnAnio_SeAcepta()
        => PeriodoReporte.Resolver(Ahora.AddDays(-366), Ahora, Ahora);

    [Fact]
    public void Periodo_MayorA366Dias_Lanza400()
        => Assert.Throws<BadRequestException>(() => PeriodoReporte.Resolver(Ahora.AddDays(-367), Ahora, Ahora));

    [Fact]
    public void Periodo_Invertido_Lanza400()
        => Assert.Throws<BadRequestException>(() => PeriodoReporte.Resolver(Ahora, Ahora.AddDays(-1), Ahora));

    [Fact]
    public void Filas_SobreElTope_Lanza400()
        => Assert.Throws<BadRequestException>(() => PeriodoReporte.ValidarFilas(PeriodoReporte.MaxFilasPdf + 1, PeriodoReporte.MaxFilasPdf, "PDF"));
}
