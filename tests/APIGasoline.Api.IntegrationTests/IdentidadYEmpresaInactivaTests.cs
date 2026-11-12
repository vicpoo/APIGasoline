using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using APIGasoline.Api.IntegrationTests.Infraestructura;
using APIGasoline.Application.Common.Interfaces;
using APIGasoline.Domain.Entities;
using APIGasoline.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using static APIGasoline.Api.IntegrationTests.Infraestructura.Victimas;

namespace APIGasoline.Api.IntegrationTests;

// =====================================================================================
//  REVISIÓN DE CÓDIGO F008 y F009
//  - F008: quien aprueba, rechaza o cambia un estatus es el empleado del TOKEN; lo que diga el
//          cuerpo se ignora (antes se podía firmar a nombre de otro).
//  - F009: una empresa inactiva no inicia sesión ni renueva el token, y desactivarla cierra sus
//          sesiones. Solo la plataforma la activa o desactiva.
// =====================================================================================

[Collection(ColeccionApi.Nombre)]
public sealed class ActorDelTokenTests(Escenario e)
{
    [Fact]
    public async Task Aprobar_queda_a_nombre_del_token_aunque_el_cuerpo_diga_otro_empleado()
    {
        var solicitudId = await e.SolicitudPendienteAsync(e.A);

        // El cuerpo dice que aprueba el propio solicitante (lo que la regla de dominio prohíbe)
        var respuesta = await e.EnviarAsync(Actor.AdminA, HttpMethod.Post, $"/api/solicitudes-reserva/{solicitudId}/aprobar",
            Escenario.ComoJson(new { empleadoConfirmadorId = e.A.OperadorEmpleadoId }));

        Assert.True(respuesta.Exito, respuesta.ToString());
        Assert.Equal(e.A.AdminEmpleadoId, await ConfirmadorAsync(solicitudId));
    }

    [Fact]
    public async Task Rechazar_queda_a_nombre_del_token_aunque_el_cuerpo_diga_otro_empleado()
    {
        var solicitudId = await e.SolicitudPendienteAsync(e.A);

        var respuesta = await e.EnviarAsync(Actor.AdminA, HttpMethod.Post, $"/api/solicitudes-reserva/{solicitudId}/rechazar",
            Escenario.ComoJson(new { empleadoConfirmadorId = e.A.OperadorEmpleadoId, motivo = "Rechazo de prueba" }));

        Assert.True(respuesta.Exito, respuesta.ToString());
        Assert.Equal(e.A.AdminEmpleadoId, await ConfirmadorAsync(solicitudId));
    }

    [Fact]
    public async Task Aprobar_sin_mandar_empleado_tambien_funciona()
    {
        var solicitudId = await e.SolicitudPendienteAsync(e.A);

        var respuesta = await e.EnviarAsync(Actor.AdminA, HttpMethod.Post, $"/api/solicitudes-reserva/{solicitudId}/aprobar",
            Escenario.ComoJson(new { motivo = "Sin empleado en el cuerpo" }));

        Assert.True(respuesta.Exito, respuesta.ToString());
        Assert.Equal(e.A.AdminEmpleadoId, await ConfirmadorAsync(solicitudId));
    }

    [Fact]
    public async Task Cambiar_estatus_queda_a_nombre_del_token_aunque_el_cuerpo_diga_otro_empleado()
    {
        var carroId = await e.CarroAsync(e.A);

        var cambio = await e.ExigirAsync(Actor.AdminA, HttpMethod.Post, "/api/cambios-estatus-carro", Escenario.ComoJson(new
        {
            carroId,
            empleadoId = e.A.OperadorEmpleadoId,
            estatusNuevo = "enMantenimiento",
            motivo = "Servicio de prueba",
            tipoMotivo = "servicio",
        }));

        Assert.Equal(e.A.AdminEmpleadoId, cambio.GetProperty("empleadoId").GetGuid());
    }

    [Fact]
    public async Task Un_usuario_sin_empleado_no_puede_aprobar()
    {
        var solicitudId = await e.SolicitudPendienteAsync(e.A);

        // El administrador de la plataforma no es empleado de ninguna empresa
        var respuesta = await e.EnviarAsync(Actor.Plataforma, HttpMethod.Post, $"/api/solicitudes-reserva/{solicitudId}/aprobar",
            Escenario.ComoJson(new { empleadoConfirmadorId = e.A.AdminEmpleadoId }));

        Assert.Equal(HttpStatusCode.Forbidden, respuesta.Status);
        Assert.Null(await ConfirmadorAsync(solicitudId));
    }

    private Task<Guid?> ConfirmadorAsync(Guid solicitudId)
        => e.EnBdAsync(async c => (await c.SolicitudesReserva.SingleAsync(s => s.SolicitudReservaId == solicitudId)).EmpleadoConfirmadorId);
}

[Collection(ColeccionApi.Nombre)]
public sealed class EmpresaInactivaTests(Escenario e)
{
    [Fact]
    public async Task Desactivar_cierra_las_sesiones_y_bloquea_login_y_refresh_hasta_reactivarla()
    {
        var (empresaId, email) = await EmpresaConUsuarioAsync();
        using var anonimo = e.Factory.CreateClient();

        // Activa: entra normal
        var login = await LoginAsync(anonimo, email);
        Assert.Equal(HttpStatusCode.OK, login.Status);
        var refreshToken = JsonDocument.Parse(login.Cuerpo).RootElement.GetProperty("refreshToken").GetString();

        // La plataforma la desactiva: sus sesiones se cierran
        var desactivar = await e.EnviarAsync(Actor.Plataforma, HttpMethod.Put, $"/api/empresas/{empresaId}",
            Escenario.ComoJson(new { nombre = "Empresa inactiva de prueba", activo = false }));
        Assert.True(desactivar.Exito, desactivar.ToString());

        var abiertas = await e.EnBdAsync(c => c.Sesiones.CountAsync(s => s.EmpresaId == empresaId && s.FechaRevocacion == null));
        var motivos = await e.EnBdAsync(c => c.Sesiones.Where(s => s.EmpresaId == empresaId).Select(s => s.MotivoRevocacion).ToListAsync());
        Assert.Equal(0, abiertas);
        Assert.All(motivos, m => Assert.Equal(MotivoRevocacionSesion.EmpresaDesactivada, m));

        // Ni login ni refresh
        Assert.Equal(HttpStatusCode.Forbidden, (await LoginAsync(anonimo, email)).Status);
        var refresh = await EnviarAnonimoAsync(anonimo, "/api/auth/refresh", new { refreshToken });
        Assert.False(refresh.Exito, $"Renovó el token con la empresa inactiva: {refresh}");

        // Reactivada, vuelve a entrar
        var activar = await e.EnviarAsync(Actor.Plataforma, HttpMethod.Put, $"/api/empresas/{empresaId}",
            Escenario.ComoJson(new { nombre = "Empresa inactiva de prueba", activo = true }));
        Assert.True(activar.Exito, activar.ToString());
        Assert.Equal(HttpStatusCode.OK, (await LoginAsync(anonimo, email)).Status);
    }

    [Fact]
    public async Task El_administrador_de_la_empresa_no_puede_desactivarla()
    {
        var respuesta = await e.EnviarAsync(Actor.AdminA, HttpMethod.Put, $"/api/empresas/{e.A.EmpresaId}",
            Escenario.ComoJson(new { nombre = "Empresa Demo A", activo = false }));

        Assert.Equal(HttpStatusCode.Forbidden, respuesta.Status);
        Assert.True(await e.EnBdAsync(c => c.Empresas.Where(x => x.EmpresaId == e.A.EmpresaId).Select(x => x.Activo).SingleAsync()));
    }

    [Fact]
    public async Task El_administrador_de_la_empresa_si_puede_editarla_sin_cambiar_su_estado()
    {
        var respuesta = await e.EnviarAsync(Actor.AdminA, HttpMethod.Put, $"/api/empresas/{e.A.EmpresaId}",
            Escenario.ComoJson(new { nombre = "Empresa Demo A", activo = true }));

        Assert.True(respuesta.Exito, respuesta.ToString());
    }

    /// <summary>Empresa nueva (no la A ni la B: el resto de las pruebas las necesita activas) con un usuario y su empleado.</summary>
    private Task<(Guid EmpresaId, string Email)> EmpresaConUsuarioAsync()
        => e.EnBdAsync(async (c, servicios) =>
        {
            var empresa = Empresa.Crear(Unico("Empresa F009 "));
            c.Empresas.Add(empresa);

            var email = $"{Unico("inactiva.")}@pruebas.local".ToLowerInvariant();
            var hash = servicios.GetRequiredService<IPasswordHasher>().Hash(PasswordVictima);
            var usuario = Usuario.Crear(email, hash, empresaId: empresa.EmpresaId);
            c.Usuarios.Add(usuario);
            c.Empleados.Add(Empleado.Crear(empresa.EmpresaId, usuario.UsuarioId, "Empleado de empresa inactiva"));

            await c.SaveChangesAsync();
            return (empresa.EmpresaId, email);
        });

    private static Task<Respuesta> LoginAsync(HttpClient cliente, string email)
        => EnviarAnonimoAsync(cliente, "/api/auth/login", new { email, password = PasswordVictima });

    private static async Task<Respuesta> EnviarAnonimoAsync(HttpClient cliente, string url, object cuerpo)
    {
        using var respuesta = await cliente.PostAsJsonAsync(url, cuerpo);
        return new Respuesta(respuesta.StatusCode, await respuesta.Content.ReadAsStringAsync());
    }
}
