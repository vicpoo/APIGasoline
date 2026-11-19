# Cambios de APIGasoline

El formato sigue [Keep a Changelog](https://keepachangelog.com/es-ES/1.1.0/) y las versiones siguen [SemVer](https://semver.org/lang/es/).

## [Sin publicar]

Correcciones de las revisiones de aislamiento multiempresa (A001 a A009) y de correcciones (N001 a N006). HTTPS (A004) queda pendiente.

### ⚠️ Cambios que pueden afectar a los clientes

- **Lote GPS simultáneo (N001):** si el mismo lote choca con otro envío al mismo tiempo, la API reintenta sola; si aun así no puede, responde **503 con `Retry-After`** en lugar de 500.
- **Administrar a otro usuario (A008):** desactivarlo, cambiarle el correo o cerrarle sesiones exige tener todos sus privilegios (403 si no).
- **Registrar un celular ya registrado en otra empresa (A006):** el mensaje del 409 ya no dice "de otra empresa".
- **Placas únicas por empresa (A006):** dos empresas pueden registrar las mismas placas (migración `PlacasUnicasPorEmpresa`); repetirlas en la misma empresa sigue respondiendo 409.
- **AutoMapper 15.1.4 (N005):** corrige GHSA-rvv3-g6hj-g44x, GHSA-6vhm-j9h7-f9gh y GHSA-gpwp-5797-mjff. Desde la versión 15 pide licencia: en producción se configura `AUTOMAPPER_LICENSE_KEY` en el `.env` (sin ella solo escribe un aviso en el log).

### Seguridad

- **A005:** `auditoria_evento`, `outbox_mensaje` y `sesion_usuario` entran al aislamiento: filtro de EF, guardián de escrituras y Row-Level Security (migración `AislamientoTablasTecnicas`). Los eventos de auditoría de asignaciones de roles quedan con la empresa de quien hizo el cambio.
- **N004:** la bitácora es de solo inserción para el login de la API (`DENY UPDATE, DELETE`, migración `BitacoraSoloInsercion` y `crear-logins.sql`).
- **A007:** la BD exige que el usuario de un empleado sea de la misma empresa o de ninguna (migración `EmpleadoUsuarioMismaEmpresa`).
- **A003:** tope de 16 hosts para firmar URLs de MinIO con `PublicEndpoint = auto`. En producción `MINIO_PUBLIC_ENDPOINT` queda fijo y `ALLOWED_HOSTS` limita el encabezado `Host` (`AllowedHosts` en el compose).
- **N005:** `System.Security.Cryptography.Pkcs` fijado en 10.0.14.

### Corregido

- **A001:** las fechas JSON con `Z` (UTC) se pasan a la hora local del servidor, como las que traen offset.
- **A002:** eliminar una carga ya no borra la foto del ticket (es evidencia).
- **N002, N003:** documentación de 400/413 y restos de MySQL.

### Documentación

- `docs/RiesgosAceptados.md`: riesgos aceptados por escrito (A004 sin HTTPS por ahora, A006 correo único global, N006 asignaciones sin paginar).

### Pruebas

- `FechasConZonaTests`, casos de A005 y A007 en `SeguridadPorFilasTests`, dos casos de techo de usuarios en la matriz de lo propio (el gestor de roles de las pruebas ahora también tiene `usuario.ver` y `usuario.gestionar`), `MigracionesTests` (A009) y lotes GPS encimados (N001).

## [2.0.0] - 2026-11-12

Versión que cierra la revisión de código CR-9e1766d9852c005af924 (hallazgos F001 a F017) y cambia el motor a SQL Server. El despliegue paso a paso está en `docs/Despliegue-AWS.md`.

### ⚠️ Cambios que rompen compatibilidad

- **Motor de base de datos:** de MySQL 8 a **SQL Server 2022**. Las migraciones de MySQL se retiraron; una base de MySQL no se migra sola.
- **Estructura organizacional:** la empresa es la raíz. `EmpresaDto` ya no tiene `corporativoId` ni `divisionId`; `CorporativoDto` y `DivisionDto` tienen `empresaId` y `DivisionDto` ya no tiene `corporativoId`. Corporativos y divisiones son de cada empresa y se ligan N:M (`/api/corporativos/{id}/divisiones/{divisionId}`).
- **Listados paginados:** carros, viajes, solicitudes, cargas, notificaciones y puntos GPS devuelven una página (50 por defecto, máximo 200; puntos 1000 y máximo 5000). El total viene en `X-Total-Count`. Una app que no pagina ve solo los 50 más recientes.
- **Reportes de flota:** sin fechas son del último mes (antes, todo el historial) y el periodo máximo es de 366 días.
- **El actor sale del token:** `empleadoConfirmadorId` (aprobar y rechazar solicitudes) y `empleadoId` (cambiar estatus de un carro) se ignoran; un usuario sin empleado ya no puede hacer esas acciones.
- **Empresa inactiva:** no puede iniciar sesión (403) ni renovar el token (401), y solo la plataforma la activa o desactiva.
- **Producción:** la API ya no usa `sa`. Hay que agregar `MSSQL_APP_PASSWORD` y `MSSQL_MIGRADOR_PASSWORD` al `.env`.

### Seguridad

- Aislamiento entre empresas en tres capas: filtro global de EF Core, guardián de escrituras (`AislamientoEmpresaInterceptor`) y **Row-Level Security** de SQL Server (`rls.aislamiento_empresa`).
- `usuario.empresa_id`: cada cuenta es de una empresa y nunca pasa a otra.
- **Lo propio:** sin el privilegio de "ver todos", un empleado no ve ni toca viajes, notificaciones, dispositivos ni solicitudes de otros.
- **Techo de privilegios:** nadie da, quita ni asigna más privilegios de los que tiene.
- Aprobar, rechazar y cambiar estatus quedan a nombre del empleado del token.
- Desactivar una empresa cierra sus sesiones.
- Producción con dos logins: `apigasoline_app` (solo datos) y `apigasoline_migrador` (dueño solo de su base), creados por el contenedor `db-init`.
- Unicidad global (placas, correo, token del celular) responde 409 sin decir de quién es el dato repetido.

### Agregado

- Corporativos y divisiones por empresa, con ligas N:M y centros de costo colgados de un corporativo y una división (opcionales).
- `corporativo.gestionar` pasa a ámbito empresa.
- Índices únicos contra duplicados por concurrencia: un viaje en curso por carro, una pausa abierta por viaje y un punto GPS por momento de captura. El mismo lote GPS reenviado no duplica puntos.
- Paginación (`?pagina=`, `?tamanoPagina=`) con `X-Total-Count`, `X-Pagina` y `X-Tamano-Pagina`; las URLs firmadas de fotos solo se generan para la página devuelta.
- Tope de filas en reportes PDF (5,000 cargas) y Excel (50,000).
- `GET /salud` (público): estado y versión de la API.
- Commands y Queries `sealed` con interfaces `ICommand`/`IQuery`, y pruebas de arquitectura que lo exigen.
- Orquestación local con .NET Aspire (`dotnet run --project APIGasoline.AppHost`).

### Pruebas y CI

- Pruebas de integración con SQL Server real (Testcontainers): matriz de fugas entre empresas, lo propio y techo de privilegios, Row-Level Security, empresa inactiva, concurrencia y paginación.
- Pruebas de arquitectura del aislamiento (ninguna tabla sin filtro ni regla) y de AutoMapper (`AssertConfigurationIsValid`).
- GitHub Actions: compilación en Release, todas las pruebas, revisión de migraciones pendientes, `migraciones.sql` como artefacto y aviso de paquetes vulnerables.

### Retirado

- `migraciones.sql` del repositorio (lo genera el CI), `BD/esquema-mysql.sql`, `docs/BaseDatos.dbml` (queda `BD/BaseDatos.dbml`), `docs/CompletarUbicacionCargas.sql`, el plan de Postman duplicado y los `TestResults`.
- Contraseñas fijas en `pruebas-carga` (ahora por variables de entorno).

## [1.x] - hasta octubre de 2026

Versión sobre MySQL 8 (modelo v1 a v10: auditoría, RBAC, centros de costo, pausas, sesiones con refresh token).
