using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace APIGasoline.Infrastructure.Migrations
{
    /// <summary>
    /// v10 — Sesiones de usuario (refresh tokens con rotación).
    /// Una fila por inicio de sesión en un dispositivo. El token nunca se guarda: solo su hash SHA-256
    /// (token_hash) y el del token anterior (token_hash_anterior) para detectar reuso.
    /// FK a usuario con ON DELETE CASCADE (si se borra el usuario, sus sesiones ya no sirven).
    /// </summary>
    public partial class SesionesUsuario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "sesion_usuario",
                columns: table => new
                {
                    sesion_usuario_id = table.Column<Guid>(type: "binary(16)", nullable: false),
                    usuario_id = table.Column<Guid>(type: "binary(16)", nullable: false),
                    empresa_id = table.Column<Guid>(type: "binary(16)", nullable: true),
                    token_hash = table.Column<string>(type: "char(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    token_hash_anterior = table.Column<string>(type: "char(64)", maxLength: 64, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    plataforma = table.Column<string>(type: "enum('android','ios','web')", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    nombre_dispositivo = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ip = table.Column<string>(type: "varchar(45)", maxLength: 45, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    user_agent = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    fecha_creacion = table.Column<DateTime>(type: "timestamp", nullable: false),
                    fecha_ultimo_uso = table.Column<DateTime>(type: "timestamp", nullable: false),
                    fecha_rotacion = table.Column<DateTime>(type: "timestamp", nullable: true),
                    expira_en = table.Column<DateTime>(type: "timestamp", nullable: false),
                    expira_absoluta_en = table.Column<DateTime>(type: "timestamp", nullable: false),
                    fecha_revocacion = table.Column<DateTime>(type: "timestamp", nullable: true),
                    motivo_revocacion = table.Column<string>(type: "enum('cierre_sesion','cerrada_por_usuario','cerrada_por_administrador','cambio_password','usuario_desactivado','empleado_dado_de_baja','empleado_reasignado','empresa_eliminada','reuso_detectado','acceso_revocado','limite_sesiones')", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    revocada_por_usuario_id = table.Column<Guid>(type: "binary(16)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sesion_usuario", x => x.sesion_usuario_id);
                    table.ForeignKey(
                        name: "fk_sesion_usuario_usuario",
                        column: x => x.usuario_id,
                        principalTable: "usuario",
                        principalColumn: "usuario_id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "uq_sesion_usuario_token_hash",
                table: "sesion_usuario",
                column: "token_hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_sesion_usuario_token_hash_anterior",
                table: "sesion_usuario",
                column: "token_hash_anterior");

            migrationBuilder.CreateIndex(
                name: "ix_sesion_usuario_usuario_id_fecha_revocacion_expira_en",
                table: "sesion_usuario",
                columns: new[] { "usuario_id", "fecha_revocacion", "expira_en" });

            migrationBuilder.CreateIndex(
                name: "ix_sesion_usuario_empresa_id_fecha_revocacion",
                table: "sesion_usuario",
                columns: new[] { "empresa_id", "fecha_revocacion" });

            migrationBuilder.CreateIndex(
                name: "ix_sesion_usuario_expira_en",
                table: "sesion_usuario",
                column: "expira_en");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "sesion_usuario");
        }
    }
}
