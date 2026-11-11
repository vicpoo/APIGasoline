using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace APIGasoline.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AgregarEmpresaAUsuario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "empresa_id",
                table: "usuario",
                type: "binary(16)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_usuario_empresa_id",
                table: "usuario",
                column: "empresa_id");

            migrationBuilder.AddForeignKey(
                name: "fk_usuario_empresa",
                table: "usuario",
                column: "empresa_id",
                principalTable: "empresa",
                principalColumn: "empresa_id",
                onDelete: ReferentialAction.Restrict);

            // Las cuentas que ya existen quedan en la empresa de su empleado (aunque esté eliminado)...
            // EXEC: en el script idempotente (migraciones.sql) todo va en un solo lote y SQL Server
            // no compilaría un UPDATE de una columna que se agrega en ese mismo lote.
            migrationBuilder.Sql("""
                EXEC(N'
                UPDATE u SET u.empresa_id = e.empresa_id
                FROM usuario AS u
                JOIN empleado AS e ON e.usuario_id = u.usuario_id;
                ');
                """);

            // ...y, si no tienen empleado, en la de sus roles de empresa cuando todos son de la misma.
            // Las demás (administradores de la plataforma) se quedan sin empresa.
            migrationBuilder.Sql("""
                EXEC(N'
                UPDATE u SET u.empresa_id = r.empresa_id
                FROM usuario AS u
                CROSS APPLY (
                    SELECT MIN(rol.empresa_id) AS empresa_id
                    FROM usuario_rol AS ur
                    JOIN rol ON rol.rol_id = ur.rol_id
                    WHERE ur.usuario_id = u.usuario_id AND rol.empresa_id IS NOT NULL
                    HAVING COUNT(DISTINCT rol.empresa_id) = 1
                ) AS r
                WHERE u.empresa_id IS NULL;
                ');
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_usuario_empresa",
                table: "usuario");

            migrationBuilder.DropIndex(
                name: "ix_usuario_empresa_id",
                table: "usuario");

            migrationBuilder.DropColumn(
                name: "empresa_id",
                table: "usuario");
        }
    }
}
