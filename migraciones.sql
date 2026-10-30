CREATE TABLE IF NOT EXISTS `__EFMigrationsHistory` (
    `MigrationId` varchar(150) CHARACTER SET utf8mb4 NOT NULL,
    `ProductVersion` varchar(32) CHARACTER SET utf8mb4 NOT NULL,
    CONSTRAINT `PK___EFMigrationsHistory` PRIMARY KEY (`MigrationId`)
) CHARACTER SET=utf8mb4;

START TRANSACTION;
DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261009162833_Inicial') THEN

    ALTER DATABASE CHARACTER SET utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261009162833_Inicial') THEN

    CREATE TABLE `corporativo` (
        `corporativo_id` binary(16) NOT NULL,
        `nombre` varchar(150) CHARACTER SET utf8mb4 NOT NULL,
        `activo` tinyint(1) NOT NULL DEFAULT TRUE,
        `fecha_registro` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
        CONSTRAINT `PK_corporativo` PRIMARY KEY (`corporativo_id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261009162833_Inicial') THEN

    CREATE TABLE `privilegio` (
        `privilegio_id` binary(16) NOT NULL,
        `nombre` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
        `descripcion` varchar(255) CHARACTER SET utf8mb4 NULL,
        `activo` tinyint(1) NOT NULL DEFAULT TRUE,
        CONSTRAINT `PK_privilegio` PRIMARY KEY (`privilegio_id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261009162833_Inicial') THEN

    CREATE TABLE `usuario` (
        `usuario_id` binary(16) NOT NULL,
        `email` varchar(150) CHARACTER SET utf8mb4 NOT NULL,
        `password_hash` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
        `estado` enum('activo','inactivo') CHARACTER SET utf8mb4 NOT NULL DEFAULT 'activo',
        `fecha_registro` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
        CONSTRAINT `PK_usuario` PRIMARY KEY (`usuario_id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261009162833_Inicial') THEN

    CREATE TABLE `division` (
        `division_id` binary(16) NOT NULL,
        `corporativo_id` binary(16) NOT NULL,
        `nombre` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
        `activo` tinyint(1) NOT NULL DEFAULT TRUE,
        `fecha_registro` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
        CONSTRAINT `PK_division` PRIMARY KEY (`division_id`),
        CONSTRAINT `uq_division_corporativo_id_division_id` UNIQUE (`corporativo_id`, `division_id`),
        CONSTRAINT `fk_division_corporativo` FOREIGN KEY (`corporativo_id`) REFERENCES `corporativo` (`corporativo_id`) ON DELETE RESTRICT
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261009162833_Inicial') THEN

    CREATE TABLE `empresa` (
        `empresa_id` binary(16) NOT NULL,
        `corporativo_id` binary(16) NULL,
        `division_id` binary(16) NULL,
        `nombre` varchar(150) CHARACTER SET utf8mb4 NOT NULL,
        `delete` tinyint(1) NOT NULL DEFAULT FALSE,
        `fecha_registro` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
        CONSTRAINT `PK_empresa` PRIMARY KEY (`empresa_id`),
        CONSTRAINT `fk_empresa_corporativo` FOREIGN KEY (`corporativo_id`) REFERENCES `corporativo` (`corporativo_id`) ON DELETE RESTRICT,
        CONSTRAINT `fk_empresa_division` FOREIGN KEY (`corporativo_id`, `division_id`) REFERENCES `division` (`corporativo_id`, `division_id`) ON DELETE RESTRICT
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261009162833_Inicial') THEN

    CREATE TABLE `carro` (
        `carro_id` binary(16) NOT NULL,
        `empresa_id` binary(16) NOT NULL,
        `marca` varchar(80) CHARACTER SET utf8mb4 NOT NULL,
        `modelo` varchar(80) CHARACTER SET utf8mb4 NOT NULL,
        `anio` int NULL,
        `color` varchar(50) CHARACTER SET utf8mb4 NULL,
        `placas` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
        `foto_vehiculo` varchar(255) CHARACTER SET utf8mb4 NULL,
        `capacidad_tanque` decimal(6,2) NULL,
        `kilometraje_actual` decimal(10,2) NOT NULL DEFAULT 0.0,
        `estado` enum('disponible','ocupado','en_mantenimiento') CHARACTER SET utf8mb4 NOT NULL DEFAULT 'disponible',
        `fecha_alta` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
        CONSTRAINT `PK_carro` PRIMARY KEY (`carro_id`),
        CONSTRAINT `uq_carro_empresa_id_carro_id` UNIQUE (`empresa_id`, `carro_id`),
        CONSTRAINT `fk_carro_empresa` FOREIGN KEY (`empresa_id`) REFERENCES `empresa` (`empresa_id`) ON DELETE RESTRICT
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261009162833_Inicial') THEN

    CREATE TABLE `centro_costo` (
        `centro_costo_id` binary(16) NOT NULL,
        `empresa_id` binary(16) NOT NULL,
        `nombre` varchar(150) CHARACTER SET utf8mb4 NOT NULL,
        `clave` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
        `activo` tinyint(1) NOT NULL DEFAULT TRUE,
        `fecha_registro` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
        CONSTRAINT `PK_centro_costo` PRIMARY KEY (`centro_costo_id`),
        CONSTRAINT `fk_centro_costo_empresa` FOREIGN KEY (`empresa_id`) REFERENCES `empresa` (`empresa_id`) ON DELETE RESTRICT
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261009162833_Inicial') THEN

    CREATE TABLE `empleado` (
        `empleado_id` binary(16) NOT NULL,
        `empresa_id` binary(16) NOT NULL,
        `usuario_id` binary(16) NOT NULL,
        `nombre_completo` varchar(150) CHARACTER SET utf8mb4 NOT NULL,
        `foto_perfil` varchar(255) CHARACTER SET utf8mb4 NULL,
        `foto_licencia` varchar(255) CHARACTER SET utf8mb4 NULL,
        `delete` tinyint(1) NOT NULL DEFAULT FALSE,
        `fecha_alta` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
        CONSTRAINT `PK_empleado` PRIMARY KEY (`empleado_id`),
        CONSTRAINT `uq_empleado_empresa_id_empleado_id` UNIQUE (`empresa_id`, `empleado_id`),
        CONSTRAINT `fk_empleado_empresa` FOREIGN KEY (`empresa_id`) REFERENCES `empresa` (`empresa_id`) ON DELETE RESTRICT,
        CONSTRAINT `fk_empleado_usuario` FOREIGN KEY (`usuario_id`) REFERENCES `usuario` (`usuario_id`) ON DELETE RESTRICT
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261009162833_Inicial') THEN

    CREATE TABLE `rol` (
        `rol_id` binary(16) NOT NULL,
        `empresa_id` binary(16) NULL,
        `nombre` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
        `descripcion` varchar(255) CHARACTER SET utf8mb4 NULL,
        `activo` tinyint(1) NOT NULL DEFAULT TRUE,
        CONSTRAINT `PK_rol` PRIMARY KEY (`rol_id`),
        CONSTRAINT `fk_rol_empresa` FOREIGN KEY (`empresa_id`) REFERENCES `empresa` (`empresa_id`) ON DELETE RESTRICT
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261009162833_Inicial') THEN

    CREATE TABLE `notificacion` (
        `notificacion_id` binary(16) NOT NULL,
        `empresa_id` binary(16) NOT NULL,
        `empleado_destinatario_id` binary(16) NOT NULL,
        `tipo` enum('nueva_solicitud','resultado_solicitud','cambio_estatus_forzado') CHARACTER SET utf8mb4 NOT NULL,
        `referencia_id` binary(16) NULL,
        `mensaje` varchar(255) CHARACTER SET utf8mb4 NULL,
        `leida` tinyint(1) NOT NULL DEFAULT FALSE,
        `fecha_envio` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
        CONSTRAINT `PK_notificacion` PRIMARY KEY (`notificacion_id`),
        CONSTRAINT `fk_notificacion_empleado_destinatario` FOREIGN KEY (`empresa_id`, `empleado_destinatario_id`) REFERENCES `empleado` (`empresa_id`, `empleado_id`) ON DELETE RESTRICT,
        CONSTRAINT `fk_notificacion_empresa` FOREIGN KEY (`empresa_id`) REFERENCES `empresa` (`empresa_id`) ON DELETE RESTRICT
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261009162833_Inicial') THEN

    CREATE TABLE `solicitud_reserva` (
        `solicitud_reserva_id` binary(16) NOT NULL,
        `empresa_id` binary(16) NOT NULL,
        `carro_id` binary(16) NOT NULL,
        `empleado_solicitante_id` binary(16) NOT NULL,
        `fecha_solicitud` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
        `estado` enum('pendiente','aprobada','rechazada','cancelada') CHARACTER SET utf8mb4 NOT NULL DEFAULT 'pendiente',
        `empleado_confirmador_id` binary(16) NULL,
        `fecha_resolucion` timestamp NULL,
        `motivo_resolucion` varchar(255) CHARACTER SET utf8mb4 NULL,
        CONSTRAINT `PK_solicitud_reserva` PRIMARY KEY (`solicitud_reserva_id`),
        CONSTRAINT `uq_solicitud_reserva_empresa_id_solicitud_reserva_id` UNIQUE (`empresa_id`, `solicitud_reserva_id`),
        CONSTRAINT `fk_solicitud_reserva_carro` FOREIGN KEY (`empresa_id`, `carro_id`) REFERENCES `carro` (`empresa_id`, `carro_id`) ON DELETE RESTRICT,
        CONSTRAINT `fk_solicitud_reserva_empleado_confirmador` FOREIGN KEY (`empresa_id`, `empleado_confirmador_id`) REFERENCES `empleado` (`empresa_id`, `empleado_id`) ON DELETE RESTRICT,
        CONSTRAINT `fk_solicitud_reserva_empleado_solicitante` FOREIGN KEY (`empresa_id`, `empleado_solicitante_id`) REFERENCES `empleado` (`empresa_id`, `empleado_id`) ON DELETE RESTRICT,
        CONSTRAINT `fk_solicitud_reserva_empresa` FOREIGN KEY (`empresa_id`) REFERENCES `empresa` (`empresa_id`) ON DELETE RESTRICT
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261009162833_Inicial') THEN

    CREATE TABLE `rol_privilegio` (
        `rol_privilegio_id` binary(16) NOT NULL,
        `rol_id` binary(16) NOT NULL,
        `privilegio_id` binary(16) NOT NULL,
        CONSTRAINT `PK_rol_privilegio` PRIMARY KEY (`rol_privilegio_id`),
        CONSTRAINT `fk_rol_privilegio_privilegio` FOREIGN KEY (`privilegio_id`) REFERENCES `privilegio` (`privilegio_id`) ON DELETE CASCADE,
        CONSTRAINT `fk_rol_privilegio_rol` FOREIGN KEY (`rol_id`) REFERENCES `rol` (`rol_id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261009162833_Inicial') THEN

    CREATE TABLE `usuario_rol` (
        `usuario_rol_id` binary(16) NOT NULL,
        `usuario_id` binary(16) NOT NULL,
        `rol_id` binary(16) NOT NULL,
        CONSTRAINT `PK_usuario_rol` PRIMARY KEY (`usuario_rol_id`),
        CONSTRAINT `fk_usuario_rol_rol` FOREIGN KEY (`rol_id`) REFERENCES `rol` (`rol_id`) ON DELETE CASCADE,
        CONSTRAINT `fk_usuario_rol_usuario` FOREIGN KEY (`usuario_id`) REFERENCES `usuario` (`usuario_id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261009162833_Inicial') THEN

    CREATE TABLE `viaje` (
        `viaje_id` binary(16) NOT NULL,
        `empresa_id` binary(16) NOT NULL,
        `carro_id` binary(16) NOT NULL,
        `solicitud_reserva_id` binary(16) NULL,
        `empleado_operador_id` binary(16) NOT NULL,
        `fecha_inicio` timestamp NOT NULL,
        `ubicacion_inicio_lat` decimal(9,6) NULL,
        `ubicacion_inicio_lng` decimal(9,6) NULL,
        `kilometraje_inicial` decimal(10,2) NOT NULL,
        `fecha_fin` timestamp NULL,
        `ubicacion_fin_lat` decimal(9,6) NULL,
        `ubicacion_fin_lng` decimal(9,6) NULL,
        `kilometraje_final` decimal(10,2) NULL,
        `distancia_recorrida` decimal(10,2) NULL,
        `estado` enum('en_curso','finalizado','interrumpido') CHARACTER SET utf8mb4 NOT NULL DEFAULT 'en_curso',
        `motivo_interrupcion` varchar(255) CHARACTER SET utf8mb4 NULL,
        `sincronizado` tinyint(1) NOT NULL DEFAULT FALSE,
        CONSTRAINT `PK_viaje` PRIMARY KEY (`viaje_id`),
        CONSTRAINT `uq_viaje_empresa_id_viaje_id` UNIQUE (`empresa_id`, `viaje_id`),
        CONSTRAINT `fk_viaje_carro` FOREIGN KEY (`empresa_id`, `carro_id`) REFERENCES `carro` (`empresa_id`, `carro_id`) ON DELETE RESTRICT,
        CONSTRAINT `fk_viaje_empleado_operador` FOREIGN KEY (`empresa_id`, `empleado_operador_id`) REFERENCES `empleado` (`empresa_id`, `empleado_id`) ON DELETE RESTRICT,
        CONSTRAINT `fk_viaje_empresa` FOREIGN KEY (`empresa_id`) REFERENCES `empresa` (`empresa_id`) ON DELETE RESTRICT,
        CONSTRAINT `fk_viaje_solicitud_reserva` FOREIGN KEY (`empresa_id`, `solicitud_reserva_id`) REFERENCES `solicitud_reserva` (`empresa_id`, `solicitud_reserva_id`) ON DELETE RESTRICT
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261009162833_Inicial') THEN

    CREATE TABLE `cambio_estatus_carro` (
        `cambio_estatus_carro_id` binary(16) NOT NULL,
        `empresa_id` binary(16) NOT NULL,
        `carro_id` binary(16) NOT NULL,
        `empleado_id` binary(16) NOT NULL,
        `viaje_afectado_id` binary(16) NULL,
        `estatus_anterior` enum('disponible','ocupado','en_mantenimiento') CHARACTER SET utf8mb4 NOT NULL,
        `estatus_nuevo` enum('disponible','ocupado','en_mantenimiento') CHARACTER SET utf8mb4 NOT NULL,
        `motivo` varchar(255) CHARACTER SET utf8mb4 NULL,
        `fecha_cambio` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
        `sincronizado` tinyint(1) NOT NULL DEFAULT FALSE,
        CONSTRAINT `PK_cambio_estatus_carro` PRIMARY KEY (`cambio_estatus_carro_id`),
        CONSTRAINT `fk_cambio_estatus_carro_carro` FOREIGN KEY (`empresa_id`, `carro_id`) REFERENCES `carro` (`empresa_id`, `carro_id`) ON DELETE RESTRICT,
        CONSTRAINT `fk_cambio_estatus_carro_empleado` FOREIGN KEY (`empresa_id`, `empleado_id`) REFERENCES `empleado` (`empresa_id`, `empleado_id`) ON DELETE RESTRICT,
        CONSTRAINT `fk_cambio_estatus_carro_empresa` FOREIGN KEY (`empresa_id`) REFERENCES `empresa` (`empresa_id`) ON DELETE RESTRICT,
        CONSTRAINT `fk_cambio_estatus_carro_viaje` FOREIGN KEY (`empresa_id`, `viaje_afectado_id`) REFERENCES `viaje` (`empresa_id`, `viaje_id`) ON DELETE RESTRICT
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261009162833_Inicial') THEN

    CREATE TABLE `carga_combustible` (
        `carga_combustible_id` binary(16) NOT NULL,
        `empresa_id` binary(16) NOT NULL,
        `viaje_id` binary(16) NOT NULL,
        `litros` decimal(8,2) NOT NULL,
        `monto` decimal(10,2) NULL,
        `foto_comprobante` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
        `fecha_carga` timestamp NOT NULL,
        `ubicacion_lat` decimal(9,6) NULL,
        `ubicacion_lng` decimal(9,6) NULL,
        `sincronizado` tinyint(1) NOT NULL DEFAULT FALSE,
        CONSTRAINT `PK_carga_combustible` PRIMARY KEY (`carga_combustible_id`),
        CONSTRAINT `fk_carga_combustible_empresa` FOREIGN KEY (`empresa_id`) REFERENCES `empresa` (`empresa_id`) ON DELETE RESTRICT,
        CONSTRAINT `fk_carga_combustible_viaje` FOREIGN KEY (`empresa_id`, `viaje_id`) REFERENCES `viaje` (`empresa_id`, `viaje_id`) ON DELETE RESTRICT
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261009162833_Inicial') THEN

    CREATE TABLE `punto_ruta` (
        `punto_ruta_id` binary(16) NOT NULL,
        `empresa_id` binary(16) NOT NULL,
        `viaje_id` binary(16) NOT NULL,
        `latitud` decimal(9,6) NOT NULL,
        `longitud` decimal(9,6) NOT NULL,
        `precision_metros` decimal(6,2) NULL,
        `altitud` decimal(7,2) NULL,
        `velocidad` decimal(6,2) NULL,
        `timestamp_captura` timestamp NOT NULL,
        `sincronizado` tinyint(1) NOT NULL DEFAULT FALSE,
        CONSTRAINT `PK_punto_ruta` PRIMARY KEY (`punto_ruta_id`),
        CONSTRAINT `fk_punto_ruta_empresa` FOREIGN KEY (`empresa_id`) REFERENCES `empresa` (`empresa_id`) ON DELETE RESTRICT,
        CONSTRAINT `fk_punto_ruta_viaje` FOREIGN KEY (`empresa_id`, `viaje_id`) REFERENCES `viaje` (`empresa_id`, `viaje_id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261009162833_Inicial') THEN

    CREATE TABLE `ruta_procesada` (
        `ruta_procesada_id` binary(16) NOT NULL,
        `empresa_id` binary(16) NOT NULL,
        `viaje_id` binary(16) NOT NULL,
        `puntos_recibidos` int NOT NULL DEFAULT 0,
        `puntos_descartados` int NOT NULL DEFAULT 0,
        `distancia_gps_km` decimal(10,3) NOT NULL DEFAULT 0.0,
        `tiempo_movimiento_seg` int NOT NULL DEFAULT 0,
        `tiempo_detenido_seg` int NOT NULL DEFAULT 0,
        `velocidad_promedio_kmh` decimal(6,2) NULL,
        `velocidad_maxima_kmh` decimal(6,2) NULL,
        `distancia_odometro_km` decimal(10,2) NULL,
        `diferencia_km` decimal(10,2) NULL,
        `diferencia_porcentaje` decimal(7,2) NULL,
        `alerta_kilometraje` tinyint(1) NOT NULL DEFAULT FALSE,
        `litros_cargados` decimal(8,2) NOT NULL DEFAULT 0.0,
        `rendimiento_km_por_litro` decimal(6,2) NULL,
        `polyline` text CHARACTER SET utf8mb4 NULL,
        `version_algoritmo` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
        `fecha_proceso` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
        CONSTRAINT `PK_ruta_procesada` PRIMARY KEY (`ruta_procesada_id`),
        CONSTRAINT `fk_ruta_procesada_empresa` FOREIGN KEY (`empresa_id`) REFERENCES `empresa` (`empresa_id`) ON DELETE RESTRICT,
        CONSTRAINT `fk_ruta_procesada_viaje` FOREIGN KEY (`empresa_id`, `viaje_id`) REFERENCES `viaje` (`empresa_id`, `viaje_id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261009162833_Inicial') THEN

    CREATE INDEX `ix_cambio_estatus_carro_empresa_id` ON `cambio_estatus_carro` (`empresa_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261009162833_Inicial') THEN

    CREATE INDEX `ix_cambio_estatus_carro_empresa_id_carro_id` ON `cambio_estatus_carro` (`empresa_id`, `carro_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261009162833_Inicial') THEN

    CREATE INDEX `ix_cambio_estatus_carro_empresa_id_empleado_id` ON `cambio_estatus_carro` (`empresa_id`, `empleado_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261009162833_Inicial') THEN

    CREATE INDEX `ix_cambio_estatus_carro_empresa_id_viaje_afectado_id` ON `cambio_estatus_carro` (`empresa_id`, `viaje_afectado_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261009162833_Inicial') THEN

    CREATE UNIQUE INDEX `uq_cambio_estatus_carro_empresa_id_cambio_estatus_carro_id` ON `cambio_estatus_carro` (`empresa_id`, `cambio_estatus_carro_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261009162833_Inicial') THEN

    CREATE INDEX `ix_carga_combustible_empresa_id` ON `carga_combustible` (`empresa_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261009162833_Inicial') THEN

    CREATE INDEX `ix_carga_combustible_empresa_id_viaje_id` ON `carga_combustible` (`empresa_id`, `viaje_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261009162833_Inicial') THEN

    CREATE UNIQUE INDEX `uq_carga_combustible_empresa_id_carga_combustible_id` ON `carga_combustible` (`empresa_id`, `carga_combustible_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261009162833_Inicial') THEN

    CREATE INDEX `ix_carro_empresa_id` ON `carro` (`empresa_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261009162833_Inicial') THEN

    CREATE UNIQUE INDEX `uq_carro_placas` ON `carro` (`placas`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261009162833_Inicial') THEN

    CREATE INDEX `ix_centro_costo_empresa_id` ON `centro_costo` (`empresa_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261009162833_Inicial') THEN

    CREATE UNIQUE INDEX `uq_centro_costo_empresa_id_centro_costo_id` ON `centro_costo` (`empresa_id`, `centro_costo_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261009162833_Inicial') THEN

    CREATE UNIQUE INDEX `uq_centro_costo_empresa_id_clave` ON `centro_costo` (`empresa_id`, `clave`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261009162833_Inicial') THEN

    CREATE INDEX `ix_division_corporativo_id` ON `division` (`corporativo_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261009162833_Inicial') THEN

    CREATE INDEX `ix_empleado_empresa_id` ON `empleado` (`empresa_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261009162833_Inicial') THEN

    CREATE UNIQUE INDEX `uq_empleado_usuario_id` ON `empleado` (`usuario_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261009162833_Inicial') THEN

    CREATE INDEX `ix_empresa_corporativo_id` ON `empresa` (`corporativo_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261009162833_Inicial') THEN

    CREATE INDEX `ix_empresa_corporativo_id_division_id` ON `empresa` (`corporativo_id`, `division_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261009162833_Inicial') THEN

    CREATE INDEX `ix_empresa_division_id` ON `empresa` (`division_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261009162833_Inicial') THEN

    CREATE UNIQUE INDEX `uq_empresa_corporativo_id_empresa_id` ON `empresa` (`corporativo_id`, `empresa_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261009162833_Inicial') THEN

    CREATE INDEX `ix_notificacion_empleado_destinatario_id_leida` ON `notificacion` (`empleado_destinatario_id`, `leida`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261009162833_Inicial') THEN

    CREATE INDEX `ix_notificacion_empresa_id` ON `notificacion` (`empresa_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261009162833_Inicial') THEN

    CREATE INDEX `ix_notificacion_empresa_id_empleado_destinatario_id` ON `notificacion` (`empresa_id`, `empleado_destinatario_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261009162833_Inicial') THEN

    CREATE UNIQUE INDEX `uq_notificacion_empresa_id_notificacion_id` ON `notificacion` (`empresa_id`, `notificacion_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261009162833_Inicial') THEN

    CREATE UNIQUE INDEX `uq_privilegio_nombre` ON `privilegio` (`nombre`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261009162833_Inicial') THEN

    CREATE INDEX `ix_punto_ruta_empresa_id` ON `punto_ruta` (`empresa_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261009162833_Inicial') THEN

    CREATE INDEX `ix_punto_ruta_empresa_id_viaje_id` ON `punto_ruta` (`empresa_id`, `viaje_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261009162833_Inicial') THEN

    CREATE INDEX `ix_punto_ruta_viaje_id_timestamp_captura` ON `punto_ruta` (`viaje_id`, `timestamp_captura`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261009162833_Inicial') THEN

    CREATE UNIQUE INDEX `uq_punto_ruta_empresa_id_punto_ruta_id` ON `punto_ruta` (`empresa_id`, `punto_ruta_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261009162833_Inicial') THEN

    CREATE INDEX `ix_rol_empresa_id` ON `rol` (`empresa_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261009162833_Inicial') THEN

    CREATE UNIQUE INDEX `uq_rol_empresa_id_nombre` ON `rol` (`empresa_id`, `nombre`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261009162833_Inicial') THEN

    CREATE INDEX `ix_rol_privilegio_privilegio_id` ON `rol_privilegio` (`privilegio_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261009162833_Inicial') THEN

    CREATE INDEX `ix_rol_privilegio_rol_id` ON `rol_privilegio` (`rol_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261009162833_Inicial') THEN

    CREATE UNIQUE INDEX `uq_rol_privilegio_rol_id_privilegio_id` ON `rol_privilegio` (`rol_id`, `privilegio_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261009162833_Inicial') THEN

    CREATE INDEX `ix_ruta_procesada_empresa_id` ON `ruta_procesada` (`empresa_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261009162833_Inicial') THEN

    CREATE INDEX `ix_ruta_procesada_empresa_id_alerta_kilometraje` ON `ruta_procesada` (`empresa_id`, `alerta_kilometraje`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261009162833_Inicial') THEN

    CREATE INDEX `ix_ruta_procesada_empresa_id_viaje_id` ON `ruta_procesada` (`empresa_id`, `viaje_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261009162833_Inicial') THEN

    CREATE UNIQUE INDEX `uq_ruta_procesada_empresa_id_ruta_procesada_id` ON `ruta_procesada` (`empresa_id`, `ruta_procesada_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261009162833_Inicial') THEN

    CREATE UNIQUE INDEX `uq_ruta_procesada_viaje_id` ON `ruta_procesada` (`viaje_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261009162833_Inicial') THEN

    CREATE INDEX `ix_solicitud_reserva_empresa_id` ON `solicitud_reserva` (`empresa_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261009162833_Inicial') THEN

    CREATE INDEX `ix_solicitud_reserva_empresa_id_carro_id` ON `solicitud_reserva` (`empresa_id`, `carro_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261009162833_Inicial') THEN

    CREATE INDEX `ix_solicitud_reserva_empresa_id_empleado_confirmador_id` ON `solicitud_reserva` (`empresa_id`, `empleado_confirmador_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261009162833_Inicial') THEN

    CREATE INDEX `ix_solicitud_reserva_empresa_id_empleado_solicitante_id` ON `solicitud_reserva` (`empresa_id`, `empleado_solicitante_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261009162833_Inicial') THEN

    CREATE UNIQUE INDEX `uq_usuario_email` ON `usuario` (`email`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261009162833_Inicial') THEN

    CREATE INDEX `ix_usuario_rol_rol_id` ON `usuario_rol` (`rol_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261009162833_Inicial') THEN

    CREATE INDEX `ix_usuario_rol_usuario_id` ON `usuario_rol` (`usuario_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261009162833_Inicial') THEN

    CREATE UNIQUE INDEX `uq_usuario_rol_usuario_id_rol_id` ON `usuario_rol` (`usuario_id`, `rol_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261009162833_Inicial') THEN

    CREATE INDEX `ix_viaje_empresa_id` ON `viaje` (`empresa_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261009162833_Inicial') THEN

    CREATE INDEX `ix_viaje_empresa_id_carro_id` ON `viaje` (`empresa_id`, `carro_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261009162833_Inicial') THEN

    CREATE INDEX `ix_viaje_empresa_id_empleado_operador_id` ON `viaje` (`empresa_id`, `empleado_operador_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261009162833_Inicial') THEN

    CREATE INDEX `ix_viaje_empresa_id_solicitud_reserva_id` ON `viaje` (`empresa_id`, `solicitud_reserva_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261009162833_Inicial') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20261009162833_Inicial', '10.0.12');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

COMMIT;

START TRANSACTION;
DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261009181101_OutboxYDispositivos') THEN

    CREATE TABLE `dispositivo_empleado` (
        `dispositivo_empleado_id` binary(16) NOT NULL,
        `empresa_id` binary(16) NOT NULL,
        `empleado_id` binary(16) NOT NULL,
        `token_fcm` varchar(512) CHARACTER SET utf8mb4 NOT NULL,
        `plataforma` enum('android','ios','web') CHARACTER SET utf8mb4 NOT NULL,
        `activo` tinyint(1) NOT NULL DEFAULT TRUE,
        `fecha_registro` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
        `fecha_ultimo_uso` timestamp NULL,
        CONSTRAINT `PK_dispositivo_empleado` PRIMARY KEY (`dispositivo_empleado_id`),
        CONSTRAINT `fk_dispositivo_empleado_empleado` FOREIGN KEY (`empresa_id`, `empleado_id`) REFERENCES `empleado` (`empresa_id`, `empleado_id`) ON DELETE CASCADE,
        CONSTRAINT `fk_dispositivo_empleado_empresa` FOREIGN KEY (`empresa_id`) REFERENCES `empresa` (`empresa_id`) ON DELETE RESTRICT
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261009181101_OutboxYDispositivos') THEN

    CREATE TABLE `outbox_mensaje` (
        `outbox_mensaje_id` binary(16) NOT NULL,
        `empresa_id` binary(16) NOT NULL,
        `tipo` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
        `agregado_id` binary(16) NOT NULL,
        `payload` json NOT NULL,
        `estado` enum('pendiente','procesando','enviado','fallido') CHARACTER SET utf8mb4 NOT NULL DEFAULT 'pendiente',
        `intentos` int NOT NULL DEFAULT 0,
        `proximo_intento` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
        `ultimo_error` varchar(500) CHARACTER SET utf8mb4 NULL,
        `fecha_creacion` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
        `fecha_procesado` timestamp NULL,
        CONSTRAINT `PK_outbox_mensaje` PRIMARY KEY (`outbox_mensaje_id`),
        CONSTRAINT `fk_outbox_mensaje_empresa` FOREIGN KEY (`empresa_id`) REFERENCES `empresa` (`empresa_id`) ON DELETE RESTRICT
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261009181101_OutboxYDispositivos') THEN

    CREATE INDEX `ix_dispositivo_empleado_empleado_id_activo` ON `dispositivo_empleado` (`empleado_id`, `activo`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261009181101_OutboxYDispositivos') THEN

    CREATE INDEX `ix_dispositivo_empleado_empresa_id` ON `dispositivo_empleado` (`empresa_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261009181101_OutboxYDispositivos') THEN

    CREATE INDEX `ix_dispositivo_empleado_empresa_id_empleado_id` ON `dispositivo_empleado` (`empresa_id`, `empleado_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261009181101_OutboxYDispositivos') THEN

    CREATE UNIQUE INDEX `uq_dispositivo_empleado_empresa_id_dispositivo_empleado_id` ON `dispositivo_empleado` (`empresa_id`, `dispositivo_empleado_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261009181101_OutboxYDispositivos') THEN

    CREATE UNIQUE INDEX `uq_dispositivo_empleado_token_fcm` ON `dispositivo_empleado` (`token_fcm`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261009181101_OutboxYDispositivos') THEN

    CREATE INDEX `ix_outbox_mensaje_agregado_id` ON `outbox_mensaje` (`agregado_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261009181101_OutboxYDispositivos') THEN

    CREATE INDEX `ix_outbox_mensaje_empresa_id` ON `outbox_mensaje` (`empresa_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261009181101_OutboxYDispositivos') THEN

    CREATE INDEX `ix_outbox_mensaje_estado_proximo_intento` ON `outbox_mensaje` (`estado`, `proximo_intento`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261009181101_OutboxYDispositivos') THEN

    CREATE UNIQUE INDEX `uq_outbox_mensaje_empresa_id_outbox_mensaje_id` ON `outbox_mensaje` (`empresa_id`, `outbox_mensaje_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261009181101_OutboxYDispositivos') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20261009181101_OutboxYDispositivos', '10.0.12');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

COMMIT;

START TRANSACTION;
DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261013170116_AuditoriaYTrazabilidadV4') THEN

    ALTER TABLE `punto_ruta` DROP COLUMN `sincronizado`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261013170116_AuditoriaYTrazabilidadV4') THEN

    ALTER TABLE `cambio_estatus_carro` DROP COLUMN `sincronizado`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261013170116_AuditoriaYTrazabilidadV4') THEN

    ALTER TABLE `viaje` RENAME COLUMN `sincronizado` TO `corregido`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261013170116_AuditoriaYTrazabilidadV4') THEN

    ALTER TABLE `empresa` RENAME COLUMN `delete` TO `eliminado`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261013170116_AuditoriaYTrazabilidadV4') THEN

    ALTER TABLE `empleado` RENAME COLUMN `delete` TO `eliminado`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261013170116_AuditoriaYTrazabilidadV4') THEN

    ALTER TABLE `carga_combustible` RENAME COLUMN `sincronizado` TO `corregido`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261013170116_AuditoriaYTrazabilidadV4') THEN

    ALTER TABLE `viaje` ADD `fecha_recepcion` timestamp NULL DEFAULT CURRENT_TIMESTAMP;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261013170116_AuditoriaYTrazabilidadV4') THEN

    ALTER TABLE `viaje` ADD `foto_tablero_final` varchar(255) CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261013170116_AuditoriaYTrazabilidadV4') THEN

    ALTER TABLE `viaje` ADD `foto_tablero_inicial` varchar(255) CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261013170116_AuditoriaYTrazabilidadV4') THEN

    ALTER TABLE `solicitud_reserva` MODIFY COLUMN `estado` enum('pendiente','aprobada','rechazada','cancelada','finalizada') CHARACTER SET utf8mb4 NOT NULL DEFAULT 'pendiente';

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261013170116_AuditoriaYTrazabilidadV4') THEN

    ALTER TABLE `solicitud_reserva` ADD `fecha_liberacion` timestamp NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261013170116_AuditoriaYTrazabilidadV4') THEN

    ALTER TABLE `solicitud_reserva` ADD `observaciones_liberacion` varchar(500) CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261013170116_AuditoriaYTrazabilidadV4') THEN

    ALTER TABLE `punto_ruta` ADD `fecha_recepcion` timestamp NULL DEFAULT CURRENT_TIMESTAMP;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261013170116_AuditoriaYTrazabilidadV4') THEN

    ALTER TABLE `notificacion` MODIFY COLUMN `tipo` enum('nueva_solicitud','resultado_solicitud','cambio_estatus_forzado','reporte_mantenimiento') CHARACTER SET utf8mb4 NOT NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261013170116_AuditoriaYTrazabilidadV4') THEN

    ALTER TABLE `empresa` ADD `activo` tinyint(1) NOT NULL DEFAULT TRUE;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261013170116_AuditoriaYTrazabilidadV4') THEN

    ALTER TABLE `empleado` ADD `fecha_carga_licencia` timestamp NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261013170116_AuditoriaYTrazabilidadV4') THEN

    ALTER TABLE `carro` ADD `dado_de_baja` tinyint(1) NOT NULL DEFAULT FALSE;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261013170116_AuditoriaYTrazabilidadV4') THEN

    ALTER TABLE `carro` ADD `fecha_baja` timestamp NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261013170116_AuditoriaYTrazabilidadV4') THEN

    ALTER TABLE `carro` ADD `motivo_baja` varchar(255) CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261013170116_AuditoriaYTrazabilidadV4') THEN

    ALTER TABLE `carga_combustible` ADD `fecha_recepcion` timestamp NULL DEFAULT CURRENT_TIMESTAMP;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261013170116_AuditoriaYTrazabilidadV4') THEN

    ALTER TABLE `carga_combustible` ADD `fecha_subida_comprobante` timestamp NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261013170116_AuditoriaYTrazabilidadV4') THEN

    ALTER TABLE `cambio_estatus_carro` ADD `fecha_recepcion` timestamp NULL DEFAULT CURRENT_TIMESTAMP;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261013170116_AuditoriaYTrazabilidadV4') THEN

    ALTER TABLE `cambio_estatus_carro` ADD `foto_evidencia` varchar(255) CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261013170116_AuditoriaYTrazabilidadV4') THEN

    ALTER TABLE `cambio_estatus_carro` ADD `tipo_motivo` enum('liberacion','falla_mecanica','choque_o_golpe','servicio','emergencia','otro') CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261013170116_AuditoriaYTrazabilidadV4') THEN

    CREATE TABLE `auditoria_evento` (
        `auditoria_evento_id` binary(16) NOT NULL,
        `empresa_id` binary(16) NULL,
        `usuario_id` binary(16) NULL,
        `accion` enum('crear','actualizar','eliminar','aprobar','rechazar','cancelar','cambiar_estado','corregir') CHARACTER SET utf8mb4 NOT NULL,
        `entidad` varchar(60) CHARACTER SET utf8mb4 NOT NULL,
        `entidad_id` binary(16) NOT NULL,
        `valores_anteriores` json NULL,
        `valores_nuevos` json NULL,
        `motivo` varchar(500) CHARACTER SET utf8mb4 NULL,
        `origen` enum('movil','web','sistema') CHARACTER SET utf8mb4 NOT NULL,
        `fecha_hora` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
        CONSTRAINT `PK_auditoria_evento` PRIMARY KEY (`auditoria_evento_id`),
        CONSTRAINT `fk_auditoria_evento_empresa` FOREIGN KEY (`empresa_id`) REFERENCES `empresa` (`empresa_id`) ON DELETE RESTRICT,
        CONSTRAINT `fk_auditoria_evento_usuario` FOREIGN KEY (`usuario_id`) REFERENCES `usuario` (`usuario_id`) ON DELETE RESTRICT
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261013170116_AuditoriaYTrazabilidadV4') THEN

    CREATE TABLE `revision_auditoria` (
        `revision_auditoria_id` binary(16) NOT NULL,
        `empresa_id` binary(16) NOT NULL,
        `viaje_id` binary(16) NULL,
        `carro_id` binary(16) NULL,
        `periodo_desde` date NULL,
        `periodo_hasta` date NULL,
        `empleado_revisor_id` binary(16) NOT NULL,
        `fecha_revision` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
        `estado_revision` enum('pendiente_informacion','revisado','con_observaciones') CHARACTER SET utf8mb4 NOT NULL DEFAULT 'pendiente_informacion',
        `observaciones` varchar(1000) CHARACTER SET utf8mb4 NULL,
        `fecha_cierre` timestamp NULL,
        CONSTRAINT `PK_revision_auditoria` PRIMARY KEY (`revision_auditoria_id`),
        CONSTRAINT `ck_revision_auditoria_alcance` CHECK ((viaje_id IS NOT NULL) OR (carro_id IS NOT NULL AND periodo_desde IS NOT NULL AND periodo_hasta IS NOT NULL)),
        CONSTRAINT `fk_revision_auditoria_carro` FOREIGN KEY (`empresa_id`, `carro_id`) REFERENCES `carro` (`empresa_id`, `carro_id`) ON DELETE RESTRICT,
        CONSTRAINT `fk_revision_auditoria_empleado_revisor` FOREIGN KEY (`empresa_id`, `empleado_revisor_id`) REFERENCES `empleado` (`empresa_id`, `empleado_id`) ON DELETE RESTRICT,
        CONSTRAINT `fk_revision_auditoria_empresa` FOREIGN KEY (`empresa_id`) REFERENCES `empresa` (`empresa_id`) ON DELETE RESTRICT,
        CONSTRAINT `fk_revision_auditoria_viaje` FOREIGN KEY (`empresa_id`, `viaje_id`) REFERENCES `viaje` (`empresa_id`, `viaje_id`) ON DELETE RESTRICT
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261013170116_AuditoriaYTrazabilidadV4') THEN

    CREATE INDEX `ix_viaje_empresa_id_carro_id_fecha_inicio` ON `viaje` (`empresa_id`, `carro_id`, `fecha_inicio`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261013170116_AuditoriaYTrazabilidadV4') THEN

    CREATE INDEX `ix_solicitud_reserva_empresa_id_carro_id_estado` ON `solicitud_reserva` (`empresa_id`, `carro_id`, `estado`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261013170116_AuditoriaYTrazabilidadV4') THEN

    CREATE INDEX `ix_cambio_estatus_carro_empresa_id_tipo_motivo` ON `cambio_estatus_carro` (`empresa_id`, `tipo_motivo`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261013170116_AuditoriaYTrazabilidadV4') THEN

    CREATE INDEX `ix_auditoria_evento_empresa_id_entidad_entidad_id` ON `auditoria_evento` (`empresa_id`, `entidad`, `entidad_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261013170116_AuditoriaYTrazabilidadV4') THEN

    CREATE INDEX `ix_auditoria_evento_empresa_id_fecha_hora` ON `auditoria_evento` (`empresa_id`, `fecha_hora`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261013170116_AuditoriaYTrazabilidadV4') THEN

    CREATE INDEX `ix_auditoria_evento_usuario_id_fecha_hora` ON `auditoria_evento` (`usuario_id`, `fecha_hora`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261013170116_AuditoriaYTrazabilidadV4') THEN

    CREATE INDEX `ix_revision_auditoria_empresa_id` ON `revision_auditoria` (`empresa_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261013170116_AuditoriaYTrazabilidadV4') THEN

    CREATE INDEX `ix_revision_auditoria_empresa_id_carro_id_periodo_desde` ON `revision_auditoria` (`empresa_id`, `carro_id`, `periodo_desde`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261013170116_AuditoriaYTrazabilidadV4') THEN

    CREATE INDEX `ix_revision_auditoria_empresa_id_empleado_revisor_id` ON `revision_auditoria` (`empresa_id`, `empleado_revisor_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261013170116_AuditoriaYTrazabilidadV4') THEN

    CREATE INDEX `ix_revision_auditoria_empresa_id_estado_revision` ON `revision_auditoria` (`empresa_id`, `estado_revision`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261013170116_AuditoriaYTrazabilidadV4') THEN

    CREATE INDEX `ix_revision_auditoria_empresa_id_viaje_id` ON `revision_auditoria` (`empresa_id`, `viaje_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261013170116_AuditoriaYTrazabilidadV4') THEN

    CREATE UNIQUE INDEX `uq_revision_auditoria_empresa_id_revision_auditoria_id` ON `revision_auditoria` (`empresa_id`, `revision_auditoria_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261013170116_AuditoriaYTrazabilidadV4') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20261013170116_AuditoriaYTrazabilidadV4', '10.0.12');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

COMMIT;

START TRANSACTION;
DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261015180000_RbacPrivilegiosV5') THEN

    ALTER TABLE `privilegio` DROP INDEX `uq_privilegio_nombre`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261015180000_RbacPrivilegiosV5') THEN

    ALTER TABLE `privilegio` ADD `clave` varchar(100) CHARACTER SET utf8mb4 NOT NULL DEFAULT '';

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261015180000_RbacPrivilegiosV5') THEN

    ALTER TABLE `privilegio` ADD `modulo` varchar(50) CHARACTER SET utf8mb4 NOT NULL DEFAULT '';

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261015180000_RbacPrivilegiosV5') THEN

    ALTER TABLE `privilegio` ADD `ambito` enum('plataforma','empresa') CHARACTER SET utf8mb4 NOT NULL DEFAULT 'empresa';

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261015180000_RbacPrivilegiosV5') THEN

    UPDATE privilegio SET clave = CONCAT('legado.p', LOWER(HEX(privilegio_id))), modulo = 'legado' WHERE clave = '';

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261015180000_RbacPrivilegiosV5') THEN

    CREATE UNIQUE INDEX `uq_privilegio_clave` ON `privilegio` (`clave`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261015180000_RbacPrivilegiosV5') THEN

    CREATE INDEX `ix_privilegio_modulo` ON `privilegio` (`modulo`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261015180000_RbacPrivilegiosV5') THEN

    ALTER TABLE `rol` DROP INDEX `uq_rol_empresa_id_nombre`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261015180000_RbacPrivilegiosV5') THEN

    ALTER TABLE `rol` ADD `empresa_clave` binary(16) AS (COALESCE(`empresa_id`, 0x00000000000000000000000000000000)) STORED;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261015180000_RbacPrivilegiosV5') THEN

    CREATE UNIQUE INDEX `uq_rol_empresa_clave_nombre` ON `rol` (`empresa_clave`, `nombre`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261015180000_RbacPrivilegiosV5') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20261015180000_RbacPrivilegiosV5', '10.0.12');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

COMMIT;

START TRANSACTION;
DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261020120000_CentroCostoEnViajeYCarga') THEN

    ALTER TABLE `centro_costo` DROP INDEX `uq_centro_costo_empresa_id_centro_costo_id`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261020120000_CentroCostoEnViajeYCarga') THEN

    ALTER TABLE `centro_costo` ADD CONSTRAINT `uq_centro_costo_empresa_id_centro_costo_id` UNIQUE (`empresa_id`, `centro_costo_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261020120000_CentroCostoEnViajeYCarga') THEN

    ALTER TABLE `viaje` ADD `centro_costo_id` binary(16) NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261020120000_CentroCostoEnViajeYCarga') THEN

    CREATE INDEX `ix_viaje_empresa_id_centro_costo_id` ON `viaje` (`empresa_id`, `centro_costo_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261020120000_CentroCostoEnViajeYCarga') THEN

    ALTER TABLE `viaje` ADD CONSTRAINT `fk_viaje_centro_costo` FOREIGN KEY (`empresa_id`, `centro_costo_id`) REFERENCES `centro_costo` (`empresa_id`, `centro_costo_id`) ON DELETE RESTRICT;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261020120000_CentroCostoEnViajeYCarga') THEN

    ALTER TABLE `carga_combustible` ADD `centro_costo_id` binary(16) NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261020120000_CentroCostoEnViajeYCarga') THEN

    CREATE INDEX `ix_carga_combustible_empresa_id_centro_costo_id` ON `carga_combustible` (`empresa_id`, `centro_costo_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261020120000_CentroCostoEnViajeYCarga') THEN

    ALTER TABLE `carga_combustible` ADD CONSTRAINT `fk_carga_combustible_centro_costo` FOREIGN KEY (`empresa_id`, `centro_costo_id`) REFERENCES `centro_costo` (`empresa_id`, `centro_costo_id`) ON DELETE RESTRICT;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261020120000_CentroCostoEnViajeYCarga') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20261020120000_CentroCostoEnViajeYCarga', '10.0.12');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

COMMIT;

START TRANSACTION;
DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261021120000_PausasDeViaje') THEN

    CREATE TABLE `viaje_pausa` (
        `viaje_pausa_id` binary(16) NOT NULL,
        `empresa_id` binary(16) NOT NULL,
        `viaje_id` binary(16) NOT NULL,
        `fecha_inicio` timestamp NOT NULL,
        `fecha_fin` timestamp NULL,
        `motivo` varchar(255) CHARACTER SET utf8mb4 NULL,
        `ubicacion_lat` decimal(9,6) NULL,
        `ubicacion_lng` decimal(9,6) NULL,
        CONSTRAINT `PK_viaje_pausa` PRIMARY KEY (`viaje_pausa_id`),
        CONSTRAINT `fk_viaje_pausa_empresa` FOREIGN KEY (`empresa_id`) REFERENCES `empresa` (`empresa_id`) ON DELETE RESTRICT,
        CONSTRAINT `fk_viaje_pausa_viaje` FOREIGN KEY (`empresa_id`, `viaje_id`) REFERENCES `viaje` (`empresa_id`, `viaje_id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261021120000_PausasDeViaje') THEN

    CREATE INDEX `ix_viaje_pausa_empresa_id` ON `viaje_pausa` (`empresa_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261021120000_PausasDeViaje') THEN

    CREATE INDEX `ix_viaje_pausa_empresa_id_viaje_id` ON `viaje_pausa` (`empresa_id`, `viaje_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261021120000_PausasDeViaje') THEN

    CREATE INDEX `ix_viaje_pausa_viaje_id_fecha_inicio` ON `viaje_pausa` (`viaje_id`, `fecha_inicio`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261021120000_PausasDeViaje') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20261021120000_PausasDeViaje', '10.0.12');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

COMMIT;

START TRANSACTION;
DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261022120000_MotivoSolicitud') THEN

    ALTER TABLE `solicitud_reserva` ADD `motivo_solicitud` varchar(255) CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261022120000_MotivoSolicitud') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20261022120000_MotivoSolicitud', '10.0.12');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

COMMIT;

START TRANSACTION;
DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261030120000_TipoVehiculo') THEN

    ALTER TABLE `carro` ADD `tipo_vehiculo` enum('carro','moto','camioneta','camion') CHARACTER SET utf8mb4 NOT NULL DEFAULT 'carro';

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261030120000_TipoVehiculo') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20261030120000_TipoVehiculo', '10.0.12');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

COMMIT;

