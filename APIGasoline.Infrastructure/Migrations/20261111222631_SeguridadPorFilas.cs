using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace APIGasoline.Infrastructure.Migrations
{
    /// <summary>
    /// Row-Level Security: segundo candado del aislamiento entre empresas, dentro de SQL Server.
    /// La política <c>rls.aislamiento_empresa</c> filtra (lecturas) y bloquea (escrituras) las filas
    /// según el SESSION_CONTEXT que manda la API en cada conexión (<see cref="ContextoSesionSqlInterceptor"/>).
    /// Sin ese contexto (SSMS, otro programa) no se ve ninguna fila de estas tablas.
    /// Mismas reglas que el filtro de EF (AppDbContext) y el guardián (AislamientoEmpresaInterceptor).
    /// Los nombres van escritos aquí (y no tomados de Persistence.SeguridadPorFilas) porque una
    /// migración ya aplicada no debe cambiar si cambia el código.
    /// </summary>
    public partial class SeguridadPorFilas : Migration
    {
        /// <summary>Tablas con columna empresa_id: cada fila es de UNA empresa (empresa_id NULL solo la ve la plataforma).</summary>
        private static readonly string[] TablasConEmpresa =
        [
            "carro", "centro_costo", "empleado", "solicitud_reserva", "viaje", "viaje_pausa", "punto_ruta",
            "ruta_procesada", "carga_combustible", "cambio_estatus_carro", "notificacion", "dispositivo_empleado",
            "revision_auditoria", "empresa", "usuario",
        ];

        private const string Politica = "rls.aislamiento_empresa";
        private const string VeTodas = "CAST(SESSION_CONTEXT(N'apigasoline.ve_todas') AS bit) = 1";
        private const string EmpresaDeLaSesion = "CAST(SESSION_CONTEXT(N'apigasoline.empresa_id') AS binary(16))";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            Ejecutar(migrationBuilder, "CREATE SCHEMA rls AUTHORIZATION dbo;");

            // La fila es de la empresa de la sesión (o la sesión ve todas)
            Ejecutar(migrationBuilder, $"""
                CREATE FUNCTION rls.fn_misma_empresa(@empresa_id binary(16))
                RETURNS TABLE
                WITH SCHEMABINDING
                AS
                RETURN SELECT 1 AS permitido
                WHERE {VeTodas} OR @empresa_id = {EmpresaDeLaSesion};
                """);

            // Roles: también se LEEN los globales (empresa_id NULL); escribirlos solo la plataforma
            Ejecutar(migrationBuilder, $"""
                CREATE FUNCTION rls.fn_rol_visible(@empresa_id binary(16))
                RETURNS TABLE
                WITH SCHEMABINDING
                AS
                RETURN SELECT 1 AS permitido
                WHERE {VeTodas} OR @empresa_id IS NULL OR @empresa_id = {EmpresaDeLaSesion};
                """);

            // Asignaciones de roles: se ven las de los usuarios de la empresa
            Ejecutar(migrationBuilder, $"""
                CREATE FUNCTION rls.fn_usuario_de_la_empresa(@usuario_id binary(16))
                RETURNS TABLE
                WITH SCHEMABINDING
                AS
                RETURN SELECT 1 AS permitido
                WHERE {VeTodas}
                   OR EXISTS (SELECT 1 FROM dbo.usuario AS u
                              WHERE u.usuario_id = @usuario_id AND u.empresa_id = {EmpresaDeLaSesion});
                """);

            // ...y solo se escriben si el usuario Y el rol son de la empresa (nunca un rol global)
            Ejecutar(migrationBuilder, $"""
                CREATE FUNCTION rls.fn_asignacion_de_la_empresa(@usuario_id binary(16), @rol_id binary(16))
                RETURNS TABLE
                WITH SCHEMABINDING
                AS
                RETURN SELECT 1 AS permitido
                WHERE {VeTodas}
                   OR (EXISTS (SELECT 1 FROM dbo.usuario AS u
                               WHERE u.usuario_id = @usuario_id AND u.empresa_id = {EmpresaDeLaSesion})
                       AND EXISTS (SELECT 1 FROM dbo.rol AS r
                                   WHERE r.rol_id = @rol_id AND r.empresa_id = {EmpresaDeLaSesion}));
                """);

            // Privilegios de roles: se ven los de los roles visibles (propios y globales)...
            Ejecutar(migrationBuilder, $"""
                CREATE FUNCTION rls.fn_rol_privilegio_visible(@rol_id binary(16))
                RETURNS TABLE
                WITH SCHEMABINDING
                AS
                RETURN SELECT 1 AS permitido
                WHERE {VeTodas}
                   OR EXISTS (SELECT 1 FROM dbo.rol AS r
                              WHERE r.rol_id = @rol_id AND (r.empresa_id IS NULL OR r.empresa_id = {EmpresaDeLaSesion}));
                """);

            // ...y solo se escriben los de roles de la empresa
            Ejecutar(migrationBuilder, $"""
                CREATE FUNCTION rls.fn_rol_de_la_empresa(@rol_id binary(16))
                RETURNS TABLE
                WITH SCHEMABINDING
                AS
                RETURN SELECT 1 AS permitido
                WHERE {VeTodas}
                   OR EXISTS (SELECT 1 FROM dbo.rol AS r
                              WHERE r.rol_id = @rol_id AND r.empresa_id = {EmpresaDeLaSesion});
                """);

            var predicados = new List<string>();

            // FILTER: no se leen (ni se pueden editar o borrar) filas de otra empresa.
            // BLOCK AFTER INSERT/UPDATE: no se puede dejar una fila en otra empresa.
            foreach (var tabla in TablasConEmpresa)
            {
                predicados.Add($"ADD FILTER PREDICATE rls.fn_misma_empresa(empresa_id) ON dbo.{tabla}");
                predicados.Add($"ADD BLOCK PREDICATE rls.fn_misma_empresa(empresa_id) ON dbo.{tabla} AFTER INSERT");
                predicados.Add($"ADD BLOCK PREDICATE rls.fn_misma_empresa(empresa_id) ON dbo.{tabla} AFTER UPDATE");
            }

            // Roles, asignaciones y privilegios de roles: también BEFORE UPDATE/DELETE, porque se LEEN
            // filas que no se pueden tocar (roles globales y lo que cuelga de ellos)
            AgregarConBloqueoCompleto(predicados, "rol", "rls.fn_rol_visible(empresa_id)", "rls.fn_misma_empresa(empresa_id)");
            AgregarConBloqueoCompleto(predicados, "usuario_rol", "rls.fn_usuario_de_la_empresa(usuario_id)", "rls.fn_asignacion_de_la_empresa(usuario_id, rol_id)");
            AgregarConBloqueoCompleto(predicados, "rol_privilegio", "rls.fn_rol_privilegio_visible(rol_id)", "rls.fn_rol_de_la_empresa(rol_id)");

            Ejecutar(migrationBuilder,
                $"CREATE SECURITY POLICY {Politica}\n    {string.Join(",\n    ", predicados)}\nWITH (STATE = ON, SCHEMABINDING = ON);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            Ejecutar(migrationBuilder, $"DROP SECURITY POLICY IF EXISTS {Politica};");
            Ejecutar(migrationBuilder, "DROP FUNCTION IF EXISTS rls.fn_rol_de_la_empresa;");
            Ejecutar(migrationBuilder, "DROP FUNCTION IF EXISTS rls.fn_rol_privilegio_visible;");
            Ejecutar(migrationBuilder, "DROP FUNCTION IF EXISTS rls.fn_asignacion_de_la_empresa;");
            Ejecutar(migrationBuilder, "DROP FUNCTION IF EXISTS rls.fn_usuario_de_la_empresa;");
            Ejecutar(migrationBuilder, "DROP FUNCTION IF EXISTS rls.fn_rol_visible;");
            Ejecutar(migrationBuilder, "DROP FUNCTION IF EXISTS rls.fn_misma_empresa;");
            Ejecutar(migrationBuilder, "DROP SCHEMA IF EXISTS rls;");
        }

        private static void AgregarConBloqueoCompleto(List<string> predicados, string tabla, string lectura, string escritura)
        {
            predicados.Add($"ADD FILTER PREDICATE {lectura} ON dbo.{tabla}");
            predicados.Add($"ADD BLOCK PREDICATE {escritura} ON dbo.{tabla} AFTER INSERT");
            predicados.Add($"ADD BLOCK PREDICATE {escritura} ON dbo.{tabla} AFTER UPDATE");
            predicados.Add($"ADD BLOCK PREDICATE {escritura} ON dbo.{tabla} BEFORE UPDATE");
            predicados.Add($"ADD BLOCK PREDICATE {escritura} ON dbo.{tabla} BEFORE DELETE");
        }

        /// <summary>
        /// Cada sentencia en su propio EXEC: CREATE SCHEMA / FUNCTION / SECURITY POLICY tienen que ser
        /// la única del lote, y en migraciones.sql (script idempotente) todo va junto.
        /// </summary>
        private static void Ejecutar(MigrationBuilder migrationBuilder, string sql)
            => migrationBuilder.Sql($"EXEC(N'{sql.Replace("'", "''")}');");
    }
}
