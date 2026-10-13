using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace APIGasoline.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AuditoriaYTrazabilidadV4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "sincronizado",
                table: "punto_ruta");

            migrationBuilder.DropColumn(
                name: "sincronizado",
                table: "cambio_estatus_carro");

            migrationBuilder.RenameColumn(
                name: "sincronizado",
                table: "viaje",
                newName: "corregido");

            migrationBuilder.RenameColumn(
                name: "delete",
                table: "empresa",
                newName: "eliminado");

            migrationBuilder.RenameColumn(
                name: "delete",
                table: "empleado",
                newName: "eliminado");

            migrationBuilder.RenameColumn(
                name: "sincronizado",
                table: "carga_combustible",
                newName: "corregido");

            migrationBuilder.AddColumn<DateTime>(
                name: "fecha_recepcion",
                table: "viaje",
                type: "timestamp",
                nullable: true,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<string>(
                name: "foto_tablero_final",
                table: "viaje",
                type: "varchar(255)",
                maxLength: 255,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "foto_tablero_inicial",
                table: "viaje",
                type: "varchar(255)",
                maxLength: 255,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "estado",
                table: "solicitud_reserva",
                type: "enum('pendiente','aprobada','rechazada','cancelada','finalizada')",
                nullable: false,
                defaultValueSql: "'pendiente'",
                oldClrType: typeof(string),
                oldType: "enum('pendiente','aprobada','rechazada','cancelada')",
                oldDefaultValueSql: "'pendiente'")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "fecha_liberacion",
                table: "solicitud_reserva",
                type: "timestamp",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "observaciones_liberacion",
                table: "solicitud_reserva",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "fecha_recepcion",
                table: "punto_ruta",
                type: "timestamp",
                nullable: true,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AlterColumn<string>(
                name: "tipo",
                table: "notificacion",
                type: "enum('nueva_solicitud','resultado_solicitud','cambio_estatus_forzado','reporte_mantenimiento')",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "enum('nueva_solicitud','resultado_solicitud','cambio_estatus_forzado')")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "activo",
                table: "empresa",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "fecha_carga_licencia",
                table: "empleado",
                type: "timestamp",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "dado_de_baja",
                table: "carro",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "fecha_baja",
                table: "carro",
                type: "timestamp",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "motivo_baja",
                table: "carro",
                type: "varchar(255)",
                maxLength: 255,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "fecha_recepcion",
                table: "carga_combustible",
                type: "timestamp",
                nullable: true,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<DateTime>(
                name: "fecha_subida_comprobante",
                table: "carga_combustible",
                type: "timestamp",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "fecha_recepcion",
                table: "cambio_estatus_carro",
                type: "timestamp",
                nullable: true,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<string>(
                name: "foto_evidencia",
                table: "cambio_estatus_carro",
                type: "varchar(255)",
                maxLength: 255,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "tipo_motivo",
                table: "cambio_estatus_carro",
                type: "enum('liberacion','falla_mecanica','choque_o_golpe','servicio','emergencia','otro')",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "auditoria_evento",
                columns: table => new
                {
                    auditoria_evento_id = table.Column<Guid>(type: "binary(16)", nullable: false),
                    empresa_id = table.Column<Guid>(type: "binary(16)", nullable: true),
                    usuario_id = table.Column<Guid>(type: "binary(16)", nullable: true),
                    accion = table.Column<string>(type: "enum('crear','actualizar','eliminar','aprobar','rechazar','cancelar','cambiar_estado','corregir')", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    entidad = table.Column<string>(type: "varchar(60)", maxLength: 60, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    entidad_id = table.Column<Guid>(type: "binary(16)", nullable: false),
                    valores_anteriores = table.Column<string>(type: "json", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    valores_nuevos = table.Column<string>(type: "json", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    motivo = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    origen = table.Column<string>(type: "enum('movil','web','sistema')", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    fecha_hora = table.Column<DateTime>(type: "timestamp", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_auditoria_evento", x => x.auditoria_evento_id);
                    table.ForeignKey(
                        name: "fk_auditoria_evento_empresa",
                        column: x => x.empresa_id,
                        principalTable: "empresa",
                        principalColumn: "empresa_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_auditoria_evento_usuario",
                        column: x => x.usuario_id,
                        principalTable: "usuario",
                        principalColumn: "usuario_id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "revision_auditoria",
                columns: table => new
                {
                    revision_auditoria_id = table.Column<Guid>(type: "binary(16)", nullable: false),
                    empresa_id = table.Column<Guid>(type: "binary(16)", nullable: false),
                    viaje_id = table.Column<Guid>(type: "binary(16)", nullable: true),
                    carro_id = table.Column<Guid>(type: "binary(16)", nullable: true),
                    periodo_desde = table.Column<DateOnly>(type: "date", nullable: true),
                    periodo_hasta = table.Column<DateOnly>(type: "date", nullable: true),
                    empleado_revisor_id = table.Column<Guid>(type: "binary(16)", nullable: false),
                    fecha_revision = table.Column<DateTime>(type: "timestamp", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    estado_revision = table.Column<string>(type: "enum('pendiente_informacion','revisado','con_observaciones')", nullable: false, defaultValueSql: "'pendiente_informacion'")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    observaciones = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    fecha_cierre = table.Column<DateTime>(type: "timestamp", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_revision_auditoria", x => x.revision_auditoria_id);
                    table.CheckConstraint("ck_revision_auditoria_alcance", "(viaje_id IS NOT NULL) OR (carro_id IS NOT NULL AND periodo_desde IS NOT NULL AND periodo_hasta IS NOT NULL)");
                    table.ForeignKey(
                        name: "fk_revision_auditoria_carro",
                        columns: x => new { x.empresa_id, x.carro_id },
                        principalTable: "carro",
                        principalColumns: new[] { "empresa_id", "carro_id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_revision_auditoria_empleado_revisor",
                        columns: x => new { x.empresa_id, x.empleado_revisor_id },
                        principalTable: "empleado",
                        principalColumns: new[] { "empresa_id", "empleado_id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_revision_auditoria_empresa",
                        column: x => x.empresa_id,
                        principalTable: "empresa",
                        principalColumn: "empresa_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_revision_auditoria_viaje",
                        columns: x => new { x.empresa_id, x.viaje_id },
                        principalTable: "viaje",
                        principalColumns: new[] { "empresa_id", "viaje_id" },
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "ix_viaje_empresa_id_carro_id_fecha_inicio",
                table: "viaje",
                columns: new[] { "empresa_id", "carro_id", "fecha_inicio" });

            migrationBuilder.CreateIndex(
                name: "ix_solicitud_reserva_empresa_id_carro_id_estado",
                table: "solicitud_reserva",
                columns: new[] { "empresa_id", "carro_id", "estado" });

            migrationBuilder.CreateIndex(
                name: "ix_cambio_estatus_carro_empresa_id_tipo_motivo",
                table: "cambio_estatus_carro",
                columns: new[] { "empresa_id", "tipo_motivo" });

            migrationBuilder.CreateIndex(
                name: "ix_auditoria_evento_empresa_id_entidad_entidad_id",
                table: "auditoria_evento",
                columns: new[] { "empresa_id", "entidad", "entidad_id" });

            migrationBuilder.CreateIndex(
                name: "ix_auditoria_evento_empresa_id_fecha_hora",
                table: "auditoria_evento",
                columns: new[] { "empresa_id", "fecha_hora" });

            migrationBuilder.CreateIndex(
                name: "ix_auditoria_evento_usuario_id_fecha_hora",
                table: "auditoria_evento",
                columns: new[] { "usuario_id", "fecha_hora" });

            migrationBuilder.CreateIndex(
                name: "ix_revision_auditoria_empresa_id",
                table: "revision_auditoria",
                column: "empresa_id");

            migrationBuilder.CreateIndex(
                name: "ix_revision_auditoria_empresa_id_carro_id_periodo_desde",
                table: "revision_auditoria",
                columns: new[] { "empresa_id", "carro_id", "periodo_desde" });

            migrationBuilder.CreateIndex(
                name: "ix_revision_auditoria_empresa_id_empleado_revisor_id",
                table: "revision_auditoria",
                columns: new[] { "empresa_id", "empleado_revisor_id" });

            migrationBuilder.CreateIndex(
                name: "ix_revision_auditoria_empresa_id_estado_revision",
                table: "revision_auditoria",
                columns: new[] { "empresa_id", "estado_revision" });

            migrationBuilder.CreateIndex(
                name: "ix_revision_auditoria_empresa_id_viaje_id",
                table: "revision_auditoria",
                columns: new[] { "empresa_id", "viaje_id" });

            migrationBuilder.CreateIndex(
                name: "uq_revision_auditoria_empresa_id_revision_auditoria_id",
                table: "revision_auditoria",
                columns: new[] { "empresa_id", "revision_auditoria_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "auditoria_evento");

            migrationBuilder.DropTable(
                name: "revision_auditoria");

            migrationBuilder.DropIndex(
                name: "ix_viaje_empresa_id_carro_id_fecha_inicio",
                table: "viaje");

            migrationBuilder.DropIndex(
                name: "ix_solicitud_reserva_empresa_id_carro_id_estado",
                table: "solicitud_reserva");

            migrationBuilder.DropIndex(
                name: "ix_cambio_estatus_carro_empresa_id_tipo_motivo",
                table: "cambio_estatus_carro");

            migrationBuilder.DropColumn(
                name: "fecha_recepcion",
                table: "viaje");

            migrationBuilder.DropColumn(
                name: "foto_tablero_final",
                table: "viaje");

            migrationBuilder.DropColumn(
                name: "foto_tablero_inicial",
                table: "viaje");

            migrationBuilder.DropColumn(
                name: "fecha_liberacion",
                table: "solicitud_reserva");

            migrationBuilder.DropColumn(
                name: "observaciones_liberacion",
                table: "solicitud_reserva");

            migrationBuilder.DropColumn(
                name: "fecha_recepcion",
                table: "punto_ruta");

            migrationBuilder.DropColumn(
                name: "activo",
                table: "empresa");

            migrationBuilder.DropColumn(
                name: "fecha_carga_licencia",
                table: "empleado");

            migrationBuilder.DropColumn(
                name: "dado_de_baja",
                table: "carro");

            migrationBuilder.DropColumn(
                name: "fecha_baja",
                table: "carro");

            migrationBuilder.DropColumn(
                name: "motivo_baja",
                table: "carro");

            migrationBuilder.DropColumn(
                name: "fecha_recepcion",
                table: "carga_combustible");

            migrationBuilder.DropColumn(
                name: "fecha_subida_comprobante",
                table: "carga_combustible");

            migrationBuilder.DropColumn(
                name: "fecha_recepcion",
                table: "cambio_estatus_carro");

            migrationBuilder.DropColumn(
                name: "foto_evidencia",
                table: "cambio_estatus_carro");

            migrationBuilder.DropColumn(
                name: "tipo_motivo",
                table: "cambio_estatus_carro");

            migrationBuilder.RenameColumn(
                name: "corregido",
                table: "viaje",
                newName: "sincronizado");

            migrationBuilder.RenameColumn(
                name: "eliminado",
                table: "empresa",
                newName: "delete");

            migrationBuilder.RenameColumn(
                name: "eliminado",
                table: "empleado",
                newName: "delete");

            migrationBuilder.RenameColumn(
                name: "corregido",
                table: "carga_combustible",
                newName: "sincronizado");

            migrationBuilder.AlterColumn<string>(
                name: "estado",
                table: "solicitud_reserva",
                type: "enum('pendiente','aprobada','rechazada','cancelada')",
                nullable: false,
                defaultValueSql: "'pendiente'",
                oldClrType: typeof(string),
                oldType: "enum('pendiente','aprobada','rechazada','cancelada','finalizada')",
                oldDefaultValueSql: "'pendiente'")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "sincronizado",
                table: "punto_ruta",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "tipo",
                table: "notificacion",
                type: "enum('nueva_solicitud','resultado_solicitud','cambio_estatus_forzado')",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "enum('nueva_solicitud','resultado_solicitud','cambio_estatus_forzado','reporte_mantenimiento')")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "sincronizado",
                table: "cambio_estatus_carro",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }
    }
}
