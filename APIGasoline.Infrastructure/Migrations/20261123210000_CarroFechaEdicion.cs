using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace APIGasoline.Infrastructure.Migrations
{
    /// <summary>
    /// Conflictos entre celulares: <c>carro.fecha_edicion</c> guarda cuándo se hizo el último cambio de los
    /// datos del carro. Al editar se compara con la hora en que el celular hizo su cambio: gana el más
    /// reciente, no el último que llega (antes un celular que estuvo sin red pisaba un cambio más nuevo).
    /// Los carros que ya existen quedan con NULL: el siguiente cambio se acepta y empieza a contar.
    /// </summary>
    public partial class CarroFechaEdicion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "fecha_edicion",
                table: "carro",
                type: "datetime2(0)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "fecha_edicion",
                table: "carro");
        }
    }
}
