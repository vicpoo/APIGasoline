using APIGasoline.Api.IntegrationTests.Infraestructura;

namespace APIGasoline.Api.IntegrationTests.Matriz;

/// <summary>Datos que reciben las funciones de un caso para armar la URL y el cuerpo.</summary>
public sealed class Contexto
{
    public required Escenario E { get; init; }

    /// <summary>Quién hace la petición (el atacante o el dueño en el control).</summary>
    public required Actor Actor { get; init; }

    /// <summary>true = intento de la otra empresa; false = control con el dueño.</summary>
    public required bool EsAtaque { get; init; }

    /// <summary>Empresa del registro atacado (normalmente B).</summary>
    public required DatosEmpresa Objetivo { get; init; }

    /// <summary>Empresa de quien hace la petición.</summary>
    public DatosEmpresa DelActor => E.De(Actor);

    /// <summary>Registro creado para este intento (víctima).</summary>
    public Guid Id { get; init; }

    /// <summary>Segundo id de la víctima cuando hace falta (ej. el usuario de una sesión).</summary>
    public Guid IdRelacionado { get; init; }
}

/// <summary>Registro recién creado que se va a atacar.</summary>
public sealed record Victima(Guid Id, Guid IdRelacionado = default);

/// <summary>
/// Lectura de datos de otra empresa. Detalle: debe responder 403/404 sin datos.
/// Lista: puede responder 2xx, pero sin ningún registro de la otra empresa.
/// </summary>
public sealed record CasoLectura(string Nombre, string Plantilla, bool EsDetalle, Func<Escenario, string> Url)
{
    /// <summary>La respuesta es un archivo (PDF, Excel): no se puede buscar texto en ella.</summary>
    public bool RespuestaBinaria { get; init; }

    public override string ToString() => Nombre;
}

/// <summary>
/// Modificar o borrar un registro de otra empresa (o pasar uno propio a otra empresa).
/// Ataque: <see cref="Atacante"/> sobre una víctima nueva → no debe responder 2xx y el registro
/// debe quedar igual. Control: <see cref="Control"/> hace lo mismo sobre otra víctima nueva → debe
/// responder 2xx (si no, el caso está mal armado y el ataque "pasaría" por un 400 cualquiera).
/// </summary>
public sealed record CasoEscritura(
    string Nombre,
    HttpMethod Metodo,
    string Plantilla,
    Func<Escenario, DatosEmpresa, Task<Victima>> CrearVictima,
    Func<Contexto, string> Url,
    Func<Contexto, string> UrlDetalle,
    Func<Contexto, Task<HttpContent?>>? Cuerpo = null)
{
    public Actor Atacante { get; init; } = Actor.AdminA;

    /// <summary>Dueño que demuestra que la petición es válida. null = sin control (se explica en <see cref="SinControlPorque"/>).</summary>
    public Actor? Control { get; init; } = Actor.AdminB;

    public string? SinControlPorque { get; init; }

    /// <summary>De qué empresa es la víctima. "Mover a otra empresa" ataca un registro propio (A).</summary>
    public bool VictimaEsDelAtacante { get; init; }

    public override string ToString() => Nombre;
}

/// <summary>
/// Crear algo dentro de otra empresa (o colgado de registros de otra empresa).
/// Ataque: no debe responder 2xx y <see cref="Contar"/> no debe cambiar. Control: el dueño sí crea.
/// </summary>
public sealed record CasoCreacion(
    string Nombre,
    HttpMethod Metodo,
    string Plantilla,
    Func<Contexto, string> Url,
    Func<Contexto, Task<HttpContent>> Cuerpo,
    Func<Escenario, DatosEmpresa, Task<int>> Contar)
{
    public Actor Atacante { get; init; } = Actor.AdminA;
    public Actor Control { get; init; } = Actor.AdminB;

    public override string ToString() => Nombre;
}

/// <summary>Acción solo de plataforma: el admin de una empresa debe recibir 403.</summary>
public sealed record CasoSoloPlataforma(HttpMethod Metodo, string Plantilla, string Url)
{
    public override string ToString() => $"{Metodo} /{Plantilla}";
}
