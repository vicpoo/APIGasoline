using APIGasoline.Domain.Rutas;

namespace APIGasoline.Domain.Tests.Rutas;

/// <summary>
/// Formato Encoded Polyline de Google (5 decimales). El caso base es el ejemplo de la documentación oficial.
/// </summary>
public class CodificadorPolylineTests
{
    private static readonly (double, double)[] EjemploGoogle = [(38.5, -120.2), (40.7, -120.95), (43.252, -126.453)];
    private const string EjemploGoogleCodificado = "_p~iF~ps|U_ulLnnqC_mqNvxq`@";

    [Fact]
    public void Codificar_EjemploDeLaDocumentacionDeGoogle_DaLaCadenaOficial()
    {
        Assert.Equal(EjemploGoogleCodificado, CodificadorPolyline.Codificar(EjemploGoogle));
    }

    [Fact]
    public void Decodificar_CadenaOficialDeGoogle_DaLosPuntosOriginales()
    {
        Assert.Equal(EjemploGoogle, CodificadorPolyline.Decodificar(EjemploGoogleCodificado));
    }

    [Fact]
    public void CodificarYDecodificar_RutaDeTuxtla_ConservaCincoDecimales()
    {
        (double, double)[] ruta = [(16.75306, -93.11556), (16.75012, -93.10034), (16.74587, -93.08521)];

        var resultado = CodificadorPolyline.Decodificar(CodificadorPolyline.Codificar(ruta));

        Assert.Equal(ruta, resultado);
    }

    [Fact]
    public void Codificar_SinPuntos_DevuelveCadenaVacia()
    {
        Assert.Equal(string.Empty, CodificadorPolyline.Codificar([]));
    }

    [Fact]
    public void Decodificar_CadenaVacia_DevuelveListaVacia()
    {
        Assert.Empty(CodificadorPolyline.Decodificar(string.Empty));
    }

    [Fact]
    public void Decodificar_CadenaIncompleta_LanzaFormatException()
    {
        // Se corta a la mitad de un valor (los caracteres con valor >= 0x20 indican que el valor continúa)
        Assert.Throws<FormatException>(() => CodificadorPolyline.Decodificar("_p~"));
    }
}
