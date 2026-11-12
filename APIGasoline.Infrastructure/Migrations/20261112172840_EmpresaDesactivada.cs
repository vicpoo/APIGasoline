using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace APIGasoline.Infrastructure.Migrations
{
    /// <summary>
    /// Fase 9A (F009): nuevo motivo de cierre de sesión 'empresa_desactivada' (se cierran las sesiones
    /// de una empresa cuando se marca como inactiva).
    /// </summary>
    public partial class EmpresaDesactivada : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "ck_sesion_usuario_motivo_revocacion",
                table: "sesion_usuario");

            migrationBuilder.AddCheckConstraint(
                name: "ck_sesion_usuario_motivo_revocacion",
                table: "sesion_usuario",
                sql: "[motivo_revocacion] IN ('cierre_sesion', 'cerrada_por_usuario', 'cerrada_por_administrador', 'cambio_password', 'usuario_desactivado', 'empleado_dado_de_baja', 'empleado_reasignado', 'empresa_eliminada', 'reuso_detectado', 'acceso_revocado', 'limite_sesiones', 'empresa_desactivada')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // El motivo nuevo no existía: esas sesiones quedan como 'acceso_revocado'
            migrationBuilder.Sql("EXEC(N'UPDATE dbo.sesion_usuario SET motivo_revocacion = ''acceso_revocado'' WHERE motivo_revocacion = ''empresa_desactivada'';');");

            migrationBuilder.DropCheckConstraint(
                name: "ck_sesion_usuario_motivo_revocacion",
                table: "sesion_usuario");

            migrationBuilder.AddCheckConstraint(
                name: "ck_sesion_usuario_motivo_revocacion",
                table: "sesion_usuario",
                sql: "[motivo_revocacion] IN ('cierre_sesion', 'cerrada_por_usuario', 'cerrada_por_administrador', 'cambio_password', 'usuario_desactivado', 'empleado_dado_de_baja', 'empleado_reasignado', 'empresa_eliminada', 'reuso_detectado', 'acceso_revocado', 'limite_sesiones')");
        }
    }
}
