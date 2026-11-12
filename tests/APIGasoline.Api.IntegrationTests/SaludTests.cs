using System.Net;
using System.Text.Json;
using APIGasoline.Api.IntegrationTests.Infraestructura;

namespace APIGasoline.Api.IntegrationTests;

/// <summary>GET /salud: público, para el despliegue y el monitoreo (Fase 12).</summary>
[Collection(ColeccionApi.Nombre)]
public sealed class SaludTests(Escenario e)
{
    [Fact]
    public async Task Responde_sin_sesion_con_la_version()
    {
        using var anonimo = e.Factory.CreateClient();

        using var respuesta = await anonimo.GetAsync("/salud");
        var cuerpo = JsonDocument.Parse(await respuesta.Content.ReadAsStringAsync()).RootElement;

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        Assert.Equal("ok", cuerpo.GetProperty("estado").GetString());
        Assert.Equal("2.0.0", cuerpo.GetProperty("version").GetString());
    }
}
