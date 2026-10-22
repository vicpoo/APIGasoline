using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace APIGasoline.Infrastructure.Migrations
{
    /// <summary>
    /// v7 — Pausas de viaje (pausar / reanudar). Una fila por pausa; fecha_fin NULL = el viaje está en pausa.
    /// FK compuesta (empresa_id, viaje_id) tenant-safe y ON DELETE CASCADE (las pausas son parte del viaje).
    /// </summary>
    public partial class PausasDeViaje : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "viaje_pausa",
                columns: table => new
                {
                    viaje_pausa_id = table.Column<Guid>(type: "binary(16)", nullable: false),
                    empresa_id = table.Column<Guid>(type: "binary(16)", nullable: false),
                    viaje_id = table.Column<Guid>(type: "binary(16)", nullable: false),
                    fecha_inicio = table.Column<DateTime>(type: "timestamp", nullable: false),
                    fecha_fin = table.Column<DateTime>(type: "timestamp", nullable: true),
                    motivo = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ubicacion_lat = table.Column<decimal>(type: "decimal(9,6)", precision: 9, scale: 6, nullable: true),
                    ubicacion_lng = table.Column<decimal>(type: "decimal(9,6)", precision: 9, scale: 6, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_viaje_pausa", x => x.viaje_pausa_id);
                    table.ForeignKey(
                        name: "fk_viaje_pausa_empresa",
                        column: x => x.empresa_id,
                        principalTable: "empresa",
                        principalColumn: "empresa_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_viaje_pausa_viaje",
                        columns: x => new { x.empresa_id, x.viaje_id },
                        principalTable: "viaje",
                        principalColumns: new[] { "empresa_id", "viaje_id" },
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "ix_viaje_pausa_empresa_id",
                table: "viaje_pausa",
                column: "empresa_id");

            migrationBuilder.CreateIndex(
                name: "ix_viaje_pausa_empresa_id_viaje_id",
                table: "viaje_pausa",
                columns: new[] { "empresa_id", "viaje_id" });

            migrationBuilder.CreateIndex(
                name: "ix_viaje_pausa_viaje_id_fecha_inicio",
                table: "viaje_pausa",
                columns: new[] { "viaje_id", "fecha_inicio" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "viaje_pausa");
        }
    }
}
