using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace APIGasoline.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class OutboxYDispositivos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "dispositivo_empleado",
                columns: table => new
                {
                    dispositivo_empleado_id = table.Column<Guid>(type: "binary(16)", nullable: false),
                    empresa_id = table.Column<Guid>(type: "binary(16)", nullable: false),
                    empleado_id = table.Column<Guid>(type: "binary(16)", nullable: false),
                    token_fcm = table.Column<string>(type: "varchar(512)", maxLength: 512, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    plataforma = table.Column<string>(type: "enum('android','ios','web')", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    activo = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    fecha_registro = table.Column<DateTime>(type: "timestamp", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    fecha_ultimo_uso = table.Column<DateTime>(type: "timestamp", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dispositivo_empleado", x => x.dispositivo_empleado_id);
                    table.ForeignKey(
                        name: "fk_dispositivo_empleado_empleado",
                        columns: x => new { x.empresa_id, x.empleado_id },
                        principalTable: "empleado",
                        principalColumns: new[] { "empresa_id", "empleado_id" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_dispositivo_empleado_empresa",
                        column: x => x.empresa_id,
                        principalTable: "empresa",
                        principalColumn: "empresa_id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "outbox_mensaje",
                columns: table => new
                {
                    outbox_mensaje_id = table.Column<Guid>(type: "binary(16)", nullable: false),
                    empresa_id = table.Column<Guid>(type: "binary(16)", nullable: false),
                    tipo = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    agregado_id = table.Column<Guid>(type: "binary(16)", nullable: false),
                    payload = table.Column<string>(type: "json", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    estado = table.Column<string>(type: "enum('pendiente','procesando','enviado','fallido')", nullable: false, defaultValueSql: "'pendiente'")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    intentos = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    proximo_intento = table.Column<DateTime>(type: "timestamp", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ultimo_error = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    fecha_creacion = table.Column<DateTime>(type: "timestamp", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    fecha_procesado = table.Column<DateTime>(type: "timestamp", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_outbox_mensaje", x => x.outbox_mensaje_id);
                    table.ForeignKey(
                        name: "fk_outbox_mensaje_empresa",
                        column: x => x.empresa_id,
                        principalTable: "empresa",
                        principalColumn: "empresa_id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "ix_dispositivo_empleado_empleado_id_activo",
                table: "dispositivo_empleado",
                columns: new[] { "empleado_id", "activo" });

            migrationBuilder.CreateIndex(
                name: "ix_dispositivo_empleado_empresa_id",
                table: "dispositivo_empleado",
                column: "empresa_id");

            migrationBuilder.CreateIndex(
                name: "ix_dispositivo_empleado_empresa_id_empleado_id",
                table: "dispositivo_empleado",
                columns: new[] { "empresa_id", "empleado_id" });

            migrationBuilder.CreateIndex(
                name: "uq_dispositivo_empleado_empresa_id_dispositivo_empleado_id",
                table: "dispositivo_empleado",
                columns: new[] { "empresa_id", "dispositivo_empleado_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "uq_dispositivo_empleado_token_fcm",
                table: "dispositivo_empleado",
                column: "token_fcm",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_outbox_mensaje_agregado_id",
                table: "outbox_mensaje",
                column: "agregado_id");

            migrationBuilder.CreateIndex(
                name: "ix_outbox_mensaje_empresa_id",
                table: "outbox_mensaje",
                column: "empresa_id");

            migrationBuilder.CreateIndex(
                name: "ix_outbox_mensaje_estado_proximo_intento",
                table: "outbox_mensaje",
                columns: new[] { "estado", "proximo_intento" });

            migrationBuilder.CreateIndex(
                name: "uq_outbox_mensaje_empresa_id_outbox_mensaje_id",
                table: "outbox_mensaje",
                columns: new[] { "empresa_id", "outbox_mensaje_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "dispositivo_empleado");

            migrationBuilder.DropTable(
                name: "outbox_mensaje");
        }
    }
}
