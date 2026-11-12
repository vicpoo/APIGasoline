using System.Net;
using APIGasoline.Api.IntegrationTests.Infraestructura;
using APIGasoline.Infrastructure.Persistence;
using APIGasoline.Infrastructure.Persistence.Converters;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace APIGasoline.Api.IntegrationTests;

// =====================================================================================
//  ROW-LEVEL SECURITY (segundo candado, dentro de SQL Server)
//  Aquí se habla con SQL Server DIRECTO, sin la API ni el filtro de EF: es lo que pasaría si
//  alguien escribiera SQL a mano o se saltara el filtro. SQL Server tampoco debe dejar ver ni
//  tocar filas de otra empresa.
// =====================================================================================

[Collection(ColeccionApi.Nombre)]
public sealed class SeguridadPorFilasTests(Escenario e)
{
    /// <summary>"The attempted operation failed because the target object ... has a block predicate".</summary>
    private const int ErrorPredicadoDeBloqueo = 33504;

    [Fact]
    public async Task Sin_contexto_de_sesion_no_se_ve_ninguna_fila()
    {
        await using var conexion = await AbrirAsync(contexto: false);

        Assert.Equal(0, await ContarAsync(conexion, "SELECT COUNT(*) FROM carro"));
        Assert.Equal(0, await ContarAsync(conexion, "SELECT COUNT(*) FROM empleado"));
        Assert.Equal(0, await ContarAsync(conexion, "SELECT COUNT(*) FROM usuario"));
        Assert.Equal(0, await ContarAsync(conexion, "SELECT COUNT(*) FROM empresa"));
    }

    [Fact]
    public async Task Control_la_plataforma_ve_las_dos_empresas()
    {
        await using var conexion = await AbrirAsync(veTodas: true);

        Assert.True(await ContarDeAsync(conexion, "carro", e.A.EmpresaId) > 0);
        Assert.True(await ContarDeAsync(conexion, "carro", e.B.EmpresaId) > 0);
    }

    [Fact]
    public async Task Con_la_empresa_A_solo_se_ve_lo_de_A()
    {
        await using var conexion = await AbrirAsync(empresa: e.A.EmpresaId);

        foreach (var tabla in new[] { "carro", "empleado", "viaje", "punto_ruta", "carga_combustible", "notificacion", "usuario" })
        {
            Assert.Equal(0, await ContarDeAsync(conexion, tabla, e.B.EmpresaId));
            Assert.True(await ContarDeAsync(conexion, tabla, e.A.EmpresaId) > 0, $"Control: A debería ver sus filas de {tabla}.");
        }

        Assert.Equal(1, await ContarAsync(conexion, "SELECT COUNT(*) FROM empresa"));
    }

    [Fact]
    public async Task Con_la_empresa_A_no_se_puede_crear_nada_en_B()
    {
        await using var conexion = await AbrirAsync(empresa: e.A.EmpresaId);

        var error = await Assert.ThrowsAsync<SqlException>(() => EjecutarAsync(conexion,
            "INSERT INTO centro_costo (centro_costo_id, empresa_id, nombre, clave) VALUES (@id, @b, N'Intruso', N'RLS-1')",
            ("@id", Guid.CreateVersion7()), ("@b", e.B.EmpresaId)));

        Assert.Equal(ErrorPredicadoDeBloqueo, error.Number);
    }

    /// <summary>Fase 8: corporativos, divisiones y sus ligas también son de cada empresa.</summary>
    [Fact]
    public async Task Con_la_empresa_A_la_estructura_organizacional_de_B_no_existe()
    {
        var (corporativoB, divisionB) = await e.CorporativoYDivisionAsync(e.B, ligados: false);
        await using var conexion = await AbrirAsync(empresa: e.A.EmpresaId);

        foreach (var tabla in new[] { "corporativo", "division", "corporativo_division" })
        {
            Assert.Equal(0, await ContarDeAsync(conexion, tabla, e.B.EmpresaId));
            Assert.True(await ContarDeAsync(conexion, tabla, e.A.EmpresaId) > 0, $"Control: A debería ver sus filas de {tabla}.");
        }

        // Ligar en B (un corporativo y una división reales de B, sin ligar) lo frena el predicado de bloqueo
        var error = await Assert.ThrowsAsync<SqlException>(() => EjecutarAsync(conexion,
            "INSERT INTO corporativo_division (corporativo_division_id, empresa_id, corporativo_id, division_id) VALUES (@id, @b, @c, @d)",
            ("@id", Guid.CreateVersion7()), ("@b", e.B.EmpresaId), ("@c", corporativoB), ("@d", divisionB)));

        Assert.Equal(ErrorPredicadoDeBloqueo, error.Number);
    }

    [Fact]
    public async Task La_BD_no_deja_ligar_ni_usar_un_corporativo_de_otra_empresa()
    {
        // Aun viendo todas las empresas (plataforma), las FK compuestas con empresa_id lo impiden
        await using var conexion = await AbrirAsync(veTodas: true);

        var liga = await Assert.ThrowsAsync<SqlException>(() => EjecutarAsync(conexion,
            "INSERT INTO corporativo_division (corporativo_division_id, empresa_id, corporativo_id, division_id) VALUES (@id, @a, @c, @d)",
            ("@id", Guid.CreateVersion7()), ("@a", e.A.EmpresaId), ("@c", e.A.CorporativoId), ("@d", e.B.DivisionId)));

        var centro = await Assert.ThrowsAsync<SqlException>(() => EjecutarAsync(conexion,
            "UPDATE centro_costo SET corporativo_id = @c WHERE centro_costo_id = @id",
            ("@c", e.B.CorporativoId), ("@id", e.A.CentroCostoId)));

        Assert.Equal(547, liga.Number);   // violación de FK
        Assert.Equal(547, centro.Number);
    }

    [Fact]
    public async Task Con_la_empresa_A_no_se_puede_editar_ni_borrar_lo_de_B()
    {
        await using var conexion = await AbrirAsync(empresa: e.A.EmpresaId);

        // Las filas de B "no existen": el UPDATE y el DELETE no encuentran nada
        Assert.Equal(0, await EjecutarAsync(conexion, "UPDATE carro SET color = N'Robado' WHERE empresa_id = @b", ("@b", e.B.EmpresaId)));
        Assert.Equal(0, await EjecutarAsync(conexion, "DELETE FROM notificacion WHERE empresa_id = @b", ("@b", e.B.EmpresaId)));
    }

    [Fact]
    public async Task Con_la_empresa_A_no_se_puede_pasar_un_registro_a_B()
    {
        var centroId = await e.CentroCostoAsync(e.A);
        await using var conexion = await AbrirAsync(empresa: e.A.EmpresaId);

        var error = await Assert.ThrowsAsync<SqlException>(() => EjecutarAsync(conexion,
            "UPDATE centro_costo SET empresa_id = @b WHERE centro_costo_id = @id",
            ("@b", e.B.EmpresaId), ("@id", centroId)));

        Assert.Equal(ErrorPredicadoDeBloqueo, error.Number);
    }

    [Fact]
    public async Task Con_la_empresa_A_se_leen_los_roles_globales_pero_no_se_tocan()
    {
        await using var conexion = await AbrirAsync(empresa: e.A.EmpresaId);

        Assert.True(await ContarAsync(conexion, "SELECT COUNT(*) FROM rol WHERE empresa_id IS NULL") > 0);

        var error = await Assert.ThrowsAsync<SqlException>(() => EjecutarAsync(conexion,
            "UPDATE rol SET descripcion = N'Tomado' WHERE empresa_id IS NULL"));

        Assert.Equal(ErrorPredicadoDeBloqueo, error.Number);
    }

    [Fact]
    public async Task Con_la_empresa_A_no_se_asigna_un_rol_global()
    {
        await using var conexion = await AbrirAsync(empresa: e.A.EmpresaId);

        var error = await Assert.ThrowsAsync<SqlException>(() => EjecutarAsync(conexion,
            """
            INSERT INTO usuario_rol (usuario_rol_id, usuario_id, rol_id)
            SELECT TOP (1) @id, @usuario, r.rol_id FROM rol AS r WHERE r.empresa_id IS NULL
            """,
            ("@id", Guid.CreateVersion7()), ("@usuario", e.A.AdminUsuarioId)));

        Assert.Equal(ErrorPredicadoDeBloqueo, error.Number);
    }

    [Fact]
    public async Task Cada_tabla_filtrada_en_EF_tiene_su_politica_en_SQL_Server()
    {
        var tablasFiltradas = await e.EnBdAsync(c => Task.FromResult(c.Model.GetEntityTypes()
            .Where(t => t.FindDeclaredQueryFilter(FiltrosConsulta.Empresa) is not null)
            .Select(t => t.GetTableName()!)
            .ToHashSet()));

        await using var conexion = await AbrirAsync(veTodas: true);
        var conPolitica = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);

        await using (var comando = conexion.CreateCommand())
        {
            comando.CommandText = """
                SELECT OBJECT_NAME(p.target_object_id), p.predicate_type_desc + ISNULL(' ' + p.operation_desc, '')
                FROM sys.security_predicates AS p
                JOIN sys.security_policies AS s ON s.object_id = p.object_id
                WHERE SCHEMA_NAME(s.schema_id) + '.' + s.name = @politica AND s.is_enabled = 1
                """;
            comando.Parameters.AddWithValue("@politica", SeguridadPorFilas.Politica);

            await using var lector = await comando.ExecuteReaderAsync();
            while (await lector.ReadAsync())
            {
                var tabla = lector.GetString(0);
                if (!conPolitica.TryGetValue(tabla, out var tipos))
                    conPolitica[tabla] = tipos = [];
                tipos.Add(lector.GetString(1));
            }
        }

        var faltan = tablasFiltradas
            .Where(t => !conPolitica.TryGetValue(t, out var tipos)
                        || !tipos.Contains("FILTER")
                        || !tipos.Contains("BLOCK AFTER INSERT")
                        || !tipos.Contains("BLOCK AFTER UPDATE"))
            .OrderBy(t => t)
            .ToList();

        Assert.True(faltan.Count == 0,
            "Estas tablas tienen el filtro por empresa en EF pero no su política de Row-Level Security " +
            $"(FILTER + BLOCK AFTER INSERT/UPDATE): {string.Join(", ", faltan)}. Agrégalas con una migración.");
    }

    [Fact]
    public async Task Repetir_las_placas_de_otra_empresa_responde_409_sin_decir_de_quien_son()
    {
        var placasDeB = await e.EnBdAsync(async c => (await c.Carros.FirstAsync(x => x.CarroId == e.B.Carro1Id)).Placas);

        var respuesta = await e.EnviarAsync(Actor.AdminA, HttpMethod.Post, "/api/carros", Escenario.ComoJson(new
        {
            empresaId = e.A.EmpresaId,
            marca = "Marca",
            modelo = "Modelo",
            anio = 2022,
            placas = placasDeB,
            kilometrajeActual = 1000,
        }));

        Assert.Equal(HttpStatusCode.Conflict, respuesta.Status);
        Assert.DoesNotContain(e.B.EmpresaId.ToString(), respuesta.Cuerpo, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(placasDeB, respuesta.Cuerpo, StringComparison.OrdinalIgnoreCase);
    }

    // ---- Conexión directa a SQL Server con el SESSION_CONTEXT que mandaría la API

    private async Task<SqlConnection> AbrirAsync(Guid? empresa = null, bool veTodas = false, bool contexto = true)
    {
        var conexion = new SqlConnection(e.CadenaConexion);
        await conexion.OpenAsync();

        if (contexto && (veTodas || empresa is not null))
        {
            await EjecutarAsync(conexion,
                $"EXEC sys.sp_set_session_context N'{SeguridadPorFilas.ClaveEmpresa}', @e; " +
                $"EXEC sys.sp_set_session_context N'{SeguridadPorFilas.ClaveVeTodas}', @t;",
                ("@e", empresa ?? Guid.Empty), ("@t", veTodas));
        }

        return conexion;
    }

    private static async Task<int> ContarAsync(SqlConnection conexion, string sql, params (string Nombre, object Valor)[] parametros)
    {
        await using var comando = Comando(conexion, sql, parametros);
        return Convert.ToInt32(await comando.ExecuteScalarAsync());
    }

    private static Task<int> ContarDeAsync(SqlConnection conexion, string tabla, Guid empresaId)
        => ContarAsync(conexion, $"SELECT COUNT(*) FROM [{tabla}] WHERE empresa_id = @e", ("@e", empresaId));

    private static async Task<int> EjecutarAsync(SqlConnection conexion, string sql, params (string Nombre, object Valor)[] parametros)
    {
        await using var comando = Comando(conexion, sql, parametros);
        return await comando.ExecuteNonQueryAsync();
    }

    private static SqlCommand Comando(SqlConnection conexion, string sql, (string Nombre, object Valor)[] parametros)
    {
        var comando = conexion.CreateCommand();
        comando.CommandText = sql;

        foreach (var (nombre, valor) in parametros)
        {
            comando.Parameters.Add(valor is Guid id
                ? new SqlParameter(nombre, System.Data.SqlDbType.Binary, 16) { Value = GuidBinarioConverter.ABytes(id) }
                : new SqlParameter(nombre, valor));
        }

        return comando;
    }
}
