using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace APIGasoline.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RutaProcesada : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "altitud",
                table: "punto_ruta",
                type: "decimal(7,2)",
                precision: 7,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "precision_metros",
                table: "punto_ruta",
                type: "decimal(6,2)",
                precision: 6,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "velocidad",
                table: "punto_ruta",
                type: "decimal(6,2)",
                precision: 6,
                scale: 2,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ruta_procesada",
                columns: table => new
                {
                    ruta_procesada_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    empresa_id = table.Column<int>(type: "int", nullable: false),
                    viaje_id = table.Column<int>(type: "int", nullable: false),
                    puntos_recibidos = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    puntos_descartados = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    distancia_gps_km = table.Column<decimal>(type: "decimal(10,3)", precision: 10, scale: 3, nullable: false, defaultValue: 0m),
                    tiempo_movimiento_seg = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    tiempo_detenido_seg = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    velocidad_promedio_kmh = table.Column<decimal>(type: "decimal(6,2)", precision: 6, scale: 2, nullable: true),
                    velocidad_maxima_kmh = table.Column<decimal>(type: "decimal(6,2)", precision: 6, scale: 2, nullable: true),
                    distancia_odometro_km = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    diferencia_km = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    diferencia_porcentaje = table.Column<decimal>(type: "decimal(7,2)", precision: 7, scale: 2, nullable: true),
                    alerta_kilometraje = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    litros_cargados = table.Column<decimal>(type: "decimal(8,2)", precision: 8, scale: 2, nullable: false, defaultValue: 0m),
                    rendimiento_km_por_litro = table.Column<decimal>(type: "decimal(6,2)", precision: 6, scale: 2, nullable: true),
                    polyline = table.Column<string>(type: "text", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    version_algoritmo = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    fecha_proceso = table.Column<DateTime>(type: "timestamp", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ruta_procesada", x => x.ruta_procesada_id);
                    table.ForeignKey(
                        name: "fk_ruta_procesada_empresa",
                        column: x => x.empresa_id,
                        principalTable: "empresa",
                        principalColumn: "empresa_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_ruta_procesada_viaje",
                        columns: x => new { x.empresa_id, x.viaje_id },
                        principalTable: "viaje",
                        principalColumns: new[] { "empresa_id", "viaje_id" },
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "ix_ruta_procesada_empresa_id",
                table: "ruta_procesada",
                column: "empresa_id");

            migrationBuilder.CreateIndex(
                name: "ix_ruta_procesada_empresa_id_alerta_kilometraje",
                table: "ruta_procesada",
                columns: new[] { "empresa_id", "alerta_kilometraje" });

            migrationBuilder.CreateIndex(
                name: "ix_ruta_procesada_empresa_id_viaje_id",
                table: "ruta_procesada",
                columns: new[] { "empresa_id", "viaje_id" });

            migrationBuilder.CreateIndex(
                name: "uq_ruta_procesada_empresa_id_ruta_procesada_id",
                table: "ruta_procesada",
                columns: new[] { "empresa_id", "ruta_procesada_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "uq_ruta_procesada_viaje_id",
                table: "ruta_procesada",
                column: "viaje_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ruta_procesada");

            migrationBuilder.DropColumn(
                name: "altitud",
                table: "punto_ruta");

            migrationBuilder.DropColumn(
                name: "precision_metros",
                table: "punto_ruta");

            migrationBuilder.DropColumn(
                name: "velocidad",
                table: "punto_ruta");
        }
    }
}
