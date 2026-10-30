using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace APIGasoline.Infrastructure.Migrations
{
    /// <summary>
    /// v9 — Tipo de vehículo del carro: enum('carro','moto','camioneta','camion').
    /// NOT NULL con DEFAULT 'carro' para que los carros que ya existen queden como "carro".
    /// </summary>
    public partial class TipoVehiculo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "tipo_vehiculo",
                table: "carro",
                type: "enum('carro','moto','camioneta','camion')",
                nullable: false,
                defaultValueSql: "'carro'")
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "tipo_vehiculo",
                table: "carro");
        }
    }
}
