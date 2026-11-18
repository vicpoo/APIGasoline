using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace APIGasoline.Infrastructure.Migrations
{
    /// <summary>
    /// A007: la BD exige que el usuario de un empleado sea de la misma empresa que el empleado (o de ninguna).
    /// Es el vínculo más sensible: de él sale la empresa con la que se emite el token
    /// (empleado?.EmpresaId ?? usuario.EmpresaId). Las demás relaciones lo logran con FK compuestas
    /// (empresa_id, id), pero usuario.empresa_id admite NULL (usuarios de la plataforma) y una FK
    /// compuesta con una columna nula no se evalúa.
    ///
    /// Se resuelve en los predicados de bloqueo de Row-Level Security que ya tienen empleado y usuario
    /// (un disparador obligaría a quitar el OUTPUT con el que EF guarda). Cada función conserva la regla
    /// anterior (la fila es de la empresa de la sesión) y agrega la coherencia empleado-usuario:
    /// - empleado: su usuario debe existir para la sesión y tener la misma empresa o ninguna. Con una
    ///   empresa en la sesión, un usuario de otra empresa no es visible, así que también se rechaza.
    /// - usuario: no puede quedar en una empresa distinta de la de su empleado.
    /// No revisa las filas que ya existen: antes de aplicarla conviene correr la consulta de
    /// BD/README.md (sección de Row-Level Security) para encontrar empleados incoherentes.
    /// Los nombres van escritos aquí (y no tomados del código) porque una migración aplicada no debe cambiar.
    /// </summary>
    public partial class EmpleadoUsuarioMismaEmpresa : Migration
    {
        private const string Politica = "rls.aislamiento_empresa";
        private const string VeTodas = "CAST(SESSION_CONTEXT(N'apigasoline.ve_todas') AS bit) = 1";
        private const string EmpresaDeLaSesion = "CAST(SESSION_CONTEXT(N'apigasoline.empresa_id') AS binary(16))";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            Ejecutar(migrationBuilder, $"""
                CREATE FUNCTION rls.fn_empleado_escribible(@empresa_id binary(16), @usuario_id binary(16))
                RETURNS TABLE
                WITH SCHEMABINDING
                AS
                RETURN SELECT 1 AS permitido
                WHERE ({VeTodas} OR @empresa_id = {EmpresaDeLaSesion})
                  AND EXISTS (SELECT 1 FROM dbo.usuario AS u
                              WHERE u.usuario_id = @usuario_id
                                AND (u.empresa_id IS NULL OR u.empresa_id = @empresa_id));
                """);

            Ejecutar(migrationBuilder, $"""
                CREATE FUNCTION rls.fn_usuario_escribible(@empresa_id binary(16), @usuario_id binary(16))
                RETURNS TABLE
                WITH SCHEMABINDING
                AS
                RETURN SELECT 1 AS permitido
                WHERE ({VeTodas} OR @empresa_id = {EmpresaDeLaSesion})
                  AND (@empresa_id IS NULL
                       OR NOT EXISTS (SELECT 1 FROM dbo.empleado AS e
                                      WHERE e.usuario_id = @usuario_id AND e.empresa_id <> @empresa_id));
                """);

            Ejecutar(migrationBuilder, $"""
                ALTER SECURITY POLICY {Politica}
                    ALTER BLOCK PREDICATE rls.fn_empleado_escribible(empresa_id, usuario_id) ON dbo.empleado AFTER INSERT,
                    ALTER BLOCK PREDICATE rls.fn_empleado_escribible(empresa_id, usuario_id) ON dbo.empleado AFTER UPDATE,
                    ALTER BLOCK PREDICATE rls.fn_usuario_escribible(empresa_id, usuario_id) ON dbo.usuario AFTER INSERT,
                    ALTER BLOCK PREDICATE rls.fn_usuario_escribible(empresa_id, usuario_id) ON dbo.usuario AFTER UPDATE;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            Ejecutar(migrationBuilder, $"""
                ALTER SECURITY POLICY {Politica}
                    ALTER BLOCK PREDICATE rls.fn_misma_empresa(empresa_id) ON dbo.empleado AFTER INSERT,
                    ALTER BLOCK PREDICATE rls.fn_misma_empresa(empresa_id) ON dbo.empleado AFTER UPDATE,
                    ALTER BLOCK PREDICATE rls.fn_misma_empresa(empresa_id) ON dbo.usuario AFTER INSERT,
                    ALTER BLOCK PREDICATE rls.fn_misma_empresa(empresa_id) ON dbo.usuario AFTER UPDATE;
                """);

            Ejecutar(migrationBuilder, "DROP FUNCTION IF EXISTS rls.fn_usuario_escribible;");
            Ejecutar(migrationBuilder, "DROP FUNCTION IF EXISTS rls.fn_empleado_escribible;");
        }

        /// <summary>
        /// Cada sentencia en su propio EXEC: CREATE FUNCTION tiene que ser la única del lote, y en
        /// migraciones.sql (script idempotente) todo va junto.
        /// </summary>
        private static void Ejecutar(MigrationBuilder migrationBuilder, string sql)
            => migrationBuilder.Sql($"EXEC(N'{sql.Replace("'", "''")}');");
    }
}
