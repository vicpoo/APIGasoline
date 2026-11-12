using System.Net;
using APIGasoline.Api.IntegrationTests.Infraestructura;
using APIGasoline.Api.IntegrationTests.Matriz;

namespace APIGasoline.Api.IntegrationTests;

// =====================================================================================
//  MATRIZ DENTRO DE LA EMPRESA (revisión de código F002, F003, F004, F005 y F007)
//  - Lo propio: el operador A2 no puede ver ni tocar lo del operador A.
//  - Techo de privilegios: el gestor de roles no puede dar ni quitar más de lo que tiene.
//  Un fallo "FUGA DENTRO DE LA EMPRESA:" o "ESCALADA:" es un hueco real.
//  Los controles (abajo) confirman que el dueño y quien tiene el privilegio SÍ pueden.
// =====================================================================================

[Collection(ColeccionApi.Nombre)]
public sealed class LoPropioLecturaTests(Escenario e)
{
    public static TheoryData<string> Casos => new(CatalogoLoPropio.Lecturas.Select(c => c.Nombre));

    [Theory]
    [MemberData(nameof(Casos))]
    public async Task Un_operador_no_ve_lo_de_otro(string caso)
    {
        var c = CatalogoLoPropio.Lecturas.Single(x => x.Nombre == caso);
        var url = await c.Url(e);

        var respuesta = await e.GetAsync(c.Atacante, url);

        Assert.False(respuesta.Exito, $"FUGA DENTRO DE LA EMPRESA: {c.Atacante} hizo GET {url} y respondió {respuesta}");
        Assert.True(respuesta.Status is HttpStatusCode.Forbidden or HttpStatusCode.NotFound,
            $"Se esperaba 403 o 404 (no un error): {respuesta}");
    }
}

[Collection(ColeccionApi.Nombre)]
public sealed class LoPropioEscrituraTests(Escenario e)
{
    public static TheoryData<string> Casos => new(CatalogoLoPropio.TodasLasEscrituras.Select(c => c.Nombre));

    [Theory]
    [MemberData(nameof(Casos))]
    public async Task Nadie_toca_lo_de_otro_ni_da_mas_de_lo_que_tiene(string caso)
    {
        var c = CatalogoLoPropio.TodasLasEscrituras.Single(x => x.Nombre == caso);
        var p = await c.Preparar(e);

        var antes = await p.Estado();
        var respuesta = await e.EnviarAsync(c.Atacante, c.Metodo, p.Url, p.Cuerpo);
        var despues = await p.Estado();

        var etiqueta = c.Atacante == Actor.GestorRolesA ? "ESCALADA" : "FUGA DENTRO DE LA EMPRESA";
        Assert.False(respuesta.Exito, $"{etiqueta}: {c.Atacante} hizo {c.Metodo} {p.Url} y respondió {respuesta}");
        Assert.True(respuesta.Status is HttpStatusCode.Forbidden or HttpStatusCode.NotFound,
            $"Se esperaba 403 o 404 (no un error): {respuesta}");
        Assert.True(antes == despues, $"{etiqueta}: el registro cambió aunque la API respondió {respuesta.Status}.\nAntes:   {antes}\nDespués: {despues}");
    }
}

[Collection(ColeccionApi.Nombre)]
public sealed class ControlesDeLoPropioTests(Escenario e)
{
    public static TheoryData<string, Actor> Lecturas => Combinar(CatalogoLoPropio.Lecturas.Select(c => (c.Nombre, c.Controles)));

    public static TheoryData<string, Actor> Escrituras => Combinar(CatalogoLoPropio.TodasLasEscrituras.Select(c => (c.Nombre, c.Controles)));

    [Theory]
    [MemberData(nameof(Lecturas))]
    public async Task El_dueno_o_quien_tiene_el_privilegio_si_lo_ve(string caso, Actor actor)
    {
        var c = CatalogoLoPropio.Lecturas.Single(x => x.Nombre == caso);
        var url = await c.Url(e);

        var respuesta = await e.GetAsync(actor, url);

        Assert.True(respuesta.Exito, $"CASO MAL ARMADO: {actor} hizo GET {url} y respondió {respuesta}");
    }

    [Theory]
    [MemberData(nameof(Escrituras))]
    public async Task El_dueno_o_quien_tiene_el_privilegio_si_lo_hace(string caso, Actor actor)
    {
        var c = CatalogoLoPropio.TodasLasEscrituras.Single(x => x.Nombre == caso);
        var p = await c.Preparar(e);

        var antes = await p.Estado();
        var respuesta = await e.EnviarAsync(actor, c.Metodo, p.Url, p.Cuerpo);
        var despues = await p.Estado();

        Assert.True(respuesta.Exito, $"CASO MAL ARMADO: {actor} hizo {c.Metodo} {p.Url} y respondió {respuesta}");
        Assert.True(antes != despues, $"CASO MAL ARMADO: {actor} respondió {respuesta.Status} pero el registro no cambió; revisa Estado del caso.");
    }

    /// <summary>El techo no estorba: el gestor sí da un privilegio que él tiene.</summary>
    [Fact]
    public async Task El_gestor_si_da_un_privilegio_que_tiene()
    {
        var rolId = await e.RolAsync(e.A);

        var respuesta = await e.EnviarAsync(Actor.GestorRolesA, HttpMethod.Post, "/api/rol-privilegios",
            Escenario.ComoJson(new { rolId, privilegioId = await e.PrivilegioAsync("carro.ver") }));

        Assert.True(respuesta.Exito, $"El gestor debería poder dar carro.ver (sí lo tiene): {respuesta}");
    }

    private static TheoryData<string, Actor> Combinar(IEnumerable<(string Nombre, Actor[] Controles)> casos)
    {
        var datos = new TheoryData<string, Actor>();
        foreach (var (nombre, controles) in casos)
        foreach (var actor in controles)
            datos.Add(nombre, actor);
        return datos;
    }
}
