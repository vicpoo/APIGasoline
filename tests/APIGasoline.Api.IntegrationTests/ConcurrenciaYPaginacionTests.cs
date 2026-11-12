using System.Net;
using System.Text.Json;
using APIGasoline.Api.IntegrationTests.Infraestructura;
using APIGasoline.Domain.Entities;
using APIGasoline.Infrastructure.Persistence.Converters;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using static APIGasoline.Api.IntegrationTests.Infraestructura.Victimas;

namespace APIGasoline.Api.IntegrationTests;

// =====================================================================================
//  REVISIÓN DE CÓDIGO F010, F011 y F012
//  - F010: la BD no deja duplicar lo que dos peticiones simultáneas podían duplicar.
//  - F011: los listados se paginan (pagina / tamanoPagina, total en X-Total-Count).
//  - F012: los reportes de flota tienen periodo acotado.
// =====================================================================================

[Collection(ColeccionApi.Nombre)]
public sealed class ConcurrenciaTests(Escenario e)
{
    private const int ErrorIndiceUnico = 2601;

    [Fact]
    public async Task La_BD_no_deja_dos_viajes_en_curso_del_mismo_carro()
    {
        var viajeId = await e.ViajeAsync(e.A);

        var error = await Assert.ThrowsAsync<DbUpdateException>(() => e.EnBdAsync(async c =>
        {
            var primero = await c.Viajes.SingleAsync(v => v.ViajeId == viajeId);
            c.Viajes.Add(Viaje.Iniciar(e.A.EmpresaId, primero.CarroId, null, e.A.OperadorEmpleadoId, DateTime.Now.AddMinutes(-5), null, null, 35001m, null));
            return await c.SaveChangesAsync();
        }));

        Assert.Equal(ErrorIndiceUnico, (error.InnerException as SqlException)?.Number);
    }

    [Fact]
    public async Task La_BD_no_deja_dos_pausas_abiertas_del_mismo_viaje()
    {
        var viajeId = await e.ViajeAsync(e.A, EstadoVictima.Pausado);

        var error = await Assert.ThrowsAsync<SqlException>(() => e.EnBdAsync(c => c.Database.ExecuteSqlRawAsync(
            "INSERT INTO viaje_pausa (viaje_pausa_id, empresa_id, viaje_id, fecha_inicio) VALUES ({0}, {1}, {2}, SYSDATETIME())",
            GuidBinarioConverter.ABytes(Guid.CreateVersion7()),
            GuidBinarioConverter.ABytes(e.A.EmpresaId),
            GuidBinarioConverter.ABytes(viajeId))));

        Assert.Equal(ErrorIndiceUnico, error.Number);
    }

    [Fact]
    public async Task El_mismo_lote_GPS_enviado_dos_veces_al_mismo_tiempo_no_duplica_puntos()
    {
        var viajeId = await e.ViajeAsync(e.A);
        var inicio = DateTime.Now.AddMinutes(-50);
        var lote = new
        {
            puntos = Enumerable.Range(1, 20).Select(i => new
            {
                latitud = 16.75m + i * 0.001m,
                longitud = -93.12m - i * 0.001m,
                timestampCaptura = Escenario.Fecha(inicio.AddSeconds(i * 30)),
                precisionMetros = 5,
            }),
        };

        var respuestas = await Task.WhenAll(Enumerable.Range(0, 3).Select(_ =>
            e.EnviarAsync(Actor.OperadorA, HttpMethod.Post, $"/api/viajes/{viajeId}/puntos-ruta", Escenario.ComoJson(lote))));

        // Ninguno falla con 500, y en la BD queda cada punto una sola vez
        Assert.All(respuestas, r => Assert.True(r.Exito || r.Status == HttpStatusCode.Conflict, r.ToString()));
        Assert.Contains(respuestas, r => r.Exito);
        Assert.Equal(20, await e.EnBdAsync(c => c.PuntosRuta.CountAsync(p => p.ViajeId == viajeId)));
    }

    [Fact]
    public async Task Reenviar_un_lote_ya_guardado_responde_exito_sin_duplicar()
    {
        var viajeId = await e.ViajeAsync(e.A);
        var lote = new { puntos = new[] { new { latitud = 16.7m, longitud = -93.1m, timestampCaptura = Escenario.Fecha(DateTime.Now.AddMinutes(-40)), precisionMetros = 5 } } };

        var primero = await e.EnviarAsync(Actor.OperadorA, HttpMethod.Post, $"/api/viajes/{viajeId}/puntos-ruta", Escenario.ComoJson(lote));
        var segundo = await e.EnviarAsync(Actor.OperadorA, HttpMethod.Post, $"/api/viajes/{viajeId}/puntos-ruta", Escenario.ComoJson(lote));

        Assert.True(primero.Exito, primero.ToString());
        Assert.True(segundo.Exito, segundo.ToString());
        Assert.Equal(1, await e.EnBdAsync(c => c.PuntosRuta.CountAsync(p => p.ViajeId == viajeId)));
    }
}

[Collection(ColeccionApi.Nombre)]
public sealed class PaginacionTests(Escenario e)
{
    [Theory]
    [InlineData("/api/carros")]
    [InlineData("/api/viajes")]
    [InlineData("/api/solicitudes-reserva")]
    [InlineData("/api/cargas-combustible")]
    [InlineData("/api/notificaciones")]
    public async Task Los_listados_devuelven_una_pagina_y_el_total(string url)
    {
        var respuesta = await e.GetAsync(Actor.Plataforma, $"{url}?tamanoPagina=1");

        Assert.True(respuesta.Exito, respuesta.ToString());
        Assert.True(JsonDocument.Parse(respuesta.Cuerpo).RootElement.GetArrayLength() <= 1);
        Assert.True(int.TryParse(respuesta.Header("X-Total-Count"), out var total) && total >= 1, $"Falta X-Total-Count: {respuesta}");
        Assert.Equal("1", respuesta.Header("X-Tamano-Pagina"));
    }

    [Fact]
    public async Task El_tamano_de_pagina_no_pasa_de_200()
    {
        var respuesta = await e.GetAsync(Actor.Plataforma, "/api/carros?tamanoPagina=100000");

        Assert.Equal("200", respuesta.Header("X-Tamano-Pagina"));
    }

    [Fact]
    public async Task Las_paginas_no_se_enciman()
    {
        var p1 = await e.GetAsync(Actor.Plataforma, "/api/carros?pagina=1&tamanoPagina=2");
        var p2 = await e.GetAsync(Actor.Plataforma, "/api/carros?pagina=2&tamanoPagina=2");

        var ids1 = Ids(p1, "carroId");
        var ids2 = Ids(p2, "carroId");

        Assert.Equal(2, ids1.Count);
        Assert.Empty(ids1.Intersect(ids2));
    }

    private static List<Guid> Ids(Respuesta r, string campo)
        => JsonDocument.Parse(r.Cuerpo).RootElement.EnumerateArray().Select(x => x.GetProperty(campo).GetGuid()).ToList();
}

[Collection(ColeccionApi.Nombre)]
public sealed class PeriodoReportesTests(Escenario e)
{
    [Theory]
    [InlineData("/api/reportes/flota")]
    [InlineData("/api/reportes/flota/pdf")]
    [InlineData("/api/reportes/flota/excel")]
    public async Task Un_periodo_de_mas_de_366_dias_responde_400(string url)
    {
        var desde = Escenario.Fecha(DateTime.Now.AddDays(-400));
        var hasta = Escenario.Fecha(DateTime.Now);

        var respuesta = await e.GetAsync(Actor.AdminA, $"{url}?desde={desde}&hasta={hasta}");

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.Status);
    }

    [Fact]
    public async Task Sin_fechas_el_reporte_es_del_ultimo_mes()
    {
        var reporte = JsonDocument.Parse((await e.GetAsync(Actor.AdminA, "/api/reportes/flota")).Cuerpo).RootElement;

        var desde = reporte.GetProperty("desde").GetDateTime();
        var hasta = reporte.GetProperty("hasta").GetDateTime();

        Assert.InRange((hasta - desde).TotalDays, 27, 32);
    }
}
