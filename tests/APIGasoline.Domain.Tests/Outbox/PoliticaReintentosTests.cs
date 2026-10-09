using APIGasoline.Domain.Outbox;

namespace APIGasoline.Domain.Tests.Outbox;

public class PoliticaReintentosTests
{
    [Theory]
    [InlineData(1, 30)]
    [InlineData(2, 120)]
    [InlineData(3, 600)]
    [InlineData(4, 1800)]
    public void EsperaTras_CadaFalloEsperaMas(int intentosFallidos, int segundosEsperados)
    {
        Assert.Equal(TimeSpan.FromSeconds(segundosEsperados), PoliticaReintentos.EsperaTras(intentosFallidos));
    }

    [Fact]
    public void EsperaTras_MasFallosQueEsperas_UsaLaUltima()
    {
        Assert.Equal(TimeSpan.FromMinutes(30), PoliticaReintentos.EsperaTras(10));
    }

    [Fact]
    public void EsperaTras_SinFallos_LanzaArgumentOutOfRange()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => PoliticaReintentos.EsperaTras(0));
    }

    [Theory]
    [InlineData(4, false)]
    [InlineData(5, true)]
    [InlineData(6, true)]
    public void SeAgotaron_AlLlegarAlMaximo_DevuelveTrue(int intentosFallidos, bool esperado)
    {
        Assert.Equal(esperado, PoliticaReintentos.SeAgotaron(intentosFallidos));
    }
}
