using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace APIGasoline.Infrastructure.Migrations
{
    /// <summary>
    /// A006: las placas pasan de ser únicas en toda la BD a ser únicas dentro de cada empresa.
    /// Dos empresas del grupo pueden registrar el mismo vehículo (por ejemplo, tras una venta entre
    /// ellas) y una empresa ya no puede averiguar si unas placas existen en otra (antes respondía 409).
    /// No toca datos: lo que era único en toda la BD también lo es por empresa.
    /// </summary>
    public partial class PlacasUnicasPorEmpresa : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "uq_carro_placas",
                table: "carro");

            migrationBuilder.CreateIndex(
                name: "uq_carro_empresa_id_placas",
                table: "carro",
                columns: new[] { "empresa_id", "placas" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Si ya hay placas repetidas entre empresas, el índice global no se puede volver a crear
            migrationBuilder.DropIndex(
                name: "uq_carro_empresa_id_placas",
                table: "carro");

            migrationBuilder.CreateIndex(
                name: "uq_carro_placas",
                table: "carro",
                column: "placas",
                unique: true);
        }
    }
}
