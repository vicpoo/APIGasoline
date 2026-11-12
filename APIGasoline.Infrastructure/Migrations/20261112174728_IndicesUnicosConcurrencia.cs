using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace APIGasoline.Infrastructure.Migrations
{
    /// <summary>
    /// Fase 9B (F010): índices únicos para lo que antes solo revisaba la aplicación ("leer y luego
    /// insertar") y dos peticiones simultáneas podían duplicar:
    /// - un solo viaje en curso por carro (índice filtrado estado = 'en_curso'),
    /// - una sola pausa abierta por viaje (índice filtrado fecha_fin IS NULL),
    /// - un punto GPS por (viaje, timestamp_captura).
    /// Antes de crearlos se limpian los duplicados que ya hubiera: puntos repetidos se borran (queda
    /// el primero), pausas abiertas de más se cierran en su mismo inicio y viajes en curso de más
    /// (los más viejos de cada carro) quedan interrumpidos.
    /// </summary>
    public partial class IndicesUnicosConcurrencia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Las tres tablas tienen Row-Level Security: esta conexión debe ver todas las empresas
            Ejecutar(migrationBuilder, "EXEC sys.sp_set_session_context @key = N'apigasoline.ve_todas', @value = 1;");

            Ejecutar(migrationBuilder, """
                WITH repetidos AS (
                    SELECT ROW_NUMBER() OVER (PARTITION BY viaje_id, timestamp_captura ORDER BY punto_ruta_id) AS n
                    FROM dbo.punto_ruta)
                DELETE FROM repetidos WHERE n > 1;
                """);

            Ejecutar(migrationBuilder, """
                WITH abiertas AS (
                    SELECT fecha_inicio, fecha_fin,
                           ROW_NUMBER() OVER (PARTITION BY viaje_id ORDER BY fecha_inicio DESC, viaje_pausa_id DESC) AS n
                    FROM dbo.viaje_pausa
                    WHERE fecha_fin IS NULL)
                UPDATE abiertas SET fecha_fin = fecha_inicio WHERE n > 1;
                """);

            Ejecutar(migrationBuilder, """
                WITH en_curso AS (
                    SELECT estado, fecha_fin, motivo_interrupcion,
                           ROW_NUMBER() OVER (PARTITION BY carro_id ORDER BY fecha_inicio DESC, viaje_id DESC) AS n
                    FROM dbo.viaje
                    WHERE estado = 'en_curso')
                UPDATE en_curso
                SET estado = 'interrumpido',
                    fecha_fin = SYSDATETIME(),
                    motivo_interrupcion = N'Cerrado por la migración: el carro tenía otro viaje en curso más reciente.'
                WHERE n > 1;
                """);

            migrationBuilder.DropIndex(
                name: "ix_punto_ruta_viaje_id_timestamp_captura",
                table: "punto_ruta");

            migrationBuilder.CreateIndex(
                name: "uq_viaje_pausa_viaje_id_abierta",
                table: "viaje_pausa",
                column: "viaje_id",
                unique: true,
                filter: "[fecha_fin] IS NULL");

            migrationBuilder.CreateIndex(
                name: "uq_viaje_carro_id_en_curso",
                table: "viaje",
                column: "carro_id",
                unique: true,
                filter: "[estado] = 'en_curso'");

            migrationBuilder.CreateIndex(
                name: "uq_punto_ruta_viaje_id_timestamp_captura",
                table: "punto_ruta",
                columns: new[] { "viaje_id", "timestamp_captura" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "uq_viaje_pausa_viaje_id_abierta",
                table: "viaje_pausa");

            migrationBuilder.DropIndex(
                name: "uq_viaje_carro_id_en_curso",
                table: "viaje");

            migrationBuilder.DropIndex(
                name: "uq_punto_ruta_viaje_id_timestamp_captura",
                table: "punto_ruta");

            migrationBuilder.CreateIndex(
                name: "ix_punto_ruta_viaje_id_timestamp_captura",
                table: "punto_ruta",
                columns: new[] { "viaje_id", "timestamp_captura" });
        }

        /// <summary>Cada sentencia en su propio EXEC (en migraciones.sql todo va en un solo lote).</summary>
        private static void Ejecutar(MigrationBuilder migrationBuilder, string sql)
            => migrationBuilder.Sql($"EXEC(N'{sql.Replace("'", "''")}');");
    }
}
