# Cambios de APIGasoline

El formato sigue [Keep a Changelog](https://keepachangelog.com/es-ES/1.1.0/) y las versiones siguen [SemVer](https://semver.org/lang/es/).

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
