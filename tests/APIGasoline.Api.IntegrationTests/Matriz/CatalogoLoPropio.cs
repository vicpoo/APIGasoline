using APIGasoline.Api.IntegrationTests.Infraestructura;
using Microsoft.EntityFrameworkCore;
using static APIGasoline.Api.IntegrationTests.Infraestructura.Victimas;

namespace APIGasoline.Api.IntegrationTests.Matriz;

/// <summary>Lo que se va a mandar en un caso, con una víctima recién creada.</summary>
/// <param name="Estado">Cómo se ve el registro víctima (para comprobar que el ataque no lo cambió).</param>
public sealed record Preparado(string Url, HttpContent? Cuerpo, Func<Task<string>> Estado);

/// <summary>Leer algo de otro empleado de la misma empresa.</summary>
public sealed record CasoPropioLectura(string Nombre, Func<Escenario, Task<string>> Url)
{
    public Actor Atacante { get; init; } = Actor.OperadorA2;

    /// <summary>Quienes SÍ pueden: el dueño y quien tiene el privilegio para lo de otros.</summary>
    public Actor[] Controles { get; init; } = [Actor.OperadorA, Actor.AdminA];

    public override string ToString() => Nombre;
}

/// <summary>Cambiar algo de otro empleado de la misma empresa (o dar más de lo que se tiene).</summary>
public sealed record CasoPropioEscritura(string Nombre, HttpMethod Metodo, Func<Escenario, Task<Preparado>> Preparar)
{
    public Actor Atacante { get; init; } = Actor.OperadorA2;

    public Actor[] Controles { get; init; } = [Actor.OperadorA, Actor.AdminA];

    public override string ToString() => Nombre;
}

/// <summary>
/// La matriz DENTRO de una empresa (revisión de código F002 a F005 y F007):
/// - Lo propio: el operador A2 intenta ver o tocar lo del operador A (viajes, GPS, cargas,
///   notificaciones, celular, solicitudes). Controles: el operador A (dueño) y el admin A (privilegio).
/// - Techo de privilegios: el gestor de roles de A (rol.gestionar + rol.asignar, pero sin los demás
///   privilegios) intenta dar o quitar más de lo que tiene. Control: el admin A.
/// </summary>
public static class CatalogoLoPropio
{
    private static readonly HttpMethod Get = HttpMethod.Get, Post = HttpMethod.Post, Put = HttpMethod.Put, Delete = HttpMethod.Delete;

    private static string Ahora(int minutos = 0) => Escenario.Fecha(DateTime.Now.AddMinutes(minutos));

    /// <summary>El registro visto por la plataforma (ve todo), normalizado para comparar.</summary>
    private static Func<Task<string>> Detalle(Escenario e, string url)
        => async () => FugasEscrituraTests.Normalizar(await e.GetAsync(Actor.Plataforma, url));

    private static Func<Task<string>> EnBd(Escenario e, Func<APIGasoline.Infrastructure.Persistence.AppDbContext, Task<string>> consulta)
        => () => e.EnBdAsync(consulta);

    private static HttpContent Json(object cuerpo) => Escenario.ComoJson(cuerpo);

    // ================================================================ LECTURAS (lo del operador A)

    public static IReadOnlyList<CasoPropioLectura> Lecturas { get; } =
    [
        new("Ver el viaje de otro operador", e => Task.FromResult($"/api/viajes/{e.A.ViajeId}")),
        new("Ver los puntos GPS del viaje de otro operador", e => Task.FromResult($"/api/viajes/{e.A.ViajeId}/puntos-ruta")),
        new("Descargar el GPX del viaje de otro operador", e => Task.FromResult($"/api/viajes/{e.A.ViajeId}/puntos-ruta/gpx")),
        new("Ver la ruta procesada del viaje de otro operador", e => Task.FromResult($"/api/viajes/{e.A.ViajeId}/ruta-procesada")),
        new("Ver el mapa del viaje de otro operador", e => Task.FromResult($"/api/viajes/{e.A.ViajeId}/ruta-procesada/mapa")),
        new("Ver un punto GPS de otro operador", e => Task.FromResult($"/api/puntos-ruta/{e.A.PuntoRutaId}")),
        new("Ver una carga de otro operador", e => Task.FromResult($"/api/cargas-combustible/{e.A.CargaId}")),
        new("Listar las cargas del viaje de otro operador", e => Task.FromResult($"/api/cargas-combustible?viajeId={e.A.ViajeId}")),
        new("Ver la notificación de otro empleado", async e => $"/api/notificaciones/{await e.NotificacionAsync(e.A, e.A.OperadorEmpleadoId)}"),
    ];

    // ================================================================ ESCRITURAS (lo del operador A)

    public static IReadOnlyList<CasoPropioEscritura> Escrituras { get; } =
    [
        new("Mandar puntos GPS al viaje de otro operador", Post, async e =>
        {
            var viajeId = await e.ViajeAsync(e.A);
            return new Preparado($"/api/viajes/{viajeId}/puntos-ruta",
                Json(new { puntos = new[] { new { latitud = 16.7m, longitud = -93.1m, timestampCaptura = Ahora(-20), precisionMetros = 5 } } }),
                Detalle(e, $"/api/viajes/{viajeId}/puntos-ruta"));
        }),
        new("Cambiar la foto del tablero del viaje de otro operador", Put, async e =>
        {
            var viajeId = await e.ViajeAsync(e.A);
            return new Preparado($"/api/viajes/{viajeId}/foto-tablero/inicial",
                Escenario.Formulario([], "archivo"),
                Detalle(e, $"/api/viajes/{viajeId}"));
        }),
        new("Pausar el viaje de otro operador", Post, async e =>
        {
            var viajeId = await e.ViajeAsync(e.A);
            return new Preparado($"/api/viajes/{viajeId}/pausar", Json(new { motivo = "Pausa ajena" }), Detalle(e, $"/api/viajes/{viajeId}"));
        }),
        new("Marcar como leída la notificación de otro empleado", Post, async e =>
        {
            var id = await e.NotificacionAsync(e.A, e.A.OperadorEmpleadoId);
            return new Preparado($"/api/notificaciones/{id}/marcar-leida", null, Detalle(e, $"/api/notificaciones/{id}"));
        }),
        new("Borrar la notificación de otro empleado", Delete, async e =>
        {
            var id = await e.NotificacionAsync(e.A, e.A.OperadorEmpleadoId);
            return new Preparado($"/api/notificaciones/{id}", null, Detalle(e, $"/api/notificaciones/{id}"));
        }),
        new("Desactivar el celular de otro empleado", Delete, async e =>
        {
            var id = await e.DispositivoDeAsync(e.A, e.A.OperadorEmpleadoId);
            return new Preparado($"/api/dispositivos/{id}", null,
                EnBd(e, async c => (await c.Dispositivos.SingleAsync(x => x.DispositivoEmpleadoId == id)).Activo.ToString()));
        }),
        new("Cancelar la solicitud de otro operador", Post, async e =>
        {
            var id = await e.SolicitudPendienteAsync(e.A);
            return new Preparado($"/api/solicitudes-reserva/{id}/cancelar", Json(new { motivo = "Cancelación ajena" }),
                Detalle(e, $"/api/solicitudes-reserva/{id}"));
        }),
    ];

    // ================================================================ TECHO DE PRIVILEGIOS (gestor de roles de A)

    private const string PrivilegioQueNoTiene = "viaje.eliminar";

    public static IReadOnlyList<CasoPropioEscritura> Techo { get; } =
    [
        new("Dar a un rol un privilegio que no tengo", Post, async e =>
        {
            var rolId = await e.RolAsync(e.A);
            return new Preparado("/api/rol-privilegios", Json(new { rolId, privilegioId = await e.PrivilegioAsync(PrivilegioQueNoTiene) }),
                EnBd(e, async c => (await c.RolPrivilegios.CountAsync(x => x.RolId == rolId)).ToString()));
        }) { Atacante = Actor.GestorRolesA, Controles = [Actor.AdminA] },

        new("Cambiar el privilegio de un rol por uno que no tengo", Put, async e =>
        {
            var id = await e.RolPrivilegioAsync(e.A); // carro.ver: ese sí lo tiene
            var rolId = await e.EnBdAsync(async c => (await c.RolPrivilegios.SingleAsync(x => x.RolPrivilegioId == id)).RolId);
            return new Preparado($"/api/rol-privilegios/{id}", Json(new { rolId, privilegioId = await e.PrivilegioAsync(PrivilegioQueNoTiene) }),
                EnBd(e, async c => (await c.RolPrivilegios.SingleAsync(x => x.RolPrivilegioId == id)).PrivilegioId.ToString()));
        }) { Atacante = Actor.GestorRolesA, Controles = [Actor.AdminA] },

        new("Quitar a un rol un privilegio que no tengo", Delete, async e =>
        {
            var id = await e.RolPrivilegioAsync(e.A, PrivilegioQueNoTiene);
            return new Preparado($"/api/rol-privilegios/{id}", null,
                EnBd(e, async c => (await c.RolPrivilegios.AnyAsync(x => x.RolPrivilegioId == id)).ToString()));
        }) { Atacante = Actor.GestorRolesA, Controles = [Actor.AdminA] },

        new("Darle a alguien el rol de administrador", Post, async e =>
        {
            var (usuarioId, _) = await e.EmpleadoAsync(e.A);
            return new Preparado("/api/usuario-roles", Json(new { usuarioId, rolId = e.A.RolAdminId }),
                EnBd(e, async c => (await c.UsuarioRoles.CountAsync(x => x.UsuarioId == usuarioId)).ToString()));
        }) { Atacante = Actor.GestorRolesA, Controles = [Actor.AdminA] },

        new("Quitarle a alguien el rol de administrador", Delete, async e =>
        {
            var id = await e.UsuarioConRolAsync(e.A, e.A.RolAdminId);
            return new Preparado($"/api/usuario-roles/{id}", null,
                EnBd(e, async c => (await c.UsuarioRoles.AnyAsync(x => x.UsuarioRolId == id)).ToString()));
        }) { Atacante = Actor.GestorRolesA, Controles = [Actor.AdminA] },

        new("Borrar un rol con privilegios que no tengo", Delete, async e =>
        {
            var rolId = await RolConPrivilegioQueNoTieneAsync(e);
            return new Preparado($"/api/roles/{rolId}", null,
                EnBd(e, async c => (await c.Roles.AnyAsync(x => x.RolId == rolId)).ToString()));
        }) { Atacante = Actor.GestorRolesA, Controles = [Actor.AdminA] },

        new("Editar un rol con privilegios que no tengo", Put, async e =>
        {
            var rolId = await RolConPrivilegioQueNoTieneAsync(e);
            return new Preparado($"/api/roles/{rolId}", Json(new { nombre = Unico("Rol editado "), descripcion = "Editado", activo = true }),
                EnBd(e, async c => (await c.Roles.SingleAsync(x => x.RolId == rolId)).Nombre));
        }) { Atacante = Actor.GestorRolesA, Controles = [Actor.AdminA] },
    ];

    private static async Task<Guid> RolConPrivilegioQueNoTieneAsync(Escenario e)
    {
        var id = await e.RolPrivilegioAsync(e.A, PrivilegioQueNoTiene);
        return await e.EnBdAsync(async c => (await c.RolPrivilegios.SingleAsync(x => x.RolPrivilegioId == id)).RolId);
    }

    /// <summary>Escrituras de lo propio más las del techo de privilegios.</summary>
    public static IReadOnlyList<CasoPropioEscritura> TodasLasEscrituras { get; } = [.. Escrituras, .. Techo];
}
