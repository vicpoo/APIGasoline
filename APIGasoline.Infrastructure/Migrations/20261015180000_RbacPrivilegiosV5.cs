using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace APIGasoline.Infrastructure.Migrations
{
    /// <summary>
    /// v5 — RBAC: privilegio.clave/modulo/ambito y rol.empresa_clave (columna generada)
    /// para que el nombre de los roles globales también sea único.
    /// </summary>
    public partial class RbacPrivilegiosV5 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ---------- privilegio ----------

            // El nombre pasa a ser solo texto visible: deja de ser único
            migrationBuilder.DropIndex(
                name: "uq_privilegio_nombre",
                table: "privilegio");

            migrationBuilder.AddColumn<string>(
                name: "clave",
                table: "privilegio",
                type: "varchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "modulo",
                table: "privilegio",
                type: "varchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ambito",
                table: "privilegio",
                type: "enum('plataforma','empresa')",
                nullable: false,
                defaultValueSql: "'empresa'")
                .Annotation("MySql:CharSet", "utf8mb4");

            // Privilegios que ya existían: se les da una clave única temporal (legado.p<id en hex>)
            // ANTES de crear el índice único. La clave es inmutable: para usar la definitiva,
            // elimina el privilegio legado y créalo de nuevo con POST /api/privilegios.
            migrationBuilder.Sql(
                "UPDATE privilegio " +
                "SET clave = CONCAT('legado.p', LOWER(HEX(privilegio_id))), modulo = 'legado' " +
                "WHERE clave = '';");

            migrationBuilder.CreateIndex(
                name: "uq_privilegio_clave",
                table: "privilegio",
                column: "clave",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_privilegio_modulo",
                table: "privilegio",
                column: "modulo");

            // ---------- rol ----------

            // (empresa_id, nombre) no evitaba roles globales repetidos: MySQL permite varios NULL.
            // fk_rol_empresa sigue usando ix_rol_empresa_id.
            migrationBuilder.DropIndex(
                name: "uq_rol_empresa_id_nombre",
                table: "rol");

            migrationBuilder.AddColumn<Guid>(
                name: "empresa_clave",
                table: "rol",
                type: "binary(16)",
                nullable: false,
                computedColumnSql: "COALESCE(`empresa_id`, 0x00000000000000000000000000000000)",
                stored: true);

            migrationBuilder.CreateIndex(
                name: "uq_rol_empresa_clave_nombre",
                table: "rol",
                columns: new[] { "empresa_clave", "nombre" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "uq_rol_empresa_clave_nombre",
                table: "rol");

            migrationBuilder.DropColumn(
                name: "empresa_clave",
                table: "rol");

            migrationBuilder.CreateIndex(
                name: "uq_rol_empresa_id_nombre",
                table: "rol",
                columns: new[] { "empresa_id", "nombre" },
                unique: true);

            migrationBuilder.DropIndex(
                name: "uq_privilegio_clave",
                table: "privilegio");

            migrationBuilder.DropIndex(
                name: "ix_privilegio_modulo",
                table: "privilegio");

            migrationBuilder.DropColumn(
                name: "ambito",
                table: "privilegio");

            migrationBuilder.DropColumn(
                name: "clave",
                table: "privilegio");

            migrationBuilder.DropColumn(
                name: "modulo",
                table: "privilegio");

            // Falla si después de v5 se crearon dos privilegios con el mismo nombre visible
            migrationBuilder.CreateIndex(
                name: "uq_privilegio_nombre",
                table: "privilegio",
                column: "nombre",
                unique: true);
        }
    }
}
