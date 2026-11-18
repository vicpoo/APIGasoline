using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace APIGasoline.Infrastructure.Migrations
{
    /// <summary>
    /// N004: la bitácora (auditoria_evento) es de solo inserción para el login con el que opera la API.
    /// db_datawriter le da INSERT, UPDATE y DELETE en todas las tablas; aquí se le niegan UPDATE y DELETE
    /// en la bitácora, así que si alguien toma control de la API no puede editar ni borrar su rastro.
    /// Solo aplica donde existe el usuario apigasoline_app (producción, lo crea BD/produccion/crear-logins.sql);
    /// en desarrollo y en las pruebas no existe y la migración no hace nada.
    /// crear-logins.sql repite el DENY en cada arranque por si el login se crea después de esta migración.
    /// </summary>
    public partial class BitacoraSoloInsercion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF USER_ID(N'apigasoline_app') IS NOT NULL
                    EXEC(N'DENY UPDATE, DELETE ON dbo.auditoria_evento TO [apigasoline_app];');
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF USER_ID(N'apigasoline_app') IS NOT NULL
                    EXEC(N'REVOKE UPDATE, DELETE ON dbo.auditoria_evento FROM [apigasoline_app];');
                """);
        }
    }
}
