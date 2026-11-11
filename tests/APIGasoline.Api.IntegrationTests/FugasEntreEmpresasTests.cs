using System.Net;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using APIGasoline.Api.IntegrationTests.Infraestructura;
using APIGasoline.Api.IntegrationTests.Matriz;

namespace APIGasoline.Api.IntegrationTests;

// =====================================================================================
//  MATRIZ DE FUGAS ENTRE EMPRESAS
//  Atacante: el administrador de la Empresa A (todos los privilegios de ámbito empresa).
//  Víctima: la Empresa B. Cada prueba que falla con "FUGA:" es un hueco real de seguridad.
//  Las pruebas de ControlesDeLaMatriz confirman que cada caso está bien armado.
// =====================================================================================

[Collection(ColeccionApi.Nombre)]
public sealed class FugasLecturaTests(Escenario e)
{
    public static TheoryData<string> Casos => new(CatalogoFugas.Lecturas.Select(c => c.Nombre));

    [Theory]
    [MemberData(nameof(Casos))]
    public async Task A_no_puede_ver_datos_de_B(string caso)
    {
        var c = CatalogoFugas.Lecturas.Single(x => x.Nombre == caso);
        var url = c.Url(e);

        var respuesta = await e.GetAsync(Actor.AdminA, url);

        if (c.EsDetalle)
            Assert.False(respuesta.Exito, $"FUGA: AdminA hizo GET {url} y respondió {respuesta}");

        // Los ids que van en la propia URL no cuentan: un 404 o un 403 los repite en su mensaje
        var idsDeLaUrl = Regex.Matches(url, "[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}")
            .Select(m => m.Value)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var marcas = (await e.MarcasDeAsync(e.B)).Where(m => !idsDeLaUrl.Contains(m));
        var encontradas = marcas.Where(m => respuesta.Cuerpo.Contains(m, StringComparison.OrdinalIgnoreCase)).Take(5).ToList();

        Assert.True(encontradas.Count == 0,
            $"FUGA: GET {url} como AdminA devolvió datos de la Empresa B ({string.Join(", ", encontradas)}). Respuesta: {respuesta}");
    }
}

[Collection(ColeccionApi.Nombre)]
public sealed class FugasEscrituraTests(Escenario e)
{
    public static TheoryData<string> Casos => new(CatalogoFugas.Escrituras.Select(c => c.Nombre));

    [Theory]
    [MemberData(nameof(Casos))]
    public async Task A_no_puede_modificar_datos_de_B(string caso)
    {
        var c = CatalogoFugas.Escrituras.Single(x => x.Nombre == caso);
        var objetivo = c.VictimaEsDelAtacante ? e.De(c.Atacante) : e.B;
        var victima = await c.CrearVictima(e, objetivo);
        var ctx = new Contexto { E = e, Actor = c.Atacante, EsAtaque = true, Objetivo = objetivo, Id = victima.Id, IdRelacionado = victima.IdRelacionado };

        var antes = await e.GetAsync(Actor.Plataforma, c.UrlDetalle(ctx));
        var url = c.Url(ctx);
        var respuesta = await e.EnviarAsync(c.Atacante, c.Metodo, url, c.Cuerpo is null ? null : await c.Cuerpo(ctx));
        var despues = await e.GetAsync(Actor.Plataforma, c.UrlDetalle(ctx));

        Assert.False(respuesta.Exito, $"FUGA: {c.Atacante} hizo {c.Metodo} {url} sobre un registro de la empresa {objetivo.Letra} y respondió {respuesta}");
        Assert.True(Normalizar(antes) == Normalizar(despues), $"FUGA: el registro cambió aunque la API respondió {respuesta.Status}.\nAntes:   {antes}\nDespués: {despues}");
    }

    /// <summary>Campos que cambian solos con el reloj (no por el ataque): se ignoran al comparar.</summary>
    private static readonly HashSet<string> CamposQueCambianConElTiempo = new(StringComparer.OrdinalIgnoreCase) { "segundosEnPausa" };

    private static string Normalizar(Respuesta r)
    {
        try
        {
            var nodo = JsonNode.Parse(r.Cuerpo);
            Quitar(nodo);
            return $"{(int)r.Status} {nodo?.ToJsonString()}";
        }
        catch (System.Text.Json.JsonException)
        {
            return r.ToString();
        }
    }

    private static void Quitar(JsonNode? nodo)
    {
        switch (nodo)
        {
            case JsonObject objeto:
                foreach (var campo in objeto.Select(p => p.Key).Where(CamposQueCambianConElTiempo.Contains).ToList())
                    objeto.Remove(campo);
                foreach (var (_, valor) in objeto)
                    Quitar(valor);
                break;
            case JsonArray arreglo:
                foreach (var valor in arreglo)
                    Quitar(valor);
                break;
        }
    }
}

[Collection(ColeccionApi.Nombre)]
public sealed class FugasCreacionTests(Escenario e)
{
    public static TheoryData<string> Casos => new(CatalogoFugas.Creaciones.Select(c => c.Nombre));

    [Theory]
    [MemberData(nameof(Casos))]
    public async Task A_no_puede_crear_registros_dentro_de_B(string caso)
    {
        var c = CatalogoFugas.Creaciones.Single(x => x.Nombre == caso);
        var ctx = new Contexto { E = e, Actor = c.Atacante, EsAtaque = true, Objetivo = e.B };

        var cuerpo = await c.Cuerpo(ctx);
        var antes = await c.Contar(e, e.B);
        var url = c.Url(ctx);
        var respuesta = await e.EnviarAsync(c.Atacante, c.Metodo, url, cuerpo);
        var despues = await c.Contar(e, e.B);

        Assert.False(respuesta.Exito, $"FUGA: {c.Atacante} hizo {c.Metodo} {url} con datos de la Empresa B y respondió {respuesta}");
        Assert.True(antes == despues, $"FUGA: se creó un registro en la Empresa B ({antes} → {despues}) aunque la API respondió {respuesta.Status}.");
    }
}

[Collection(ColeccionApi.Nombre)]
public sealed class SoloPlataformaTests(Escenario e)
{
    public static TheoryData<string> Casos => new(CatalogoFugas.SoloPlataforma.Select(c => c.ToString()));

    [Theory]
    [MemberData(nameof(Casos))]
    public async Task El_admin_de_una_empresa_no_puede_usar_acciones_de_plataforma(string caso)
    {
        var c = CatalogoFugas.SoloPlataforma.Single(x => x.ToString() == caso);
        var cuerpo = c.Metodo == HttpMethod.Get || c.Metodo == HttpMethod.Delete ? null : Escenario.ComoJson(new { });

        var respuesta = await e.EnviarAsync(Actor.AdminA, c.Metodo, c.Url, cuerpo);

        Assert.True(respuesta.Status == HttpStatusCode.Forbidden, $"FUGA: AdminA hizo {caso} y respondió {respuesta} (se esperaba 403).");
    }
}
