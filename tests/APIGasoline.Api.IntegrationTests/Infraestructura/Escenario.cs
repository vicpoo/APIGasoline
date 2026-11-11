using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using APIGasoline.Application.Features.Auth.Commands;
using APIGasoline.Infrastructure.Persistence;
using APIGasoline.Infrastructure.Persistence.Converters;
using MediatR;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.MsSql;

namespace APIGasoline.Api.IntegrationTests.Infraestructura;

/// <summary>Quién hace la petición.</summary>
public enum Actor
{
    /// <summary>Administrador de plataforma: todos los privilegios, incluido plataforma.multiempresa.</summary>
    Plataforma,
    /// <summary>Administrador de la Empresa Demo A (todos los privilegios de ámbito empresa).</summary>
    AdminA,
    OperadorA,
    /// <summary>Administrador de la Empresa Demo B (la "víctima" de la matriz de fugas).</summary>
    AdminB,
    OperadorB,
}

/// <summary>Respuesta HTTP ya leída.</summary>
public sealed record Respuesta(HttpStatusCode Status, string Cuerpo)
{
    public bool Exito => (int)Status is >= 200 and < 300;

    public override string ToString()
        => $"{(int)Status} {Status}: {(Cuerpo.Length > 400 ? Cuerpo[..400] + "…" : Cuerpo)}";
}

/// <summary>Ids de una empresa demo y de la operación que se arma por la API.</summary>
public sealed class DatosEmpresa
{
    public required string Letra { get; init; }
    public Guid EmpresaId { get; set; }
    public Guid AdminUsuarioId { get; set; }
    public Guid AdminEmpleadoId { get; set; }
    public Guid OperadorUsuarioId { get; set; }
    public Guid OperadorEmpleadoId { get; set; }
    public Guid RolAdminId { get; set; }
    public Guid RolOperadorId { get; set; }
    public Guid CentroCostoId { get; set; }
    public Guid Carro1Id { get; set; }
    public Guid Carro2Id { get; set; }

    // Operación (la arma ArmarOperacionAsync por la API)
    public Guid SolicitudId { get; set; }
    public Guid ViajeId { get; set; }
    public Guid PuntoRutaId { get; set; }
    public Guid CargaId { get; set; }
    public Guid RutaProcesadaId { get; set; }
    public Guid CambioEstatusId { get; set; }
    public Guid RevisionId { get; set; }
    public Guid DispositivoId { get; set; }
    public Guid NotificacionId { get; set; }
    public Guid OutboxMensajeId { get; set; }
    public Guid AuditoriaEventoId { get; set; }
    public Guid SesionOperadorId { get; set; }
    public Guid UsuarioRolId { get; set; }
    public Guid RolPrivilegioId { get; set; }
}

[CollectionDefinition(Nombre)]
public sealed class ColeccionApi : ICollectionFixture<Escenario>
{
    public const string Nombre = "API con dos empresas";
}

/// <summary>
/// Escenario compartido por todas las pruebas de la colección:
/// SQL Server en Docker + la API en memoria + Empresa Demo A y Empresa Demo B, cada una con
/// operación real (solicitud aprobada, viaje con puntos GPS y carga, ruta procesada, cambio de
/// estatus, revisión de auditoría, dispositivo, notificaciones, outbox y sesiones).
/// </summary>
public sealed class Escenario : IAsyncLifetime
{
    public const string PasswordDemo = SembrarDatosDemoCommandHandler.PasswordDemo;

    public static readonly JsonSerializerOptions Json = CrearOpcionesJson();

    private readonly MsSqlContainer _sql = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest").Build();
    private readonly Dictionary<Actor, HttpClient> _clientes = new();
    private ApiFactory? _factory;

    public ApiFactory Factory => _factory ?? throw new InvalidOperationException("El escenario no se ha inicializado.");

    public DatosEmpresa A { get; } = new() { Letra = "A" };
    public DatosEmpresa B { get; } = new() { Letra = "B" };

    public DatosEmpresa De(Actor actor) => actor is Actor.AdminB or Actor.OperadorB ? B : A;

    public async Task InitializeAsync()
    {
        await _sql.StartAsync();

        var cadena = new SqlConnectionStringBuilder(_sql.GetConnectionString())
        {
            InitialCatalog = "apigasoline_pruebas",
            TrustServerCertificate = true,
        }.ConnectionString;

        _factory = new ApiFactory(cadena);

        // Crear el primer cliente arranca la API: migra la BD y siembra la seguridad (Program.cs)
        _clientes[Actor.Plataforma] = Factory.CreateClient();
        await IniciarSesionAsync(Actor.Plataforma, ApiFactory.AdminPlataformaEmail, ApiFactory.AdminPlataformaPassword);

        await EnviarComandoAsync(new SembrarDatosDemoCommand());

        foreach (var (actor, email) in new[]
                 {
                     (Actor.AdminA, "admin.a@demo.local"), (Actor.OperadorA, "operador.a@demo.local"),
                     (Actor.AdminB, "admin.b@demo.local"), (Actor.OperadorB, "operador.b@demo.local"),
                 })
        {
            _clientes[actor] = Factory.CreateClient();
            await IniciarSesionAsync(actor, email, PasswordDemo);
        }

        await CargarDatosDemoAsync(A);
        await CargarDatosDemoAsync(B);

        await ArmarOperacionAsync(A, Actor.AdminA, Actor.OperadorA);
        await ArmarOperacionAsync(B, Actor.AdminB, Actor.OperadorB);
    }

    public async Task DisposeAsync()
    {
        foreach (var cliente in _clientes.Values)
            cliente.Dispose();

        if (_factory is not null)
            await _factory.DisposeAsync();

        await _sql.DisposeAsync();
    }

    // ------------------------------------------------------------------ HTTP

    public async Task<Respuesta> EnviarAsync(Actor actor, HttpMethod metodo, string url, HttpContent? cuerpo = null)
    {
        using var peticion = new HttpRequestMessage(metodo, url) { Content = cuerpo };
        using var respuesta = await _clientes[actor].SendAsync(peticion);
        return new Respuesta(respuesta.StatusCode, await respuesta.Content.ReadAsStringAsync());
    }

    public Task<Respuesta> GetAsync(Actor actor, string url) => EnviarAsync(actor, HttpMethod.Get, url);

    /// <summary>Petición que DEBE funcionar (armado del escenario). Si falla, explica qué pasó.</summary>
    public async Task<JsonElement> ExigirAsync(Actor actor, HttpMethod metodo, string url, HttpContent? cuerpo = null)
    {
        var respuesta = await EnviarAsync(actor, metodo, url, cuerpo);

        if (!respuesta.Exito)
            throw new InvalidOperationException($"Armado del escenario: {actor} {metodo} {url} falló → {respuesta}");

        return string.IsNullOrWhiteSpace(respuesta.Cuerpo)
            ? default
            : JsonDocument.Parse(respuesta.Cuerpo).RootElement.Clone();
    }

    public static HttpContent ComoJson(object cuerpo) => JsonContent.Create(cuerpo, cuerpo.GetType(), options: Json);

    /// <summary>multipart/form-data con un JPEG mínimo válido (la API revisa la firma del archivo).</summary>
    public static MultipartFormDataContent Formulario(IEnumerable<KeyValuePair<string, string?>> campos, string? campoArchivo = null)
    {
        var formulario = new MultipartFormDataContent();

        foreach (var (nombre, valor) in campos)
        {
            if (valor is not null)
                formulario.Add(new StringContent(valor), nombre);
        }

        if (campoArchivo is not null)
        {
            var jpeg = new ByteArrayContent([0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46, 0x49, 0x46, 0x00, 0x01, 0xFF, 0xD9]);
            jpeg.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
            formulario.Add(jpeg, campoArchivo, "prueba.jpg");
        }

        return formulario;
    }

    public static string Fecha(DateTime fecha) => fecha.ToString("yyyy-MM-ddTHH:mm:ss");

    // ------------------------------------------------------------------ BD

    public async Task<T> EnBdAsync<T>(Func<AppDbContext, IServiceProvider, Task<T>> accion)
    {
        using var scope = Factory.Services.CreateScope();
        var contexto = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return await accion(contexto, scope.ServiceProvider);
    }

    public Task<T> EnBdAsync<T>(Func<AppDbContext, Task<T>> accion) => EnBdAsync((c, _) => accion(c));

    public async Task EnviarComandoAsync(IRequest comando)
    {
        using var scope = Factory.Services.CreateScope();
        await scope.ServiceProvider.GetRequiredService<ISender>().Send(comando);
    }

    /// <summary>
    /// Todo lo que identifica a la empresa en las respuestas: los Id de TODAS sus filas en TODAS las
    /// tablas (se leen de la BD en el momento, así incluyen lo que crearon las pruebas), los de sus
    /// usuarios y asignaciones de roles, y textos propios (nombre, placas, correos).
    /// </summary>
    public async Task<IReadOnlyCollection<string>> MarcasDeAsync(DatosEmpresa empresa)
    {
        var marcas = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            $"Empresa Demo {empresa.Letra}",
            $"DEMO-{empresa.Letra}0",
            $".{empresa.Letra.ToLowerInvariant()}@demo.local",
        };

        foreach (var id in await IdsDeEmpresaAsync(empresa.EmpresaId))
            marcas.Add(id.ToString());

        return marcas;
    }

    public async Task<IReadOnlyCollection<Guid>> IdsDeEmpresaAsync(Guid empresaId)
    {
        return await EnBdAsync(async contexto =>
        {
            var ids = new HashSet<Guid> { empresaId };
            var conexion = contexto.Database.GetDbConnection();
            await conexion.OpenAsync();

            try
            {
                var empresaBytes = GuidBinarioConverter.ABytes(empresaId);

                foreach (var entidad in contexto.Model.GetEntityTypes())
                {
                    var tabla = entidad.GetTableName();
                    var empresa = entidad.FindProperty("EmpresaId");
                    var llave = entidad.FindPrimaryKey()?.Properties.SingleOrDefault();

                    if (tabla is null || empresa is null || llave is null || tabla == "empresa")
                        continue;

                    var almacen = StoreObjectIdentifier.Table(tabla, entidad.GetSchema());
                    await LeerIdsAsync(conexion, ids,
                        $"SELECT [{llave.GetColumnName(almacen)}] FROM [{tabla}] WHERE [{empresa.GetColumnName(almacen)}] = @e",
                        empresaBytes);
                }

                // Lo que no tiene empresa_id propio pero es de la empresa
                await LeerIdsAsync(conexion, ids, "SELECT u.usuario_id FROM usuario u JOIN empleado e ON e.usuario_id = u.usuario_id WHERE e.empresa_id = @e", empresaBytes);
                await LeerIdsAsync(conexion, ids, "SELECT ur.usuario_rol_id FROM usuario_rol ur JOIN rol r ON r.rol_id = ur.rol_id WHERE r.empresa_id = @e", empresaBytes);
                await LeerIdsAsync(conexion, ids, "SELECT ur.usuario_rol_id FROM usuario_rol ur JOIN empleado e ON e.usuario_id = ur.usuario_id WHERE e.empresa_id = @e", empresaBytes);
                await LeerIdsAsync(conexion, ids, "SELECT rp.rol_privilegio_id FROM rol_privilegio rp JOIN rol r ON r.rol_id = rp.rol_id WHERE r.empresa_id = @e", empresaBytes);
            }
            finally
            {
                await conexion.CloseAsync();
            }

            return (IReadOnlyCollection<Guid>)ids;
        });
    }

    /// <summary>Cuántas filas de <paramref name="tabla"/> pertenecen a la empresa (para ver si alguien creó algo ahí).</summary>
    public Task<int> ContarAsync(string tabla, Guid empresaId)
        => ContarSqlAsync($"SELECT COUNT(*) FROM [{tabla}] WHERE empresa_id = @e", empresaId);

    public Task<int> ContarSqlAsync(string sql, Guid empresaId)
        => EnBdAsync(async contexto =>
        {
            var conexion = contexto.Database.GetDbConnection();
            await conexion.OpenAsync();

            try
            {
                await using var comando = conexion.CreateCommand();
                comando.CommandText = sql;
                comando.Parameters.Add(new SqlParameter("@e", GuidBinarioConverter.ABytes(empresaId)));
                return Convert.ToInt32(await comando.ExecuteScalarAsync());
            }
            finally
            {
                await conexion.CloseAsync();
            }
        });

    private static async Task LeerIdsAsync(System.Data.Common.DbConnection conexion, HashSet<Guid> ids, string sql, byte[] empresa)
    {
        await using var comando = conexion.CreateCommand();
        comando.CommandText = sql;
        comando.Parameters.Add(new SqlParameter("@e", empresa));

        await using var lector = await comando.ExecuteReaderAsync();
        while (await lector.ReadAsync())
            ids.Add(GuidBinarioConverter.AGuid((byte[])lector[0]));
    }

    // ------------------------------------------------------------------ Armado

    private async Task IniciarSesionAsync(Actor actor, string email, string password)
    {
        var respuesta = await EnviarAsync(actor, HttpMethod.Post, "/api/auth/login", ComoJson(new { email, password }));

        if (!respuesta.Exito)
            throw new InvalidOperationException($"No se pudo iniciar sesión como {email}: {respuesta}");

        var token = JsonDocument.Parse(respuesta.Cuerpo).RootElement.GetProperty("token").GetString();
        _clientes[actor].DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    private Task CargarDatosDemoAsync(DatosEmpresa d) => EnBdAsync(async contexto =>
    {
        var empresa = await contexto.Empresas.SingleAsync(e => e.Nombre == $"Empresa Demo {d.Letra}");
        d.EmpresaId = empresa.EmpresaId;

        var admin = await contexto.Usuarios.SingleAsync(u => u.Email == $"admin.{d.Letra.ToLowerInvariant()}@demo.local");
        var operador = await contexto.Usuarios.SingleAsync(u => u.Email == $"operador.{d.Letra.ToLowerInvariant()}@demo.local");
        d.AdminUsuarioId = admin.UsuarioId;
        d.OperadorUsuarioId = operador.UsuarioId;
        d.AdminEmpleadoId = (await contexto.Empleados.SingleAsync(e => e.UsuarioId == admin.UsuarioId)).EmpleadoId;
        d.OperadorEmpleadoId = (await contexto.Empleados.SingleAsync(e => e.UsuarioId == operador.UsuarioId)).EmpleadoId;

        d.RolAdminId = (await contexto.Roles.SingleAsync(r => r.EmpresaId == d.EmpresaId && r.Nombre == SembrarDatosDemoCommandHandler.RolAdministradorEmpresa)).RolId;
        d.RolOperadorId = (await contexto.Roles.SingleAsync(r => r.EmpresaId == d.EmpresaId && r.Nombre == SembrarDatosDemoCommandHandler.RolOperador)).RolId;
        d.UsuarioRolId = (await contexto.UsuarioRoles.FirstAsync(ur => ur.RolId == d.RolOperadorId)).UsuarioRolId;
        d.RolPrivilegioId = (await contexto.RolPrivilegios.FirstAsync(rp => rp.RolId == d.RolOperadorId)).RolPrivilegioId;

        d.CentroCostoId = (await contexto.CentrosCosto.SingleAsync(c => c.EmpresaId == d.EmpresaId)).CentroCostoId;
        d.Carro1Id = (await contexto.Carros.SingleAsync(c => c.Placas == $"DEMO-{d.Letra}01")).CarroId;
        d.Carro2Id = (await contexto.Carros.SingleAsync(c => c.Placas == $"DEMO-{d.Letra}02")).CarroId;
        return 0;
    });

    /// <summary>La operación normal de la empresa, hecha por la API con sus propios usuarios.</summary>
    private async Task ArmarOperacionAsync(DatosEmpresa d, Actor admin, Actor operador)
    {
        var inicio = DateTime.Now.AddHours(-2);

        var solicitud = await ExigirAsync(operador, HttpMethod.Post, "/api/solicitudes-reserva", ComoJson(new
        {
            empresaId = d.EmpresaId,
            carroId = d.Carro1Id,
            empleadoSolicitanteId = d.OperadorEmpleadoId,
            motivo = "Ruta de pruebas",
        }));
        d.SolicitudId = solicitud.GetProperty("solicitudReservaId").GetGuid();

        await ExigirAsync(admin, HttpMethod.Post, $"/api/solicitudes-reserva/{d.SolicitudId}/aprobar",
            ComoJson(new { empleadoConfirmadorId = d.AdminEmpleadoId }));

        var viaje = await ExigirAsync(operador, HttpMethod.Post, "/api/viajes", ComoJson(new
        {
            empresaId = d.EmpresaId,
            carroId = d.Carro1Id,
            solicitudReservaId = d.SolicitudId,
            empleadoOperadorId = d.OperadorEmpleadoId,
            centroCostoId = d.CentroCostoId,
            fechaInicio = Fecha(inicio),
            kilometrajeInicial = 35000,
        }));
        d.ViajeId = viaje.GetProperty("viajeId").GetGuid();

        await ExigirAsync(operador, HttpMethod.Post, $"/api/viajes/{d.ViajeId}/puntos-ruta", ComoJson(new
        {
            puntos = Enumerable.Range(1, 5).Select(i => new
            {
                latitud = 16.75m + i * 0.002m,
                longitud = -93.12m - i * 0.002m,
                timestampCaptura = Fecha(inicio.AddMinutes(i * 5)),
                precisionMetros = 5,
            }),
        }));

        await ExigirAsync(operador, HttpMethod.Post, "/api/cargas-combustible", Formulario(new Dictionary<string, string?>
        {
            ["viajeId"] = d.ViajeId.ToString(),
            ["litros"] = "20",
            ["monto"] = "500",
            ["fechaCarga"] = Fecha(inicio.AddMinutes(30)),
            ["centroCostoId"] = d.CentroCostoId.ToString(),
        }, "fotoComprobante"));

        await ExigirAsync(operador, HttpMethod.Post, $"/api/viajes/{d.ViajeId}/finalizar", ComoJson(new
        {
            fechaFin = Fecha(inicio.AddMinutes(60)),
            kilometrajeFinal = 35012,
        }));

        var cambio = await ExigirAsync(admin, HttpMethod.Post, "/api/cambios-estatus-carro", ComoJson(new
        {
            carroId = d.Carro2Id,
            empleadoId = d.AdminEmpleadoId,
            estatusNuevo = "enMantenimiento",
            motivo = "Servicio de pruebas",
            tipoMotivo = "servicio",
        }));
        d.CambioEstatusId = cambio.GetProperty("cambioEstatusCarroId").GetGuid();

        var revision = await ExigirAsync(admin, HttpMethod.Post, "/api/auditoria/revisiones", ComoJson(new
        {
            viajeId = d.ViajeId,
            observaciones = "Revisión de pruebas",
        }));
        d.RevisionId = revision.GetProperty("revisionAuditoriaId").GetGuid();

        var dispositivo = await ExigirAsync(operador, HttpMethod.Post, "/api/dispositivos", ComoJson(new
        {
            empleadoId = d.OperadorEmpleadoId,
            tokenFcm = $"token-pruebas-{d.Letra}-{Guid.NewGuid():N}",
            plataforma = "android",
        }));
        d.DispositivoId = dispositivo.GetProperty("dispositivoEmpleadoId").GetGuid();

        await EnBdAsync(async contexto =>
        {
            d.PuntoRutaId = (await contexto.PuntosRuta.FirstAsync(p => p.ViajeId == d.ViajeId)).PuntoRutaId;
            d.CargaId = (await contexto.CargasCombustible.FirstAsync(c => c.ViajeId == d.ViajeId)).CargaCombustibleId;
            // Estos los genera la API sola; si alguno no aparece, su control de lectura lo va a señalar
            d.RutaProcesadaId = (await contexto.RutasProcesadas.FirstOrDefaultAsync(r => r.ViajeId == d.ViajeId))?.RutaProcesadaId ?? Guid.Empty;
            d.NotificacionId = (await contexto.Notificaciones.FirstOrDefaultAsync(n => n.EmpresaId == d.EmpresaId))?.NotificacionId ?? Guid.Empty;
            d.OutboxMensajeId = (await contexto.OutboxMensajes.FirstOrDefaultAsync(o => o.EmpresaId == d.EmpresaId))?.OutboxMensajeId ?? Guid.Empty;
            d.AuditoriaEventoId = (await contexto.AuditoriaEventos.FirstOrDefaultAsync(a => a.EmpresaId == d.EmpresaId))?.AuditoriaEventoId ?? Guid.Empty;
            d.SesionOperadorId = (await contexto.Sesiones.FirstAsync(s => s.UsuarioId == d.OperadorUsuarioId)).SesionUsuarioId;
            return 0;
        });
    }

    private static JsonSerializerOptions CrearOpcionesJson()
    {
        var opciones = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        opciones.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
        return opciones;
    }
}
