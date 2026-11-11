-- ============================================================
--  APIGasoline — Esquema de la base de datos (SQL Server)
--  Generado con `dotnet ef migrations script` a partir de las migraciones
--  20261111182911_InicialSqlServer y 20261111211442_AgregarEmpresaAUsuario
--  (APIGasoline.Infrastructure/Migrations).
--
--  24 tablas · 47 llaves foráneas · 21 CHECK
--  IDs en binary(16) (UUID v7 en orden RFC 4122) · fechas en datetime2(0)
--  Intercalación de la BD: Modern_Spanish_CI_AI (la pone EF al crear la BD).
--
--  La forma oficial de crear la BD es con las migraciones
--  (BaseDatos:MigrarAlIniciar = true, o `dotnet ef database update`).
-- ============================================================

IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
CREATE TABLE [corporativo] (
    [corporativo_id] binary(16) NOT NULL,
    [nombre] nvarchar(150) NOT NULL,
    [activo] bit NOT NULL DEFAULT CAST(1 AS bit),
    [fecha_registro] datetime2(0) NULL DEFAULT (SYSDATETIME()),
    CONSTRAINT [PK_corporativo] PRIMARY KEY ([corporativo_id])
);

CREATE TABLE [privilegio] (
    [privilegio_id] binary(16) NOT NULL,
    [clave] nvarchar(100) NOT NULL,
    [modulo] nvarchar(50) NOT NULL,
    [nombre] nvarchar(100) NOT NULL,
    [descripcion] nvarchar(255) NULL,
    [ambito] varchar(10) NOT NULL DEFAULT ('empresa'),
    [activo] bit NOT NULL DEFAULT CAST(1 AS bit),
    CONSTRAINT [PK_privilegio] PRIMARY KEY ([privilegio_id]),
    CONSTRAINT [ck_privilegio_ambito] CHECK ([ambito] IN ('plataforma', 'empresa'))
);

CREATE TABLE [usuario] (
    [usuario_id] binary(16) NOT NULL,
    [email] nvarchar(150) NOT NULL,
    [password_hash] nvarchar(255) NOT NULL,
    [estado] varchar(8) NOT NULL DEFAULT ('activo'),
    [fecha_registro] datetime2(0) NULL DEFAULT (SYSDATETIME()),
    CONSTRAINT [PK_usuario] PRIMARY KEY ([usuario_id]),
    CONSTRAINT [ck_usuario_estado] CHECK ([estado] IN ('activo', 'inactivo'))
);

CREATE TABLE [division] (
    [division_id] binary(16) NOT NULL,
    [corporativo_id] binary(16) NOT NULL,
    [nombre] nvarchar(100) NOT NULL,
    [activo] bit NOT NULL DEFAULT CAST(1 AS bit),
    [fecha_registro] datetime2(0) NULL DEFAULT (SYSDATETIME()),
    CONSTRAINT [PK_division] PRIMARY KEY ([division_id]),
    CONSTRAINT [uq_division_corporativo_id_division_id] UNIQUE ([corporativo_id], [division_id]),
    CONSTRAINT [fk_division_corporativo] FOREIGN KEY ([corporativo_id]) REFERENCES [corporativo] ([corporativo_id]) ON DELETE NO ACTION
);

CREATE TABLE [sesion_usuario] (
    [sesion_usuario_id] binary(16) NOT NULL,
    [usuario_id] binary(16) NOT NULL,
    [empresa_id] binary(16) NULL,
    [token_hash] char(64) NOT NULL,
    [token_hash_anterior] char(64) NULL,
    [plataforma] varchar(7) NULL,
    [nombre_dispositivo] nvarchar(100) NULL,
    [ip] nvarchar(45) NULL,
    [user_agent] nvarchar(255) NULL,
    [fecha_creacion] datetime2(0) NOT NULL,
    [fecha_ultimo_uso] datetime2(0) NOT NULL,
    [fecha_rotacion] datetime2(0) NULL,
    [expira_en] datetime2(0) NOT NULL,
    [expira_absoluta_en] datetime2(0) NOT NULL,
    [fecha_revocacion] datetime2(0) NULL,
    [motivo_revocacion] varchar(25) NULL,
    [revocada_por_usuario_id] binary(16) NULL,
    CONSTRAINT [PK_sesion_usuario] PRIMARY KEY ([sesion_usuario_id]),
    CONSTRAINT [ck_sesion_usuario_motivo_revocacion] CHECK ([motivo_revocacion] IN ('cierre_sesion', 'cerrada_por_usuario', 'cerrada_por_administrador', 'cambio_password', 'usuario_desactivado', 'empleado_dado_de_baja', 'empleado_reasignado', 'empresa_eliminada', 'reuso_detectado', 'acceso_revocado', 'limite_sesiones')),
    CONSTRAINT [ck_sesion_usuario_plataforma] CHECK ([plataforma] IN ('android', 'ios', 'web')),
    CONSTRAINT [fk_sesion_usuario_usuario] FOREIGN KEY ([usuario_id]) REFERENCES [usuario] ([usuario_id]) ON DELETE CASCADE
);

CREATE TABLE [empresa] (
    [empresa_id] binary(16) NOT NULL,
    [corporativo_id] binary(16) NULL,
    [division_id] binary(16) NULL,
    [nombre] nvarchar(150) NOT NULL,
    [activo] bit NOT NULL DEFAULT CAST(1 AS bit),
    [eliminado] bit NOT NULL DEFAULT CAST(0 AS bit),
    [fecha_registro] datetime2(0) NULL DEFAULT (SYSDATETIME()),
    CONSTRAINT [PK_empresa] PRIMARY KEY ([empresa_id]),
    CONSTRAINT [fk_empresa_corporativo] FOREIGN KEY ([corporativo_id]) REFERENCES [corporativo] ([corporativo_id]) ON DELETE NO ACTION,
    CONSTRAINT [fk_empresa_division] FOREIGN KEY ([corporativo_id], [division_id]) REFERENCES [division] ([corporativo_id], [division_id]) ON DELETE NO ACTION
);

CREATE TABLE [auditoria_evento] (
    [auditoria_evento_id] binary(16) NOT NULL,
    [empresa_id] binary(16) NULL,
    [usuario_id] binary(16) NULL,
    [accion] varchar(14) NOT NULL,
    [entidad] nvarchar(60) NOT NULL,
    [entidad_id] binary(16) NOT NULL,
    [valores_anteriores] nvarchar(max) NULL,
    [valores_nuevos] nvarchar(max) NULL,
    [motivo] nvarchar(500) NULL,
    [origen] varchar(7) NOT NULL,
    [fecha_hora] datetime2(0) NULL DEFAULT (SYSDATETIME()),
    CONSTRAINT [PK_auditoria_evento] PRIMARY KEY ([auditoria_evento_id]),
    CONSTRAINT [ck_auditoria_evento_accion] CHECK ([accion] IN ('crear', 'actualizar', 'eliminar', 'aprobar', 'rechazar', 'cancelar', 'cambiar_estado', 'corregir')),
    CONSTRAINT [ck_auditoria_evento_origen] CHECK ([origen] IN ('movil', 'web', 'sistema')),
    CONSTRAINT [ck_auditoria_evento_valores_anteriores_json] CHECK ([valores_anteriores] IS NULL OR ISJSON([valores_anteriores]) = 1),
    CONSTRAINT [ck_auditoria_evento_valores_nuevos_json] CHECK ([valores_nuevos] IS NULL OR ISJSON([valores_nuevos]) = 1),
    CONSTRAINT [fk_auditoria_evento_empresa] FOREIGN KEY ([empresa_id]) REFERENCES [empresa] ([empresa_id]) ON DELETE NO ACTION,
    CONSTRAINT [fk_auditoria_evento_usuario] FOREIGN KEY ([usuario_id]) REFERENCES [usuario] ([usuario_id]) ON DELETE NO ACTION
);

CREATE TABLE [carro] (
    [carro_id] binary(16) NOT NULL,
    [empresa_id] binary(16) NOT NULL,
    [marca] nvarchar(80) NOT NULL,
    [modelo] nvarchar(80) NOT NULL,
    [anio] int NULL,
    [color] nvarchar(50) NULL,
    [placas] nvarchar(20) NOT NULL,
    [foto_vehiculo] nvarchar(255) NULL,
    [capacidad_tanque] decimal(6,2) NULL,
    [kilometraje_actual] decimal(10,2) NOT NULL DEFAULT 0.0,
    [estado] varchar(16) NOT NULL DEFAULT ('disponible'),
    [tipo_vehiculo] varchar(9) NOT NULL DEFAULT ('carro'),
    [dado_de_baja] bit NOT NULL DEFAULT CAST(0 AS bit),
    [fecha_baja] datetime2(0) NULL,
    [motivo_baja] nvarchar(255) NULL,
    [fecha_alta] datetime2(0) NULL DEFAULT (SYSDATETIME()),
    CONSTRAINT [PK_carro] PRIMARY KEY ([carro_id]),
    CONSTRAINT [uq_carro_empresa_id_carro_id] UNIQUE ([empresa_id], [carro_id]),
    CONSTRAINT [ck_carro_estado] CHECK ([estado] IN ('disponible', 'ocupado', 'en_mantenimiento')),
    CONSTRAINT [ck_carro_tipo_vehiculo] CHECK ([tipo_vehiculo] IN ('carro', 'moto', 'camioneta', 'camion')),
    CONSTRAINT [fk_carro_empresa] FOREIGN KEY ([empresa_id]) REFERENCES [empresa] ([empresa_id]) ON DELETE NO ACTION
);

CREATE TABLE [centro_costo] (
    [centro_costo_id] binary(16) NOT NULL,
    [empresa_id] binary(16) NOT NULL,
    [nombre] nvarchar(150) NOT NULL,
    [clave] nvarchar(100) NOT NULL,
    [activo] bit NOT NULL DEFAULT CAST(1 AS bit),
    [fecha_registro] datetime2(0) NULL DEFAULT (SYSDATETIME()),
    CONSTRAINT [PK_centro_costo] PRIMARY KEY ([centro_costo_id]),
    CONSTRAINT [uq_centro_costo_empresa_id_centro_costo_id] UNIQUE ([empresa_id], [centro_costo_id]),
    CONSTRAINT [fk_centro_costo_empresa] FOREIGN KEY ([empresa_id]) REFERENCES [empresa] ([empresa_id]) ON DELETE NO ACTION
);

CREATE TABLE [empleado] (
    [empleado_id] binary(16) NOT NULL,
    [empresa_id] binary(16) NOT NULL,
    [usuario_id] binary(16) NOT NULL,
    [nombre_completo] nvarchar(150) NOT NULL,
    [foto_perfil] nvarchar(255) NULL,
    [foto_licencia] nvarchar(255) NULL,
    [fecha_carga_licencia] datetime2(0) NULL,
    [eliminado] bit NOT NULL DEFAULT CAST(0 AS bit),
    [fecha_alta] datetime2(0) NULL DEFAULT (SYSDATETIME()),
    CONSTRAINT [PK_empleado] PRIMARY KEY ([empleado_id]),
    CONSTRAINT [uq_empleado_empresa_id_empleado_id] UNIQUE ([empresa_id], [empleado_id]),
    CONSTRAINT [fk_empleado_empresa] FOREIGN KEY ([empresa_id]) REFERENCES [empresa] ([empresa_id]) ON DELETE NO ACTION,
    CONSTRAINT [fk_empleado_usuario] FOREIGN KEY ([usuario_id]) REFERENCES [usuario] ([usuario_id]) ON DELETE NO ACTION
);

CREATE TABLE [outbox_mensaje] (
    [outbox_mensaje_id] binary(16) NOT NULL,
    [empresa_id] binary(16) NOT NULL,
    [tipo] nvarchar(100) NOT NULL,
    [agregado_id] binary(16) NOT NULL,
    [payload] nvarchar(max) NOT NULL,
    [estado] varchar(10) NOT NULL DEFAULT ('pendiente'),
    [intentos] int NOT NULL DEFAULT 0,
    [proximo_intento] datetime2(0) NOT NULL DEFAULT (SYSDATETIME()),
    [ultimo_error] nvarchar(500) NULL,
    [fecha_creacion] datetime2(0) NOT NULL DEFAULT (SYSDATETIME()),
    [fecha_procesado] datetime2(0) NULL,
    CONSTRAINT [PK_outbox_mensaje] PRIMARY KEY ([outbox_mensaje_id]),
    CONSTRAINT [ck_outbox_mensaje_estado] CHECK ([estado] IN ('pendiente', 'procesando', 'enviado', 'fallido')),
    CONSTRAINT [ck_outbox_mensaje_payload_json] CHECK ([payload] IS NULL OR ISJSON([payload]) = 1),
    CONSTRAINT [fk_outbox_mensaje_empresa] FOREIGN KEY ([empresa_id]) REFERENCES [empresa] ([empresa_id]) ON DELETE NO ACTION
);

CREATE TABLE [rol] (
    [rol_id] binary(16) NOT NULL,
    [empresa_id] binary(16) NULL,
    [nombre] nvarchar(100) NOT NULL,
    [descripcion] nvarchar(255) NULL,
    [activo] bit NOT NULL DEFAULT CAST(1 AS bit),
    CONSTRAINT [PK_rol] PRIMARY KEY ([rol_id]),
    CONSTRAINT [fk_rol_empresa] FOREIGN KEY ([empresa_id]) REFERENCES [empresa] ([empresa_id]) ON DELETE NO ACTION
);

CREATE TABLE [dispositivo_empleado] (
    [dispositivo_empleado_id] binary(16) NOT NULL,
    [empresa_id] binary(16) NOT NULL,
    [empleado_id] binary(16) NOT NULL,
    [token_fcm] nvarchar(512) NOT NULL,
    [plataforma] varchar(7) NOT NULL,
    [activo] bit NOT NULL DEFAULT CAST(1 AS bit),
    [fecha_registro] datetime2(0) NOT NULL DEFAULT (SYSDATETIME()),
    [fecha_ultimo_uso] datetime2(0) NULL,
    CONSTRAINT [PK_dispositivo_empleado] PRIMARY KEY ([dispositivo_empleado_id]),
    CONSTRAINT [ck_dispositivo_empleado_plataforma] CHECK ([plataforma] IN ('android', 'ios', 'web')),
    CONSTRAINT [fk_dispositivo_empleado_empleado] FOREIGN KEY ([empresa_id], [empleado_id]) REFERENCES [empleado] ([empresa_id], [empleado_id]) ON DELETE CASCADE,
    CONSTRAINT [fk_dispositivo_empleado_empresa] FOREIGN KEY ([empresa_id]) REFERENCES [empresa] ([empresa_id]) ON DELETE NO ACTION
);

CREATE TABLE [notificacion] (
    [notificacion_id] binary(16) NOT NULL,
    [empresa_id] binary(16) NOT NULL,
    [empleado_destinatario_id] binary(16) NOT NULL,
    [tipo] varchar(22) NOT NULL,
    [referencia_id] binary(16) NULL,
    [mensaje] nvarchar(255) NULL,
    [leida] bit NOT NULL DEFAULT CAST(0 AS bit),
    [fecha_envio] datetime2(0) NULL DEFAULT (SYSDATETIME()),
    CONSTRAINT [PK_notificacion] PRIMARY KEY ([notificacion_id]),
    CONSTRAINT [ck_notificacion_tipo] CHECK ([tipo] IN ('nueva_solicitud', 'resultado_solicitud', 'cambio_estatus_forzado', 'reporte_mantenimiento')),
    CONSTRAINT [fk_notificacion_empleado_destinatario] FOREIGN KEY ([empresa_id], [empleado_destinatario_id]) REFERENCES [empleado] ([empresa_id], [empleado_id]) ON DELETE NO ACTION,
    CONSTRAINT [fk_notificacion_empresa] FOREIGN KEY ([empresa_id]) REFERENCES [empresa] ([empresa_id]) ON DELETE NO ACTION
);

CREATE TABLE [solicitud_reserva] (
    [solicitud_reserva_id] binary(16) NOT NULL,
    [empresa_id] binary(16) NOT NULL,
    [carro_id] binary(16) NOT NULL,
    [empleado_solicitante_id] binary(16) NOT NULL,
    [motivo_solicitud] nvarchar(255) NULL,
    [fecha_solicitud] datetime2(0) NULL DEFAULT (SYSDATETIME()),
    [estado] varchar(10) NOT NULL DEFAULT ('pendiente'),
    [empleado_confirmador_id] binary(16) NULL,
    [fecha_resolucion] datetime2(0) NULL,
    [motivo_resolucion] nvarchar(255) NULL,
    [fecha_liberacion] datetime2(0) NULL,
    [observaciones_liberacion] nvarchar(500) NULL,
    CONSTRAINT [PK_solicitud_reserva] PRIMARY KEY ([solicitud_reserva_id]),
    CONSTRAINT [uq_solicitud_reserva_empresa_id_solicitud_reserva_id] UNIQUE ([empresa_id], [solicitud_reserva_id]),
    CONSTRAINT [ck_solicitud_reserva_estado] CHECK ([estado] IN ('pendiente', 'aprobada', 'rechazada', 'cancelada', 'finalizada')),
    CONSTRAINT [fk_solicitud_reserva_carro] FOREIGN KEY ([empresa_id], [carro_id]) REFERENCES [carro] ([empresa_id], [carro_id]) ON DELETE NO ACTION,
    CONSTRAINT [fk_solicitud_reserva_empleado_confirmador] FOREIGN KEY ([empresa_id], [empleado_confirmador_id]) REFERENCES [empleado] ([empresa_id], [empleado_id]) ON DELETE NO ACTION,
    CONSTRAINT [fk_solicitud_reserva_empleado_solicitante] FOREIGN KEY ([empresa_id], [empleado_solicitante_id]) REFERENCES [empleado] ([empresa_id], [empleado_id]) ON DELETE NO ACTION,
    CONSTRAINT [fk_solicitud_reserva_empresa] FOREIGN KEY ([empresa_id]) REFERENCES [empresa] ([empresa_id]) ON DELETE NO ACTION
);

CREATE TABLE [rol_privilegio] (
    [rol_privilegio_id] binary(16) NOT NULL,
    [rol_id] binary(16) NOT NULL,
    [privilegio_id] binary(16) NOT NULL,
    CONSTRAINT [PK_rol_privilegio] PRIMARY KEY ([rol_privilegio_id]),
    CONSTRAINT [fk_rol_privilegio_privilegio] FOREIGN KEY ([privilegio_id]) REFERENCES [privilegio] ([privilegio_id]) ON DELETE CASCADE,
    CONSTRAINT [fk_rol_privilegio_rol] FOREIGN KEY ([rol_id]) REFERENCES [rol] ([rol_id]) ON DELETE CASCADE
);

CREATE TABLE [usuario_rol] (
    [usuario_rol_id] binary(16) NOT NULL,
    [usuario_id] binary(16) NOT NULL,
    [rol_id] binary(16) NOT NULL,
    CONSTRAINT [PK_usuario_rol] PRIMARY KEY ([usuario_rol_id]),
    CONSTRAINT [fk_usuario_rol_rol] FOREIGN KEY ([rol_id]) REFERENCES [rol] ([rol_id]) ON DELETE CASCADE,
    CONSTRAINT [fk_usuario_rol_usuario] FOREIGN KEY ([usuario_id]) REFERENCES [usuario] ([usuario_id]) ON DELETE CASCADE
);

CREATE TABLE [viaje] (
    [viaje_id] binary(16) NOT NULL,
    [empresa_id] binary(16) NOT NULL,
    [carro_id] binary(16) NOT NULL,
    [solicitud_reserva_id] binary(16) NULL,
    [empleado_operador_id] binary(16) NOT NULL,
    [centro_costo_id] binary(16) NULL,
    [fecha_inicio] datetime2(0) NOT NULL,
    [ubicacion_inicio_lat] decimal(9,6) NULL,
    [ubicacion_inicio_lng] decimal(9,6) NULL,
    [kilometraje_inicial] decimal(10,2) NOT NULL,
    [foto_tablero_inicial] nvarchar(255) NULL,
    [fecha_fin] datetime2(0) NULL,
    [ubicacion_fin_lat] decimal(9,6) NULL,
    [ubicacion_fin_lng] decimal(9,6) NULL,
    [kilometraje_final] decimal(10,2) NULL,
    [foto_tablero_final] nvarchar(255) NULL,
    [distancia_recorrida] decimal(10,2) NULL,
    [estado] varchar(12) NOT NULL DEFAULT ('en_curso'),
    [motivo_interrupcion] nvarchar(255) NULL,
    [corregido] bit NOT NULL DEFAULT CAST(0 AS bit),
    [fecha_recepcion] datetime2(0) NULL DEFAULT (SYSDATETIME()),
    CONSTRAINT [PK_viaje] PRIMARY KEY ([viaje_id]),
    CONSTRAINT [uq_viaje_empresa_id_viaje_id] UNIQUE ([empresa_id], [viaje_id]),
    CONSTRAINT [ck_viaje_estado] CHECK ([estado] IN ('en_curso', 'finalizado', 'interrumpido')),
    CONSTRAINT [fk_viaje_carro] FOREIGN KEY ([empresa_id], [carro_id]) REFERENCES [carro] ([empresa_id], [carro_id]) ON DELETE NO ACTION,
    CONSTRAINT [fk_viaje_centro_costo] FOREIGN KEY ([empresa_id], [centro_costo_id]) REFERENCES [centro_costo] ([empresa_id], [centro_costo_id]) ON DELETE NO ACTION,
    CONSTRAINT [fk_viaje_empleado_operador] FOREIGN KEY ([empresa_id], [empleado_operador_id]) REFERENCES [empleado] ([empresa_id], [empleado_id]) ON DELETE NO ACTION,
    CONSTRAINT [fk_viaje_empresa] FOREIGN KEY ([empresa_id]) REFERENCES [empresa] ([empresa_id]) ON DELETE NO ACTION,
    CONSTRAINT [fk_viaje_solicitud_reserva] FOREIGN KEY ([empresa_id], [solicitud_reserva_id]) REFERENCES [solicitud_reserva] ([empresa_id], [solicitud_reserva_id]) ON DELETE NO ACTION
);

CREATE TABLE [cambio_estatus_carro] (
    [cambio_estatus_carro_id] binary(16) NOT NULL,
    [empresa_id] binary(16) NOT NULL,
    [carro_id] binary(16) NOT NULL,
    [empleado_id] binary(16) NOT NULL,
    [viaje_afectado_id] binary(16) NULL,
    [estatus_anterior] varchar(16) NOT NULL,
    [estatus_nuevo] varchar(16) NOT NULL,
    [tipo_motivo] varchar(14) NULL,
    [motivo] nvarchar(255) NULL,
    [foto_evidencia] nvarchar(255) NULL,
    [fecha_cambio] datetime2(0) NULL DEFAULT (SYSDATETIME()),
    [fecha_recepcion] datetime2(0) NULL DEFAULT (SYSDATETIME()),
    CONSTRAINT [PK_cambio_estatus_carro] PRIMARY KEY ([cambio_estatus_carro_id]),
    CONSTRAINT [ck_cambio_estatus_carro_estatus_anterior] CHECK ([estatus_anterior] IN ('disponible', 'ocupado', 'en_mantenimiento')),
    CONSTRAINT [ck_cambio_estatus_carro_estatus_nuevo] CHECK ([estatus_nuevo] IN ('disponible', 'ocupado', 'en_mantenimiento')),
    CONSTRAINT [ck_cambio_estatus_carro_tipo_motivo] CHECK ([tipo_motivo] IN ('liberacion', 'falla_mecanica', 'choque_o_golpe', 'servicio', 'emergencia', 'otro')),
    CONSTRAINT [fk_cambio_estatus_carro_carro] FOREIGN KEY ([empresa_id], [carro_id]) REFERENCES [carro] ([empresa_id], [carro_id]) ON DELETE NO ACTION,
    CONSTRAINT [fk_cambio_estatus_carro_empleado] FOREIGN KEY ([empresa_id], [empleado_id]) REFERENCES [empleado] ([empresa_id], [empleado_id]) ON DELETE NO ACTION,
    CONSTRAINT [fk_cambio_estatus_carro_empresa] FOREIGN KEY ([empresa_id]) REFERENCES [empresa] ([empresa_id]) ON DELETE NO ACTION,
    CONSTRAINT [fk_cambio_estatus_carro_viaje] FOREIGN KEY ([empresa_id], [viaje_afectado_id]) REFERENCES [viaje] ([empresa_id], [viaje_id]) ON DELETE NO ACTION
);

CREATE TABLE [carga_combustible] (
    [carga_combustible_id] binary(16) NOT NULL,
    [empresa_id] binary(16) NOT NULL,
    [viaje_id] binary(16) NOT NULL,
    [centro_costo_id] binary(16) NULL,
    [litros] decimal(8,2) NOT NULL,
    [monto] decimal(10,2) NULL,
    [foto_comprobante] nvarchar(255) NOT NULL,
    [fecha_carga] datetime2(0) NOT NULL,
    [ubicacion_lat] decimal(9,6) NULL,
    [ubicacion_lng] decimal(9,6) NULL,
    [fecha_subida_comprobante] datetime2(0) NULL,
    [corregido] bit NOT NULL DEFAULT CAST(0 AS bit),
    [fecha_recepcion] datetime2(0) NULL DEFAULT (SYSDATETIME()),
    CONSTRAINT [PK_carga_combustible] PRIMARY KEY ([carga_combustible_id]),
    CONSTRAINT [fk_carga_combustible_centro_costo] FOREIGN KEY ([empresa_id], [centro_costo_id]) REFERENCES [centro_costo] ([empresa_id], [centro_costo_id]) ON DELETE NO ACTION,
    CONSTRAINT [fk_carga_combustible_empresa] FOREIGN KEY ([empresa_id]) REFERENCES [empresa] ([empresa_id]) ON DELETE NO ACTION,
    CONSTRAINT [fk_carga_combustible_viaje] FOREIGN KEY ([empresa_id], [viaje_id]) REFERENCES [viaje] ([empresa_id], [viaje_id]) ON DELETE NO ACTION
);

CREATE TABLE [punto_ruta] (
    [punto_ruta_id] binary(16) NOT NULL,
    [empresa_id] binary(16) NOT NULL,
    [viaje_id] binary(16) NOT NULL,
    [latitud] decimal(9,6) NOT NULL,
    [longitud] decimal(9,6) NOT NULL,
    [precision_metros] decimal(6,2) NULL,
    [altitud] decimal(7,2) NULL,
    [velocidad] decimal(6,2) NULL,
    [timestamp_captura] datetime2(0) NOT NULL,
    [fecha_recepcion] datetime2(0) NULL DEFAULT (SYSDATETIME()),
    CONSTRAINT [PK_punto_ruta] PRIMARY KEY ([punto_ruta_id]),
    CONSTRAINT [fk_punto_ruta_empresa] FOREIGN KEY ([empresa_id]) REFERENCES [empresa] ([empresa_id]) ON DELETE NO ACTION,
    CONSTRAINT [fk_punto_ruta_viaje] FOREIGN KEY ([empresa_id], [viaje_id]) REFERENCES [viaje] ([empresa_id], [viaje_id]) ON DELETE CASCADE
);

CREATE TABLE [revision_auditoria] (
    [revision_auditoria_id] binary(16) NOT NULL,
    [empresa_id] binary(16) NOT NULL,
    [viaje_id] binary(16) NULL,
    [carro_id] binary(16) NULL,
    [periodo_desde] date NULL,
    [periodo_hasta] date NULL,
    [empleado_revisor_id] binary(16) NOT NULL,
    [fecha_revision] datetime2(0) NULL DEFAULT (SYSDATETIME()),
    [estado_revision] varchar(21) NOT NULL DEFAULT ('pendiente_informacion'),
    [observaciones] nvarchar(1000) NULL,
    [fecha_cierre] datetime2(0) NULL,
    CONSTRAINT [PK_revision_auditoria] PRIMARY KEY ([revision_auditoria_id]),
    CONSTRAINT [ck_revision_auditoria_alcance] CHECK ((viaje_id IS NOT NULL) OR (carro_id IS NOT NULL AND periodo_desde IS NOT NULL AND periodo_hasta IS NOT NULL)),
    CONSTRAINT [ck_revision_auditoria_estado_revision] CHECK ([estado_revision] IN ('pendiente_informacion', 'revisado', 'con_observaciones')),
    CONSTRAINT [fk_revision_auditoria_carro] FOREIGN KEY ([empresa_id], [carro_id]) REFERENCES [carro] ([empresa_id], [carro_id]) ON DELETE NO ACTION,
    CONSTRAINT [fk_revision_auditoria_empleado_revisor] FOREIGN KEY ([empresa_id], [empleado_revisor_id]) REFERENCES [empleado] ([empresa_id], [empleado_id]) ON DELETE NO ACTION,
    CONSTRAINT [fk_revision_auditoria_empresa] FOREIGN KEY ([empresa_id]) REFERENCES [empresa] ([empresa_id]) ON DELETE NO ACTION,
    CONSTRAINT [fk_revision_auditoria_viaje] FOREIGN KEY ([empresa_id], [viaje_id]) REFERENCES [viaje] ([empresa_id], [viaje_id]) ON DELETE NO ACTION
);

CREATE TABLE [ruta_procesada] (
    [ruta_procesada_id] binary(16) NOT NULL,
    [empresa_id] binary(16) NOT NULL,
    [viaje_id] binary(16) NOT NULL,
    [puntos_recibidos] int NOT NULL DEFAULT 0,
    [puntos_descartados] int NOT NULL DEFAULT 0,
    [distancia_gps_km] decimal(10,3) NOT NULL DEFAULT 0.0,
    [tiempo_movimiento_seg] int NOT NULL DEFAULT 0,
    [tiempo_detenido_seg] int NOT NULL DEFAULT 0,
    [velocidad_promedio_kmh] decimal(6,2) NULL,
    [velocidad_maxima_kmh] decimal(6,2) NULL,
    [distancia_odometro_km] decimal(10,2) NULL,
    [diferencia_km] decimal(10,2) NULL,
    [diferencia_porcentaje] decimal(7,2) NULL,
    [alerta_kilometraje] bit NOT NULL DEFAULT CAST(0 AS bit),
    [litros_cargados] decimal(8,2) NOT NULL DEFAULT 0.0,
    [rendimiento_km_por_litro] decimal(6,2) NULL,
    [polyline] nvarchar(max) NULL,
    [version_algoritmo] nvarchar(20) NOT NULL,
    [fecha_proceso] datetime2(0) NOT NULL DEFAULT (SYSDATETIME()),
    CONSTRAINT [PK_ruta_procesada] PRIMARY KEY ([ruta_procesada_id]),
    CONSTRAINT [fk_ruta_procesada_empresa] FOREIGN KEY ([empresa_id]) REFERENCES [empresa] ([empresa_id]) ON DELETE NO ACTION,
    CONSTRAINT [fk_ruta_procesada_viaje] FOREIGN KEY ([empresa_id], [viaje_id]) REFERENCES [viaje] ([empresa_id], [viaje_id]) ON DELETE CASCADE
);

CREATE TABLE [viaje_pausa] (
    [viaje_pausa_id] binary(16) NOT NULL,
    [empresa_id] binary(16) NOT NULL,
    [viaje_id] binary(16) NOT NULL,
    [fecha_inicio] datetime2(0) NOT NULL,
    [fecha_fin] datetime2(0) NULL,
    [motivo] nvarchar(255) NULL,
    [ubicacion_lat] decimal(9,6) NULL,
    [ubicacion_lng] decimal(9,6) NULL,
    CONSTRAINT [PK_viaje_pausa] PRIMARY KEY ([viaje_pausa_id]),
    CONSTRAINT [fk_viaje_pausa_empresa] FOREIGN KEY ([empresa_id]) REFERENCES [empresa] ([empresa_id]) ON DELETE NO ACTION,
    CONSTRAINT [fk_viaje_pausa_viaje] FOREIGN KEY ([empresa_id], [viaje_id]) REFERENCES [viaje] ([empresa_id], [viaje_id]) ON DELETE CASCADE
);

CREATE INDEX [ix_auditoria_evento_empresa_id_entidad_entidad_id] ON [auditoria_evento] ([empresa_id], [entidad], [entidad_id]);

CREATE INDEX [ix_auditoria_evento_empresa_id_fecha_hora] ON [auditoria_evento] ([empresa_id], [fecha_hora]);

CREATE INDEX [ix_auditoria_evento_usuario_id_fecha_hora] ON [auditoria_evento] ([usuario_id], [fecha_hora]);

CREATE INDEX [ix_cambio_estatus_carro_empresa_id] ON [cambio_estatus_carro] ([empresa_id]);

CREATE INDEX [ix_cambio_estatus_carro_empresa_id_carro_id] ON [cambio_estatus_carro] ([empresa_id], [carro_id]);

CREATE INDEX [ix_cambio_estatus_carro_empresa_id_empleado_id] ON [cambio_estatus_carro] ([empresa_id], [empleado_id]);

CREATE INDEX [ix_cambio_estatus_carro_empresa_id_tipo_motivo] ON [cambio_estatus_carro] ([empresa_id], [tipo_motivo]);

CREATE INDEX [ix_cambio_estatus_carro_empresa_id_viaje_afectado_id] ON [cambio_estatus_carro] ([empresa_id], [viaje_afectado_id]);

CREATE UNIQUE INDEX [uq_cambio_estatus_carro_empresa_id_cambio_estatus_carro_id] ON [cambio_estatus_carro] ([empresa_id], [cambio_estatus_carro_id]);

CREATE INDEX [ix_carga_combustible_empresa_id] ON [carga_combustible] ([empresa_id]);

CREATE INDEX [ix_carga_combustible_empresa_id_centro_costo_id] ON [carga_combustible] ([empresa_id], [centro_costo_id]);

CREATE INDEX [ix_carga_combustible_empresa_id_viaje_id] ON [carga_combustible] ([empresa_id], [viaje_id]);

CREATE UNIQUE INDEX [uq_carga_combustible_empresa_id_carga_combustible_id] ON [carga_combustible] ([empresa_id], [carga_combustible_id]);

CREATE INDEX [ix_carro_empresa_id] ON [carro] ([empresa_id]);

CREATE UNIQUE INDEX [uq_carro_placas] ON [carro] ([placas]);

CREATE INDEX [ix_centro_costo_empresa_id] ON [centro_costo] ([empresa_id]);

CREATE UNIQUE INDEX [uq_centro_costo_empresa_id_clave] ON [centro_costo] ([empresa_id], [clave]);

CREATE INDEX [ix_dispositivo_empleado_empleado_id_activo] ON [dispositivo_empleado] ([empleado_id], [activo]);

CREATE INDEX [ix_dispositivo_empleado_empresa_id] ON [dispositivo_empleado] ([empresa_id]);

CREATE INDEX [ix_dispositivo_empleado_empresa_id_empleado_id] ON [dispositivo_empleado] ([empresa_id], [empleado_id]);

CREATE UNIQUE INDEX [uq_dispositivo_empleado_empresa_id_dispositivo_empleado_id] ON [dispositivo_empleado] ([empresa_id], [dispositivo_empleado_id]);

CREATE UNIQUE INDEX [uq_dispositivo_empleado_token_fcm] ON [dispositivo_empleado] ([token_fcm]);

CREATE INDEX [ix_division_corporativo_id] ON [division] ([corporativo_id]);

CREATE INDEX [ix_empleado_empresa_id] ON [empleado] ([empresa_id]);

CREATE UNIQUE INDEX [uq_empleado_usuario_id] ON [empleado] ([usuario_id]);

CREATE INDEX [ix_empresa_corporativo_id] ON [empresa] ([corporativo_id]);

CREATE INDEX [ix_empresa_corporativo_id_division_id] ON [empresa] ([corporativo_id], [division_id]);

CREATE INDEX [ix_empresa_division_id] ON [empresa] ([division_id]);

CREATE UNIQUE INDEX [uq_empresa_corporativo_id_empresa_id] ON [empresa] ([corporativo_id], [empresa_id]) WHERE [corporativo_id] IS NOT NULL;

CREATE INDEX [ix_notificacion_empleado_destinatario_id_leida] ON [notificacion] ([empleado_destinatario_id], [leida]);

CREATE INDEX [ix_notificacion_empresa_id] ON [notificacion] ([empresa_id]);

CREATE INDEX [ix_notificacion_empresa_id_empleado_destinatario_id] ON [notificacion] ([empresa_id], [empleado_destinatario_id]);

CREATE UNIQUE INDEX [uq_notificacion_empresa_id_notificacion_id] ON [notificacion] ([empresa_id], [notificacion_id]);

CREATE INDEX [ix_outbox_mensaje_agregado_id] ON [outbox_mensaje] ([agregado_id]);

CREATE INDEX [ix_outbox_mensaje_empresa_id] ON [outbox_mensaje] ([empresa_id]);

CREATE INDEX [ix_outbox_mensaje_estado_proximo_intento] ON [outbox_mensaje] ([estado], [proximo_intento]);

CREATE UNIQUE INDEX [uq_outbox_mensaje_empresa_id_outbox_mensaje_id] ON [outbox_mensaje] ([empresa_id], [outbox_mensaje_id]);

CREATE INDEX [ix_privilegio_modulo] ON [privilegio] ([modulo]);

CREATE UNIQUE INDEX [uq_privilegio_clave] ON [privilegio] ([clave]);

CREATE INDEX [ix_punto_ruta_empresa_id] ON [punto_ruta] ([empresa_id]);

CREATE INDEX [ix_punto_ruta_empresa_id_viaje_id] ON [punto_ruta] ([empresa_id], [viaje_id]);

CREATE INDEX [ix_punto_ruta_viaje_id_timestamp_captura] ON [punto_ruta] ([viaje_id], [timestamp_captura]);

CREATE UNIQUE INDEX [uq_punto_ruta_empresa_id_punto_ruta_id] ON [punto_ruta] ([empresa_id], [punto_ruta_id]);

CREATE INDEX [ix_revision_auditoria_empresa_id] ON [revision_auditoria] ([empresa_id]);

CREATE INDEX [ix_revision_auditoria_empresa_id_carro_id_periodo_desde] ON [revision_auditoria] ([empresa_id], [carro_id], [periodo_desde]);

CREATE INDEX [ix_revision_auditoria_empresa_id_empleado_revisor_id] ON [revision_auditoria] ([empresa_id], [empleado_revisor_id]);

CREATE INDEX [ix_revision_auditoria_empresa_id_estado_revision] ON [revision_auditoria] ([empresa_id], [estado_revision]);

CREATE INDEX [ix_revision_auditoria_empresa_id_viaje_id] ON [revision_auditoria] ([empresa_id], [viaje_id]);

CREATE UNIQUE INDEX [uq_revision_auditoria_empresa_id_revision_auditoria_id] ON [revision_auditoria] ([empresa_id], [revision_auditoria_id]);

CREATE INDEX [ix_rol_empresa_id] ON [rol] ([empresa_id]);

CREATE UNIQUE INDEX [uq_rol_empresa_id_nombre] ON [rol] ([empresa_id], [nombre]);

CREATE INDEX [ix_rol_privilegio_privilegio_id] ON [rol_privilegio] ([privilegio_id]);

CREATE INDEX [ix_rol_privilegio_rol_id] ON [rol_privilegio] ([rol_id]);

CREATE UNIQUE INDEX [uq_rol_privilegio_rol_id_privilegio_id] ON [rol_privilegio] ([rol_id], [privilegio_id]);

CREATE INDEX [ix_ruta_procesada_empresa_id] ON [ruta_procesada] ([empresa_id]);

CREATE INDEX [ix_ruta_procesada_empresa_id_alerta_kilometraje] ON [ruta_procesada] ([empresa_id], [alerta_kilometraje]);

CREATE INDEX [ix_ruta_procesada_empresa_id_viaje_id] ON [ruta_procesada] ([empresa_id], [viaje_id]);

CREATE UNIQUE INDEX [uq_ruta_procesada_empresa_id_ruta_procesada_id] ON [ruta_procesada] ([empresa_id], [ruta_procesada_id]);

CREATE UNIQUE INDEX [uq_ruta_procesada_viaje_id] ON [ruta_procesada] ([viaje_id]);

CREATE INDEX [ix_sesion_usuario_empresa_id_fecha_revocacion] ON [sesion_usuario] ([empresa_id], [fecha_revocacion]);

CREATE INDEX [ix_sesion_usuario_expira_en] ON [sesion_usuario] ([expira_en]);

CREATE INDEX [ix_sesion_usuario_token_hash_anterior] ON [sesion_usuario] ([token_hash_anterior]);

CREATE INDEX [ix_sesion_usuario_usuario_id_fecha_revocacion_expira_en] ON [sesion_usuario] ([usuario_id], [fecha_revocacion], [expira_en]);

CREATE UNIQUE INDEX [uq_sesion_usuario_token_hash] ON [sesion_usuario] ([token_hash]);

CREATE INDEX [ix_solicitud_reserva_empresa_id] ON [solicitud_reserva] ([empresa_id]);

CREATE INDEX [ix_solicitud_reserva_empresa_id_carro_id] ON [solicitud_reserva] ([empresa_id], [carro_id]);

CREATE INDEX [ix_solicitud_reserva_empresa_id_carro_id_estado] ON [solicitud_reserva] ([empresa_id], [carro_id], [estado]);

CREATE INDEX [ix_solicitud_reserva_empresa_id_empleado_confirmador_id] ON [solicitud_reserva] ([empresa_id], [empleado_confirmador_id]);

CREATE INDEX [ix_solicitud_reserva_empresa_id_empleado_solicitante_id] ON [solicitud_reserva] ([empresa_id], [empleado_solicitante_id]);

CREATE UNIQUE INDEX [uq_usuario_email] ON [usuario] ([email]);

CREATE INDEX [ix_usuario_rol_rol_id] ON [usuario_rol] ([rol_id]);

CREATE INDEX [ix_usuario_rol_usuario_id] ON [usuario_rol] ([usuario_id]);

CREATE UNIQUE INDEX [uq_usuario_rol_usuario_id_rol_id] ON [usuario_rol] ([usuario_id], [rol_id]);

CREATE INDEX [ix_viaje_empresa_id] ON [viaje] ([empresa_id]);

CREATE INDEX [ix_viaje_empresa_id_carro_id] ON [viaje] ([empresa_id], [carro_id]);

CREATE INDEX [ix_viaje_empresa_id_carro_id_fecha_inicio] ON [viaje] ([empresa_id], [carro_id], [fecha_inicio]);

CREATE INDEX [ix_viaje_empresa_id_centro_costo_id] ON [viaje] ([empresa_id], [centro_costo_id]);

CREATE INDEX [ix_viaje_empresa_id_empleado_operador_id] ON [viaje] ([empresa_id], [empleado_operador_id]);

CREATE INDEX [ix_viaje_empresa_id_solicitud_reserva_id] ON [viaje] ([empresa_id], [solicitud_reserva_id]);

CREATE INDEX [ix_viaje_pausa_empresa_id] ON [viaje_pausa] ([empresa_id]);

CREATE INDEX [ix_viaje_pausa_empresa_id_viaje_id] ON [viaje_pausa] ([empresa_id], [viaje_id]);

CREATE INDEX [ix_viaje_pausa_viaje_id_fecha_inicio] ON [viaje_pausa] ([viaje_id], [fecha_inicio]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20261111182911_InicialSqlServer', N'10.0.12');

COMMIT;
GO

BEGIN TRANSACTION;
ALTER TABLE [usuario] ADD [empresa_id] binary(16) NULL;

CREATE INDEX [ix_usuario_empresa_id] ON [usuario] ([empresa_id]);

ALTER TABLE [usuario] ADD CONSTRAINT [fk_usuario_empresa] FOREIGN KEY ([empresa_id]) REFERENCES [empresa] ([empresa_id]) ON DELETE NO ACTION;

EXEC(N'
UPDATE u SET u.empresa_id = e.empresa_id
FROM usuario AS u
JOIN empleado AS e ON e.usuario_id = u.usuario_id;
');

EXEC(N'
UPDATE u SET u.empresa_id = r.empresa_id
FROM usuario AS u
CROSS APPLY (
    SELECT MIN(rol.empresa_id) AS empresa_id
    FROM usuario_rol AS ur
    JOIN rol ON rol.rol_id = ur.rol_id
    WHERE ur.usuario_id = u.usuario_id AND rol.empresa_id IS NOT NULL
    HAVING COUNT(DISTINCT rol.empresa_id) = 1
) AS r
WHERE u.empresa_id IS NULL;
');

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20261111211442_AgregarEmpresaAUsuario', N'10.0.12');

COMMIT;
GO

