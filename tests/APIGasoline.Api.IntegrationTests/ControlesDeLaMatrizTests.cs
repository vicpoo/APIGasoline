using APIGasoline.Api.IntegrationTests.Infraestructura;
using APIGasoline.Api.IntegrationTests.Matriz;

namespace APIGasoline.Api.IntegrationTests;

// =====================================================================================
//  CONTROLES DE LA MATRIZ
//  Comprueban que cada caso de FugasEntreEmpresasTests está bien armado: el dueño (o la
//  plataforma) SÍ puede hacer lo mismo. Si un control falla, el caso correspondiente de la
//  matriz no prueba nada (podría "pasar" por un 400 o un 404 que no tiene que ver con empresas).
// =====================================================================================

[Collection(ColeccionApi.Nombre)]
public sealed class ControlLecturaTests(Escenario e)
{
    public static TheoryData<string> Casos => new(CatalogoFugas.Lecturas.Select(c => c.Nombre));

    [Theory]
    [MemberData(nameof(Casos))]
    public async Task La_plataforma_si_ve_los_datos_de_B(string caso)
    {
        var c = CatalogoFugas.Lecturas.Single(x => x.Nombre == caso);
        var url = c.Url(e);

        var respuesta = await e.GetAsync(Actor.Plataforma, url);
        Assert.True(respuesta.Exito, $"CASO MAL ARMADO: la plataforma hizo GET {url} y respondió {respuesta}");

        if (c.RespuestaBinaria || caso.StartsWith("Contar", StringComparison.Ordinal))
            return; // archivo o número: no trae ids que buscar

        var marcas = await e.MarcasDeAsync(e.B);
        Assert.True(marcas.Any(m => respuesta.Cuerpo.Contains(m, StringComparison.OrdinalIgnoreCase)),
            $"CASO NO CONCLUYENTE: la plataforma tampoco ve datos de B en GET {url}, así que no prueba nada. Respuesta: {respuesta}");
    }
}

[Collection(ColeccionApi.Nombre)]
public sealed class ControlEscrituraTests(Escenario e)
{
    public static TheoryData<string> Casos => new(CatalogoFugas.Escrituras.Where(c => c.Control is not null).Select(c => c.Nombre));

    [Theory]
    [MemberData(nameof(Casos))]
    public async Task El_dueno_si_puede_hacerlo(string caso)
    {
        var c = CatalogoFugas.Escrituras.Single(x => x.Nombre == caso);
        var dueno = c.Control!.Value;
        var objetivo = c.VictimaEsDelAtacante ? e.De(c.Atacante) : e.B;
        var victima = await c.CrearVictima(e, objetivo);
        var ctx = new Contexto { E = e, Actor = dueno, EsAtaque = false, Objetivo = objetivo, Id = victima.Id, IdRelacionado = victima.IdRelacionado };

        var url = c.Url(ctx);
        var respuesta = await e.EnviarAsync(dueno, c.Metodo, url, c.Cuerpo is null ? null : await c.Cuerpo(ctx));

        Assert.True(respuesta.Exito, $"CASO MAL ARMADO: el dueño ({dueno}) hizo {c.Metodo} {url} y respondió {respuesta}");
    }
}

[Collection(ColeccionApi.Nombre)]
public sealed class ControlCreacionTests(Escenario e)
{
    public static TheoryData<string> Casos => new(CatalogoFugas.Creaciones.Select(c => c.Nombre));

    [Theory]
    [MemberData(nameof(Casos))]
    public async Task El_dueno_si_puede_crearlo(string caso)
    {
        var c = CatalogoFugas.Creaciones.Single(x => x.Nombre == caso);
        var ctx = new Contexto { E = e, Actor = c.Control, EsAtaque = false, Objetivo = e.B };

        var cuerpo = await c.Cuerpo(ctx);
        var antes = await c.Contar(e, e.B);
        var url = c.Url(ctx);
        var respuesta = await e.EnviarAsync(c.Control, c.Metodo, url, cuerpo);
        var despues = await c.Contar(e, e.B);

        Assert.True(respuesta.Exito, $"CASO MAL ARMADO: el dueño ({c.Control}) hizo {c.Metodo} {url} y respondió {respuesta}");
        Assert.True(despues == antes + 1, $"CASO MAL ARMADO: el contador no subió ({antes} → {despues}); revisa la función Contar del caso.");
    }
}

[Collection(ColeccionApi.Nombre)]
public sealed class ControlGeneralTests(Escenario e)
{
    [Fact]
    public async Task La_plataforma_ve_los_carros_de_las_dos_empresas()
    {
        var respuesta = await e.GetAsync(Actor.Plataforma, "/api/carros");

        Assert.True(respuesta.Exito, respuesta.ToString());
        Assert.Contains(e.A.Carro1Id.ToString(), respuesta.Cuerpo, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(e.B.Carro1Id.ToString(), respuesta.Cuerpo, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData(Actor.AdminA)]
    [InlineData(Actor.AdminB)]
    public async Task Cada_admin_ve_sus_propios_carros(Actor admin)
    {
        var propia = e.De(admin);

        var respuesta = await e.GetAsync(admin, $"/api/carros/{propia.Carro1Id}");

        Assert.True(respuesta.Exito, respuesta.ToString());
    }
}
