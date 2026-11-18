using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace APIGasoline.Infrastructure.Migrations
{
    /// <summary>
    /// A005: las últimas tablas con datos de empresa entran a la política de Row-Level Security.
    /// <list type="bullet">
    /// <item>auditoria_evento y outbox_mensaje: por su columna empresa_id. Un evento sin empresa
    /// (cambios de la plataforma a roles globales o al catálogo) solo lo ve y lo escribe la plataforma.</item>
    /// <item>sesion_usuario: por el usuario dueño, porque su empresa_id queda vacía cuando el usuario
    /// no tiene empleado.</item>
    /// </list>
    /// Solo agrega predicados a la política: no toca datos, así que no necesita SESSION_CONTEXT.
    /// Los nombres van escritos aquí (y no tomados del código) porque una migración aplicada no debe cambiar.
    /// </summary>
    public partial class AislamientoTablasTecnicas : Migration
    {
        private const string Politica = "rls.aislamiento_empresa";
        private static readonly string[] TablasConEmpresa = ["auditoria_evento", "outbox_mensaje"];
        private const string Sesiones = "sesion_usuario";
        private const string PredicadoSesiones = "rls.fn_usuario_de_la_empresa(usuario_id)";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var predicados = new List<string>();

            foreach (var tabla in TablasConEmpresa)
                predicados.AddRange(Predicados("ADD", tabla, " rls.fn_misma_empresa(empresa_id)"));

            predicados.AddRange(Predicados("ADD", Sesiones, $" {PredicadoSesiones}"));

            Ejecutar(migrationBuilder, $"ALTER SECURITY POLICY {Politica}\n    {string.Join(",\n    ", predicados)};");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var predicados = new List<string>();

            foreach (var tabla in TablasConEmpresa)
                predicados.AddRange(Predicados("DROP", tabla, ""));

            predicados.AddRange(Predicados("DROP", Sesiones, ""));

            Ejecutar(migrationBuilder, $"ALTER SECURITY POLICY {Politica}\n    {string.Join(",\n    ", predicados)};");
        }

        /// <summary>Leer (FILTER) y escribir (BLOCK AFTER INSERT/UPDATE). Al quitar (DROP) no lleva función.</summary>
        private static IEnumerable<string> Predicados(string operacion, string tabla, string funcion)
        {
            yield return $"{operacion} FILTER PREDICATE{funcion} ON dbo.{tabla}";
            yield return $"{operacion} BLOCK PREDICATE{funcion} ON dbo.{tabla} AFTER INSERT";
            yield return $"{operacion} BLOCK PREDICATE{funcion} ON dbo.{tabla} AFTER UPDATE";
        }

        /// <summary>
        /// Cada sentencia en su propio EXEC: en migraciones.sql (script idempotente) todo va en un lote.
        /// </summary>
        private static void Ejecutar(MigrationBuilder migrationBuilder, string sql)
            => migrationBuilder.Sql($"EXEC(N'{sql.Replace("'", "''")}');");
    }
}
