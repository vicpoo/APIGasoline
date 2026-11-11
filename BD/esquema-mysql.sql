-- ============================================================
--  APIGasoline — Esquema de la base de datos (MySQL 8)
--  Extraído de la API: se aplicaron las 9 migraciones de EF Core
--  (APIGasoline.Infrastructure/Migrations, última: 20261106120000_SesionesUsuario)
--  sobre un MySQL 8.0 vacío y se exportó el resultado con mysqldump --no-data.
--
--  24 tablas · 46 llaves foráneas · 1 CHECK · 1 columna generada
--
--  OJO: la forma oficial de crear la BD sigue siendo con las migraciones
--  (BaseDatos:MigrarAlIniciar = true, o `dotnet ef database update`).
--  Este archivo es una "foto" para leer el modelo, revisarlo o cargarlo a mano.
--  No crea __EFMigrationsHistory: si lo usas, la API va a intentar migrar otra vez.
-- ============================================================

CREATE DATABASE IF NOT EXISTS `apigasoline`
  DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
USE `apigasoline`;

SET FOREIGN_KEY_CHECKS = 0;


CREATE TABLE `auditoria_evento` (
  `auditoria_evento_id` binary(16) NOT NULL,
  `empresa_id` binary(16) DEFAULT NULL,
  `usuario_id` binary(16) DEFAULT NULL,
  `accion` enum('crear','actualizar','eliminar','aprobar','rechazar','cancelar','cambiar_estado','corregir') NOT NULL,
  `entidad` varchar(60) NOT NULL,
  `entidad_id` binary(16) NOT NULL,
  `valores_anteriores` json DEFAULT NULL,
  `valores_nuevos` json DEFAULT NULL,
  `motivo` varchar(500) DEFAULT NULL,
  `origen` enum('movil','web','sistema') NOT NULL,
  `fecha_hora` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`auditoria_evento_id`),
  KEY `ix_auditoria_evento_empresa_id_entidad_entidad_id` (`empresa_id`,`entidad`,`entidad_id`),
  KEY `ix_auditoria_evento_empresa_id_fecha_hora` (`empresa_id`,`fecha_hora`),
  KEY `ix_auditoria_evento_usuario_id_fecha_hora` (`usuario_id`,`fecha_hora`),
  CONSTRAINT `fk_auditoria_evento_empresa` FOREIGN KEY (`empresa_id`) REFERENCES `empresa` (`empresa_id`) ON DELETE RESTRICT,
  CONSTRAINT `fk_auditoria_evento_usuario` FOREIGN KEY (`usuario_id`) REFERENCES `usuario` (`usuario_id`) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE `cambio_estatus_carro` (
  `cambio_estatus_carro_id` binary(16) NOT NULL,
  `empresa_id` binary(16) NOT NULL,
  `carro_id` binary(16) NOT NULL,
  `empleado_id` binary(16) NOT NULL,
  `viaje_afectado_id` binary(16) DEFAULT NULL,
  `estatus_anterior` enum('disponible','ocupado','en_mantenimiento') NOT NULL,
  `estatus_nuevo` enum('disponible','ocupado','en_mantenimiento') NOT NULL,
  `motivo` varchar(255) DEFAULT NULL,
  `fecha_cambio` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  `fecha_recepcion` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  `foto_evidencia` varchar(255) DEFAULT NULL,
  `tipo_motivo` enum('liberacion','falla_mecanica','choque_o_golpe','servicio','emergencia','otro') DEFAULT NULL,
  PRIMARY KEY (`cambio_estatus_carro_id`),
  UNIQUE KEY `uq_cambio_estatus_carro_empresa_id_cambio_estatus_carro_id` (`empresa_id`,`cambio_estatus_carro_id`),
  KEY `ix_cambio_estatus_carro_empresa_id` (`empresa_id`),
  KEY `ix_cambio_estatus_carro_empresa_id_carro_id` (`empresa_id`,`carro_id`),
  KEY `ix_cambio_estatus_carro_empresa_id_empleado_id` (`empresa_id`,`empleado_id`),
  KEY `ix_cambio_estatus_carro_empresa_id_viaje_afectado_id` (`empresa_id`,`viaje_afectado_id`),
  KEY `ix_cambio_estatus_carro_empresa_id_tipo_motivo` (`empresa_id`,`tipo_motivo`),
  CONSTRAINT `fk_cambio_estatus_carro_carro` FOREIGN KEY (`empresa_id`, `carro_id`) REFERENCES `carro` (`empresa_id`, `carro_id`) ON DELETE RESTRICT,
  CONSTRAINT `fk_cambio_estatus_carro_empleado` FOREIGN KEY (`empresa_id`, `empleado_id`) REFERENCES `empleado` (`empresa_id`, `empleado_id`) ON DELETE RESTRICT,
  CONSTRAINT `fk_cambio_estatus_carro_empresa` FOREIGN KEY (`empresa_id`) REFERENCES `empresa` (`empresa_id`) ON DELETE RESTRICT,
  CONSTRAINT `fk_cambio_estatus_carro_viaje` FOREIGN KEY (`empresa_id`, `viaje_afectado_id`) REFERENCES `viaje` (`empresa_id`, `viaje_id`) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE `carga_combustible` (
  `carga_combustible_id` binary(16) NOT NULL,
  `empresa_id` binary(16) NOT NULL,
  `viaje_id` binary(16) NOT NULL,
  `litros` decimal(8,2) NOT NULL,
  `monto` decimal(10,2) DEFAULT NULL,
  `foto_comprobante` varchar(255) NOT NULL,
  `fecha_carga` timestamp NOT NULL,
  `ubicacion_lat` decimal(9,6) DEFAULT NULL,
  `ubicacion_lng` decimal(9,6) DEFAULT NULL,
  `corregido` tinyint(1) NOT NULL DEFAULT '0',
  `fecha_recepcion` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  `fecha_subida_comprobante` timestamp NULL DEFAULT NULL,
  `centro_costo_id` binary(16) DEFAULT NULL,
  PRIMARY KEY (`carga_combustible_id`),
  UNIQUE KEY `uq_carga_combustible_empresa_id_carga_combustible_id` (`empresa_id`,`carga_combustible_id`),
  KEY `ix_carga_combustible_empresa_id` (`empresa_id`),
  KEY `ix_carga_combustible_empresa_id_viaje_id` (`empresa_id`,`viaje_id`),
  KEY `ix_carga_combustible_empresa_id_centro_costo_id` (`empresa_id`,`centro_costo_id`),
  CONSTRAINT `fk_carga_combustible_centro_costo` FOREIGN KEY (`empresa_id`, `centro_costo_id`) REFERENCES `centro_costo` (`empresa_id`, `centro_costo_id`) ON DELETE RESTRICT,
  CONSTRAINT `fk_carga_combustible_empresa` FOREIGN KEY (`empresa_id`) REFERENCES `empresa` (`empresa_id`) ON DELETE RESTRICT,
  CONSTRAINT `fk_carga_combustible_viaje` FOREIGN KEY (`empresa_id`, `viaje_id`) REFERENCES `viaje` (`empresa_id`, `viaje_id`) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE `carro` (
  `carro_id` binary(16) NOT NULL,
  `empresa_id` binary(16) NOT NULL,
  `marca` varchar(80) NOT NULL,
  `modelo` varchar(80) NOT NULL,
  `anio` int DEFAULT NULL,
  `color` varchar(50) DEFAULT NULL,
  `placas` varchar(20) NOT NULL,
  `foto_vehiculo` varchar(255) DEFAULT NULL,
  `capacidad_tanque` decimal(6,2) DEFAULT NULL,
  `kilometraje_actual` decimal(10,2) NOT NULL DEFAULT '0.00',
  `estado` enum('disponible','ocupado','en_mantenimiento') NOT NULL DEFAULT 'disponible',
  `fecha_alta` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  `dado_de_baja` tinyint(1) NOT NULL DEFAULT '0',
  `fecha_baja` timestamp NULL DEFAULT NULL,
  `motivo_baja` varchar(255) DEFAULT NULL,
  `tipo_vehiculo` enum('carro','moto','camioneta','camion') NOT NULL DEFAULT 'carro',
  PRIMARY KEY (`carro_id`),
  UNIQUE KEY `uq_carro_empresa_id_carro_id` (`empresa_id`,`carro_id`),
  UNIQUE KEY `uq_carro_placas` (`placas`),
  KEY `ix_carro_empresa_id` (`empresa_id`),
  CONSTRAINT `fk_carro_empresa` FOREIGN KEY (`empresa_id`) REFERENCES `empresa` (`empresa_id`) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE `centro_costo` (
  `centro_costo_id` binary(16) NOT NULL,
  `empresa_id` binary(16) NOT NULL,
  `nombre` varchar(150) NOT NULL,
  `clave` varchar(100) NOT NULL,
  `activo` tinyint(1) NOT NULL DEFAULT '1',
  `fecha_registro` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`centro_costo_id`),
  UNIQUE KEY `uq_centro_costo_empresa_id_clave` (`empresa_id`,`clave`),
  UNIQUE KEY `uq_centro_costo_empresa_id_centro_costo_id` (`empresa_id`,`centro_costo_id`),
  KEY `ix_centro_costo_empresa_id` (`empresa_id`),
  CONSTRAINT `fk_centro_costo_empresa` FOREIGN KEY (`empresa_id`) REFERENCES `empresa` (`empresa_id`) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE `corporativo` (
  `corporativo_id` binary(16) NOT NULL,
  `nombre` varchar(150) NOT NULL,
  `activo` tinyint(1) NOT NULL DEFAULT '1',
  `fecha_registro` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`corporativo_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE `dispositivo_empleado` (
  `dispositivo_empleado_id` binary(16) NOT NULL,
  `empresa_id` binary(16) NOT NULL,
  `empleado_id` binary(16) NOT NULL,
  `token_fcm` varchar(512) NOT NULL,
  `plataforma` enum('android','ios','web') NOT NULL,
  `activo` tinyint(1) NOT NULL DEFAULT '1',
  `fecha_registro` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `fecha_ultimo_uso` timestamp NULL DEFAULT NULL,
  PRIMARY KEY (`dispositivo_empleado_id`),
  UNIQUE KEY `uq_dispositivo_empleado_empresa_id_dispositivo_empleado_id` (`empresa_id`,`dispositivo_empleado_id`),
  UNIQUE KEY `uq_dispositivo_empleado_token_fcm` (`token_fcm`),
  KEY `ix_dispositivo_empleado_empleado_id_activo` (`empleado_id`,`activo`),
  KEY `ix_dispositivo_empleado_empresa_id` (`empresa_id`),
  KEY `ix_dispositivo_empleado_empresa_id_empleado_id` (`empresa_id`,`empleado_id`),
  CONSTRAINT `fk_dispositivo_empleado_empleado` FOREIGN KEY (`empresa_id`, `empleado_id`) REFERENCES `empleado` (`empresa_id`, `empleado_id`) ON DELETE CASCADE,
  CONSTRAINT `fk_dispositivo_empleado_empresa` FOREIGN KEY (`empresa_id`) REFERENCES `empresa` (`empresa_id`) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE `division` (
  `division_id` binary(16) NOT NULL,
  `corporativo_id` binary(16) NOT NULL,
  `nombre` varchar(100) NOT NULL,
  `activo` tinyint(1) NOT NULL DEFAULT '1',
  `fecha_registro` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`division_id`),
  UNIQUE KEY `uq_division_corporativo_id_division_id` (`corporativo_id`,`division_id`),
  KEY `ix_division_corporativo_id` (`corporativo_id`),
  CONSTRAINT `fk_division_corporativo` FOREIGN KEY (`corporativo_id`) REFERENCES `corporativo` (`corporativo_id`) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE `empleado` (
  `empleado_id` binary(16) NOT NULL,
  `empresa_id` binary(16) NOT NULL,
  `usuario_id` binary(16) NOT NULL,
  `nombre_completo` varchar(150) NOT NULL,
  `foto_perfil` varchar(255) DEFAULT NULL,
  `foto_licencia` varchar(255) DEFAULT NULL,
  `eliminado` tinyint(1) NOT NULL DEFAULT '0',
  `fecha_alta` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  `fecha_carga_licencia` timestamp NULL DEFAULT NULL,
  PRIMARY KEY (`empleado_id`),
  UNIQUE KEY `uq_empleado_empresa_id_empleado_id` (`empresa_id`,`empleado_id`),
  UNIQUE KEY `uq_empleado_usuario_id` (`usuario_id`),
  KEY `ix_empleado_empresa_id` (`empresa_id`),
  CONSTRAINT `fk_empleado_empresa` FOREIGN KEY (`empresa_id`) REFERENCES `empresa` (`empresa_id`) ON DELETE RESTRICT,
  CONSTRAINT `fk_empleado_usuario` FOREIGN KEY (`usuario_id`) REFERENCES `usuario` (`usuario_id`) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE `empresa` (
  `empresa_id` binary(16) NOT NULL,
  `corporativo_id` binary(16) DEFAULT NULL,
  `division_id` binary(16) DEFAULT NULL,
  `nombre` varchar(150) NOT NULL,
  `eliminado` tinyint(1) NOT NULL DEFAULT '0',
  `fecha_registro` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  `activo` tinyint(1) NOT NULL DEFAULT '1',
  PRIMARY KEY (`empresa_id`),
  UNIQUE KEY `uq_empresa_corporativo_id_empresa_id` (`corporativo_id`,`empresa_id`),
  KEY `ix_empresa_corporativo_id` (`corporativo_id`),
  KEY `ix_empresa_corporativo_id_division_id` (`corporativo_id`,`division_id`),
  KEY `ix_empresa_division_id` (`division_id`),
  CONSTRAINT `fk_empresa_corporativo` FOREIGN KEY (`corporativo_id`) REFERENCES `corporativo` (`corporativo_id`) ON DELETE RESTRICT,
  CONSTRAINT `fk_empresa_division` FOREIGN KEY (`corporativo_id`, `division_id`) REFERENCES `division` (`corporativo_id`, `division_id`) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE `notificacion` (
  `notificacion_id` binary(16) NOT NULL,
  `empresa_id` binary(16) NOT NULL,
  `empleado_destinatario_id` binary(16) NOT NULL,
  `tipo` enum('nueva_solicitud','resultado_solicitud','cambio_estatus_forzado','reporte_mantenimiento') NOT NULL,
  `referencia_id` binary(16) DEFAULT NULL,
  `mensaje` varchar(255) DEFAULT NULL,
  `leida` tinyint(1) NOT NULL DEFAULT '0',
  `fecha_envio` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`notificacion_id`),
  UNIQUE KEY `uq_notificacion_empresa_id_notificacion_id` (`empresa_id`,`notificacion_id`),
  KEY `ix_notificacion_empleado_destinatario_id_leida` (`empleado_destinatario_id`,`leida`),
  KEY `ix_notificacion_empresa_id` (`empresa_id`),
  KEY `ix_notificacion_empresa_id_empleado_destinatario_id` (`empresa_id`,`empleado_destinatario_id`),
  CONSTRAINT `fk_notificacion_empleado_destinatario` FOREIGN KEY (`empresa_id`, `empleado_destinatario_id`) REFERENCES `empleado` (`empresa_id`, `empleado_id`) ON DELETE RESTRICT,
  CONSTRAINT `fk_notificacion_empresa` FOREIGN KEY (`empresa_id`) REFERENCES `empresa` (`empresa_id`) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE `outbox_mensaje` (
  `outbox_mensaje_id` binary(16) NOT NULL,
  `empresa_id` binary(16) NOT NULL,
  `tipo` varchar(100) NOT NULL,
  `agregado_id` binary(16) NOT NULL,
  `payload` json NOT NULL,
  `estado` enum('pendiente','procesando','enviado','fallido') NOT NULL DEFAULT 'pendiente',
  `intentos` int NOT NULL DEFAULT '0',
  `proximo_intento` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ultimo_error` varchar(500) DEFAULT NULL,
  `fecha_creacion` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `fecha_procesado` timestamp NULL DEFAULT NULL,
  PRIMARY KEY (`outbox_mensaje_id`),
  UNIQUE KEY `uq_outbox_mensaje_empresa_id_outbox_mensaje_id` (`empresa_id`,`outbox_mensaje_id`),
  KEY `ix_outbox_mensaje_agregado_id` (`agregado_id`),
  KEY `ix_outbox_mensaje_empresa_id` (`empresa_id`),
  KEY `ix_outbox_mensaje_estado_proximo_intento` (`estado`,`proximo_intento`),
  CONSTRAINT `fk_outbox_mensaje_empresa` FOREIGN KEY (`empresa_id`) REFERENCES `empresa` (`empresa_id`) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE `privilegio` (
  `privilegio_id` binary(16) NOT NULL,
  `nombre` varchar(100) NOT NULL,
  `descripcion` varchar(255) DEFAULT NULL,
  `activo` tinyint(1) NOT NULL DEFAULT '1',
  `clave` varchar(100) NOT NULL DEFAULT '',
  `modulo` varchar(50) NOT NULL DEFAULT '',
  `ambito` enum('plataforma','empresa') NOT NULL DEFAULT 'empresa',
  PRIMARY KEY (`privilegio_id`),
  UNIQUE KEY `uq_privilegio_clave` (`clave`),
  KEY `ix_privilegio_modulo` (`modulo`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE `punto_ruta` (
  `punto_ruta_id` binary(16) NOT NULL,
  `empresa_id` binary(16) NOT NULL,
  `viaje_id` binary(16) NOT NULL,
  `latitud` decimal(9,6) NOT NULL,
  `longitud` decimal(9,6) NOT NULL,
  `precision_metros` decimal(6,2) DEFAULT NULL,
  `altitud` decimal(7,2) DEFAULT NULL,
  `velocidad` decimal(6,2) DEFAULT NULL,
  `timestamp_captura` timestamp NOT NULL,
  `fecha_recepcion` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`punto_ruta_id`),
  UNIQUE KEY `uq_punto_ruta_empresa_id_punto_ruta_id` (`empresa_id`,`punto_ruta_id`),
  KEY `ix_punto_ruta_empresa_id` (`empresa_id`),
  KEY `ix_punto_ruta_empresa_id_viaje_id` (`empresa_id`,`viaje_id`),
  KEY `ix_punto_ruta_viaje_id_timestamp_captura` (`viaje_id`,`timestamp_captura`),
  CONSTRAINT `fk_punto_ruta_empresa` FOREIGN KEY (`empresa_id`) REFERENCES `empresa` (`empresa_id`) ON DELETE RESTRICT,
  CONSTRAINT `fk_punto_ruta_viaje` FOREIGN KEY (`empresa_id`, `viaje_id`) REFERENCES `viaje` (`empresa_id`, `viaje_id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE `revision_auditoria` (
  `revision_auditoria_id` binary(16) NOT NULL,
  `empresa_id` binary(16) NOT NULL,
  `viaje_id` binary(16) DEFAULT NULL,
  `carro_id` binary(16) DEFAULT NULL,
  `periodo_desde` date DEFAULT NULL,
  `periodo_hasta` date DEFAULT NULL,
  `empleado_revisor_id` binary(16) NOT NULL,
  `fecha_revision` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  `estado_revision` enum('pendiente_informacion','revisado','con_observaciones') NOT NULL DEFAULT 'pendiente_informacion',
  `observaciones` varchar(1000) DEFAULT NULL,
  `fecha_cierre` timestamp NULL DEFAULT NULL,
  PRIMARY KEY (`revision_auditoria_id`),
  UNIQUE KEY `uq_revision_auditoria_empresa_id_revision_auditoria_id` (`empresa_id`,`revision_auditoria_id`),
  KEY `ix_revision_auditoria_empresa_id` (`empresa_id`),
  KEY `ix_revision_auditoria_empresa_id_carro_id_periodo_desde` (`empresa_id`,`carro_id`,`periodo_desde`),
  KEY `ix_revision_auditoria_empresa_id_empleado_revisor_id` (`empresa_id`,`empleado_revisor_id`),
  KEY `ix_revision_auditoria_empresa_id_estado_revision` (`empresa_id`,`estado_revision`),
  KEY `ix_revision_auditoria_empresa_id_viaje_id` (`empresa_id`,`viaje_id`),
  CONSTRAINT `fk_revision_auditoria_carro` FOREIGN KEY (`empresa_id`, `carro_id`) REFERENCES `carro` (`empresa_id`, `carro_id`) ON DELETE RESTRICT,
  CONSTRAINT `fk_revision_auditoria_empleado_revisor` FOREIGN KEY (`empresa_id`, `empleado_revisor_id`) REFERENCES `empleado` (`empresa_id`, `empleado_id`) ON DELETE RESTRICT,
  CONSTRAINT `fk_revision_auditoria_empresa` FOREIGN KEY (`empresa_id`) REFERENCES `empresa` (`empresa_id`) ON DELETE RESTRICT,
  CONSTRAINT `fk_revision_auditoria_viaje` FOREIGN KEY (`empresa_id`, `viaje_id`) REFERENCES `viaje` (`empresa_id`, `viaje_id`) ON DELETE RESTRICT,
  CONSTRAINT `ck_revision_auditoria_alcance` CHECK (((`viaje_id` is not null) or ((`carro_id` is not null) and (`periodo_desde` is not null) and (`periodo_hasta` is not null))))
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE `rol` (
  `rol_id` binary(16) NOT NULL,
  `empresa_id` binary(16) DEFAULT NULL,
  `nombre` varchar(100) NOT NULL,
  `descripcion` varchar(255) DEFAULT NULL,
  `activo` tinyint(1) NOT NULL DEFAULT '1',
  `empresa_clave` binary(16) GENERATED ALWAYS AS (coalesce(`empresa_id`,0x00000000000000000000000000000000)) STORED,
  PRIMARY KEY (`rol_id`),
  UNIQUE KEY `uq_rol_empresa_clave_nombre` (`empresa_clave`,`nombre`),
  KEY `ix_rol_empresa_id` (`empresa_id`),
  CONSTRAINT `fk_rol_empresa` FOREIGN KEY (`empresa_id`) REFERENCES `empresa` (`empresa_id`) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE `rol_privilegio` (
  `rol_privilegio_id` binary(16) NOT NULL,
  `rol_id` binary(16) NOT NULL,
  `privilegio_id` binary(16) NOT NULL,
  PRIMARY KEY (`rol_privilegio_id`),
  UNIQUE KEY `uq_rol_privilegio_rol_id_privilegio_id` (`rol_id`,`privilegio_id`),
  KEY `ix_rol_privilegio_privilegio_id` (`privilegio_id`),
  KEY `ix_rol_privilegio_rol_id` (`rol_id`),
  CONSTRAINT `fk_rol_privilegio_privilegio` FOREIGN KEY (`privilegio_id`) REFERENCES `privilegio` (`privilegio_id`) ON DELETE CASCADE,
  CONSTRAINT `fk_rol_privilegio_rol` FOREIGN KEY (`rol_id`) REFERENCES `rol` (`rol_id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE `ruta_procesada` (
  `ruta_procesada_id` binary(16) NOT NULL,
  `empresa_id` binary(16) NOT NULL,
  `viaje_id` binary(16) NOT NULL,
  `puntos_recibidos` int NOT NULL DEFAULT '0',
  `puntos_descartados` int NOT NULL DEFAULT '0',
  `distancia_gps_km` decimal(10,3) NOT NULL DEFAULT '0.000',
  `tiempo_movimiento_seg` int NOT NULL DEFAULT '0',
  `tiempo_detenido_seg` int NOT NULL DEFAULT '0',
  `velocidad_promedio_kmh` decimal(6,2) DEFAULT NULL,
  `velocidad_maxima_kmh` decimal(6,2) DEFAULT NULL,
  `distancia_odometro_km` decimal(10,2) DEFAULT NULL,
  `diferencia_km` decimal(10,2) DEFAULT NULL,
  `diferencia_porcentaje` decimal(7,2) DEFAULT NULL,
  `alerta_kilometraje` tinyint(1) NOT NULL DEFAULT '0',
  `litros_cargados` decimal(8,2) NOT NULL DEFAULT '0.00',
  `rendimiento_km_por_litro` decimal(6,2) DEFAULT NULL,
  `polyline` text,
  `version_algoritmo` varchar(20) NOT NULL,
  `fecha_proceso` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`ruta_procesada_id`),
  UNIQUE KEY `uq_ruta_procesada_empresa_id_ruta_procesada_id` (`empresa_id`,`ruta_procesada_id`),
  UNIQUE KEY `uq_ruta_procesada_viaje_id` (`viaje_id`),
  KEY `ix_ruta_procesada_empresa_id` (`empresa_id`),
  KEY `ix_ruta_procesada_empresa_id_alerta_kilometraje` (`empresa_id`,`alerta_kilometraje`),
  KEY `ix_ruta_procesada_empresa_id_viaje_id` (`empresa_id`,`viaje_id`),
  CONSTRAINT `fk_ruta_procesada_empresa` FOREIGN KEY (`empresa_id`) REFERENCES `empresa` (`empresa_id`) ON DELETE RESTRICT,
  CONSTRAINT `fk_ruta_procesada_viaje` FOREIGN KEY (`empresa_id`, `viaje_id`) REFERENCES `viaje` (`empresa_id`, `viaje_id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE `sesion_usuario` (
  `sesion_usuario_id` binary(16) NOT NULL,
  `usuario_id` binary(16) NOT NULL,
  `empresa_id` binary(16) DEFAULT NULL,
  `token_hash` char(64) NOT NULL,
  `token_hash_anterior` char(64) DEFAULT NULL,
  `plataforma` enum('android','ios','web') DEFAULT NULL,
  `nombre_dispositivo` varchar(100) DEFAULT NULL,
  `ip` varchar(45) DEFAULT NULL,
  `user_agent` varchar(255) DEFAULT NULL,
  `fecha_creacion` timestamp NOT NULL,
  `fecha_ultimo_uso` timestamp NOT NULL,
  `fecha_rotacion` timestamp NULL DEFAULT NULL,
  `expira_en` timestamp NOT NULL,
  `expira_absoluta_en` timestamp NOT NULL,
  `fecha_revocacion` timestamp NULL DEFAULT NULL,
  `motivo_revocacion` enum('cierre_sesion','cerrada_por_usuario','cerrada_por_administrador','cambio_password','usuario_desactivado','empleado_dado_de_baja','empleado_reasignado','empresa_eliminada','reuso_detectado','acceso_revocado','limite_sesiones') DEFAULT NULL,
  `revocada_por_usuario_id` binary(16) DEFAULT NULL,
  PRIMARY KEY (`sesion_usuario_id`),
  UNIQUE KEY `uq_sesion_usuario_token_hash` (`token_hash`),
  KEY `ix_sesion_usuario_token_hash_anterior` (`token_hash_anterior`),
  KEY `ix_sesion_usuario_usuario_id_fecha_revocacion_expira_en` (`usuario_id`,`fecha_revocacion`,`expira_en`),
  KEY `ix_sesion_usuario_empresa_id_fecha_revocacion` (`empresa_id`,`fecha_revocacion`),
  KEY `ix_sesion_usuario_expira_en` (`expira_en`),
  CONSTRAINT `fk_sesion_usuario_usuario` FOREIGN KEY (`usuario_id`) REFERENCES `usuario` (`usuario_id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE `solicitud_reserva` (
  `solicitud_reserva_id` binary(16) NOT NULL,
  `empresa_id` binary(16) NOT NULL,
  `carro_id` binary(16) NOT NULL,
  `empleado_solicitante_id` binary(16) NOT NULL,
  `fecha_solicitud` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  `estado` enum('pendiente','aprobada','rechazada','cancelada','finalizada') NOT NULL DEFAULT 'pendiente',
  `empleado_confirmador_id` binary(16) DEFAULT NULL,
  `fecha_resolucion` timestamp NULL DEFAULT NULL,
  `motivo_resolucion` varchar(255) DEFAULT NULL,
  `fecha_liberacion` timestamp NULL DEFAULT NULL,
  `observaciones_liberacion` varchar(500) DEFAULT NULL,
  `motivo_solicitud` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`solicitud_reserva_id`),
  UNIQUE KEY `uq_solicitud_reserva_empresa_id_solicitud_reserva_id` (`empresa_id`,`solicitud_reserva_id`),
  KEY `ix_solicitud_reserva_empresa_id` (`empresa_id`),
  KEY `ix_solicitud_reserva_empresa_id_carro_id` (`empresa_id`,`carro_id`),
  KEY `ix_solicitud_reserva_empresa_id_empleado_confirmador_id` (`empresa_id`,`empleado_confirmador_id`),
  KEY `ix_solicitud_reserva_empresa_id_empleado_solicitante_id` (`empresa_id`,`empleado_solicitante_id`),
  KEY `ix_solicitud_reserva_empresa_id_carro_id_estado` (`empresa_id`,`carro_id`,`estado`),
  CONSTRAINT `fk_solicitud_reserva_carro` FOREIGN KEY (`empresa_id`, `carro_id`) REFERENCES `carro` (`empresa_id`, `carro_id`) ON DELETE RESTRICT,
  CONSTRAINT `fk_solicitud_reserva_empleado_confirmador` FOREIGN KEY (`empresa_id`, `empleado_confirmador_id`) REFERENCES `empleado` (`empresa_id`, `empleado_id`) ON DELETE RESTRICT,
  CONSTRAINT `fk_solicitud_reserva_empleado_solicitante` FOREIGN KEY (`empresa_id`, `empleado_solicitante_id`) REFERENCES `empleado` (`empresa_id`, `empleado_id`) ON DELETE RESTRICT,
  CONSTRAINT `fk_solicitud_reserva_empresa` FOREIGN KEY (`empresa_id`) REFERENCES `empresa` (`empresa_id`) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE `usuario` (
  `usuario_id` binary(16) NOT NULL,
  `email` varchar(150) NOT NULL,
  `password_hash` varchar(255) NOT NULL,
  `estado` enum('activo','inactivo') NOT NULL DEFAULT 'activo',
  `fecha_registro` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`usuario_id`),
  UNIQUE KEY `uq_usuario_email` (`email`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE `usuario_rol` (
  `usuario_rol_id` binary(16) NOT NULL,
  `usuario_id` binary(16) NOT NULL,
  `rol_id` binary(16) NOT NULL,
  PRIMARY KEY (`usuario_rol_id`),
  UNIQUE KEY `uq_usuario_rol_usuario_id_rol_id` (`usuario_id`,`rol_id`),
  KEY `ix_usuario_rol_rol_id` (`rol_id`),
  KEY `ix_usuario_rol_usuario_id` (`usuario_id`),
  CONSTRAINT `fk_usuario_rol_rol` FOREIGN KEY (`rol_id`) REFERENCES `rol` (`rol_id`) ON DELETE CASCADE,
  CONSTRAINT `fk_usuario_rol_usuario` FOREIGN KEY (`usuario_id`) REFERENCES `usuario` (`usuario_id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE `viaje` (
  `viaje_id` binary(16) NOT NULL,
  `empresa_id` binary(16) NOT NULL,
  `carro_id` binary(16) NOT NULL,
  `solicitud_reserva_id` binary(16) DEFAULT NULL,
  `empleado_operador_id` binary(16) NOT NULL,
  `fecha_inicio` timestamp NOT NULL,
  `ubicacion_inicio_lat` decimal(9,6) DEFAULT NULL,
  `ubicacion_inicio_lng` decimal(9,6) DEFAULT NULL,
  `kilometraje_inicial` decimal(10,2) NOT NULL,
  `fecha_fin` timestamp NULL DEFAULT NULL,
  `ubicacion_fin_lat` decimal(9,6) DEFAULT NULL,
  `ubicacion_fin_lng` decimal(9,6) DEFAULT NULL,
  `kilometraje_final` decimal(10,2) DEFAULT NULL,
  `distancia_recorrida` decimal(10,2) DEFAULT NULL,
  `estado` enum('en_curso','finalizado','interrumpido') NOT NULL DEFAULT 'en_curso',
  `motivo_interrupcion` varchar(255) DEFAULT NULL,
  `corregido` tinyint(1) NOT NULL DEFAULT '0',
  `fecha_recepcion` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  `foto_tablero_final` varchar(255) DEFAULT NULL,
  `foto_tablero_inicial` varchar(255) DEFAULT NULL,
  `centro_costo_id` binary(16) DEFAULT NULL,
  PRIMARY KEY (`viaje_id`),
  UNIQUE KEY `uq_viaje_empresa_id_viaje_id` (`empresa_id`,`viaje_id`),
  KEY `ix_viaje_empresa_id` (`empresa_id`),
  KEY `ix_viaje_empresa_id_carro_id` (`empresa_id`,`carro_id`),
  KEY `ix_viaje_empresa_id_empleado_operador_id` (`empresa_id`,`empleado_operador_id`),
  KEY `ix_viaje_empresa_id_solicitud_reserva_id` (`empresa_id`,`solicitud_reserva_id`),
  KEY `ix_viaje_empresa_id_carro_id_fecha_inicio` (`empresa_id`,`carro_id`,`fecha_inicio`),
  KEY `ix_viaje_empresa_id_centro_costo_id` (`empresa_id`,`centro_costo_id`),
  CONSTRAINT `fk_viaje_carro` FOREIGN KEY (`empresa_id`, `carro_id`) REFERENCES `carro` (`empresa_id`, `carro_id`) ON DELETE RESTRICT,
  CONSTRAINT `fk_viaje_centro_costo` FOREIGN KEY (`empresa_id`, `centro_costo_id`) REFERENCES `centro_costo` (`empresa_id`, `centro_costo_id`) ON DELETE RESTRICT,
  CONSTRAINT `fk_viaje_empleado_operador` FOREIGN KEY (`empresa_id`, `empleado_operador_id`) REFERENCES `empleado` (`empresa_id`, `empleado_id`) ON DELETE RESTRICT,
  CONSTRAINT `fk_viaje_empresa` FOREIGN KEY (`empresa_id`) REFERENCES `empresa` (`empresa_id`) ON DELETE RESTRICT,
  CONSTRAINT `fk_viaje_solicitud_reserva` FOREIGN KEY (`empresa_id`, `solicitud_reserva_id`) REFERENCES `solicitud_reserva` (`empresa_id`, `solicitud_reserva_id`) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE `viaje_pausa` (
  `viaje_pausa_id` binary(16) NOT NULL,
  `empresa_id` binary(16) NOT NULL,
  `viaje_id` binary(16) NOT NULL,
  `fecha_inicio` timestamp NOT NULL,
  `fecha_fin` timestamp NULL DEFAULT NULL,
  `motivo` varchar(255) DEFAULT NULL,
  `ubicacion_lat` decimal(9,6) DEFAULT NULL,
  `ubicacion_lng` decimal(9,6) DEFAULT NULL,
  PRIMARY KEY (`viaje_pausa_id`),
  KEY `ix_viaje_pausa_empresa_id` (`empresa_id`),
  KEY `ix_viaje_pausa_empresa_id_viaje_id` (`empresa_id`,`viaje_id`),
  KEY `ix_viaje_pausa_viaje_id_fecha_inicio` (`viaje_id`,`fecha_inicio`),
  CONSTRAINT `fk_viaje_pausa_empresa` FOREIGN KEY (`empresa_id`) REFERENCES `empresa` (`empresa_id`) ON DELETE RESTRICT,
  CONSTRAINT `fk_viaje_pausa_viaje` FOREIGN KEY (`empresa_id`, `viaje_id`) REFERENCES `viaje` (`empresa_id`, `viaje_id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;


SET FOREIGN_KEY_CHECKS = 1;
