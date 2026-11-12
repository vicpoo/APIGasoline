using System.Security.Cryptography;
using APIGasoline.Application.Common.Interfaces;
using APIGasoline.Domain.Entities;
using APIGasoline.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace APIGasoline.Api.IntegrationTests.Infraestructura;

/// <summary>
/// Crea registros NUEVOS de una empresa directo en la BD (con las reglas del dominio) para que cada
/// caso de la matriz ataque algo propio: si un ataque funciona (fuga), no daña a los demás casos.
/// </summary>
public static class Victimas
{
    public const string PasswordVictima = "Victima#Pruebas2026";

    public static string Unico(string prefijo = "") => prefijo + Convert.ToHexString(RandomNumberGenerator.GetBytes(4));

    public static Task<Guid> CarroAsync(this Escenario e, DatosEmpresa d, string? foto = null, EstadoCarro estado = EstadoCarro.Disponible)
        => e.EnBdAsync(async c =>
        {
            var carro = Carro.Crear(d.EmpresaId, "Marca", "Modelo", 2022, "Rojo", Unico("V"), foto, 41m, 35000m, DateTime.Now.Year, estado);
            c.Carros.Add(carro);
            await c.SaveChangesAsync();
            return carro.CarroId;
        });

    public static Task<Guid> CentroCostoAsync(this Escenario e, DatosEmpresa d)
        => e.EnBdAsync(async c =>
        {
            var centro = CentroCosto.Crear(d.EmpresaId, "Centro víctima", Unico("CC-"));
            c.CentrosCosto.Add(centro);
            await c.SaveChangesAsync();
            return centro.CentroCostoId;
        });

    public static Task<Guid> CorporativoAsync(this Escenario e, DatosEmpresa d)
        => e.EnBdAsync(async c =>
        {
            var corporativo = Corporativo.Crear(d.EmpresaId, Unico("Corporativo víctima "));
            c.Corporativos.Add(corporativo);
            await c.SaveChangesAsync();
            return corporativo.CorporativoId;
        });

    public static Task<Guid> DivisionAsync(this Escenario e, DatosEmpresa d)
        => e.EnBdAsync(async c =>
        {
            var division = Division.Crear(d.EmpresaId, Unico("División víctima "));
            c.Divisiones.Add(division);
            await c.SaveChangesAsync();
            return division.DivisionId;
        });

    /// <summary>Corporativo y división nuevos de la empresa; ligados si <paramref name="ligados"/>.</summary>
    public static Task<(Guid CorporativoId, Guid DivisionId)> CorporativoYDivisionAsync(this Escenario e, DatosEmpresa d, bool ligados)
        => e.EnBdAsync(async c =>
        {
            var corporativo = Corporativo.Crear(d.EmpresaId, Unico("Corporativo víctima "));
            var division = Division.Crear(d.EmpresaId, Unico("División víctima "));
            c.Corporativos.Add(corporativo);
            c.Divisiones.Add(division);

            if (ligados)
                c.CorporativoDivisiones.Add(CorporativoDivision.Ligar(corporativo, division));

            await c.SaveChangesAsync();
            return (corporativo.CorporativoId, division.DivisionId);
        });

    public static Task<Guid> RolAsync(this Escenario e, DatosEmpresa d)
        => e.EnBdAsync(async c =>
        {
            var rol = Rol.Crear(Unico("Rol "), "Rol víctima", activo: true, empresaId: d.EmpresaId);
            c.Roles.Add(rol);
            await c.SaveChangesAsync();
            return rol.RolId;
        });

    /// <summary>Usuario de la empresa todavía sin empleado.</summary>
    public static Task<Guid> UsuarioLibreAsync(this Escenario e, DatosEmpresa d)
        => e.EnBdAsync(async (c, servicios) =>
        {
            var hash = servicios.GetRequiredService<IPasswordHasher>().Hash(PasswordVictima);
            var usuario = Usuario.Crear($"{Unico("libre.")}.{d.Letra.ToLowerInvariant()}@pruebas.local".ToLowerInvariant(), hash, empresaId: d.EmpresaId);
            c.Usuarios.Add(usuario);
            await c.SaveChangesAsync();
            return usuario.UsuarioId;
        });

    /// <summary>Usuario + empleado de la empresa (con fotos de perfil y licencia).</summary>
    public static Task<(Guid UsuarioId, Guid EmpleadoId)> EmpleadoAsync(this Escenario e, DatosEmpresa d)
        => e.EnBdAsync(async (c, servicios) =>
        {
            var hash = servicios.GetRequiredService<IPasswordHasher>().Hash(PasswordVictima);
            var usuario = Usuario.Crear($"{Unico("victima.")}.{d.Letra.ToLowerInvariant()}@pruebas.local".ToLowerInvariant(), hash, empresaId: d.EmpresaId);
            c.Usuarios.Add(usuario);

            var empleado = Empleado.Crear(d.EmpresaId, usuario.UsuarioId, "Empleado víctima",
                fotoPerfil: $"empresa/{d.EmpresaId}/empleado/perfil-{Unico()}.jpg");
            empleado.CambiarFotoLicencia($"empresa/{d.EmpresaId}/empleado/licencia-{Unico()}.jpg", DateTime.Now);
            c.Empleados.Add(empleado);

            await c.SaveChangesAsync();
            return (usuario.UsuarioId, empleado.EmpleadoId);
        });

    public static async Task<Guid> UsuarioRolAsync(this Escenario e, DatosEmpresa d)
    {
        var (usuarioId, _) = await e.EmpleadoAsync(d);

        return await e.EnBdAsync(async c =>
        {
            var asignacion = UsuarioRol.Crear(usuarioId, d.RolOperadorId);
            c.UsuarioRoles.Add(asignacion);
            await c.SaveChangesAsync();
            return asignacion.UsuarioRolId;
        });
    }

    public static async Task<Guid> RolPrivilegioAsync(this Escenario e, DatosEmpresa d, string clave = "carro.ver")
    {
        var rolId = await e.RolAsync(d);

        return await e.EnBdAsync(async c =>
        {
            var privilegio = await c.Privilegios.FirstAsync(p => p.Clave == clave);
            var rp = RolPrivilegio.Crear(rolId, privilegio.PrivilegioId);
            c.RolPrivilegios.Add(rp);
            await c.SaveChangesAsync();
            return rp.RolPrivilegioId;
        });
    }

    /// <summary>Privilegio que el rol víctima todavía no tiene (para crear/reasignar rol_privilegio).</summary>
    public static Task<Guid> PrivilegioAsync(this Escenario e, string clave = "viaje.ver_propios")
        => e.EnBdAsync(async c => (await c.Privilegios.FirstAsync(p => p.Clave == clave)).PrivilegioId);

    public static Task<Guid> SolicitudPendienteAsync(this Escenario e, DatosEmpresa d)
        => e.EnBdAsync(async c =>
        {
            var carro = Carro.Crear(d.EmpresaId, "Marca", "Modelo", 2022, "Rojo", Unico("V"), null, 41m, 35000m, DateTime.Now.Year);
            c.Carros.Add(carro);

            var solicitud = SolicitudReserva.Crear(d.EmpresaId, carro.CarroId, d.OperadorEmpleadoId, "Solicitud víctima");
            c.SolicitudesReserva.Add(solicitud);

            await c.SaveChangesAsync();
            return solicitud.SolicitudReservaId;
        });

    /// <summary>Solicitud aprobada: el carro queda ocupado y asignado al operador.</summary>
    public static async Task<(Guid SolicitudId, Guid CarroId)> SolicitudAprobadaAsync(this Escenario e, DatosEmpresa d)
        => await e.EnBdAsync(async c =>
        {
            var carro = Carro.Crear(d.EmpresaId, "Marca", "Modelo", 2022, "Rojo", Unico("V"), null, 41m, 35000m, DateTime.Now.Year);
            carro.Ocupar();
            c.Carros.Add(carro);

            var solicitud = SolicitudReserva.Crear(d.EmpresaId, carro.CarroId, d.OperadorEmpleadoId, "Solicitud víctima");
            solicitud.Aprobar(d.AdminEmpleadoId, DateTime.Now.AddMinutes(-90));
            c.SolicitudesReserva.Add(solicitud);

            await c.SaveChangesAsync();
            return (solicitud.SolicitudReservaId, carro.CarroId);
        });

    public enum EstadoVictima { EnCurso, Pausado, Finalizado }

    /// <summary>Viaje del operador de la empresa, con su solicitud aprobada y su carro propios.</summary>
    public static async Task<Guid> ViajeAsync(this Escenario e, DatosEmpresa d, EstadoVictima estado = EstadoVictima.EnCurso)
    {
        var (solicitudId, carroId) = await e.SolicitudAprobadaAsync(d);

        return await e.EnBdAsync(async c =>
        {
            var inicio = DateTime.Now.AddMinutes(-60);
            var viaje = Viaje.Iniciar(d.EmpresaId, carroId, solicitudId, d.OperadorEmpleadoId, inicio, null, null, 35000m, d.CentroCostoId);
            c.Viajes.Add(viaje);

            if (estado == EstadoVictima.Pausado)
                c.ViajePausas.Add(viaje.Pausar(null, inicio.AddMinutes(10), "Pausa víctima", null, null));

            if (estado == EstadoVictima.Finalizado)
                viaje.Finalizar(inicio.AddMinutes(30), null, null, 35010m);

            await c.SaveChangesAsync();
            return viaje.ViajeId;
        });
    }

    public static async Task<Guid> PuntoRutaAsync(this Escenario e, DatosEmpresa d)
    {
        var viajeId = await e.ViajeAsync(d);

        return await e.EnBdAsync(async c =>
        {
            var punto = PuntoRuta.Crear(d.EmpresaId, viajeId, 16.75m, -93.12m, DateTime.Now.AddMinutes(-30), 5m);
            c.PuntosRuta.Add(punto);
            await c.SaveChangesAsync();
            return punto.PuntoRutaId;
        });
    }

    public static async Task<Guid> CargaAsync(this Escenario e, DatosEmpresa d)
    {
        var viajeId = await e.ViajeAsync(d);

        return await e.EnBdAsync(async c =>
        {
            var carga = CargaCombustible.Crear(d.EmpresaId, viajeId, 20m, 500m,
                $"empresa/{d.EmpresaId}/viaje/{viajeId}/carga/comprobante-{Unico()}.jpg",
                DateTime.Now.AddMinutes(-30), null, null, centroCostoId: d.CentroCostoId);
            c.CargasCombustible.Add(carga);
            await c.SaveChangesAsync();
            return carga.CargaCombustibleId;
        });
    }

    public static Task<Guid> NotificacionAsync(this Escenario e, DatosEmpresa d, Guid empleadoDestinatarioId)
        => e.EnBdAsync(async c =>
        {
            var notificacion = Notificacion.Crear(d.EmpresaId, empleadoDestinatarioId, TipoNotificacion.NuevaSolicitud, null, "Aviso víctima");
            c.Notificaciones.Add(notificacion);
            await c.SaveChangesAsync();
            return notificacion.NotificacionId;
        });

    public static Task<Guid> CambioEstatusAsync(this Escenario e, DatosEmpresa d)
        => e.EnBdAsync(async c =>
        {
            var carro = Carro.Crear(d.EmpresaId, "Marca", "Modelo", 2022, "Rojo", Unico("V"), null, 41m, 35000m, DateTime.Now.Year);
            c.Carros.Add(carro);

            var cambio = CambioEstatusCarro.Registrar(carro, d.AdminEmpleadoId, EstadoCarro.EnMantenimiento, "Cambio víctima",
                tipoMotivo: TipoMotivoCambio.Servicio, fotoEvidencia: $"empresa/{d.EmpresaId}/evidencia-{Unico()}.jpg");
            c.CambiosEstatusCarro.Add(cambio);

            await c.SaveChangesAsync();
            return cambio.CambioEstatusCarroId;
        });

    public static async Task<Guid> RevisionAsync(this Escenario e, DatosEmpresa d)
    {
        var viajeId = await e.ViajeAsync(d, EstadoVictima.Finalizado);

        return await e.EnBdAsync(async c =>
        {
            var revision = RevisionAuditoria.DeViaje(d.EmpresaId, viajeId, d.AdminEmpleadoId, "Revisión víctima");
            c.RevisionesAuditoria.Add(revision);
            await c.SaveChangesAsync();
            return revision.RevisionAuditoriaId;
        });
    }

    public static async Task<(Guid DispositivoId, Guid EmpleadoId)> DispositivoAsync(this Escenario e, DatosEmpresa d)
    {
        var (_, empleadoId) = await e.EmpleadoAsync(d);

        return await e.EnBdAsync(async c =>
        {
            var dispositivo = DispositivoEmpleado.Registrar(d.EmpresaId, empleadoId, Unico("token-victima-"), PlataformaDispositivo.Android, DateTime.Now);
            c.Dispositivos.Add(dispositivo);
            await c.SaveChangesAsync();
            return (dispositivo.DispositivoEmpleadoId, empleadoId);
        });
    }

    /// <summary>Celular registrado a un empleado ya existente (por ejemplo, el operador de la empresa).</summary>
    public static Task<Guid> DispositivoDeAsync(this Escenario e, DatosEmpresa d, Guid empleadoId)
        => e.EnBdAsync(async c =>
        {
            var dispositivo = DispositivoEmpleado.Registrar(d.EmpresaId, empleadoId, Unico("token-propio-"), PlataformaDispositivo.Android, DateTime.Now);
            c.Dispositivos.Add(dispositivo);
            await c.SaveChangesAsync();
            return dispositivo.DispositivoEmpleadoId;
        });

    /// <summary>Usuario nuevo de la empresa con un rol ya asignado; devuelve la asignación.</summary>
    public static async Task<Guid> UsuarioConRolAsync(this Escenario e, DatosEmpresa d, Guid rolId)
    {
        var (usuarioId, _) = await e.EmpleadoAsync(d);

        return await e.EnBdAsync(async c =>
        {
            var asignacion = UsuarioRol.Crear(usuarioId, rolId);
            c.UsuarioRoles.Add(asignacion);
            await c.SaveChangesAsync();
            return asignacion.UsuarioRolId;
        });
    }

    public static async Task<(Guid SesionId, Guid UsuarioId)> SesionAsync(this Escenario e, DatosEmpresa d)
    {
        var (usuarioId, _) = await e.EmpleadoAsync(d);

        return await e.EnBdAsync(async c =>
        {
            var sesion = SesionUsuario.Iniciar(usuarioId, d.EmpresaId, Convert.ToHexString(RandomNumberGenerator.GetBytes(32)).ToLowerInvariant(),
                PlataformaDispositivo.Android, "Celular víctima", "127.0.0.1", "pruebas", DateTime.Now, TimeSpan.FromDays(30), TimeSpan.FromDays(90));
            c.Sesiones.Add(sesion);
            await c.SaveChangesAsync();
            return (sesion.SesionUsuarioId, usuarioId);
        });
    }
}
