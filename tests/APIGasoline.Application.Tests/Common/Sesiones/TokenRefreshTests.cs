using APIGasoline.Application.Common.Sesiones;

namespace APIGasoline.Application.Tests.Common.Sesiones;

public class TokenRefreshTests
{
    [Fact]
    public void Generar_TokensDistintosDe256Bits()
    {
        var tokens = Enumerable.Range(0, 100).Select(_ => TokenRefresh.Generar()).ToHashSet();

        Assert.Equal(100, tokens.Count);
        Assert.All(tokens, t => Assert.Equal(43, t.Length)); // 32 bytes en Base64Url sin relleno
        Assert.All(tokens, t => Assert.DoesNotContain('+', t));
        Assert.All(tokens, t => Assert.DoesNotContain('/', t));
    }

    [Fact]
    public void Hash_EsSha256HexadecimalYDeterminista()
    {
        var hash = TokenRefresh.Hash("abc");

        Assert.Equal("ba7816bf8f01cfea414140de5dae2223b00361a396177a9cb410ff61f20015ad", hash);
        Assert.Equal(hash, TokenRefresh.Hash("abc"));
    }

    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData("  ", false)]
    [InlineData("token", true)]
    public void FormatoValido(string? token, bool esperado)
    {
        Assert.Equal(esperado, TokenRefresh.FormatoValido(token));
    }

    [Fact]
    public void FormatoValido_DemasiadoLargo_EsFalse()
    {
        Assert.False(TokenRefresh.FormatoValido(new string('a', TokenRefresh.LargoMaximo + 1)));
    }
}

public class SesionesSettingsTests
{
    [Fact]
    public void Validar_ValoresPorDefecto_SonValidos()
    {
        var excepcion = Record.Exception(() => new SesionesSettings().Validar());

        Assert.Null(excepcion);
    }

    [Fact]
    public void Validar_TopeMenorALaDuracion_Lanza()
    {
        Assert.Throws<InvalidOperationException>(() => new SesionesSettings { RefreshDias = 30, RefreshMaxDias = 10 }.Validar());
    }

    [Fact]
    public void Validar_SinDias_Lanza()
    {
        Assert.Throws<InvalidOperationException>(() => new SesionesSettings { RefreshDias = 0 }.Validar());
    }
}
