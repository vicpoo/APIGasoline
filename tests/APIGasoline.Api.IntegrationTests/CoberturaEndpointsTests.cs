using APIGasoline.Api.IntegrationTests.Infraestructura;
using APIGasoline.Api.IntegrationTests.Matriz;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace APIGasoline.Api.IntegrationTests;

/// <summary>
/// Nadie se queda sin probar: cada endpoint de la API debe estar en la matriz de fugas
/// (CatalogoFugas) o en su lista de excluidos con una razón. Si agregas un endpoint y no lo
/// agregas a la matriz, esta prueba falla y te dice cuál.
/// </summary>
[Collection(ColeccionApi.Nombre)]
public sealed class CoberturaEndpointsTests(Escenario e)
{
    private static string Clave(string metodo, string plantilla) => $"{metodo.ToUpperInvariant()} {plantilla.TrimStart('/')}";

    private IReadOnlySet<string> EndpointsDeLaApi()
    {
        var fuente = e.Factory.Services.GetRequiredService<EndpointDataSource>();

        return fuente.Endpoints
            .OfType<RouteEndpoint>()
            .SelectMany(endpoint => (endpoint.Metadata.GetMetadata<Microsoft.AspNetCore.Routing.IHttpMethodMetadata>()?.HttpMethods ?? [])
                .Select(metodo => Clave(metodo, endpoint.RoutePattern.RawText ?? string.Empty)))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    private static IReadOnlySet<string> EndpointsDeLaMatriz()
    {
        var claves = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var c in CatalogoFugas.Lecturas) claves.Add(Clave("GET", c.Plantilla));
        foreach (var c in CatalogoFugas.Escrituras) claves.Add(Clave(c.Metodo.Method, c.Plantilla));
        foreach (var c in CatalogoFugas.Creaciones) claves.Add(Clave(c.Metodo.Method, c.Plantilla));
        foreach (var c in CatalogoFugas.SoloPlataforma) claves.Add(Clave(c.Metodo.Method, c.Plantilla));
        foreach (var c in CatalogoFugas.Excluidos.Keys) claves.Add(c);

        return claves;
    }

    [Fact]
    public void Todos_los_endpoints_estan_en_la_matriz_de_fugas()
    {
        var faltan = EndpointsDeLaApi().Except(EndpointsDeLaMatriz(), StringComparer.OrdinalIgnoreCase).Order().ToList();

        Assert.True(faltan.Count == 0,
            "Endpoints sin caso en la matriz de fugas (agrégalos a CatalogoFugas o a Excluidos con su razón):\n  " + string.Join("\n  ", faltan));
    }

    [Fact]
    public void La_matriz_no_tiene_endpoints_que_ya_no_existen()
    {
        var sobran = EndpointsDeLaMatriz().Except(EndpointsDeLaApi(), StringComparer.OrdinalIgnoreCase).Order().ToList();

        Assert.True(sobran.Count == 0,
            "La matriz menciona endpoints que la API no tiene (¿cambió la ruta?):\n  " + string.Join("\n  ", sobran));
    }
}
