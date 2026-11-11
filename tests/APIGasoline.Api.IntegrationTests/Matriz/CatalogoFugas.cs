using APIGasoline.Api.IntegrationTests.Infraestructura;
using Microsoft.EntityFrameworkCore;
using static APIGasoline.Api.IntegrationTests.Infraestructura.Victimas;

namespace APIGasoline.Api.IntegrationTests.Matriz;

/// <summary>
/// La matriz: qué intentos de una empresa sobre otra se prueban, endpoint por endpoint.
/// Si agregas un endpoint, agrégalo aquí (o a <see cref="Excluidos"/> con su razón):
/// CoberturaEndpointsTests falla si alguno queda fuera.
/// </summary>
public static class CatalogoFugas
{
    private static readonly HttpMethod Get = HttpMethod.Get, Post = HttpMethod.Post, Put = HttpMethod.Put, Delete = HttpMethod.Delete;

    private static Task<HttpContent?> J(object cuerpo) => Task.FromResult<HttpContent?>(Escenario.ComoJson(cuerpo));
    private static Task<HttpContent> JC(object cuerpo) => Task.FromResult<HttpContent>(Escenario.ComoJson(cuerpo));
    private static string Ahora(int minutos = 0) => Escenario.Fecha(DateTime.Now.AddMinutes(minutos));
    private static Victima V(Guid id, Guid relacionado = default) => new(id, relacionado);

    private static Task<string> PlacasAsync(Escenario e, Guid carroId) => e.EnBdAsync(async c => (await c.Carros.SingleAsync(x => x.CarroId == carroId)).Placas);
    private static Task<string> ClaveAsync(Escenario e, Guid centroId) => e.EnBdAsync(async c => (await c.CentrosCosto.SingleAsync(x => x.CentroCostoId == centroId)).Clave);
    private static Task<string> EmailAsync(Escenario e, Guid usuarioId) => e.EnBdAsync(async c => (await c.Usuarios.SingleAsync(x => x.UsuarioId == usuarioId)).Email);

    // ================================================================ LECTURAS

    public static IReadOnlyList<CasoLectura> Lecturas { get; } =
    [
        // --- Detalle de un registro de B
        new("Ver carro de B", "api/carros/{carroId:guid}", true, e => $"/api/carros/{e.B.Carro1Id}"),
        new("Ver centro de costo de B", "api/centros-costo/{centroCostoId:guid}", true, e => $"/api/centros-costo/{e.B.CentroCostoId}"),
        new("Ver rol de B", "api/roles/{rolId:guid}", true, e => $"/api/roles/{e.B.RolOperadorId}"),
        new("Ver empleado de B", "api/empleados/{empleadoId:guid}", true, e => $"/api/empleados/{e.B.OperadorEmpleadoId}"),
        new("Ver empleado de B por su usuario", "api/empleados/por-usuario/{usuarioId:guid}", true, e => $"/api/empleados/por-usuario/{e.B.OperadorUsuarioId}"),
        new("Ver dispositivos de un empleado de B", "api/empleados/{empleadoId:guid}/dispositivos", true, e => $"/api/empleados/{e.B.OperadorEmpleadoId}/dispositivos"),
        new("Ver solicitud de B", "api/solicitudes-reserva/{solicitudReservaId:guid}", true, e => $"/api/solicitudes-reserva/{e.B.SolicitudId}"),
        new("Ver viaje de B", "api/viajes/{viajeId:guid}", true, e => $"/api/viajes/{e.B.ViajeId}"),
        new("Ver puntos GPS de un viaje de B", "api/viajes/{viajeId:guid}/puntos-ruta", true, e => $"/api/viajes/{e.B.ViajeId}/puntos-ruta"),
        new("Descargar GPX de un viaje de B", "api/viajes/{viajeId:guid}/puntos-ruta/gpx", true, e => $"/api/viajes/{e.B.ViajeId}/puntos-ruta/gpx"),
        new("Ver ruta procesada de un viaje de B", "api/viajes/{viajeId:guid}/ruta-procesada", true, e => $"/api/viajes/{e.B.ViajeId}/ruta-procesada"),
        new("Ver mapa de un viaje de B", "api/viajes/{viajeId:guid}/ruta-procesada/mapa", true, e => $"/api/viajes/{e.B.ViajeId}/ruta-procesada/mapa"),
        new("Ver punto GPS de B", "api/puntos-ruta/{puntoRutaId:guid}", true, e => $"/api/puntos-ruta/{e.B.PuntoRutaId}"),
        new("Ver carga de combustible de B", "api/cargas-combustible/{cargaCombustibleId:guid}", true, e => $"/api/cargas-combustible/{e.B.CargaId}"),
        new("Ver cambio de estatus de B", "api/cambios-estatus-carro/{cambioEstatusCarroId:guid}", true, e => $"/api/cambios-estatus-carro/{e.B.CambioEstatusId}"),
        new("Ver notificación de B", "api/notificaciones/{notificacionId:guid}", true, e => $"/api/notificaciones/{e.B.NotificacionId}"),
        new("Contar no leídas de un empleado de B", "api/notificaciones/no-leidas/{empleadoDestinatarioId:guid}", true, e => $"/api/notificaciones/no-leidas/{e.B.AdminEmpleadoId}"),
        new("Ver usuario de B", "api/usuarios/{usuarioId:guid}", true, e => $"/api/usuarios/{e.B.OperadorUsuarioId}"),
        new("Ver sesiones de un usuario de B", "api/usuarios/{usuarioId:guid}/sesiones", true, e => $"/api/usuarios/{e.B.OperadorUsuarioId}/sesiones"),
        new("Ver asignación de rol de B", "api/usuario-roles/{usuarioRolId:guid}", true, e => $"/api/usuario-roles/{e.B.UsuarioRolId}"),
        new("Ver privilegio de un rol de B", "api/rol-privilegios/{rolPrivilegioId:guid}", true, e => $"/api/rol-privilegios/{e.B.RolPrivilegioId}"),
        new("Ver la empresa B", "api/empresas/{empresaId:guid}", true, e => $"/api/empresas/{e.B.EmpresaId}"),
        new("Ver evento de auditoría de B", "api/auditoria/eventos/{auditoriaEventoId:guid}", true, e => $"/api/auditoria/eventos/{e.B.AuditoriaEventoId}"),
        new("Ver revisión de auditoría de B", "api/auditoria/revisiones/{revisionAuditoriaId:guid}", true, e => $"/api/auditoria/revisiones/{e.B.RevisionId}"),

        // --- Listas: sin filtro y pidiendo la empresa B
        new("Listar carros", "api/carros", false, _ => "/api/carros"),
        new("Listar carros de B", "api/carros", false, e => $"/api/carros?empresaId={e.B.EmpresaId}"),
        new("Listar centros de costo", "api/centros-costo", false, _ => "/api/centros-costo"),
        new("Listar centros de costo de B", "api/centros-costo", false, e => $"/api/centros-costo?empresaId={e.B.EmpresaId}"),
        new("Listar roles", "api/roles", false, _ => "/api/roles"),
        new("Listar roles de B", "api/roles", false, e => $"/api/roles?empresaId={e.B.EmpresaId}"),
        new("Listar empleados", "api/empleados", false, _ => "/api/empleados"),
        new("Listar empleados de B", "api/empleados", false, e => $"/api/empleados?empresaId={e.B.EmpresaId}"),
        new("Listar solicitudes", "api/solicitudes-reserva", false, _ => "/api/solicitudes-reserva"),
        new("Listar solicitudes de B", "api/solicitudes-reserva", false, e => $"/api/solicitudes-reserva?empresaId={e.B.EmpresaId}"),
        new("Listar solicitudes de un carro de B", "api/solicitudes-reserva", false, e => $"/api/solicitudes-reserva?carroId={e.B.Carro1Id}"),
        new("Listar viajes", "api/viajes", false, _ => "/api/viajes"),
        new("Listar viajes de B", "api/viajes", false, e => $"/api/viajes?empresaId={e.B.EmpresaId}"),
        new("Listar viajes de un carro de B", "api/viajes", false, e => $"/api/viajes?carroId={e.B.Carro1Id}"),
        new("Listar viajes de un operador de B", "api/viajes", false, e => $"/api/viajes?empleadoOperadorId={e.B.OperadorEmpleadoId}"),
        new("Listar cargas", "api/cargas-combustible", false, _ => "/api/cargas-combustible"),
        new("Listar cargas de B", "api/cargas-combustible", false, e => $"/api/cargas-combustible?empresaId={e.B.EmpresaId}"),
        new("Listar cargas de un viaje de B", "api/cargas-combustible", false, e => $"/api/cargas-combustible?viajeId={e.B.ViajeId}"),
        new("Listar cambios de estatus", "api/cambios-estatus-carro", false, _ => "/api/cambios-estatus-carro"),
        new("Listar cambios de estatus de B", "api/cambios-estatus-carro", false, e => $"/api/cambios-estatus-carro?empresaId={e.B.EmpresaId}"),
        new("Listar notificaciones", "api/notificaciones", false, _ => "/api/notificaciones"),
        new("Listar notificaciones de B", "api/notificaciones", false, e => $"/api/notificaciones?empresaId={e.B.EmpresaId}"),
        new("Listar notificaciones de un empleado de B", "api/notificaciones", false, e => $"/api/notificaciones?empleadoDestinatarioId={e.B.AdminEmpleadoId}"),
        new("Listar usuarios", "api/usuarios", false, _ => "/api/usuarios"),
        new("Listar asignaciones de roles", "api/usuario-roles", false, _ => "/api/usuario-roles"),
        new("Listar asignaciones de un usuario de B", "api/usuario-roles", false, e => $"/api/usuario-roles?usuarioId={e.B.OperadorUsuarioId}"),
        new("Listar asignaciones de un rol de B", "api/usuario-roles", false, e => $"/api/usuario-roles?rolId={e.B.RolOperadorId}"),
        new("Listar privilegios de roles", "api/rol-privilegios", false, _ => "/api/rol-privilegios"),
        new("Listar privilegios de un rol de B", "api/rol-privilegios", false, e => $"/api/rol-privilegios?rolId={e.B.RolOperadorId}"),
        new("Listar empresas", "api/empresas", false, _ => "/api/empresas"),
        new("Listar eventos de auditoría", "api/auditoria/eventos", false, _ => "/api/auditoria/eventos"),
        new("Listar eventos de auditoría de B", "api/auditoria/eventos", false, e => $"/api/auditoria/eventos?empresaId={e.B.EmpresaId}"),
        new("Listar eventos de un viaje de B", "api/auditoria/eventos", false, e => $"/api/auditoria/eventos?entidadId={e.B.ViajeId}"),
        new("Listar revisiones", "api/auditoria/revisiones", false, _ => "/api/auditoria/revisiones"),
        new("Listar revisiones de B", "api/auditoria/revisiones", false, e => $"/api/auditoria/revisiones?empresaId={e.B.EmpresaId}"),
        new("Listar revisiones de un viaje de B", "api/auditoria/revisiones", false, e => $"/api/auditoria/revisiones?viajeId={e.B.ViajeId}"),
        new("Revisión de combustible", "api/auditoria/revision-combustible", false, _ => $"/api/auditoria/revision-combustible?desde={Ahora(-1440)}&hasta={Ahora(60)}"),
        new("Revisión de combustible de B", "api/auditoria/revision-combustible", false, e => $"/api/auditoria/revision-combustible?empresaId={e.B.EmpresaId}&desde={Ahora(-1440)}&hasta={Ahora(60)}"),
        new("Revisión de combustible de un carro de B", "api/auditoria/revision-combustible", false, e => $"/api/auditoria/revision-combustible?carroId={e.B.Carro1Id}&desde={Ahora(-1440)}&hasta={Ahora(60)}"),
        new("Reporte de la flota", "api/reportes/flota", false, _ => "/api/reportes/flota"),
        new("Reporte de la flota de B", "api/reportes/flota", false, e => $"/api/reportes/flota?empresaId={e.B.EmpresaId}"),
        new("Reporte de un carro de B", "api/reportes/flota", false, e => $"/api/reportes/flota?carroId={e.B.Carro1Id}"),
        // PDF y Excel: el contenido es binario, así que pedir datos de B debe fallar (no se puede revisar el texto)
        new("Reporte PDF de B", "api/reportes/flota/pdf", true, e => $"/api/reportes/flota/pdf?empresaId={e.B.EmpresaId}") { RespuestaBinaria = true },
        new("Reporte PDF de un carro de B", "api/reportes/flota/pdf", true, e => $"/api/reportes/flota/pdf?carroId={e.B.Carro1Id}") { RespuestaBinaria = true },
        new("Reporte Excel de B", "api/reportes/flota/excel", true, e => $"/api/reportes/flota/excel?empresaId={e.B.EmpresaId}") { RespuestaBinaria = true },
        new("Reporte Excel de un carro de B", "api/reportes/flota/excel", true, e => $"/api/reportes/flota/excel?carroId={e.B.Carro1Id}") { RespuestaBinaria = true },
        new("Alertas de rutas", "api/rutas-procesadas/alertas", false, _ => "/api/rutas-procesadas/alertas"),
        new("Alertas de rutas de B", "api/rutas-procesadas/alertas", false, e => $"/api/rutas-procesadas/alertas?empresaId={e.B.EmpresaId}"),
        new("Listar asignaciones de carros", "api/asignaciones", false, _ => "/api/asignaciones"),
        new("Listar asignaciones de carros de B", "api/asignaciones", false, e => $"/api/asignaciones?empresaId={e.B.EmpresaId}"),
    ];

    // ================================================================ ESCRITURAS

    private static Func<Contexto, Task<HttpContent?>> CuerpoCarro(Func<Contexto, Guid> empresa) => async c => Escenario.ComoJson(new
    {
        empresaId = empresa(c),
        marca = "Marca",
        modelo = "Modelo",
        anio = 2022,
        color = "Rojo",
        placas = await PlacasAsync(c.E, c.Id),
        capacidadTanque = 41,
        kilometrajeActual = 35000,
        tipoVehiculo = "carro",
    });

    private static Func<Contexto, Task<HttpContent?>> CuerpoCentro(Func<Contexto, Guid> empresa) => async c => Escenario.ComoJson(new
    {
        empresaId = empresa(c),
        nombre = "Centro víctima",
        clave = await ClaveAsync(c.E, c.Id),
        activo = true,
    });

    private static Func<Contexto, Task<HttpContent?>> CuerpoEmpleado(Func<Contexto, Guid> empresa) => c => J(new
    {
        empresaId = empresa(c),
        usuarioId = c.IdRelacionado,
        nombreCompleto = "Empleado víctima",
    });

    private static Func<Contexto, Task<HttpContent?>> Archivo(params (string Campo, string Valor)[] campos)
        => c => Task.FromResult<HttpContent?>(Escenario.Formulario(
            campos.Select(x => new KeyValuePair<string, string?>(x.Campo, x.Valor)), "archivo"));

    private static async Task<Victima> EmpleadoV(Escenario e, DatosEmpresa d)
    {
        var (usuarioId, empleadoId) = await e.EmpleadoAsync(d);
        return V(empleadoId, usuarioId);
    }

    private static async Task<Victima> UsuarioV(Escenario e, DatosEmpresa d)
    {
        var (usuarioId, empleadoId) = await e.EmpleadoAsync(d);
        return V(usuarioId, empleadoId);
    }

    public static IReadOnlyList<CasoEscritura> Escrituras { get; } =
    [
        // --- Carros
        new("Editar carro de B", Put, "api/carros/{carroId:guid}",
            async (e, d) => V(await e.CarroAsync(d)), c => $"/api/carros/{c.Id}", c => $"/api/carros/{c.Id}",
            CuerpoCarro(c => c.Objetivo.EmpresaId)),
        new("Quitarle un carro a B (cambiar su empresaId a A)", Put, "api/carros/{carroId:guid}",
            async (e, d) => V(await e.CarroAsync(d)), c => $"/api/carros/{c.Id}", c => $"/api/carros/{c.Id}",
            CuerpoCarro(c => c.EsAtaque ? c.DelActor.EmpresaId : c.Objetivo.EmpresaId)),
        new("Pasar un carro propio a la empresa B", Put, "api/carros/{carroId:guid}",
            async (e, d) => V(await e.CarroAsync(d)), c => $"/api/carros/{c.Id}", c => $"/api/carros/{c.Id}",
            CuerpoCarro(c => c.EsAtaque ? c.E.B.EmpresaId : c.Objetivo.EmpresaId))
        { VictimaEsDelAtacante = true, Control = Actor.AdminA },
        new("Subir foto a carro de B", Put, "api/carros/{carroId:guid}/foto",
            async (e, d) => V(await e.CarroAsync(d)), c => $"/api/carros/{c.Id}/foto", c => $"/api/carros/{c.Id}", Archivo()),
        new("Borrar foto de carro de B", Delete, "api/carros/{carroId:guid}/foto",
            async (e, d) => V(await e.CarroAsync(d, foto: $"empresa/{d.EmpresaId}/carro/foto-{Unico()}.jpg")),
            c => $"/api/carros/{c.Id}/foto", c => $"/api/carros/{c.Id}"),
        new("Dar de baja carro de B", Post, "api/carros/{carroId:guid}/baja",
            async (e, d) => V(await e.CarroAsync(d)), c => $"/api/carros/{c.Id}/baja", c => $"/api/carros/{c.Id}",
            _ => J(new { motivo = "Baja de prueba" })),
        new("Eliminar carro de B", Delete, "api/carros/{carroId:guid}",
            async (e, d) => V(await e.CarroAsync(d)), c => $"/api/carros/{c.Id}", c => $"/api/carros/{c.Id}"),

        // --- Centros de costo
        new("Editar centro de costo de B", Put, "api/centros-costo/{centroCostoId:guid}",
            async (e, d) => V(await e.CentroCostoAsync(d)), c => $"/api/centros-costo/{c.Id}", c => $"/api/centros-costo/{c.Id}",
            CuerpoCentro(c => c.Objetivo.EmpresaId)),
        new("Quitarle un centro de costo a B", Put, "api/centros-costo/{centroCostoId:guid}",
            async (e, d) => V(await e.CentroCostoAsync(d)), c => $"/api/centros-costo/{c.Id}", c => $"/api/centros-costo/{c.Id}",
            CuerpoCentro(c => c.EsAtaque ? c.DelActor.EmpresaId : c.Objetivo.EmpresaId)),
        new("Pasar un centro de costo propio a la empresa B", Put, "api/centros-costo/{centroCostoId:guid}",
            async (e, d) => V(await e.CentroCostoAsync(d)), c => $"/api/centros-costo/{c.Id}", c => $"/api/centros-costo/{c.Id}",
            CuerpoCentro(c => c.EsAtaque ? c.E.B.EmpresaId : c.Objetivo.EmpresaId))
        { VictimaEsDelAtacante = true, Control = Actor.AdminA },
        new("Eliminar centro de costo de B", Delete, "api/centros-costo/{centroCostoId:guid}",
            async (e, d) => V(await e.CentroCostoAsync(d)), c => $"/api/centros-costo/{c.Id}", c => $"/api/centros-costo/{c.Id}"),

        // --- Roles y privilegios
        new("Editar rol de B", Put, "api/roles/{rolId:guid}",
            async (e, d) => V(await e.RolAsync(d)), c => $"/api/roles/{c.Id}", c => $"/api/roles/{c.Id}",
            _ => J(new { nombre = Unico("Rol "), descripcion = "Editado", activo = true })),
        new("Eliminar rol de B", Delete, "api/roles/{rolId:guid}",
            async (e, d) => V(await e.RolAsync(d)), c => $"/api/roles/{c.Id}", c => $"/api/roles/{c.Id}"),
        new("Reasignar un usuario de B a otro rol", Put, "api/usuario-roles/{usuarioRolId:guid}",
            async (e, d) => V(await e.UsuarioRolAsync(d)), c => $"/api/usuario-roles/{c.Id}", c => $"/api/usuario-roles/{c.Id}",
            async c =>
            {
                var usuarioId = await c.E.EnBdAsync(async x => (await x.UsuarioRoles.SingleAsync(u => u.UsuarioRolId == c.Id)).UsuarioId);
                return Escenario.ComoJson(new { usuarioId, rolId = c.Objetivo.RolAdminId });
            }),
        new("Quitar un rol a un usuario de B", Delete, "api/usuario-roles/{usuarioRolId:guid}",
            async (e, d) => V(await e.UsuarioRolAsync(d)), c => $"/api/usuario-roles/{c.Id}", c => $"/api/usuario-roles/{c.Id}"),
        new("Cambiar privilegio de un rol de B", Put, "api/rol-privilegios/{rolPrivilegioId:guid}",
            async (e, d) => V(await e.RolPrivilegioAsync(d)), c => $"/api/rol-privilegios/{c.Id}", c => $"/api/rol-privilegios/{c.Id}",
            async c =>
            {
                var rolId = await c.E.EnBdAsync(async x => (await x.RolPrivilegios.SingleAsync(r => r.RolPrivilegioId == c.Id)).RolId);
                return Escenario.ComoJson(new { rolId, privilegioId = await c.E.PrivilegioAsync() });
            }),
        new("Quitar privilegio a un rol de B", Delete, "api/rol-privilegios/{rolPrivilegioId:guid}",
            async (e, d) => V(await e.RolPrivilegioAsync(d)), c => $"/api/rol-privilegios/{c.Id}", c => $"/api/rol-privilegios/{c.Id}"),

        // --- Empleados, usuarios y sesiones
        new("Editar empleado de B", Put, "api/empleados/{empleadoId:guid}",
            EmpleadoV, c => $"/api/empleados/{c.Id}", c => $"/api/empleados/{c.Id}", CuerpoEmpleado(c => c.Objetivo.EmpresaId)),
        new("Pasar un empleado propio a la empresa B", Put, "api/empleados/{empleadoId:guid}",
            EmpleadoV, c => $"/api/empleados/{c.Id}", c => $"/api/empleados/{c.Id}",
            CuerpoEmpleado(c => c.EsAtaque ? c.E.B.EmpresaId : c.Objetivo.EmpresaId))
        { VictimaEsDelAtacante = true, Control = Actor.AdminA },
        new("Subir foto de perfil a empleado de B", Put, "api/empleados/{empleadoId:guid}/foto-perfil",
            EmpleadoV, c => $"/api/empleados/{c.Id}/foto-perfil", c => $"/api/empleados/{c.Id}", Archivo()),
        new("Subir licencia a empleado de B", Put, "api/empleados/{empleadoId:guid}/foto-licencia",
            EmpleadoV, c => $"/api/empleados/{c.Id}/foto-licencia", c => $"/api/empleados/{c.Id}", Archivo()),
        new("Borrar foto de perfil de empleado de B", Delete, "api/empleados/{empleadoId:guid}/foto-perfil",
            EmpleadoV, c => $"/api/empleados/{c.Id}/foto-perfil", c => $"/api/empleados/{c.Id}"),
        new("Borrar licencia de empleado de B", Delete, "api/empleados/{empleadoId:guid}/foto-licencia",
            EmpleadoV, c => $"/api/empleados/{c.Id}/foto-licencia", c => $"/api/empleados/{c.Id}"),
        new("Eliminar empleado de B", Delete, "api/empleados/{empleadoId:guid}",
            EmpleadoV, c => $"/api/empleados/{c.Id}", c => $"/api/empleados/{c.Id}"),
        new("Editar usuario de B (cambiarle el correo)", Put, "api/usuarios/{usuarioId:guid}",
            UsuarioV, c => $"/api/usuarios/{c.Id}", c => $"/api/usuarios/{c.Id}",
            async c => Escenario.ComoJson(new
            {
                email = c.EsAtaque ? $"{Unico("robado.")}@pruebas.local".ToLowerInvariant() : await EmailAsync(c.E, c.Id),
                estado = "activo",
            })),
        new("Cambiar contraseña de usuario de B", Post, "api/usuarios/{usuarioId:guid}/cambiar-password",
            UsuarioV, c => $"/api/usuarios/{c.Id}/cambiar-password", c => $"/api/usuarios/{c.Id}",
            _ => J(new { passwordActual = PasswordVictima, passwordNueva = "Nueva#Pruebas2026" })),
        // La API no deja borrar un usuario con empleado: la víctima es de B solo por su rol
        new("Eliminar usuario de B (sin empleado, con rol de B)", Delete, "api/usuarios/{usuarioId:guid}",
            async (e, d) =>
            {
                var usuarioId = await e.UsuarioLibreAsync(d);
                await e.EnBdAsync(async x =>
                {
                    x.UsuarioRoles.Add(APIGasoline.Domain.Entities.UsuarioRol.Crear(usuarioId, d.RolOperadorId));
                    return await x.SaveChangesAsync();
                });
                return V(usuarioId);
            },
            c => $"/api/usuarios/{c.Id}", c => $"/api/usuarios/{c.Id}"),
        new("Cerrar una sesión de un usuario de B", Delete, "api/usuarios/{usuarioId:guid}/sesiones/{sesionId:guid}",
            async (e, d) => { var (s, u) = await e.SesionAsync(d); return V(s, u); },
            c => $"/api/usuarios/{c.IdRelacionado}/sesiones/{c.Id}", c => $"/api/usuarios/{c.IdRelacionado}/sesiones"),
        new("Cerrar todas las sesiones de un usuario de B", Post, "api/usuarios/{usuarioId:guid}/sesiones/cerrar-todas",
            async (e, d) => { var (s, u) = await e.SesionAsync(d); return V(u, s); },
            c => $"/api/usuarios/{c.Id}/sesiones/cerrar-todas", c => $"/api/usuarios/{c.Id}/sesiones"),
        new("Cerrar una sesión de B como si fuera propia", Delete, "api/auth/sesiones/{sesionId:guid}",
            async (e, d) => { var (s, u) = await e.SesionAsync(d); return V(s, u); },
            c => $"/api/auth/sesiones/{c.Id}", c => $"/api/usuarios/{c.IdRelacionado}/sesiones")
        { Control = null, SinControlPorque = "solo el dueño de la sesión puede cerrarla por aquí y la víctima no tiene token" },
        new("Desactivar dispositivo de B", Delete, "api/dispositivos/{dispositivoEmpleadoId:guid}",
            async (e, d) => { var (disp, emp) = await e.DispositivoAsync(d); return V(disp, emp); },
            c => $"/api/dispositivos/{c.Id}", c => $"/api/empleados/{c.IdRelacionado}/dispositivos"),

        // --- Empresa
        new("Editar la empresa B", Put, "api/empresas/{empresaId:guid}",
            (e, d) => Task.FromResult(V(d.EmpresaId)), c => $"/api/empresas/{c.Id}", c => $"/api/empresas/{c.Id}",
            c => J(new { nombre = c.EsAtaque ? "Empresa tomada" : $"Empresa Demo {c.Objetivo.Letra}", activo = true })),

        // --- Solicitudes y asignaciones
        new("Aprobar solicitud de B", Post, "api/solicitudes-reserva/{solicitudReservaId:guid}/aprobar",
            async (e, d) => V(await e.SolicitudPendienteAsync(d)), c => $"/api/solicitudes-reserva/{c.Id}/aprobar", c => $"/api/solicitudes-reserva/{c.Id}",
            c => J(new { empleadoConfirmadorId = c.DelActor.AdminEmpleadoId })),
        new("Rechazar solicitud de B", Post, "api/solicitudes-reserva/{solicitudReservaId:guid}/rechazar",
            async (e, d) => V(await e.SolicitudPendienteAsync(d)), c => $"/api/solicitudes-reserva/{c.Id}/rechazar", c => $"/api/solicitudes-reserva/{c.Id}",
            c => J(new { empleadoConfirmadorId = c.DelActor.AdminEmpleadoId, motivo = "Rechazo de prueba" })),
        new("Cancelar solicitud de B", Post, "api/solicitudes-reserva/{solicitudReservaId:guid}/cancelar",
            async (e, d) => V(await e.SolicitudPendienteAsync(d)), c => $"/api/solicitudes-reserva/{c.Id}/cancelar", c => $"/api/solicitudes-reserva/{c.Id}",
            _ => J(new { motivo = "Cancelación de prueba" })),
        new("Liberar carro de una solicitud de B", Post, "api/solicitudes-reserva/{solicitudReservaId:guid}/liberar",
            async (e, d) => V((await e.SolicitudAprobadaAsync(d)).SolicitudId), c => $"/api/solicitudes-reserva/{c.Id}/liberar", c => $"/api/solicitudes-reserva/{c.Id}",
            _ => J(new { observaciones = "Liberación de prueba" })),
        new("Eliminar solicitud de B", Delete, "api/solicitudes-reserva/{solicitudReservaId:guid}",
            async (e, d) => V(await e.SolicitudPendienteAsync(d)), c => $"/api/solicitudes-reserva/{c.Id}", c => $"/api/solicitudes-reserva/{c.Id}"),
        new("Desvincular carro asignado en B", Post, "api/asignaciones/{solicitudReservaId:guid}/desvincular",
            async (e, d) => V((await e.SolicitudAprobadaAsync(d)).SolicitudId), c => $"/api/asignaciones/{c.Id}/desvincular", c => $"/api/solicitudes-reserva/{c.Id}",
            _ => J(new { motivo = "Desvinculación de prueba" })),

        // --- Viajes
        new("Pausar viaje de B", Post, "api/viajes/{viajeId:guid}/pausar",
            async (e, d) => V(await e.ViajeAsync(d)), c => $"/api/viajes/{c.Id}/pausar", c => $"/api/viajes/{c.Id}",
            _ => J(new { motivo = "Pausa de prueba" }))
        { Control = Actor.OperadorB },
        new("Reanudar viaje de B", Post, "api/viajes/{viajeId:guid}/reanudar",
            async (e, d) => V(await e.ViajeAsync(d, EstadoVictima.Pausado)), c => $"/api/viajes/{c.Id}/reanudar", c => $"/api/viajes/{c.Id}",
            _ => J(new { }))
        { Control = Actor.OperadorB },
        new("Finalizar viaje de B", Post, "api/viajes/{viajeId:guid}/finalizar",
            async (e, d) => V(await e.ViajeAsync(d)), c => $"/api/viajes/{c.Id}/finalizar", c => $"/api/viajes/{c.Id}",
            _ => J(new { fechaFin = Ahora(), kilometrajeFinal = 35020 }))
        { Control = Actor.OperadorB },
        new("Interrumpir viaje de B", Post, "api/viajes/{viajeId:guid}/interrumpir",
            async (e, d) => V(await e.ViajeAsync(d)), c => $"/api/viajes/{c.Id}/interrumpir", c => $"/api/viajes/{c.Id}",
            _ => J(new { motivo = "Interrupción de prueba" }))
        { Control = Actor.OperadorB },
        new("Corregir kilometraje de viaje de B", Post, "api/viajes/{viajeId:guid}/corregir-kilometraje",
            async (e, d) => V(await e.ViajeAsync(d, EstadoVictima.Finalizado)), c => $"/api/viajes/{c.Id}/corregir-kilometraje", c => $"/api/viajes/{c.Id}",
            _ => J(new { kilometrajeInicial = 35000, kilometrajeFinal = 35015, motivo = "Corrección de prueba" })),
        new("Eliminar viaje de B", Delete, "api/viajes/{viajeId:guid}",
            async (e, d) => V(await e.ViajeAsync(d, EstadoVictima.Finalizado)), c => $"/api/viajes/{c.Id}", c => $"/api/viajes/{c.Id}"),
        new("Subir foto del tablero a viaje de B", Put, "api/viajes/{viajeId:guid}/foto-tablero/{momento}",
            async (e, d) => V(await e.ViajeAsync(d)), c => $"/api/viajes/{c.Id}/foto-tablero/inicial", c => $"/api/viajes/{c.Id}", Archivo())
        { Control = Actor.OperadorB },
        new("Agregar puntos GPS a viaje de B", Post, "api/viajes/{viajeId:guid}/puntos-ruta",
            async (e, d) => V(await e.ViajeAsync(d)), c => $"/api/viajes/{c.Id}/puntos-ruta", c => $"/api/viajes/{c.Id}/puntos-ruta",
            _ => J(new { puntos = new[] { new { latitud = 16.7m, longitud = -93.1m, timestampCaptura = Ahora(-20), precisionMetros = 5 } } }))
        { Control = Actor.OperadorB },
        new("Eliminar punto GPS de B", Delete, "api/puntos-ruta/{puntoRutaId:guid}",
            async (e, d) => V(await e.PuntoRutaAsync(d)), c => $"/api/puntos-ruta/{c.Id}", c => $"/api/puntos-ruta/{c.Id}"),
        new("Recalcular ruta de viaje de B", Post, "api/viajes/{viajeId:guid}/ruta-procesada/recalcular",
            async (e, d) => V(await e.ViajeAsync(d, EstadoVictima.Finalizado)), c => $"/api/viajes/{c.Id}/ruta-procesada/recalcular", c => $"/api/viajes/{c.Id}/ruta-procesada"),

        // --- Combustible y estatus
        new("Editar carga de combustible de B", Put, "api/cargas-combustible/{cargaCombustibleId:guid}",
            async (e, d) => V(await e.CargaAsync(d)), c => $"/api/cargas-combustible/{c.Id}", c => $"/api/cargas-combustible/{c.Id}",
            c => Task.FromResult<HttpContent?>(Escenario.Formulario(new Dictionary<string, string?>
            {
                ["litros"] = "25",
                ["fechaCarga"] = Ahora(-30),
                ["motivo"] = "Corrección de prueba",
            }))),
        new("Eliminar carga de combustible de B", Delete, "api/cargas-combustible/{cargaCombustibleId:guid}",
            async (e, d) => V(await e.CargaAsync(d)), c => $"/api/cargas-combustible/{c.Id}?motivo=Prueba", c => $"/api/cargas-combustible/{c.Id}"),
        new("Subir evidencia a cambio de estatus de B", Put, "api/cambios-estatus-carro/{cambioEstatusCarroId:guid}/foto-evidencia",
            async (e, d) => V(await e.CambioEstatusAsync(d)), c => $"/api/cambios-estatus-carro/{c.Id}/foto-evidencia", c => $"/api/cambios-estatus-carro/{c.Id}", Archivo()),

        // --- Notificaciones
        new("Marcar como leída notificación de B", Post, "api/notificaciones/{notificacionId:guid}/marcar-leida",
            async (e, d) => V(await e.NotificacionAsync(d, d.AdminEmpleadoId)), c => $"/api/notificaciones/{c.Id}/marcar-leida", c => $"/api/notificaciones/{c.Id}"),
        new("Eliminar notificación de B", Delete, "api/notificaciones/{notificacionId:guid}",
            async (e, d) => V(await e.NotificacionAsync(d, d.AdminEmpleadoId)), c => $"/api/notificaciones/{c.Id}", c => $"/api/notificaciones/{c.Id}"),
        new("Marcar todas como leídas para un empleado de B", Post, "api/notificaciones/marcar-todas-leidas/{empleadoDestinatarioId:guid}",
            async (e, d) => V(await e.NotificacionAsync(d, d.AdminEmpleadoId), d.AdminEmpleadoId),
            c => $"/api/notificaciones/marcar-todas-leidas/{c.IdRelacionado}", c => $"/api/notificaciones/{c.Id}"),

        // --- Auditoría
        new("Registrar resultado en revisión de B", Put, "api/auditoria/revisiones/{revisionAuditoriaId:guid}/resultado",
            async (e, d) => V(await e.RevisionAsync(d)), c => $"/api/auditoria/revisiones/{c.Id}/resultado", c => $"/api/auditoria/revisiones/{c.Id}",
            _ => J(new { estadoRevision = "revisado", observaciones = "Resultado de prueba" })),
    ];

    // ================================================================ CREAR DENTRO DE B

    private static Task<int> SqlB(Escenario e, DatosEmpresa d, string sql) => e.ContarSqlAsync(sql, d.EmpresaId);

    public static IReadOnlyList<CasoCreacion> Creaciones { get; } =
    [
        new("Crear carro en B", Post, "api/carros", _ => "/api/carros",
            c => JC(new { empresaId = c.Objetivo.EmpresaId, marca = "Marca", modelo = "Modelo", anio = 2022, placas = Unico("N"), kilometrajeActual = 1000 }),
            (e, d) => e.ContarAsync("carro", d.EmpresaId)),
        new("Crear centro de costo en B", Post, "api/centros-costo", _ => "/api/centros-costo",
            c => JC(new { empresaId = c.Objetivo.EmpresaId, nombre = "Centro nuevo", clave = Unico("CC-") }),
            (e, d) => e.ContarAsync("centro_costo", d.EmpresaId)),
        new("Crear rol en B", Post, "api/roles", _ => "/api/roles",
            c => JC(new { empresaId = c.Objetivo.EmpresaId, nombre = Unico("Rol "), activo = true }),
            (e, d) => e.ContarAsync("rol", d.EmpresaId)),
        new("Crear usuario en B", Post, "api/usuarios", _ => "/api/usuarios",
            c => JC(new { empresaId = c.Objetivo.EmpresaId, email = $"{Unico("nuevo.")}@pruebas.local".ToLowerInvariant(), password = PasswordVictima, estado = "activo" }),
            (e, d) => e.ContarAsync("usuario", d.EmpresaId)),
        new("Crear empleado en B", Post, "api/empleados", _ => "/api/empleados",
            async c => Escenario.ComoJson(new { empresaId = c.Objetivo.EmpresaId, usuarioId = await c.E.UsuarioLibreAsync(c.Objetivo), nombreCompleto = "Empleado nuevo" }),
            (e, d) => e.ContarAsync("empleado", d.EmpresaId)),
        new("Solicitar carro de B a nombre de un operador de B", Post, "api/solicitudes-reserva", _ => "/api/solicitudes-reserva",
            async c => Escenario.ComoJson(new
            {
                empresaId = c.Objetivo.EmpresaId,
                carroId = await c.E.CarroAsync(c.Objetivo),
                empleadoSolicitanteId = c.Objetivo.OperadorEmpleadoId,
                motivo = "Solicitud de prueba",
            }),
            (e, d) => e.ContarAsync("solicitud_reserva", d.EmpresaId))
        { Control = Actor.OperadorB },
        new("Solicitar para mí un carro de B", Post, "api/solicitudes-reserva", _ => "/api/solicitudes-reserva",
            async c => Escenario.ComoJson(new
            {
                empresaId = c.DelActor.EmpresaId,
                carroId = await c.E.CarroAsync(c.Objetivo),
                empleadoSolicitanteId = c.DelActor.OperadorEmpleadoId,
                motivo = "Solicitud de prueba",
            }),
            (e, d) => SqlB(e, d, "SELECT COUNT(*) FROM solicitud_reserva s JOIN carro c ON c.carro_id = s.carro_id WHERE c.empresa_id = @e"))
        { Atacante = Actor.OperadorA, Control = Actor.OperadorB },
        new("Iniciar viaje en B a nombre de un operador de B", Post, "api/viajes", _ => "/api/viajes",
            async c =>
            {
                var (solicitudId, carroId) = await c.E.SolicitudAprobadaAsync(c.Objetivo);
                return Escenario.ComoJson(new
                {
                    empresaId = c.Objetivo.EmpresaId, carroId, solicitudReservaId = solicitudId,
                    empleadoOperadorId = c.Objetivo.OperadorEmpleadoId, centroCostoId = c.Objetivo.CentroCostoId,
                    fechaInicio = Ahora(-30), kilometrajeInicial = 35000,
                });
            },
            (e, d) => e.ContarAsync("viaje", d.EmpresaId))
        { Control = Actor.OperadorB },
        new("Iniciar mi viaje con un carro de B", Post, "api/viajes", _ => "/api/viajes",
            async c =>
            {
                var (solicitudId, carroId) = await c.E.SolicitudAprobadaAsync(c.Objetivo);
                return Escenario.ComoJson(new
                {
                    empresaId = c.DelActor.EmpresaId, carroId,
                    solicitudReservaId = c.EsAtaque ? (Guid?)null : solicitudId,
                    empleadoOperadorId = c.DelActor.OperadorEmpleadoId,
                    fechaInicio = Ahora(-30), kilometrajeInicial = 35000,
                });
            },
            (e, d) => SqlB(e, d, "SELECT COUNT(*) FROM viaje v JOIN carro c ON c.carro_id = v.carro_id WHERE c.empresa_id = @e"))
        { Atacante = Actor.OperadorA, Control = Actor.OperadorB },
        new("Sincronizar viaje en B", Post, "api/viajes/sincronizar", _ => "/api/viajes/sincronizar",
            async c =>
            {
                var (solicitudId, carroId) = await c.E.SolicitudAprobadaAsync(c.Objetivo);
                return Escenario.ComoJson(new
                {
                    viajeId = Guid.CreateVersion7(),
                    empresaId = c.Objetivo.EmpresaId, carroId, solicitudReservaId = solicitudId,
                    empleadoOperadorId = c.Objetivo.OperadorEmpleadoId, centroCostoId = c.Objetivo.CentroCostoId,
                    fechaInicio = Ahora(-60), kilometrajeInicial = 35000,
                    estado = "finalizado", fechaFin = Ahora(-10), kilometrajeFinal = 35008,
                });
            },
            (e, d) => e.ContarAsync("viaje", d.EmpresaId))
        { Control = Actor.OperadorB },
        new("Registrar carga en un viaje de B", Post, "api/cargas-combustible", _ => "/api/cargas-combustible",
            async c => Escenario.Formulario(new Dictionary<string, string?>
            {
                ["viajeId"] = (await c.E.ViajeAsync(c.Objetivo)).ToString(),
                ["litros"] = "15",
                ["fechaCarga"] = Ahora(-20),
            }, "fotoComprobante"),
            (e, d) => e.ContarAsync("carga_combustible", d.EmpresaId))
        { Control = Actor.OperadorB },
        new("Enviar notificación a un empleado de B", Post, "api/notificaciones", _ => "/api/notificaciones",
            c => JC(new { empleadoDestinatarioId = c.Objetivo.AdminEmpleadoId, tipo = "nuevaSolicitud", mensaje = "Aviso de prueba" }),
            (e, d) => e.ContarAsync("notificacion", d.EmpresaId)),
        new("Mandar a mantenimiento un carro de B", Post, "api/cambios-estatus-carro", _ => "/api/cambios-estatus-carro",
            async c => Escenario.ComoJson(new
            {
                carroId = await c.E.CarroAsync(c.Objetivo),
                empleadoId = c.DelActor.AdminEmpleadoId,
                estatusNuevo = "enMantenimiento",
                motivo = "Mantenimiento de prueba",
                tipoMotivo = "servicio",
            }),
            (e, d) => e.ContarAsync("cambio_estatus_carro", d.EmpresaId)),
        new("Crear revisión de un viaje de B", Post, "api/auditoria/revisiones", _ => "/api/auditoria/revisiones",
            async c => Escenario.ComoJson(new { viajeId = await c.E.ViajeAsync(c.Objetivo, EstadoVictima.Finalizado), observaciones = "Revisión de prueba" }),
            (e, d) => e.ContarAsync("revision_auditoria", d.EmpresaId)),
        new("Registrar dispositivo a un empleado de B", Post, "api/dispositivos", _ => "/api/dispositivos",
            async c => Escenario.ComoJson(new { empleadoId = (await c.E.EmpleadoAsync(c.Objetivo)).EmpleadoId, tokenFcm = Unico("token-nuevo-"), plataforma = "android" }),
            (e, d) => e.ContarAsync("dispositivo_empleado", d.EmpresaId)),
        new("Asignar un rol de B a un usuario", Post, "api/usuario-roles", _ => "/api/usuario-roles",
            async c => Escenario.ComoJson(new { usuarioId = (await c.E.EmpleadoAsync(c.DelActor)).UsuarioId, rolId = c.Objetivo.RolAdminId }),
            (e, d) => SqlB(e, d, "SELECT COUNT(*) FROM usuario_rol ur JOIN rol r ON r.rol_id = ur.rol_id WHERE r.empresa_id = @e")),
        new("Asignar un rol a un usuario de B", Post, "api/usuario-roles", _ => "/api/usuario-roles",
            async c => Escenario.ComoJson(new { usuarioId = (await c.E.EmpleadoAsync(c.Objetivo)).UsuarioId, rolId = c.DelActor.RolOperadorId }),
            (e, d) => SqlB(e, d, "SELECT COUNT(*) FROM usuario_rol ur JOIN empleado em ON em.usuario_id = ur.usuario_id WHERE em.empresa_id = @e")),
        new("Dar un privilegio a un rol de B", Post, "api/rol-privilegios", _ => "/api/rol-privilegios",
            async c => Escenario.ComoJson(new { rolId = await c.E.RolAsync(c.Objetivo), privilegioId = await c.E.PrivilegioAsync() }),
            (e, d) => SqlB(e, d, "SELECT COUNT(*) FROM rol_privilegio rp JOIN rol r ON r.rol_id = rp.rol_id WHERE r.empresa_id = @e")),
    ];

    // ================================================================ SOLO PLATAFORMA

    private static readonly Guid Cualquiera = Guid.Parse("0192f3a4-1b2c-7d3e-8f40-112233445566");

    public static IReadOnlyList<CasoSoloPlataforma> SoloPlataforma { get; } =
    [
        new(Post, "api/empresas", "/api/empresas"),
        new(Delete, "api/empresas/{empresaId:guid}", $"/api/empresas/{Cualquiera}"),
        new(Post, "api/corporativos", "/api/corporativos"),
        new(Put, "api/corporativos/{corporativoId:guid}", $"/api/corporativos/{Cualquiera}"),
        new(Delete, "api/corporativos/{corporativoId:guid}", $"/api/corporativos/{Cualquiera}"),
        new(Post, "api/divisiones", "/api/divisiones"),
        new(Put, "api/divisiones/{divisionId:guid}", $"/api/divisiones/{Cualquiera}"),
        new(Delete, "api/divisiones/{divisionId:guid}", $"/api/divisiones/{Cualquiera}"),
        new(Post, "api/privilegios", "/api/privilegios"),
        new(Put, "api/privilegios/{privilegioId:guid}", $"/api/privilegios/{Cualquiera}"),
        new(Delete, "api/privilegios/{privilegioId:guid}", $"/api/privilegios/{Cualquiera}"),
        new(Get, "api/outbox", "/api/outbox"),
        new(Post, "api/outbox/{outboxMensajeId:guid}/reintentar", $"/api/outbox/{Cualquiera}/reintentar"),
    ];

    // ================================================================ FUERA DE LA MATRIZ (con razón)

    public static IReadOnlyDictionary<string, string> Excluidos { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        ["POST api/auth/login"] = "Anónimo: todavía no hay empresa.",
        ["POST api/auth/refresh"] = "Anónimo: la sesión sale del refresh token, no de un id.",
        ["POST api/auth/logout"] = "Anónimo: cierra la sesión del refresh token que se manda.",
        ["GET api/auth/perfil"] = "Siempre es el usuario del token.",
        ["GET api/auth/privilegios"] = "Siempre es el usuario del token.",
        ["GET api/auth/sesiones"] = "Siempre son las sesiones del usuario del token.",
        ["POST api/auth/sesiones/cerrar-otras"] = "Siempre son las sesiones del usuario del token.",
        ["GET api/privilegios"] = "Catálogo global del sistema (no es de ninguna empresa).",
        ["GET api/privilegios/{privilegioId:guid}"] = "Catálogo global del sistema (no es de ninguna empresa).",
        ["GET api/corporativos"] = "DECISIÓN PENDIENTE: ¿una empresa puede ver su corporativo? (Riesgos del plan).",
        ["GET api/corporativos/{corporativoId:guid}"] = "DECISIÓN PENDIENTE: ¿una empresa puede ver su corporativo? (Riesgos del plan).",
        ["GET api/divisiones"] = "DECISIÓN PENDIENTE: ¿una empresa puede ver su división? (Riesgos del plan).",
        ["GET api/divisiones/{divisionId:guid}"] = "DECISIÓN PENDIENTE: ¿una empresa puede ver su división? (Riesgos del plan).",
    };
}
