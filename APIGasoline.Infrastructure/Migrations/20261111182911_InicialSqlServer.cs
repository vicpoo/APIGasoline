using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace APIGasoline.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InicialSqlServer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "corporativo",
                columns: table => new
                {
                    corporativo_id = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    nombre = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    fecha_registro = table.Column<DateTime>(type: "datetime2(0)", nullable: true, defaultValueSql: "SYSDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_corporativo", x => x.corporativo_id);
                });

            migrationBuilder.CreateTable(
                name: "privilegio",
                columns: table => new
                {
                    privilegio_id = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    clave = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    modulo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    descripcion = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ambito = table.Column<string>(type: "varchar(10)", nullable: false, defaultValueSql: "'empresa'"),
                    activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_privilegio", x => x.privilegio_id);
                    table.CheckConstraint("ck_privilegio_ambito", "[ambito] IN ('plataforma', 'empresa')");
                });

            migrationBuilder.CreateTable(
                name: "usuario",
                columns: table => new
                {
                    usuario_id = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    password_hash = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    estado = table.Column<string>(type: "varchar(8)", nullable: false, defaultValueSql: "'activo'"),
                    fecha_registro = table.Column<DateTime>(type: "datetime2(0)", nullable: true, defaultValueSql: "SYSDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuario", x => x.usuario_id);
                    table.CheckConstraint("ck_usuario_estado", "[estado] IN ('activo', 'inactivo')");
                });

            migrationBuilder.CreateTable(
                name: "division",
                columns: table => new
                {
                    division_id = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    corporativo_id = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    fecha_registro = table.Column<DateTime>(type: "datetime2(0)", nullable: true, defaultValueSql: "SYSDATETIME()")
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
                });

            migrationBuilder.CreateTable(
                name: "sesion_usuario",
                columns: table => new
                {
                    sesion_usuario_id = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    usuario_id = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    empresa_id = table.Column<byte[]>(type: "binary(16)", nullable: true),
                    token_hash = table.Column<string>(type: "char(64)", maxLength: 64, nullable: false),
                    token_hash_anterior = table.Column<string>(type: "char(64)", maxLength: 64, nullable: true),
                    plataforma = table.Column<string>(type: "varchar(7)", nullable: true),
                    nombre_dispositivo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ip = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    user_agent = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    fecha_creacion = table.Column<DateTime>(type: "datetime2(0)", nullable: false),
                    fecha_ultimo_uso = table.Column<DateTime>(type: "datetime2(0)", nullable: false),
                    fecha_rotacion = table.Column<DateTime>(type: "datetime2(0)", nullable: true),
                    expira_en = table.Column<DateTime>(type: "datetime2(0)", nullable: false),
                    expira_absoluta_en = table.Column<DateTime>(type: "datetime2(0)", nullable: false),
                    fecha_revocacion = table.Column<DateTime>(type: "datetime2(0)", nullable: true),
                    motivo_revocacion = table.Column<string>(type: "varchar(25)", nullable: true),
                    revocada_por_usuario_id = table.Column<byte[]>(type: "binary(16)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sesion_usuario", x => x.sesion_usuario_id);
                    table.CheckConstraint("ck_sesion_usuario_motivo_revocacion", "[motivo_revocacion] IN ('cierre_sesion', 'cerrada_por_usuario', 'cerrada_por_administrador', 'cambio_password', 'usuario_desactivado', 'empleado_dado_de_baja', 'empleado_reasignado', 'empresa_eliminada', 'reuso_detectado', 'acceso_revocado', 'limite_sesiones')");
                    table.CheckConstraint("ck_sesion_usuario_plataforma", "[plataforma] IN ('android', 'ios', 'web')");
                    table.ForeignKey(
                        name: "fk_sesion_usuario_usuario",
                        column: x => x.usuario_id,
                        principalTable: "usuario",
                        principalColumn: "usuario_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "empresa",
                columns: table => new
                {
                    empresa_id = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    corporativo_id = table.Column<byte[]>(type: "binary(16)", nullable: true),
                    division_id = table.Column<byte[]>(type: "binary(16)", nullable: true),
                    nombre = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    eliminado = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    fecha_registro = table.Column<DateTime>(type: "datetime2(0)", nullable: true, defaultValueSql: "SYSDATETIME()")
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
                });

            migrationBuilder.CreateTable(
                name: "auditoria_evento",
                columns: table => new
                {
                    auditoria_evento_id = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    empresa_id = table.Column<byte[]>(type: "binary(16)", nullable: true),
                    usuario_id = table.Column<byte[]>(type: "binary(16)", nullable: true),
                    accion = table.Column<string>(type: "varchar(14)", nullable: false),
                    entidad = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    entidad_id = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    valores_anteriores = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    valores_nuevos = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    motivo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    origen = table.Column<string>(type: "varchar(7)", nullable: false),
                    fecha_hora = table.Column<DateTime>(type: "datetime2(0)", nullable: true, defaultValueSql: "SYSDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_auditoria_evento", x => x.auditoria_evento_id);
                    table.CheckConstraint("ck_auditoria_evento_accion", "[accion] IN ('crear', 'actualizar', 'eliminar', 'aprobar', 'rechazar', 'cancelar', 'cambiar_estado', 'corregir')");
                    table.CheckConstraint("ck_auditoria_evento_origen", "[origen] IN ('movil', 'web', 'sistema')");
                    table.CheckConstraint("ck_auditoria_evento_valores_anteriores_json", "[valores_anteriores] IS NULL OR ISJSON([valores_anteriores]) = 1");
                    table.CheckConstraint("ck_auditoria_evento_valores_nuevos_json", "[valores_nuevos] IS NULL OR ISJSON([valores_nuevos]) = 1");
                    table.ForeignKey(
                        name: "fk_auditoria_evento_empresa",
                        column: x => x.empresa_id,
                        principalTable: "empresa",
                        principalColumn: "empresa_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_auditoria_evento_usuario",
                        column: x => x.usuario_id,
                        principalTable: "usuario",
                        principalColumn: "usuario_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "carro",
                columns: table => new
                {
                    carro_id = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    empresa_id = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    marca = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    modelo = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    anio = table.Column<int>(type: "int", nullable: true),
                    color = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    placas = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    foto_vehiculo = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    capacidad_tanque = table.Column<decimal>(type: "decimal(6,2)", precision: 6, scale: 2, nullable: true),
                    kilometraje_actual = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false, defaultValue: 0m),
                    estado = table.Column<string>(type: "varchar(16)", nullable: false, defaultValueSql: "'disponible'"),
                    tipo_vehiculo = table.Column<string>(type: "varchar(9)", nullable: false, defaultValueSql: "'carro'"),
                    dado_de_baja = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    fecha_baja = table.Column<DateTime>(type: "datetime2(0)", nullable: true),
                    motivo_baja = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    fecha_alta = table.Column<DateTime>(type: "datetime2(0)", nullable: true, defaultValueSql: "SYSDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_carro", x => x.carro_id);
                    table.UniqueConstraint("uq_carro_empresa_id_carro_id", x => new { x.empresa_id, x.carro_id });
                    table.CheckConstraint("ck_carro_estado", "[estado] IN ('disponible', 'ocupado', 'en_mantenimiento')");
                    table.CheckConstraint("ck_carro_tipo_vehiculo", "[tipo_vehiculo] IN ('carro', 'moto', 'camioneta', 'camion')");
                    table.ForeignKey(
                        name: "fk_carro_empresa",
                        column: x => x.empresa_id,
                        principalTable: "empresa",
                        principalColumn: "empresa_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "centro_costo",
                columns: table => new
                {
                    centro_costo_id = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    empresa_id = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    nombre = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    clave = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    fecha_registro = table.Column<DateTime>(type: "datetime2(0)", nullable: true, defaultValueSql: "SYSDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_centro_costo", x => x.centro_costo_id);
                    table.UniqueConstraint("uq_centro_costo_empresa_id_centro_costo_id", x => new { x.empresa_id, x.centro_costo_id });
                    table.ForeignKey(
                        name: "fk_centro_costo_empresa",
                        column: x => x.empresa_id,
                        principalTable: "empresa",
                        principalColumn: "empresa_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "empleado",
                columns: table => new
                {
                    empleado_id = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    empresa_id = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    usuario_id = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    nombre_completo = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    foto_perfil = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    foto_licencia = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    fecha_carga_licencia = table.Column<DateTime>(type: "datetime2(0)", nullable: true),
                    eliminado = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    fecha_alta = table.Column<DateTime>(type: "datetime2(0)", nullable: true, defaultValueSql: "SYSDATETIME()")
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
                });

            migrationBuilder.CreateTable(
                name: "outbox_mensaje",
                columns: table => new
                {
                    outbox_mensaje_id = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    empresa_id = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    tipo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    agregado_id = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    payload = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    estado = table.Column<string>(type: "varchar(10)", nullable: false, defaultValueSql: "'pendiente'"),
                    intentos = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    proximo_intento = table.Column<DateTime>(type: "datetime2(0)", nullable: false, defaultValueSql: "SYSDATETIME()"),
                    ultimo_error = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    fecha_creacion = table.Column<DateTime>(type: "datetime2(0)", nullable: false, defaultValueSql: "SYSDATETIME()"),
                    fecha_procesado = table.Column<DateTime>(type: "datetime2(0)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_outbox_mensaje", x => x.outbox_mensaje_id);
                    table.CheckConstraint("ck_outbox_mensaje_estado", "[estado] IN ('pendiente', 'procesando', 'enviado', 'fallido')");
                    table.CheckConstraint("ck_outbox_mensaje_payload_json", "[payload] IS NULL OR ISJSON([payload]) = 1");
                    table.ForeignKey(
                        name: "fk_outbox_mensaje_empresa",
                        column: x => x.empresa_id,
                        principalTable: "empresa",
                        principalColumn: "empresa_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "rol",
                columns: table => new
                {
                    rol_id = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    empresa_id = table.Column<byte[]>(type: "binary(16)", nullable: true),
                    nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    descripcion = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
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
                });

            migrationBuilder.CreateTable(
                name: "dispositivo_empleado",
                columns: table => new
                {
                    dispositivo_empleado_id = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    empresa_id = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    empleado_id = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    token_fcm = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    plataforma = table.Column<string>(type: "varchar(7)", nullable: false),
                    activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    fecha_registro = table.Column<DateTime>(type: "datetime2(0)", nullable: false, defaultValueSql: "SYSDATETIME()"),
                    fecha_ultimo_uso = table.Column<DateTime>(type: "datetime2(0)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dispositivo_empleado", x => x.dispositivo_empleado_id);
                    table.CheckConstraint("ck_dispositivo_empleado_plataforma", "[plataforma] IN ('android', 'ios', 'web')");
                    table.ForeignKey(
                        name: "fk_dispositivo_empleado_empleado",
                        columns: x => new { x.empresa_id, x.empleado_id },
                        principalTable: "empleado",
                        principalColumns: new[] { "empresa_id", "empleado_id" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_dispositivo_empleado_empresa",
                        column: x => x.empresa_id,
                        principalTable: "empresa",
                        principalColumn: "empresa_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "notificacion",
                columns: table => new
                {
                    notificacion_id = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    empresa_id = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    empleado_destinatario_id = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    tipo = table.Column<string>(type: "varchar(22)", nullable: false),
                    referencia_id = table.Column<byte[]>(type: "binary(16)", nullable: true),
                    mensaje = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    leida = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    fecha_envio = table.Column<DateTime>(type: "datetime2(0)", nullable: true, defaultValueSql: "SYSDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notificacion", x => x.notificacion_id);
                    table.CheckConstraint("ck_notificacion_tipo", "[tipo] IN ('nueva_solicitud', 'resultado_solicitud', 'cambio_estatus_forzado', 'reporte_mantenimiento')");
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
                });

            migrationBuilder.CreateTable(
                name: "solicitud_reserva",
                columns: table => new
                {
                    solicitud_reserva_id = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    empresa_id = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    carro_id = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    empleado_solicitante_id = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    motivo_solicitud = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    fecha_solicitud = table.Column<DateTime>(type: "datetime2(0)", nullable: true, defaultValueSql: "SYSDATETIME()"),
                    estado = table.Column<string>(type: "varchar(10)", nullable: false, defaultValueSql: "'pendiente'"),
                    empleado_confirmador_id = table.Column<byte[]>(type: "binary(16)", nullable: true),
                    fecha_resolucion = table.Column<DateTime>(type: "datetime2(0)", nullable: true),
                    motivo_resolucion = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    fecha_liberacion = table.Column<DateTime>(type: "datetime2(0)", nullable: true),
                    observaciones_liberacion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_solicitud_reserva", x => x.solicitud_reserva_id);
                    table.UniqueConstraint("uq_solicitud_reserva_empresa_id_solicitud_reserva_id", x => new { x.empresa_id, x.solicitud_reserva_id });
                    table.CheckConstraint("ck_solicitud_reserva_estado", "[estado] IN ('pendiente', 'aprobada', 'rechazada', 'cancelada', 'finalizada')");
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
                });

            migrationBuilder.CreateTable(
                name: "rol_privilegio",
                columns: table => new
                {
                    rol_privilegio_id = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    rol_id = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    privilegio_id = table.Column<byte[]>(type: "binary(16)", nullable: false)
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
                });

            migrationBuilder.CreateTable(
                name: "usuario_rol",
                columns: table => new
                {
                    usuario_rol_id = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    usuario_id = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    rol_id = table.Column<byte[]>(type: "binary(16)", nullable: false)
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
                });

            migrationBuilder.CreateTable(
                name: "viaje",
                columns: table => new
                {
                    viaje_id = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    empresa_id = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    carro_id = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    solicitud_reserva_id = table.Column<byte[]>(type: "binary(16)", nullable: true),
                    empleado_operador_id = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    centro_costo_id = table.Column<byte[]>(type: "binary(16)", nullable: true),
                    fecha_inicio = table.Column<DateTime>(type: "datetime2(0)", nullable: false),
                    ubicacion_inicio_lat = table.Column<decimal>(type: "decimal(9,6)", precision: 9, scale: 6, nullable: true),
                    ubicacion_inicio_lng = table.Column<decimal>(type: "decimal(9,6)", precision: 9, scale: 6, nullable: true),
                    kilometraje_inicial = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    foto_tablero_inicial = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    fecha_fin = table.Column<DateTime>(type: "datetime2(0)", nullable: true),
                    ubicacion_fin_lat = table.Column<decimal>(type: "decimal(9,6)", precision: 9, scale: 6, nullable: true),
                    ubicacion_fin_lng = table.Column<decimal>(type: "decimal(9,6)", precision: 9, scale: 6, nullable: true),
                    kilometraje_final = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    foto_tablero_final = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    distancia_recorrida = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    estado = table.Column<string>(type: "varchar(12)", nullable: false, defaultValueSql: "'en_curso'"),
                    motivo_interrupcion = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    corregido = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    fecha_recepcion = table.Column<DateTime>(type: "datetime2(0)", nullable: true, defaultValueSql: "SYSDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_viaje", x => x.viaje_id);
                    table.UniqueConstraint("uq_viaje_empresa_id_viaje_id", x => new { x.empresa_id, x.viaje_id });
                    table.CheckConstraint("ck_viaje_estado", "[estado] IN ('en_curso', 'finalizado', 'interrumpido')");
                    table.ForeignKey(
                        name: "fk_viaje_carro",
                        columns: x => new { x.empresa_id, x.carro_id },
                        principalTable: "carro",
                        principalColumns: new[] { "empresa_id", "carro_id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_viaje_centro_costo",
                        columns: x => new { x.empresa_id, x.centro_costo_id },
                        principalTable: "centro_costo",
                        principalColumns: new[] { "empresa_id", "centro_costo_id" },
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
                });

            migrationBuilder.CreateTable(
                name: "cambio_estatus_carro",
                columns: table => new
                {
                    cambio_estatus_carro_id = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    empresa_id = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    carro_id = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    empleado_id = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    viaje_afectado_id = table.Column<byte[]>(type: "binary(16)", nullable: true),
                    estatus_anterior = table.Column<string>(type: "varchar(16)", nullable: false),
                    estatus_nuevo = table.Column<string>(type: "varchar(16)", nullable: false),
                    tipo_motivo = table.Column<string>(type: "varchar(14)", nullable: true),
                    motivo = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    foto_evidencia = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    fecha_cambio = table.Column<DateTime>(type: "datetime2(0)", nullable: true, defaultValueSql: "SYSDATETIME()"),
                    fecha_recepcion = table.Column<DateTime>(type: "datetime2(0)", nullable: true, defaultValueSql: "SYSDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cambio_estatus_carro", x => x.cambio_estatus_carro_id);
                    table.CheckConstraint("ck_cambio_estatus_carro_estatus_anterior", "[estatus_anterior] IN ('disponible', 'ocupado', 'en_mantenimiento')");
                    table.CheckConstraint("ck_cambio_estatus_carro_estatus_nuevo", "[estatus_nuevo] IN ('disponible', 'ocupado', 'en_mantenimiento')");
                    table.CheckConstraint("ck_cambio_estatus_carro_tipo_motivo", "[tipo_motivo] IN ('liberacion', 'falla_mecanica', 'choque_o_golpe', 'servicio', 'emergencia', 'otro')");
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
                });

            migrationBuilder.CreateTable(
                name: "carga_combustible",
                columns: table => new
                {
                    carga_combustible_id = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    empresa_id = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    viaje_id = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    centro_costo_id = table.Column<byte[]>(type: "binary(16)", nullable: true),
                    litros = table.Column<decimal>(type: "decimal(8,2)", precision: 8, scale: 2, nullable: false),
                    monto = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    foto_comprobante = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    fecha_carga = table.Column<DateTime>(type: "datetime2(0)", nullable: false),
                    ubicacion_lat = table.Column<decimal>(type: "decimal(9,6)", precision: 9, scale: 6, nullable: true),
                    ubicacion_lng = table.Column<decimal>(type: "decimal(9,6)", precision: 9, scale: 6, nullable: true),
                    fecha_subida_comprobante = table.Column<DateTime>(type: "datetime2(0)", nullable: true),
                    corregido = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    fecha_recepcion = table.Column<DateTime>(type: "datetime2(0)", nullable: true, defaultValueSql: "SYSDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_carga_combustible", x => x.carga_combustible_id);
                    table.ForeignKey(
                        name: "fk_carga_combustible_centro_costo",
                        columns: x => new { x.empresa_id, x.centro_costo_id },
                        principalTable: "centro_costo",
                        principalColumns: new[] { "empresa_id", "centro_costo_id" },
                        onDelete: ReferentialAction.Restrict);
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
                });

            migrationBuilder.CreateTable(
                name: "punto_ruta",
                columns: table => new
                {
                    punto_ruta_id = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    empresa_id = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    viaje_id = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    latitud = table.Column<decimal>(type: "decimal(9,6)", precision: 9, scale: 6, nullable: false),
                    longitud = table.Column<decimal>(type: "decimal(9,6)", precision: 9, scale: 6, nullable: false),
                    precision_metros = table.Column<decimal>(type: "decimal(6,2)", precision: 6, scale: 2, nullable: true),
                    altitud = table.Column<decimal>(type: "decimal(7,2)", precision: 7, scale: 2, nullable: true),
                    velocidad = table.Column<decimal>(type: "decimal(6,2)", precision: 6, scale: 2, nullable: true),
                    timestamp_captura = table.Column<DateTime>(type: "datetime2(0)", nullable: false),
                    fecha_recepcion = table.Column<DateTime>(type: "datetime2(0)", nullable: true, defaultValueSql: "SYSDATETIME()")
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
                });

            migrationBuilder.CreateTable(
                name: "revision_auditoria",
                columns: table => new
                {
                    revision_auditoria_id = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    empresa_id = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    viaje_id = table.Column<byte[]>(type: "binary(16)", nullable: true),
                    carro_id = table.Column<byte[]>(type: "binary(16)", nullable: true),
                    periodo_desde = table.Column<DateOnly>(type: "date", nullable: true),
                    periodo_hasta = table.Column<DateOnly>(type: "date", nullable: true),
                    empleado_revisor_id = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    fecha_revision = table.Column<DateTime>(type: "datetime2(0)", nullable: true, defaultValueSql: "SYSDATETIME()"),
                    estado_revision = table.Column<string>(type: "varchar(21)", nullable: false, defaultValueSql: "'pendiente_informacion'"),
                    observaciones = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    fecha_cierre = table.Column<DateTime>(type: "datetime2(0)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_revision_auditoria", x => x.revision_auditoria_id);
                    table.CheckConstraint("ck_revision_auditoria_alcance", "(viaje_id IS NOT NULL) OR (carro_id IS NOT NULL AND periodo_desde IS NOT NULL AND periodo_hasta IS NOT NULL)");
                    table.CheckConstraint("ck_revision_auditoria_estado_revision", "[estado_revision] IN ('pendiente_informacion', 'revisado', 'con_observaciones')");
                    table.ForeignKey(
                        name: "fk_revision_auditoria_carro",
                        columns: x => new { x.empresa_id, x.carro_id },
                        principalTable: "carro",
                        principalColumns: new[] { "empresa_id", "carro_id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_revision_auditoria_empleado_revisor",
                        columns: x => new { x.empresa_id, x.empleado_revisor_id },
                        principalTable: "empleado",
                        principalColumns: new[] { "empresa_id", "empleado_id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_revision_auditoria_empresa",
                        column: x => x.empresa_id,
                        principalTable: "empresa",
                        principalColumn: "empresa_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_revision_auditoria_viaje",
                        columns: x => new { x.empresa_id, x.viaje_id },
                        principalTable: "viaje",
                        principalColumns: new[] { "empresa_id", "viaje_id" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ruta_procesada",
                columns: table => new
                {
                    ruta_procesada_id = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    empresa_id = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    viaje_id = table.Column<byte[]>(type: "binary(16)", nullable: false),
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
                    alerta_kilometraje = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    litros_cargados = table.Column<decimal>(type: "decimal(8,2)", precision: 8, scale: 2, nullable: false, defaultValue: 0m),
                    rendimiento_km_por_litro = table.Column<decimal>(type: "decimal(6,2)", precision: 6, scale: 2, nullable: true),
                    polyline = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    version_algoritmo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    fecha_proceso = table.Column<DateTime>(type: "datetime2(0)", nullable: false, defaultValueSql: "SYSDATETIME()")
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
                });

            migrationBuilder.CreateTable(
                name: "viaje_pausa",
                columns: table => new
                {
                    viaje_pausa_id = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    empresa_id = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    viaje_id = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    fecha_inicio = table.Column<DateTime>(type: "datetime2(0)", nullable: false),
                    fecha_fin = table.Column<DateTime>(type: "datetime2(0)", nullable: true),
                    motivo = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
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
                });

            migrationBuilder.CreateIndex(
                name: "ix_auditoria_evento_empresa_id_entidad_entidad_id",
                table: "auditoria_evento",
                columns: new[] { "empresa_id", "entidad", "entidad_id" });

            migrationBuilder.CreateIndex(
                name: "ix_auditoria_evento_empresa_id_fecha_hora",
                table: "auditoria_evento",
                columns: new[] { "empresa_id", "fecha_hora" });

            migrationBuilder.CreateIndex(
                name: "ix_auditoria_evento_usuario_id_fecha_hora",
                table: "auditoria_evento",
                columns: new[] { "usuario_id", "fecha_hora" });

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
                name: "ix_cambio_estatus_carro_empresa_id_tipo_motivo",
                table: "cambio_estatus_carro",
                columns: new[] { "empresa_id", "tipo_motivo" });

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
                name: "ix_carga_combustible_empresa_id_centro_costo_id",
                table: "carga_combustible",
                columns: new[] { "empresa_id", "centro_costo_id" });

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
                name: "uq_centro_costo_empresa_id_clave",
                table: "centro_costo",
                columns: new[] { "empresa_id", "clave" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_dispositivo_empleado_empleado_id_activo",
                table: "dispositivo_empleado",
                columns: new[] { "empleado_id", "activo" });

            migrationBuilder.CreateIndex(
                name: "ix_dispositivo_empleado_empresa_id",
                table: "dispositivo_empleado",
                column: "empresa_id");

            migrationBuilder.CreateIndex(
                name: "ix_dispositivo_empleado_empresa_id_empleado_id",
                table: "dispositivo_empleado",
                columns: new[] { "empresa_id", "empleado_id" });

            migrationBuilder.CreateIndex(
                name: "uq_dispositivo_empleado_empresa_id_dispositivo_empleado_id",
                table: "dispositivo_empleado",
                columns: new[] { "empresa_id", "dispositivo_empleado_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "uq_dispositivo_empleado_token_fcm",
                table: "dispositivo_empleado",
                column: "token_fcm",
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
                unique: true,
                filter: "[corporativo_id] IS NOT NULL");

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
                name: "ix_outbox_mensaje_agregado_id",
                table: "outbox_mensaje",
                column: "agregado_id");

            migrationBuilder.CreateIndex(
                name: "ix_outbox_mensaje_empresa_id",
                table: "outbox_mensaje",
                column: "empresa_id");

            migrationBuilder.CreateIndex(
                name: "ix_outbox_mensaje_estado_proximo_intento",
                table: "outbox_mensaje",
                columns: new[] { "estado", "proximo_intento" });

            migrationBuilder.CreateIndex(
                name: "uq_outbox_mensaje_empresa_id_outbox_mensaje_id",
                table: "outbox_mensaje",
                columns: new[] { "empresa_id", "outbox_mensaje_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_privilegio_modulo",
                table: "privilegio",
                column: "modulo");

            migrationBuilder.CreateIndex(
                name: "uq_privilegio_clave",
                table: "privilegio",
                column: "clave",
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
                name: "ix_revision_auditoria_empresa_id",
                table: "revision_auditoria",
                column: "empresa_id");

            migrationBuilder.CreateIndex(
                name: "ix_revision_auditoria_empresa_id_carro_id_periodo_desde",
                table: "revision_auditoria",
                columns: new[] { "empresa_id", "carro_id", "periodo_desde" });

            migrationBuilder.CreateIndex(
                name: "ix_revision_auditoria_empresa_id_empleado_revisor_id",
                table: "revision_auditoria",
                columns: new[] { "empresa_id", "empleado_revisor_id" });

            migrationBuilder.CreateIndex(
                name: "ix_revision_auditoria_empresa_id_estado_revision",
                table: "revision_auditoria",
                columns: new[] { "empresa_id", "estado_revision" });

            migrationBuilder.CreateIndex(
                name: "ix_revision_auditoria_empresa_id_viaje_id",
                table: "revision_auditoria",
                columns: new[] { "empresa_id", "viaje_id" });

            migrationBuilder.CreateIndex(
                name: "uq_revision_auditoria_empresa_id_revision_auditoria_id",
                table: "revision_auditoria",
                columns: new[] { "empresa_id", "revision_auditoria_id" },
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

            migrationBuilder.CreateIndex(
                name: "ix_sesion_usuario_empresa_id_fecha_revocacion",
                table: "sesion_usuario",
                columns: new[] { "empresa_id", "fecha_revocacion" });

            migrationBuilder.CreateIndex(
                name: "ix_sesion_usuario_expira_en",
                table: "sesion_usuario",
                column: "expira_en");

            migrationBuilder.CreateIndex(
                name: "ix_sesion_usuario_token_hash_anterior",
                table: "sesion_usuario",
                column: "token_hash_anterior");

            migrationBuilder.CreateIndex(
                name: "ix_sesion_usuario_usuario_id_fecha_revocacion_expira_en",
                table: "sesion_usuario",
                columns: new[] { "usuario_id", "fecha_revocacion", "expira_en" });

            migrationBuilder.CreateIndex(
                name: "uq_sesion_usuario_token_hash",
                table: "sesion_usuario",
                column: "token_hash",
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
                name: "ix_solicitud_reserva_empresa_id_carro_id_estado",
                table: "solicitud_reserva",
                columns: new[] { "empresa_id", "carro_id", "estado" });

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
                name: "ix_viaje_empresa_id_carro_id_fecha_inicio",
                table: "viaje",
                columns: new[] { "empresa_id", "carro_id", "fecha_inicio" });

            migrationBuilder.CreateIndex(
                name: "ix_viaje_empresa_id_centro_costo_id",
                table: "viaje",
                columns: new[] { "empresa_id", "centro_costo_id" });

            migrationBuilder.CreateIndex(
                name: "ix_viaje_empresa_id_empleado_operador_id",
                table: "viaje",
                columns: new[] { "empresa_id", "empleado_operador_id" });

            migrationBuilder.CreateIndex(
                name: "ix_viaje_empresa_id_solicitud_reserva_id",
                table: "viaje",
                columns: new[] { "empresa_id", "solicitud_reserva_id" });

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
                name: "auditoria_evento");

            migrationBuilder.DropTable(
                name: "cambio_estatus_carro");

            migrationBuilder.DropTable(
                name: "carga_combustible");

            migrationBuilder.DropTable(
                name: "dispositivo_empleado");

            migrationBuilder.DropTable(
                name: "notificacion");

            migrationBuilder.DropTable(
                name: "outbox_mensaje");

            migrationBuilder.DropTable(
                name: "punto_ruta");

            migrationBuilder.DropTable(
                name: "revision_auditoria");

            migrationBuilder.DropTable(
                name: "rol_privilegio");

            migrationBuilder.DropTable(
                name: "ruta_procesada");

            migrationBuilder.DropTable(
                name: "sesion_usuario");

            migrationBuilder.DropTable(
                name: "usuario_rol");

            migrationBuilder.DropTable(
                name: "viaje_pausa");

            migrationBuilder.DropTable(
                name: "privilegio");

            migrationBuilder.DropTable(
                name: "rol");

            migrationBuilder.DropTable(
                name: "viaje");

            migrationBuilder.DropTable(
                name: "centro_costo");

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
