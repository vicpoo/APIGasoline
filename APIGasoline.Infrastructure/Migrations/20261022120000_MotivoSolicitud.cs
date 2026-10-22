using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace APIGasoline.Infrastructure.Migrations
{
    /// <summary>
    /// v8 — Motivo de la solicitud de reserva (lo escribe el solicitante al pedir el carro).
    /// Columna NULL para no romper las solicitudes que ya existen; la API lo exige en las nuevas.
    /// </summary>
    public partial class MotivoSolicitud : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "motivo_solicitud",
                table: "solicitud_reserva",
                type: "varchar(255)",
                maxLength: 255,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "motivo_solicitud",
                table: "solicitud_reserva");
        }
    }
}
