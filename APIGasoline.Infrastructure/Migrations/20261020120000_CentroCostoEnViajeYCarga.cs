using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace APIGasoline.Infrastructure.Migrations
{
    /// <summary>
    /// v6 — Centro de costos en viaje y carga_combustible (opcional, misma empresa: FK compuesta).
    /// centro_costo pasa su índice único (empresa_id, centro_costo_id) a llave alterna para poder referenciarlo.
    /// </summary>
    public partial class CentroCostoEnViajeYCarga : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ---------- centro_costo: índice único -> llave alterna (mismo nombre y columnas) ----------
            migrationBuilder.DropIndex(
                name: "uq_centro_costo_empresa_id_centro_costo_id",
                table: "centro_costo");

            migrationBuilder.AddUniqueConstraint(
                name: "uq_centro_costo_empresa_id_centro_costo_id",
                table: "centro_costo",
                columns: new[] { "empresa_id", "centro_costo_id" });

            // ---------- viaje ----------
            migrationBuilder.AddColumn<Guid>(
                name: "centro_costo_id",
                table: "viaje",
                type: "binary(16)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_viaje_empresa_id_centro_costo_id",
                table: "viaje",
                columns: new[] { "empresa_id", "centro_costo_id" });

            migrationBuilder.AddForeignKey(
                name: "fk_viaje_centro_costo",
                table: "viaje",
                columns: new[] { "empresa_id", "centro_costo_id" },
                principalTable: "centro_costo",
                principalColumns: new[] { "empresa_id", "centro_costo_id" },
                onDelete: ReferentialAction.Restrict);

            // ---------- carga_combustible ----------
            migrationBuilder.AddColumn<Guid>(
                name: "centro_costo_id",
                table: "carga_combustible",
                type: "binary(16)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_carga_combustible_empresa_id_centro_costo_id",
                table: "carga_combustible",
                columns: new[] { "empresa_id", "centro_costo_id" });

            migrationBuilder.AddForeignKey(
                name: "fk_carga_combustible_centro_costo",
                table: "carga_combustible",
                columns: new[] { "empresa_id", "centro_costo_id" },
                principalTable: "centro_costo",
                principalColumns: new[] { "empresa_id", "centro_costo_id" },
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_carga_combustible_centro_costo",
                table: "carga_combustible");

            migrationBuilder.DropIndex(
                name: "ix_carga_combustible_empresa_id_centro_costo_id",
                table: "carga_combustible");

            migrationBuilder.DropColumn(
                name: "centro_costo_id",
                table: "carga_combustible");

            migrationBuilder.DropForeignKey(
                name: "fk_viaje_centro_costo",
                table: "viaje");

            migrationBuilder.DropIndex(
                name: "ix_viaje_empresa_id_centro_costo_id",
                table: "viaje");

            migrationBuilder.DropColumn(
                name: "centro_costo_id",
                table: "viaje");

            migrationBuilder.DropUniqueConstraint(
                name: "uq_centro_costo_empresa_id_centro_costo_id",
                table: "centro_costo");

            migrationBuilder.CreateIndex(
                name: "uq_centro_costo_empresa_id_centro_costo_id",
                table: "centro_costo",
                columns: new[] { "empresa_id", "centro_costo_id" },
                unique: true);
        }
    }
}
