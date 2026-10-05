using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace APIGasoline.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Inicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "corporativo",
                columns: table => new
                {
                    corporativo_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    nombre = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    activo = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    fecha_registro = table.Column<DateTime>(type: "timestamp", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_corporativo", x => x.corporativo_id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "privilegio",
                columns: table => new
                {
                    privilegio_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    nombre = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    descripcion = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    activo = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_privilegio", x => x.privilegio_id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "usuario",
                columns: table => new
                {
                    usuario_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    email = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    password_hash = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    estado = table.Column<string>(type: "enum('activo','inactivo')", nullable: false, defaultValueSql: "'activo'")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    fecha_registro = table.Column<DateTime>(type: "timestamp", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuario", x => x.usuario_id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "division",
                columns: table => new
                {
                    division_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    corporativo_id = table.Column<int>(type: "int", nullable: false),
                    nombre = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    activo = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    fecha_registro = table.Column<DateTime>(type: "timestamp", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_division", x => x.division_id);
                    table.UniqueConstraint("uq_division_corporativo_id_division_id", x => new { x.corporativo_id, x.division_id });
                    table.ForeignKey(
                        name: "fk_division_corporativo",
                        column: x => x.corporativo_id,
                        principalTable: "corporativo",
                        principalColumn: "corporativo_id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "empresa",
                columns: table => new
                {
                    empresa_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    corporativo_id = table.Column<int>(type: "int", nullable: true),
                    division_id = table.Column<int>(type: "int", nullable: true),
                    nombre = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    delete = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    fecha_registro = table.Column<DateTime>(type: "timestamp", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_empresa", x => x.empresa_id);
                    table.ForeignKey(
                        name: "fk_empresa_corporativo",
                        column: x => x.corporativo_id,
                        principalTable: "corporativo",
                        principalColumn: "corporativo_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_empresa_division",
                        columns: x => new { x.corporativo_id, x.division_id },
                        principalTable: "division",
                        principalColumns: new[] { "corporativo_id", "division_id" },
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "carro",
                columns: table => new
                {
                    carro_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    empresa_id = table.Column<int>(type: "int", nullable: false),
                    marca = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    modelo = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    anio = table.Column<int>(type: "int", nullable: true),
                    color = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    placas = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    foto_vehiculo = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    capacidad_tanque = table.Column<decimal>(type: "decimal(6,2)", precision: 6, scale: 2, nullable: true),
                    kilometraje_actual = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false, defaultValue: 0m),
                    estado = table.Column<string>(type: "enum('disponible','ocupado','en_mantenimiento')", nullable: false, defaultValueSql: "'disponible'")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    fecha_alta = table.Column<DateTime>(type: "timestamp", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_carro", x => x.carro_id);
                    table.UniqueConstraint("uq_carro_empresa_id_carro_id", x => new { x.empresa_id, x.carro_id });
                    table.ForeignKey(
                        name: "fk_carro_empresa",
                        column: x => x.empresa_id,
                        principalTable: "empresa",
                        principalColumn: "empresa_id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "centro_costo",
                columns: table => new
                {
                    centro_costo_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    empresa_id = table.Column<int>(type: "int", nullable: false),
                    nombre = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    clave = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    activo = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    fecha_registro = table.Column<DateTime>(type: "timestamp", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_centro_costo", x => x.centro_costo_id);
                    table.ForeignKey(
                        name: "fk_centro_costo_empresa",
                        column: x => x.empresa_id,
                        principalTable: "empresa",
                        principalColumn: "empresa_id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "empleado",
                columns: table => new
                {
                    empleado_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    empresa_id = table.Column<int>(type: "int", nullable: false),
                    usuario_id = table.Column<int>(type: "int", nullable: false),
                    nombre_completo = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    foto_perfil = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    foto_licencia = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    delete = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    fecha_alta = table.Column<DateTime>(type: "timestamp", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_empleado", x => x.empleado_id);
                    table.UniqueConstraint("uq_empleado_empresa_id_empleado_id", x => new { x.empresa_id, x.empleado_id });
                    table.ForeignKey(
                        name: "fk_empleado_empresa",
                        column: x => x.empresa_id,
                        principalTable: "empresa",
                        principalColumn: "empresa_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_empleado_usuario",
                        column: x => x.usuario_id,
                        principalTable: "usuario",
                        principalColumn: "usuario_id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "rol",
                columns: table => new
                {
                    rol_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    empresa_id = table.Column<int>(type: "int", nullable: true),
                    nombre = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    descripcion = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    activo = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rol", x => x.rol_id);
                    table.ForeignKey(
                        name: "fk_rol_empresa",
                        column: x => x.empresa_id,
                        principalTable: "empresa",
                        principalColumn: "empresa_id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "notificacion",
                columns: table => new
                {
                    notificacion_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    empresa_id = table.Column<int>(type: "int", nullable: false),
                    empleado_destinatario_id = table.Column<int>(type: "int", nullable: false),
                    tipo = table.Column<string>(type: "enum('nueva_solicitud','resultado_solicitud','cambio_estatus_forzado')", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    referencia_id = table.Column<int>(type: "int", nullable: true),
                    mensaje = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    leida = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    fecha_envio = table.Column<DateTime>(type: "timestamp", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notificacion", x => x.notificacion_id);
                    table.ForeignKey(
                        name: "fk_notificacion_empleado_destinatario",
                        columns: x => new { x.empresa_id, x.empleado_destinatario_id },
                        principalTable: "empleado",
                        principalColumns: new[] { "empresa_id", "empleado_id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_notificacion_empresa",
                        column: x => x.empresa_id,
                        principalTable: "empresa",
                        principalColumn: "empresa_id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "solicitud_reserva",
                columns: table => new
                {
                    solicitud_reserva_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    empresa_id = table.Column<int>(type: "int", nullable: false),
                    carro_id = table.Column<int>(type: "int", nullable: false),
                    empleado_solicitante_id = table.Column<int>(type: "int", nullable: false),
                    fecha_solicitud = table.Column<DateTime>(type: "timestamp", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    estado = table.Column<string>(type: "enum('pendiente','aprobada','rechazada','cancelada')", nullable: false, defaultValueSql: "'pendiente'")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    empleado_confirmador_id = table.Column<int>(type: "int", nullable: true),
                    fecha_resolucion = table.Column<DateTime>(type: "timestamp", nullable: true),
                    motivo_resolucion = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_solicitud_reserva", x => x.solicitud_reserva_id);
                    table.UniqueConstraint("uq_solicitud_reserva_empresa_id_solicitud_reserva_id", x => new { x.empresa_id, x.solicitud_reserva_id });
                    table.ForeignKey(
                        name: "fk_solicitud_reserva_carro",
                        columns: x => new { x.empresa_id, x.carro_id },
                        principalTable: "carro",
                        principalColumns: new[] { "empresa_id", "carro_id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_solicitud_reserva_empleado_confirmador",
                        columns: x => new { x.empresa_id, x.empleado_confirmador_id },
                        principalTable: "empleado",
                        principalColumns: new[] { "empresa_id", "empleado_id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_solicitud_reserva_empleado_solicitante",
                        columns: x => new { x.empresa_id, x.empleado_solicitante_id },
                        principalTable: "empleado",
                        principalColumns: new[] { "empresa_id", "empleado_id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_solicitud_reserva_empresa",
                        column: x => x.empresa_id,
                        principalTable: "empresa",
                        principalColumn: "empresa_id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "rol_privilegio",
                columns: table => new
                {
                    rol_privilegio_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    rol_id = table.Column<int>(type: "int", nullable: false),
                    privilegio_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rol_privilegio", x => x.rol_privilegio_id);
                    table.ForeignKey(
                        name: "fk_rol_privilegio_privilegio",
                        column: x => x.privilegio_id,
                        principalTable: "privilegio",
                        principalColumn: "privilegio_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_rol_privilegio_rol",
                        column: x => x.rol_id,
                        principalTable: "rol",
                        principalColumn: "rol_id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "usuario_rol",
                columns: table => new
                {
                    usuario_rol_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    usuario_id = table.Column<int>(type: "int", nullable: false),
                    rol_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuario_rol", x => x.usuario_rol_id);
                    table.ForeignKey(
                        name: "fk_usuario_rol_rol",
                        column: x => x.rol_id,
                        principalTable: "rol",
                        principalColumn: "rol_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_usuario_rol_usuario",
                        column: x => x.usuario_id,
                        principalTable: "usuario",
                        principalColumn: "usuario_id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "viaje",
                columns: table => new
                {
                    viaje_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    empresa_id = table.Column<int>(type: "int", nullable: false),
                    carro_id = table.Column<int>(type: "int", nullable: false),
                    solicitud_reserva_id = table.Column<int>(type: "int", nullable: true),
                    empleado_operador_id = table.Column<int>(type: "int", nullable: false),
                    fecha_inicio = table.Column<DateTime>(type: "timestamp", nullable: false),
                    ubicacion_inicio_lat = table.Column<decimal>(type: "decimal(9,6)", precision: 9, scale: 6, nullable: true),
                    ubicacion_inicio_lng = table.Column<decimal>(type: "decimal(9,6)", precision: 9, scale: 6, nullable: true),
                    kilometraje_inicial = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    fecha_fin = table.Column<DateTime>(type: "timestamp", nullable: true),
                    ubicacion_fin_lat = table.Column<decimal>(type: "decimal(9,6)", precision: 9, scale: 6, nullable: true),
                    ubicacion_fin_lng = table.Column<decimal>(type: "decimal(9,6)", precision: 9, scale: 6, nullable: true),
                    kilometraje_final = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    distancia_recorrida = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    estado = table.Column<string>(type: "enum('en_curso','finalizado','interrumpido')", nullable: false, defaultValueSql: "'en_curso'")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    motivo_interrupcion = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    sincronizado = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_viaje", x => x.viaje_id);
                    table.UniqueConstraint("uq_viaje_empresa_id_viaje_id", x => new { x.empresa_id, x.viaje_id });
                    table.ForeignKey(
                        name: "fk_viaje_carro",
                        columns: x => new { x.empresa_id, x.carro_id },
                        principalTable: "carro",
                        principalColumns: new[] { "empresa_id", "carro_id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_viaje_empleado_operador",
                        columns: x => new { x.empresa_id, x.empleado_operador_id },
                        principalTable: "empleado",
                        principalColumns: new[] { "empresa_id", "empleado_id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_viaje_empresa",
                        column: x => x.empresa_id,
                        principalTable: "empresa",
                        principalColumn: "empresa_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_viaje_solicitud_reserva",
                        columns: x => new { x.empresa_id, x.solicitud_reserva_id },
                        principalTable: "solicitud_reserva",
                        principalColumns: new[] { "empresa_id", "solicitud_reserva_id" },
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "cambio_estatus_carro",
                columns: table => new
                {
                    cambio_estatus_carro_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    empresa_id = table.Column<int>(type: "int", nullable: false),
                    carro_id = table.Column<int>(type: "int", nullable: false),
                    empleado_id = table.Column<int>(type: "int", nullable: false),
                    viaje_afectado_id = table.Column<int>(type: "int", nullable: true),
                    estatus_anterior = table.Column<string>(type: "enum('disponible','ocupado','en_mantenimiento')", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    estatus_nuevo = table.Column<string>(type: "enum('disponible','ocupado','en_mantenimiento')", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    motivo = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    fecha_cambio = table.Column<DateTime>(type: "timestamp", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    sincronizado = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cambio_estatus_carro", x => x.cambio_estatus_carro_id);
                    table.ForeignKey(
                        name: "fk_cambio_estatus_carro_carro",
                        columns: x => new { x.empresa_id, x.carro_id },
                        principalTable: "carro",
                        principalColumns: new[] { "empresa_id", "carro_id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_cambio_estatus_carro_empleado",
                        columns: x => new { x.empresa_id, x.empleado_id },
                        principalTable: "empleado",
                        principalColumns: new[] { "empresa_id", "empleado_id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_cambio_estatus_carro_empresa",
                        column: x => x.empresa_id,
                        principalTable: "empresa",
                        principalColumn: "empresa_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_cambio_estatus_carro_viaje",
                        columns: x => new { x.empresa_id, x.viaje_afectado_id },
                        principalTable: "viaje",
                        principalColumns: new[] { "empresa_id", "viaje_id" },
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "carga_combustible",
                columns: table => new
                {
                    carga_combustible_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    empresa_id = table.Column<int>(type: "int", nullable: false),
                    viaje_id = table.Column<int>(type: "int", nullable: false),
                    litros = table.Column<decimal>(type: "decimal(8,2)", precision: 8, scale: 2, nullable: false),
                    monto = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    foto_comprobante = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    fecha_carga = table.Column<DateTime>(type: "timestamp", nullable: false),
                    ubicacion_lat = table.Column<decimal>(type: "decimal(9,6)", precision: 9, scale: 6, nullable: true),
                    ubicacion_lng = table.Column<decimal>(type: "decimal(9,6)", precision: 9, scale: 6, nullable: true),
                    sincronizado = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_carga_combustible", x => x.carga_combustible_id);
                    table.ForeignKey(
                        name: "fk_carga_combustible_empresa",
                        column: x => x.empresa_id,
                        principalTable: "empresa",
                        principalColumn: "empresa_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_carga_combustible_viaje",
                        columns: x => new { x.empresa_id, x.viaje_id },
                        principalTable: "viaje",
                        principalColumns: new[] { "empresa_id", "viaje_id" },
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "punto_ruta",
                columns: table => new
                {
                    punto_ruta_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    empresa_id = table.Column<int>(type: "int", nullable: false),
                    viaje_id = table.Column<int>(type: "int", nullable: false),
                    latitud = table.Column<decimal>(type: "decimal(9,6)", precision: 9, scale: 6, nullable: false),
                    longitud = table.Column<decimal>(type: "decimal(9,6)", precision: 9, scale: 6, nullable: false),
                    timestamp_captura = table.Column<DateTime>(type: "timestamp", nullable: false),
                    sincronizado = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_punto_ruta", x => x.punto_ruta_id);
                    table.ForeignKey(
                        name: "fk_punto_ruta_empresa",
                        column: x => x.empresa_id,
                        principalTable: "empresa",
                        principalColumn: "empresa_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_punto_ruta_viaje",
                        columns: x => new { x.empresa_id, x.viaje_id },
                        principalTable: "viaje",
                        principalColumns: new[] { "empresa_id", "viaje_id" },
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "ix_cambio_estatus_carro_empresa_id",
                table: "cambio_estatus_carro",
                column: "empresa_id");

            migrationBuilder.CreateIndex(
                name: "ix_cambio_estatus_carro_empresa_id_carro_id",
                table: "cambio_estatus_carro",
                columns: new[] { "empresa_id", "carro_id" });

            migrationBuilder.CreateIndex(
                name: "ix_cambio_estatus_carro_empresa_id_empleado_id",
                table: "cambio_estatus_carro",
                columns: new[] { "empresa_id", "empleado_id" });

            migrationBuilder.CreateIndex(
                name: "ix_cambio_estatus_carro_empresa_id_viaje_afectado_id",
                table: "cambio_estatus_carro",
                columns: new[] { "empresa_id", "viaje_afectado_id" });

            migrationBuilder.CreateIndex(
                name: "uq_cambio_estatus_carro_empresa_id_cambio_estatus_carro_id",
                table: "cambio_estatus_carro",
                columns: new[] { "empresa_id", "cambio_estatus_carro_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_carga_combustible_empresa_id",
                table: "carga_combustible",
                column: "empresa_id");

            migrationBuilder.CreateIndex(
                name: "ix_carga_combustible_empresa_id_viaje_id",
                table: "carga_combustible",
                columns: new[] { "empresa_id", "viaje_id" });

            migrationBuilder.CreateIndex(
                name: "uq_carga_combustible_empresa_id_carga_combustible_id",
                table: "carga_combustible",
                columns: new[] { "empresa_id", "carga_combustible_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_carro_empresa_id",
                table: "carro",
                column: "empresa_id");

            migrationBuilder.CreateIndex(
                name: "uq_carro_placas",
                table: "carro",
                column: "placas",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_centro_costo_empresa_id",
                table: "centro_costo",
                column: "empresa_id");

            migrationBuilder.CreateIndex(
                name: "uq_centro_costo_empresa_id_centro_costo_id",
                table: "centro_costo",
                columns: new[] { "empresa_id", "centro_costo_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "uq_centro_costo_empresa_id_clave",
                table: "centro_costo",
                columns: new[] { "empresa_id", "clave" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_division_corporativo_id",
                table: "division",
                column: "corporativo_id");

            migrationBuilder.CreateIndex(
                name: "ix_empleado_empresa_id",
                table: "empleado",
                column: "empresa_id");

            migrationBuilder.CreateIndex(
                name: "uq_empleado_usuario_id",
                table: "empleado",
                column: "usuario_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_empresa_corporativo_id",
                table: "empresa",
                column: "corporativo_id");

            migrationBuilder.CreateIndex(
                name: "ix_empresa_corporativo_id_division_id",
                table: "empresa",
                columns: new[] { "corporativo_id", "division_id" });

            migrationBuilder.CreateIndex(
                name: "ix_empresa_division_id",
                table: "empresa",
                column: "division_id");

            migrationBuilder.CreateIndex(
                name: "uq_empresa_corporativo_id_empresa_id",
                table: "empresa",
                columns: new[] { "corporativo_id", "empresa_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_notificacion_empleado_destinatario_id_leida",
                table: "notificacion",
                columns: new[] { "empleado_destinatario_id", "leida" });

            migrationBuilder.CreateIndex(
                name: "ix_notificacion_empresa_id",
                table: "notificacion",
                column: "empresa_id");

            migrationBuilder.CreateIndex(
                name: "ix_notificacion_empresa_id_empleado_destinatario_id",
                table: "notificacion",
                columns: new[] { "empresa_id", "empleado_destinatario_id" });

            migrationBuilder.CreateIndex(
                name: "uq_notificacion_empresa_id_notificacion_id",
                table: "notificacion",
                columns: new[] { "empresa_id", "notificacion_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "uq_privilegio_nombre",
                table: "privilegio",
                column: "nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_punto_ruta_empresa_id",
                table: "punto_ruta",
                column: "empresa_id");

            migrationBuilder.CreateIndex(
                name: "ix_punto_ruta_empresa_id_viaje_id",
                table: "punto_ruta",
                columns: new[] { "empresa_id", "viaje_id" });

            migrationBuilder.CreateIndex(
                name: "ix_punto_ruta_viaje_id_timestamp_captura",
                table: "punto_ruta",
                columns: new[] { "viaje_id", "timestamp_captura" });

            migrationBuilder.CreateIndex(
                name: "uq_punto_ruta_empresa_id_punto_ruta_id",
                table: "punto_ruta",
                columns: new[] { "empresa_id", "punto_ruta_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_rol_empresa_id",
                table: "rol",
                column: "empresa_id");

            migrationBuilder.CreateIndex(
                name: "uq_rol_empresa_id_nombre",
                table: "rol",
                columns: new[] { "empresa_id", "nombre" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_rol_privilegio_privilegio_id",
                table: "rol_privilegio",
                column: "privilegio_id");

            migrationBuilder.CreateIndex(
                name: "ix_rol_privilegio_rol_id",
                table: "rol_privilegio",
                column: "rol_id");

            migrationBuilder.CreateIndex(
                name: "uq_rol_privilegio_rol_id_privilegio_id",
                table: "rol_privilegio",
                columns: new[] { "rol_id", "privilegio_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_solicitud_reserva_empresa_id",
                table: "solicitud_reserva",
                column: "empresa_id");

            migrationBuilder.CreateIndex(
                name: "ix_solicitud_reserva_empresa_id_carro_id",
                table: "solicitud_reserva",
                columns: new[] { "empresa_id", "carro_id" });

            migrationBuilder.CreateIndex(
                name: "ix_solicitud_reserva_empresa_id_empleado_confirmador_id",
                table: "solicitud_reserva",
                columns: new[] { "empresa_id", "empleado_confirmador_id" });

            migrationBuilder.CreateIndex(
                name: "ix_solicitud_reserva_empresa_id_empleado_solicitante_id",
                table: "solicitud_reserva",
                columns: new[] { "empresa_id", "empleado_solicitante_id" });

            migrationBuilder.CreateIndex(
                name: "uq_usuario_email",
                table: "usuario",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_usuario_rol_rol_id",
                table: "usuario_rol",
                column: "rol_id");

            migrationBuilder.CreateIndex(
                name: "ix_usuario_rol_usuario_id",
                table: "usuario_rol",
                column: "usuario_id");

            migrationBuilder.CreateIndex(
                name: "uq_usuario_rol_usuario_id_rol_id",
                table: "usuario_rol",
                columns: new[] { "usuario_id", "rol_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_viaje_empresa_id",
                table: "viaje",
                column: "empresa_id");

            migrationBuilder.CreateIndex(
                name: "ix_viaje_empresa_id_carro_id",
                table: "viaje",
                columns: new[] { "empresa_id", "carro_id" });

            migrationBuilder.CreateIndex(
                name: "ix_viaje_empresa_id_empleado_operador_id",
                table: "viaje",
                columns: new[] { "empresa_id", "empleado_operador_id" });

            migrationBuilder.CreateIndex(
                name: "ix_viaje_empresa_id_solicitud_reserva_id",
                table: "viaje",
                columns: new[] { "empresa_id", "solicitud_reserva_id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "cambio_estatus_carro");

            migrationBuilder.DropTable(
                name: "carga_combustible");

            migrationBuilder.DropTable(
                name: "centro_costo");

            migrationBuilder.DropTable(
                name: "notificacion");

            migrationBuilder.DropTable(
                name: "punto_ruta");

            migrationBuilder.DropTable(
                name: "rol_privilegio");

            migrationBuilder.DropTable(
                name: "usuario_rol");

            migrationBuilder.DropTable(
                name: "viaje");

            migrationBuilder.DropTable(
                name: "privilegio");

            migrationBuilder.DropTable(
                name: "rol");

            migrationBuilder.DropTable(
                name: "solicitud_reserva");

            migrationBuilder.DropTable(
                name: "carro");

            migrationBuilder.DropTable(
                name: "empleado");

            migrationBuilder.DropTable(
                name: "empresa");

            migrationBuilder.DropTable(
                name: "usuario");

            migrationBuilder.DropTable(
                name: "division");

            migrationBuilder.DropTable(
                name: "corporativo");
        }
    }
}
