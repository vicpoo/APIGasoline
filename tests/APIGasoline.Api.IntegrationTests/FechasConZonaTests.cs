using APIGasoline.Api.IntegrationTests.Infraestructura;
using Microsoft.EntityFrameworkCore;
using static APIGasoline.Api.IntegrationTests.Infraestructura.Victimas;

namespace APIGasoline.Api.IntegrationTests;

// =====================================================================================
//  A001: fechas con zona horaria en el JSON
//  La app manda la hora local del teléfono con offset ("...-06:00"), pero un teléfono con zona UTC
//  manda "Z". Las tres formas deben guardarse como la misma hora local del servidor.
// =====================================================================================

[Collection(ColeccionApi.Nombre)]
public sealed class FechasConZonaTests(Escenario e)
{
    public static TheoryData<string> Formatos => new() { "sin-zona", "offset", "utc" };

    [Theory]
    [MemberData(nameof(Formatos))]
    public async Task Un_punto_GPS_con_cualquier_formato_de_fecha_se_guarda_en_hora_local(string formato)
    {
        var viajeId = await e.ViajeAsync(e.A);
        var local = DateTime.Now.AddMinutes(-30);
        local = new DateTime(local.Year, local.Month, local.Day, local.Hour, local.Minute, local.Second, DateTimeKind.Local);

        var lote = new
        {
            puntos = new[] { new { latitud = 16.7m, longitud = -93.1m, timestampCaptura = Formatear(local, formato), precisionMetros = 5 } },
        };

        var respuesta = await e.EnviarAsync(Actor.OperadorA, HttpMethod.Post, $"/api/viajes/{viajeId}/puntos-ruta", Escenario.ComoJson(lote));

        Assert.True(respuesta.Exito, $"{formato}: {respuesta}");

        var guardado = await e.EnBdAsync(c => c.PuntosRuta.Where(p => p.ViajeId == viajeId).Select(p => p.TimestampCaptura).SingleAsync());
        Assert.Equal(DateTime.SpecifyKind(local, DateTimeKind.Unspecified), DateTime.SpecifyKind(guardado, DateTimeKind.Unspecified));
    }

    private static string Formatear(DateTime local, string formato) => formato switch
    {
        "sin-zona" => local.ToString("yyyy-MM-ddTHH:mm:ss"),
        "offset" => new DateTimeOffset(local).ToString("yyyy-MM-ddTHH:mm:sszzz"),
        "utc" => local.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss") + "Z",
        _ => throw new ArgumentOutOfRangeException(nameof(formato)),
    };
}
