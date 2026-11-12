# Base de datos — APIGasoline

**Versión del modelo:** 13 · **Actualizado:** 12 de noviembre de 2026 (fases 1 a 9)
**Motor actual:** SQL Server 2022 · **ORM:** Entity Framework Core 10 (Microsoft.EntityFrameworkCore.SqlServer)
**Última migración:** `20261112174728_IndicesUnicosConcurrencia`

> **Cambio de motor (fase 1):** la API pasó de MySQL 8 a SQL Server. El modelo (tablas, columnas, llaves, índices y reglas) es el mismo que se documenta abajo; solo cambian los tipos físicos. `esquema-sqlserver.sql` es el esquema actual. La tabla de equivalencias está en la sección 12 (el DDL de MySQL se retiró del repositorio en la fase 10; sigue en el historial de git).
>
> | MySQL | SQL Server |
> |---|---|
> | `binary(16)` | `binary(16)` (mismo formato RFC 4122; se conserva el orden por tiempo de los UUID v7) |
> | `enum('a','b')` | `varchar(n)` + `CHECK ([col] IN ('a','b'))` (17 columnas) |
> | `json` | `nvarchar(max)` + `CHECK (ISJSON([col]) = 1)` |
> | `timestamp` / `CURRENT_TIMESTAMP` | `datetime2(0)` / `SYSDATETIME()` |
> | `text` | `nvarchar(max)` |
> | `tinyint(1)` | `bit` |
> | `rol.empresa_clave` (columna generada) + `uq_rol_empresa_clave_nombre` | Se eliminó: `uq_rol_empresa_id_nombre (empresa_id, nombre)` sin filtro (SQL Server trata los NULL como iguales) |
> | `utf8mb4_0900_ai_ci` | `Modern_Spanish_CI_AI` |
> | `FOR UPDATE SKIP LOCKED` (outbox) | `WITH (UPDLOCK, READPAST, ROWLOCK)` |

Esta carpeta se armó originalmente **a partir del código de la API** (cuando el motor era MySQL), no de documentación previa:

1. Se aplicaron las 9 migraciones de entonces (el script idempotente que genera EF) sobre un MySQL 8.0 vacío.
2. Se leyó el resultado de `information_schema` (columnas, índices, llaves foráneas, CHECKs).
3. Se comparó contra las configuraciones de EF (`Persistence/Configurations/*.cs`) y los enums del dominio (`APIGasoline.Domain/Enums`) para confirmar que el código y las migraciones coinciden.
4. Después se contrastó con la documentación v9 que ya existía (ver la sección 9).

## Contenido de la carpeta

| Archivo | Qué es |
|---|---|
| `README.md` | Esta documentación. |
| `esquema-sqlserver.sql` | **Esquema actual (SQL Server)**: todas las migraciones en orden, generadas con `dotnet ef migrations script`. |
| `BaseDatos.dbml` | Diagrama para [dbdiagram.io](https://dbdiagram.io) (25 tablas). Es el único DBML del repositorio (el de `docs/` se retiró). Los tipos están escritos como en la versión MySQL; los equivalentes en SQL Server están arriba. |
| `produccion/crear-logins.sql` | Lo ejecuta el contenedor `db-init` en producción: crea la base y los logins `apigasoline_app` y `apigasoline_migrador` (la API no usa `sa`). |

> La forma oficial de crear la BD es con las migraciones (`BaseDatos:MigrarAlIniciar = true` o `dotnet ef database update`). `esquema-sqlserver.sql` es para leer y revisar el modelo.

---

## 1. Resumen

| Concepto | Cantidad |
|---|---|
| Tablas | 25 (+ `__EFMigrationsHistory` de EF) |
| Columnas | 239 |
| Llaves foráneas | 51 (25 compuestas con `empresa_id`) |
| Índices (sin contar PK) | 94 |
| Restricciones CHECK | 1 (`ck_revision_auditoria_alcance`) |
| Columnas generadas | 1 (`rol.empresa_clave`) |
| Columnas `enum` | 17, de 14 tipos distintos (todos coinciden con los enums del dominio) |
| Tablas con `empresa_id` | 20 (sin contar `empresa`) |
| Privilegios en el catálogo (`Permisos.cs`) | 42 |

```text
App móvil (KMP, offline-first) ─┐
                                ├──> API APIGasoline (.NET 10) ──> SQL Server (esta BD)
Web / Postman ──────────────────┘          │
                                           ├──> MinIO (fotos; en BD solo la llave)
                                           └──> Firebase Cloud Messaging (push vía outbox_mensaje)
```

---

## 2. Convenciones (verificadas en el esquema)

**Identificadores.** Todas las PK y FK son `binary(16)` con UUID v7 generado por la aplicación (`MySqlGuidFormat.Binary16`, orden RFC 4122, compatible con `UUID_TO_BIN` / `BIN_TO_UUID`). No hay `AUTO_INCREMENT` ni defaults en las llaves.

```sql
SELECT BIN_TO_UUID(usuario_id) AS usuario_id, email FROM usuario;
SELECT * FROM usuario WHERE usuario_id = UUID_TO_BIN('0192f3a4-...');
```

**Nombres.** Tablas en singular y `snake_case`; llaves `entidad_id`; índices `ix_<tabla>_<columnas>`, únicos `uq_<tabla>_<columnas>`, foráneas `fk_<tabla>_<destino>`.

**Enums.** Se guardan como `varchar(n)` + `CHECK` en `snake_case` (`en_mantenimiento`) mediante convertidores (`SnakeCaseEnumConverter` y otros). En JSON la API los expone en `camelCase`.

**Booleanos.** `bit`.

**Fechas.** `datetime2(0)`. Las capturadas en el celular (`fecha_inicio`, `fecha_carga`, `timestamp_captura`, `fecha_cambio`) conviven con `fecha_recepcion` (hora del servidor, default `SYSDATETIME()`); la diferencia indica cuánto tiempo estuvo el dato sin sincronizar.

**Intercalación.** `Modern_Spanish_CI_AI` (no distingue mayúsculas ni acentos). La pone EF al crear la base (`UseCollation`); en producción la crea `db-init` con la misma intercalación.

**Borrado lógico.** `empresa.eliminado` y `empleado.eliminado` (con filtro global `HasQueryFilter` en EF). Los carros usan `dado_de_baja` + `fecha_baja` + `motivo_baja`.

**Archivos.** En la BD solo se guarda la llave de MinIO, nunca la URL:

```text
empresa/{empresa_id}/{entidad}/{id}/{carpeta}/{tipo}-{uuid}.{ext}
```

**Concurrencia optimista.** `carro.estado` (dos aprobaciones simultáneas del mismo carro: la segunda recibe 409) y `sesion_usuario.token_hash` (dos renovaciones simultáneas del mismo refresh token: solo una gana).

---

## 3. Multiempresa (tenant = `empresa`)

### 3.1. Lo que garantiza la base de datos

- **25 llaves foráneas compuestas** `(empresa_id, x_id) → tabla(empresa_id, x_id)`. Cada tabla referenciada tiene su índice único `uq_<tabla>_empresa_id_<tabla>_id`. Con eso **la base de datos rechaza** relaciones que cruzan empresas, por ejemplo un viaje de la empresa A con un carro de la B:

| Tabla | Referencias compuestas (misma empresa) |
|---|---|
| `solicitud_reserva` | carro, empleado solicitante, empleado confirmador |
| `viaje` | carro, solicitud_reserva, empleado operador, centro_costo |
| `viaje_pausa`, `punto_ruta`, `ruta_procesada` | viaje |
| `carga_combustible` | viaje, centro_costo |
| `cambio_estatus_carro` | carro, empleado, viaje afectado |
| `dispositivo_empleado`, `notificacion` | empleado |
| `revision_auditoria` | viaje, carro, empleado revisor |
| `corporativo_division` | corporativo y división de la misma empresa (fase 8) |
| `centro_costo` | corporativo, división y la liga corporativo-división, todos opcionales (fase 8; SQL Server no revisa una FK compuesta con alguna columna NULL) |

### 3.2. Lo que la base de datos **no** garantiza (lo hace la API)

- **Leer datos de otra empresa.** La BD no filtra lecturas. Desde la fase 3 lo hace la API en un solo lugar: cada consulta de EF Core lleva el filtro global `Empresa` (`AppDbContext.AplicarFiltroPorEmpresa`) y cada `SaveChanges` pasa por un guardián (`AislamientoEmpresaInterceptor`) que rechaza con 403 crear, modificar o borrar algo de otra empresa. Ver sección 3.3.
- **Seguridad:** `usuario_rol` y `rol_privilegio` no llevan `empresa_id`. Que a un usuario de la empresa A no se le asigne un rol de la B, y que un rol de empresa no reciba privilegios de ámbito `plataforma`, se valida solo en la API.
- **`sesion_usuario.empresa_id`** y **`sesion_usuario.revocada_por_usuario_id`** no tienen FK.
- **Referencias lógicas sin FK:** `notificacion.referencia_id`, `outbox_mensaje.agregado_id`, `auditoria_evento.entidad_id`.
- **`privilegio`** no tiene `empresa_id` (es el catálogo de la plataforma). Desde la fase 8 `corporativo` y `division` sí son de una empresa.
- **`usuario.empresa_id`** (fase 3) dice de qué empresa es la cuenta; NULL = administrador de la plataforma o cuenta aún sin asignar. Al crearle su empleado se le asigna la empresa del empleado, y una cuenta nunca pasa de una empresa a otra. La migración `AgregarEmpresaAUsuario` la llenó con la empresa del empleado (o, sin empleado, la de sus roles de empresa).
- **`carro.placas` y `usuario.email`** son únicos en todo el sistema, no por empresa.

### 3.3. Aislamiento en la API (fase 3)

| Tabla | Qué ve un usuario de la empresa X (sin `plataforma.multiempresa`) |
|---|---|
| Con `empresa_id` (corporativo, division, corporativo_division, carro, centro_costo, empleado, solicitud_reserva, viaje, viaje_pausa, punto_ruta, ruta_procesada, carga_combustible, cambio_estatus_carro, notificacion, dispositivo_empleado, revision_auditoria) | Solo `empresa_id = X` |
| `empresa` | Solo X |
| `usuario` | Solo `empresa_id = X` |
| `rol` | Los de X y los globales (los globales solo se leen) |
| `usuario_rol` | Los de usuarios de X |
| `rol_privilegio` | Los de roles que puede ver |
| `privilegio` | Todo (catálogo); solo la plataforma lo modifica |
| `auditoria_evento`, `outbox_mensaje`, `sesion_usuario` | Sin filtro: sus endpoints validan la empresa o el usuario dueño |

- Un registro de otra empresa "no existe": su detalle responde 404 y las listas no lo traen.
- Escribir con el `empresa_id` de otra empresa, colgar algo de un registro ajeno, asignar o modificar un rol global, o cambiar el `empresa_id` de un registro responde **403**. Para la plataforma, cambiar `empresa_id` cuando es parte de una llave compuesta responde **409** (antes era un 500).
- Sin usuario (login, refresh, workers, siembra) y con `plataforma.multiempresa` no hay filtro.
- Las validaciones de unicidad global (placas, correo, token del celular) ignoran solo el filtro `Empresa` y devuelven sí/no.
- Las pruebas `tests/APIGasoline.Api.IntegrationTests` (matriz de fugas) y `tests/APIGasoline.Architecture.Tests` (`AislamientoEmpresaTests`) lo vigilan.

### 3.4. Row-Level Security en SQL Server (fase 6)

Segundo candado, dentro de la base de datos: aunque una consulta se salte el filtro de EF (SQL a mano, `IgnoreQueryFilters`, un error), SQL Server tampoco devuelve ni deja escribir filas de otra empresa.

- **Cómo sabe SQL Server la empresa:** cada vez que EF abre una conexión, `ContextoSesionSqlInterceptor` manda dos valores de `SESSION_CONTEXT`: `apigasoline.empresa_id` (binary(16)) y `apigasoline.ve_todas` (bit, 1 = plataforma o procesos del sistema).
- **Política:** `rls.aislamiento_empresa` (migración `SeguridadPorFilas`; la fase 8 le agrega corporativo, division y corporativo_division con `ALTER SECURITY POLICY`), sobre las mismas 21 tablas que filtra EF:

| Tablas | Leer (FILTER) | Escribir (BLOCK) |
|---|---|---|
| corporativo, division, corporativo_division, carro, centro_costo, empleado, solicitud_reserva, viaje, viaje_pausa, punto_ruta, ruta_procesada, carga_combustible, cambio_estatus_carro, notificacion, dispositivo_empleado, revision_auditoria, empresa, usuario | `rls.fn_misma_empresa(empresa_id)` | La misma, AFTER INSERT y AFTER UPDATE (no se puede dejar una fila en otra empresa) |
| rol | `rls.fn_rol_visible(empresa_id)`: propios y globales | `rls.fn_misma_empresa(empresa_id)`, también BEFORE UPDATE/DELETE (los globales no se tocan) |
| usuario_rol | `rls.fn_usuario_de_la_empresa(usuario_id)` | `rls.fn_asignacion_de_la_empresa(usuario_id, rol_id)`: usuario y rol de la empresa |
| rol_privilegio | `rls.fn_rol_privilegio_visible(rol_id)` | `rls.fn_rol_de_la_empresa(rol_id)` |

- **Sin `SESSION_CONTEXT` no se ve nada** (falla cerrado). Para consultar a mano en SSMS, Azure Data Studio o un script de mantenimiento, ejecuta primero:

  ```sql
  EXEC sys.sp_set_session_context N'apigasoline.ve_todas', 1;
  ```

- **Índices únicos de toda la BD** (placas, correo, token del celular): si el valor repetido es de otra empresa, el handler no lo ve y el que lo detecta es el índice. La API responde 409 con un mensaje que no dice el valor ni de quién es (`ViolacionUnicidad`).
- **La política está con `SCHEMABINDING = ON`:** una migración que quiera cambiar `empresa_id` (o `usuario_id`/`rol_id` en `usuario_rol` y `rol_privilegio`) en estas tablas tiene que quitar la política, cambiar la columna y volver a crearla.
- **Tablas sin RLS:** `privilegio` (catálogo de la plataforma) y `auditoria_evento`, `outbox_mensaje`, `sesion_usuario` (técnicas), igual que en EF.
- `SeguridadPorFilasTests` (pruebas de integración) habla con SQL Server directo para comprobarlo y falla si una tabla filtrada en EF no tiene su política.

---

## 4. Seguridad (RBAC y sesiones)

```text
usuario N ──< usuario_rol >── N rol N ──< rol_privilegio >── N privilegio
usuario 1 ──< sesion_usuario
```

- Privilegios efectivos de un usuario = privilegios **activos** de sus roles **activos** que sean globales (`rol.empresa_id IS NULL`) o de su empresa. No van en el JWT: la API los consulta con caché corta.
- Dos roles globales no pueden llamarse igual: en SQL Server el índice único `uq_rol_empresa_id_nombre` trata los `NULL` como iguales (en MySQL hacía falta la columna generada `rol.empresa_clave`, que se eliminó en la fase 1).
- **Sesiones (v10):** cada inicio de sesión crea una fila en `sesion_usuario`. Solo se guarda el SHA-256 del refresh token. En cada renovación el token rota y el hash anterior se conserva para detectar reuso (si alguien usa un token ya reemplazado, se revoca la sesión). La expiración es deslizante (`expira_en`) con tope absoluto (`expira_absoluta_en`). Límite de sesiones por usuario y limpieza periódica configurables en `Sesiones` (appsettings). Endpoints: `POST /api/auth/login | refresh | logout`, `GET /api/auth/sesiones`, `DELETE /api/auth/sesiones/{id}`, `POST /api/auth/sesiones/cerrar-otras`.
- **Siembra:** al arrancar (`Seguridad:SembrarAlIniciar`), `SembrarSeguridadCommand` crea los 42 privilegios del catálogo que falten, el rol global **Administrador** y, si se configuró, el usuario admin. No hay `HasData` en las migraciones: con una BD recién migrada las tablas de seguridad están vacías hasta que arranca la API.

---

## 5. Comportamiento al borrar (ON DELETE)

| Regla | Relaciones |
|---|---|
| **CASCADE** | `viaje` → `punto_ruta`, `viaje_pausa`, `ruta_procesada`; `empleado` → `dispositivo_empleado`; `usuario` → `usuario_rol`, `sesion_usuario`; `rol` → `usuario_rol`, `rol_privilegio`; `privilegio` → `rol_privilegio` |
| **RESTRICT** | Todas las demás (41). Por eso un carro o viaje con historial no se puede borrar físicamente; la API responde 409 y pide darlo de baja. |

---

## 6. Diagrama de relaciones

```text
EMPRESA (la raíz, fase 8)
         ├──< CORPORATIVO >──┐
         │                   ├── CORPORATIVO_DIVISION (N:M, misma empresa)
         ├──< DIVISION    >──┘
         ├──< CENTRO_COSTO (corporativo y división opcionales; si trae los dos, deben estar ligados)
         ├──< ROL (propios; los globales tienen empresa_id NULL)
         ├──< EMPLEADO ── 1:1 ── USUARIO ──< USUARIO_ROL >── ROL ──< ROL_PRIVILEGIO >── PRIVILEGIO
         │      ├──< DISPOSITIVO_EMPLEADO                └──< SESION_USUARIO
         │      └──< NOTIFICACION (destinatario)
         ├──< CARRO
         │      ├──< CAMBIO_ESTATUS_CARRO (empleado, viaje afectado)
         │      └──< SOLICITUD_RESERVA (solicitante, confirmador)
         │              └──< VIAJE (operador, centro de costo)
         │                     ├──< VIAJE_PAUSA
         │                     ├──< PUNTO_RUTA
         │                     ├─── RUTA_PROCESADA (1:1)
         │                     └──< CARGA_COMBUSTIBLE (centro de costo)
         ├──< OUTBOX_MENSAJE
         ├──< AUDITORIA_EVENTO
         └──< REVISION_AUDITORIA (viaje o carro + periodo, revisor)
```

---

## 7. Diccionario de datos

`NN` = NOT NULL. "FK (compuesta)" = la relación incluye `empresa_id`. Las descripciones salen de las configuraciones de EF, las entidades del dominio y la documentación v9.

### Seguridad (RBAC y sesiones)

#### `usuario`

Credenciales para iniciar sesión (correo + contraseña cifrada con PBKDF2).

| Columna | Tipo | Llaves / restricciones | Default | Descripción |
|---|---|---|---|---|
| `usuario_id` | binary(16) | PK |  | Identificador (UUID v7 generado por la app). |
| `email` | varchar(150) | NN, único |  | Correo; es el usuario de acceso. Único en todo el sistema. |
| `password_hash` | varchar(255) | NN |  | Contraseña cifrada con PBKDF2. Nunca en texto plano. |
| `estado` | enum('activo','inactivo') | NN | activo | Un usuario inactivo no puede iniciar sesión (403). |
| `fecha_registro` | timestamp |  | now() | Fecha de alta. |
| `empresa_id` | binary(16) | FK → empresa |  | Empresa dueña de la cuenta (fase 3). NULL = plataforma o aún sin asignar; se asigna al crearle su empleado. |

**Índices:** `uq_usuario_email` (email) **único**; `ix_usuario_empresa_id` (empresa_id)

**Llaves foráneas:** (empresa_id) → `empresa`(empresa_id) ON DELETE RESTRICT

#### `rol`

Agrupa privilegios con un nombre. empresa_id NULL = rol global.

| Columna | Tipo | Llaves / restricciones | Default | Descripción |
|---|---|---|---|---|
| `rol_id` | binary(16) | PK |  | Identificador (UUID v7 generado por la app). |
| `empresa_id` | binary(16) | FK → empresa |  | NULL = rol global (plataforma); con valor = rol propio de esa empresa. |
| `nombre` | varchar(100) | NN |  | Nombre visible del rol. Único dentro de la empresa (y entre roles globales). |
| `descripcion` | varchar(255) |  |  | Descripción opcional. |
| `activo` | tinyint(1) | NN | 1 | Un rol inactivo no da privilegios. |
| `empresa_clave` | binary(16) |  | generada | Columna GENERADA (STORED): COALESCE(empresa_id, 0x00…). Solo existe para que el índice único también aplique a roles globales. La API no la escribe. |

**Índices:** `ix_rol_empresa_id` (empresa_id); `uq_rol_empresa_clave_nombre` (empresa_clave, nombre) **único**

**Llaves foráneas:** (empresa_id) → `empresa`(empresa_id) ON DELETE RESTRICT

#### `privilegio`

Catálogo de acciones del sistema (modulo.accion). Lo siembra la API al arrancar.

| Columna | Tipo | Llaves / restricciones | Default | Descripción |
|---|---|---|---|---|
| `privilegio_id` | binary(16) | PK |  | Identificador (UUID v7 generado por la app). |
| `nombre` | varchar(100) | NN |  | Texto visible; se puede cambiar sin afectar permisos. |
| `descripcion` | varchar(255) |  |  | Qué permite. |
| `activo` | tinyint(1) | NN | 1 | Un privilegio inactivo no cuenta. |
| `clave` | varchar(100) | NN, único | '' | Clave inmutable modulo.accion (ej. viaje.registrar). Es lo que revisa el código. |
| `modulo` | varchar(50) | NN | '' | Módulo para agruparlo en la pantalla de roles. |
| `ambito` | enum('plataforma','empresa') | NN | empresa | plataforma = solo roles globales; empresa = cualquier rol. |

**Índices:** `ix_privilegio_modulo` (modulo); `uq_privilegio_clave` (clave) **único**

#### `usuario_rol`

Roles asignados a cada usuario (N:M).

| Columna | Tipo | Llaves / restricciones | Default | Descripción |
|---|---|---|---|---|
| `usuario_rol_id` | binary(16) | PK |  | Identificador (UUID v7 generado por la app). |
| `usuario_id` | binary(16) | NN, FK → usuario |  | Referencia a usuario. |
| `rol_id` | binary(16) | NN, FK → rol |  | Referencia a rol. |

**Índices:** `ix_usuario_rol_rol_id` (rol_id); `ix_usuario_rol_usuario_id` (usuario_id); `uq_usuario_rol_usuario_id_rol_id` (usuario_id, rol_id) **único**

**Llaves foráneas:** (rol_id) → `rol`(rol_id) ON DELETE CASCADE; (usuario_id) → `usuario`(usuario_id) ON DELETE CASCADE

#### `rol_privilegio`

Privilegios de cada rol (N:M).

| Columna | Tipo | Llaves / restricciones | Default | Descripción |
|---|---|---|---|---|
| `rol_privilegio_id` | binary(16) | PK |  | Identificador (UUID v7 generado por la app). |
| `rol_id` | binary(16) | NN, FK → rol |  | Referencia a rol. |
| `privilegio_id` | binary(16) | NN, FK → privilegio |  | Referencia a privilegio. |

**Índices:** `ix_rol_privilegio_privilegio_id` (privilegio_id); `ix_rol_privilegio_rol_id` (rol_id); `uq_rol_privilegio_rol_id_privilegio_id` (rol_id, privilegio_id) **único**

**Llaves foráneas:** (privilegio_id) → `privilegio`(privilegio_id) ON DELETE CASCADE; (rol_id) → `rol`(rol_id) ON DELETE CASCADE

#### `sesion_usuario`

Una fila por inicio de sesión en un dispositivo (refresh token rotativo, solo se guarda su hash).

| Columna | Tipo | Llaves / restricciones | Default | Descripción |
|---|---|---|---|---|
| `sesion_usuario_id` | binary(16) | PK |  | Identificador (UUID v7 generado por la app). |
| `usuario_id` | binary(16) | NN, FK → usuario |  | Usuario dueño de la sesión. Al borrar el usuario se borran sus sesiones (CASCADE). |
| `empresa_id` | binary(16) |  |  | Empresa del empleado al iniciar sesión (NULL = usuario sin empleado). Sirve para revocar por empresa. Sin FK. |
| `token_hash` | char(64) | NN, único |  | SHA-256 (hex, 64) del refresh token vigente. El token nunca se guarda. Es token de concurrencia: si llegan dos renovaciones a la vez, solo una gana. |
| `token_hash_anterior` | char(64) |  |  | Hash del token reemplazado en la última rotación. Si alguien lo reutiliza se revoca toda la sesión (señal de robo). |
| `plataforma` | enum('android','ios','web') |  |  | Sistema del dispositivo. |
| `nombre_dispositivo` | varchar(100) |  |  | Nombre que manda la app (para la lista de sesiones). |
| `ip` | varchar(45) |  |  | IP del cliente (IPv4 o IPv6). |
| `user_agent` | varchar(255) |  |  | User-Agent del cliente. |
| `fecha_creacion` | timestamp | NN |  | Inicio de sesión. |
| `fecha_ultimo_uso` | timestamp | NN |  | Última renovación. |
| `fecha_rotacion` | timestamp |  |  | Última vez que se rotó el refresh token. |
| `expira_en` | timestamp | NN |  | Expiración deslizante (Sesiones:RefreshDias, 30 por defecto). Se extiende en cada renovación. |
| `expira_absoluta_en` | timestamp | NN |  | Tope desde el inicio de sesión (Sesiones:RefreshMaxDias, 90 por defecto). Después hay que volver a escribir la contraseña. |
| `fecha_revocacion` | timestamp |  |  | NULL = sesión activa. |
| `motivo_revocacion` | enum('cierre_sesion','cerrada_por_usuario','cerrada_por_administrador','cambio_password','usuario_desactivado','empleado_dado_de_baja','empleado_reasignado','empresa_eliminada','reuso_detectado','acceso_revocado','limite_sesiones','empresa_desactivada') |  |  | Por qué se cerró (logout, cambio de contraseña, reuso detectado, límite de sesiones, empresa desactivada…). |
| `revocada_por_usuario_id` | binary(16) |  |  | Quién la cerró (si fue un administrador). Sin FK. |

**Índices:** `ix_sesion_usuario_empresa_id_fecha_revocacion` (empresa_id, fecha_revocacion); `ix_sesion_usuario_expira_en` (expira_en); `ix_sesion_usuario_token_hash_anterior` (token_hash_anterior); `ix_sesion_usuario_usuario_id_fecha_revocacion_expira_en` (usuario_id, fecha_revocacion, expira_en); `uq_sesion_usuario_token_hash` (token_hash) **único**

**Llaves foráneas:** (usuario_id) → `usuario`(usuario_id) ON DELETE CASCADE

### Estructura corporativa

Desde la fase 8 (migración `EstructuraOrganizacional`) la **empresa es la raíz**:

| Relación | Cardinalidad |
|---|---|
| Empresa → corporativos | 1:N |
| Empresa → divisiones | 1:N |
| Empresa → centros de costo | 1:N |
| Corporativo ↔ divisiones | N:M (`corporativo_division`) |
| Corporativo → centros de costo | 1:N (opcional) |
| División → centros de costo | 1:N (opcional) |

Cada empresa administra los suyos con el privilegio `corporativo.gestionar` (ámbito empresa desde la fase 8) y los ve con `corporativo.ver`.

#### `empresa`

El tenant y la raíz de la organización: todo lo de negocio cuelga de una empresa.

| Columna | Tipo | Llaves / restricciones | Default | Descripción |
|---|---|---|---|---|
| `empresa_id` | binary(16) | PK |  | Identificador (UUID v7 generado por la app). |
| `nombre` | varchar(150) | NN |  | Nombre (aparece en los reportes). |
| `eliminado` | tinyint(1) | NN | 0 | Borrado lógico. Filtro global de EF (HasQueryFilter). Sus empleados ya no pueden iniciar sesión. |
| `fecha_registro` | timestamp |  | now() | Fecha de alta. |
| `activo` | tinyint(1) | NN | 1 | Empresa activa o inactiva (no es lo mismo que eliminada). |

Sin llaves foráneas propias (fase 8: ya no apunta a corporativo ni a división).

#### `corporativo`

Grupo corporativo de una empresa.

| Columna | Tipo | Llaves / restricciones | Default | Descripción |
|---|---|---|---|---|
| `corporativo_id` | binary(16) | PK |  | Identificador (UUID v7 generado por la app). |
| `empresa_id` | binary(16) | NN, FK → empresa |  | Empresa dueña (tenant). |
| `nombre` | nvarchar(150) | NN |  | Nombre del corporativo. Único dentro de la empresa. |
| `activo` | bit | NN | 1 | Activo / inactivo. |
| `fecha_registro` | datetime2(0) |  | SYSDATETIME() | Fecha de alta. |

**Índices:** `uq_corporativo_empresa_id_corporativo_id` (empresa_id, corporativo_id) **único** (llave alterna); `uq_corporativo_empresa_id_nombre` (empresa_id, nombre) **único**

**Llaves foráneas:** (empresa_id) → `empresa`(empresa_id) ON DELETE RESTRICT

#### `division`

División de una empresa; se liga a uno o varios corporativos de la misma empresa.

| Columna | Tipo | Llaves / restricciones | Default | Descripción |
|---|---|---|---|---|
| `division_id` | binary(16) | PK |  | Identificador (UUID v7 generado por la app). |
| `empresa_id` | binary(16) | NN, FK → empresa |  | Empresa dueña (tenant). |
| `nombre` | nvarchar(100) | NN |  | Nombre de la división. Único dentro de la empresa. |
| `activo` | bit | NN | 1 | Activo / inactivo. |
| `fecha_registro` | datetime2(0) |  | SYSDATETIME() | Fecha de alta. |

**Índices:** `uq_division_empresa_id_division_id` (empresa_id, division_id) **único** (llave alterna); `uq_division_empresa_id_nombre` (empresa_id, nombre) **único**

**Llaves foráneas:** (empresa_id) → `empresa`(empresa_id) ON DELETE RESTRICT

#### `corporativo_division`

Liga N:M entre corporativos y divisiones de la misma empresa.

| Columna | Tipo | Llaves / restricciones | Default | Descripción |
|---|---|---|---|---|
| `corporativo_division_id` | binary(16) | PK |  | Identificador (UUID v7 generado por la app). |
| `empresa_id` | binary(16) | NN |  | Empresa (tenant); forma parte de las dos FK compuestas. |
| `corporativo_id` | binary(16) | NN, FK compuesta → corporativo |  | Corporativo. |
| `division_id` | binary(16) | NN, FK compuesta → division |  | División. |
| `fecha_registro` | datetime2(0) |  | SYSDATETIME() | Fecha en que se ligaron. |

**Índices:** `uq_corporativo_division_corporativo_id_division_id` (corporativo_id, division_id) **único**; `uq_corporativo_division_empresa_id_corporativo_id_division_id` (empresa_id, corporativo_id, division_id) **único** (llave alterna, la usa centro_costo); `ix_corporativo_division_empresa_id_division_id` (empresa_id, division_id)

**Llaves foráneas:** (empresa_id, corporativo_id) → `corporativo`(empresa_id, corporativo_id) ON DELETE RESTRICT; (empresa_id, division_id) → `division`(empresa_id, division_id) ON DELETE RESTRICT

#### `centro_costo`

Área a la que se carga el gasto de viajes y cargas.

| Columna | Tipo | Llaves / restricciones | Default | Descripción |
|---|---|---|---|---|
| `centro_costo_id` | binary(16) | PK |  | Identificador (UUID v7 generado por la app). |
| `empresa_id` | binary(16) | NN, FK → empresa |  | Empresa (tenant). |
| `corporativo_id` | binary(16) | FK compuesta → corporativo |  | Corporativo de la misma empresa (opcional, fase 8). |
| `division_id` | binary(16) | FK compuesta → division |  | División de la misma empresa (opcional, fase 8). Con corporativo, la pareja debe estar ligada. |
| `nombre` | varchar(150) | NN |  | Nombre (ej. Operaciones central). |
| `clave` | varchar(100) | NN |  | Clave del centro (ej. CC-104). Única dentro de la empresa. |
| `activo` | tinyint(1) | NN | 1 | Solo los activos se pueden elegir. |
| `fecha_registro` | timestamp |  | now() | Fecha de alta. |

**Índices:** `ix_centro_costo_empresa_id` (empresa_id); `ix_centro_costo_empresa_id_corporativo_id_division_id` (empresa_id, corporativo_id, division_id); `ix_centro_costo_empresa_id_division_id` (empresa_id, division_id); `uq_centro_costo_empresa_id_centro_costo_id` (empresa_id, centro_costo_id) **único**; `uq_centro_costo_empresa_id_clave` (empresa_id, clave) **único**

**Llaves foráneas:** (empresa_id) → `empresa`(empresa_id) ON DELETE RESTRICT; (empresa_id, corporativo_id) → `corporativo`(empresa_id, corporativo_id) ON DELETE RESTRICT; (empresa_id, division_id) → `division`(empresa_id, division_id) ON DELETE RESTRICT; (empresa_id, corporativo_id, division_id) → `corporativo_division`(empresa_id, corporativo_id, division_id) ON DELETE RESTRICT

### Personas

#### `empleado`

La persona (nombre, fotos, licencia), ligada 1:1 a un usuario.

| Columna | Tipo | Llaves / restricciones | Default | Descripción |
|---|---|---|---|---|
| `empleado_id` | binary(16) | PK |  | Identificador (UUID v7 generado por la app). |
| `empresa_id` | binary(16) | NN, FK → empresa |  | Empresa (tenant). |
| `usuario_id` | binary(16) | NN, único, FK → usuario |  | Usuario con el que inicia sesión (1:1, único). |
| `nombre_completo` | varchar(150) | NN |  | Nombre de la persona. |
| `foto_perfil` | varchar(255) |  |  | Llave en MinIO de la foto de perfil. |
| `foto_licencia` | varchar(255) |  |  | Llave en MinIO de la licencia de conducir. |
| `eliminado` | tinyint(1) | NN | 0 | Borrado lógico. Filtro global de EF (HasQueryFilter). |
| `fecha_alta` | timestamp |  | now() | Fecha de alta. |
| `fecha_carga_licencia` | timestamp |  |  | NULL = no ha subido licencia y no puede solicitar carros. |

**Índices:** `ix_empleado_empresa_id` (empresa_id); `uq_empleado_empresa_id_empleado_id` (empresa_id, empleado_id) **único**; `uq_empleado_usuario_id` (usuario_id) **único**

**Llaves foráneas:** (empresa_id) → `empresa`(empresa_id) ON DELETE RESTRICT; (usuario_id) → `usuario`(usuario_id) ON DELETE RESTRICT

#### `dispositivo_empleado`

Celulares (tokens FCM) de cada empleado para push.

| Columna | Tipo | Llaves / restricciones | Default | Descripción |
|---|---|---|---|---|
| `dispositivo_empleado_id` | binary(16) | PK |  | Identificador (UUID v7 generado por la app). |
| `empresa_id` | binary(16) | NN, FK → empleado (compuesta), FK → empresa |  | Empresa (tenant). |
| `empleado_id` | binary(16) | NN, FK → empleado (compuesta) |  | Referencia a empleado. |
| `token_fcm` | varchar(512) | NN, único |  | Token de Firebase Cloud Messaging. Único. |
| `plataforma` | enum('android','ios','web') | NN |  | Sistema del dispositivo. |
| `activo` | tinyint(1) | NN | 1 | Se desactiva al cerrar sesión o si Firebase rechaza el token. |
| `fecha_registro` | timestamp | NN | now() | Fecha de alta. |
| `fecha_ultimo_uso` | timestamp |  |  | Última renovación o último push exitoso. |

**Índices:** `ix_dispositivo_empleado_empleado_id_activo` (empleado_id, activo); `ix_dispositivo_empleado_empresa_id` (empresa_id); `ix_dispositivo_empleado_empresa_id_empleado_id` (empresa_id, empleado_id); `uq_dispositivo_empleado_empresa_id_dispositivo_empleado_id` (empresa_id, dispositivo_empleado_id) **único**; `uq_dispositivo_empleado_token_fcm` (token_fcm) **único**

**Llaves foráneas:** (empresa_id, empleado_id) → `empleado`(empresa_id, empleado_id) ON DELETE CASCADE; (empresa_id) → `empresa`(empresa_id) ON DELETE RESTRICT

### Vehículos

#### `carro`

Vehículos de la empresa, su estado, tipo y kilometraje.

| Columna | Tipo | Llaves / restricciones | Default | Descripción |
|---|---|---|---|---|
| `carro_id` | binary(16) | PK |  | Identificador (UUID v7 generado por la app). |
| `empresa_id` | binary(16) | NN, FK → empresa |  | Empresa (tenant). |
| `marca` | varchar(80) | NN |  | Marca. |
| `modelo` | varchar(80) | NN |  | Modelo. |
| `anio` | int |  |  | Año del modelo. |
| `color` | varchar(50) |  |  | Color. |
| `placas` | varchar(20) | NN, único |  | Únicas en todo el sistema (no por empresa). |
| `foto_vehiculo` | varchar(255) |  |  | Llave en MinIO. |
| `capacidad_tanque` | decimal(6,2) |  |  | Litros. Sirve para alertar cargas imposibles. |
| `kilometraje_actual` | decimal(10,2) | NN | 0.00 | Último kilometraje conocido. Avanza al finalizar cada viaje. |
| `estado` | enum('disponible','ocupado','en_mantenimiento') | NN | disponible | Estado operativo. Es token de concurrencia: si dos aprueban el mismo carro a la vez, el segundo recibe 409. |
| `fecha_alta` | timestamp |  | now() | Fecha de alta. |
| `dado_de_baja` | tinyint(1) | NN | 0 | Baja del vehículo (ya no se puede pedir). El historial se conserva. |
| `fecha_baja` | timestamp |  |  | Cuándo se dio de baja. |
| `motivo_baja` | varchar(255) |  |  | Por qué se dio de baja. |
| `tipo_vehiculo` | enum('carro','moto','camioneta','camion') | NN | carro | v9. La app cambia el ícono según el tipo. |

**Índices:** `ix_carro_empresa_id` (empresa_id); `uq_carro_empresa_id_carro_id` (empresa_id, carro_id) **único**; `uq_carro_placas` (placas) **único**

**Llaves foráneas:** (empresa_id) → `empresa`(empresa_id) ON DELETE RESTRICT

#### `cambio_estatus_carro`

Bitácora de cambios de estado del carro (fallas, mantenimiento, liberación).

| Columna | Tipo | Llaves / restricciones | Default | Descripción |
|---|---|---|---|---|
| `cambio_estatus_carro_id` | binary(16) | PK |  | Identificador (UUID v7 generado por la app). |
| `empresa_id` | binary(16) | NN, FK → carro (compuesta), FK → empleado (compuesta), FK → empresa, FK → viaje (compuesta) |  | Empresa (tenant). |
| `carro_id` | binary(16) | NN, FK → carro (compuesta) |  | Referencia a carro. |
| `empleado_id` | binary(16) | NN, FK → empleado (compuesta) |  | Quién hizo el cambio. |
| `viaje_afectado_id` | binary(16) | FK → viaje (compuesta) |  | Viaje que se interrumpió por el cambio. |
| `estatus_anterior` | enum('disponible','ocupado','en_mantenimiento') | NN |  | Estado antes del cambio. |
| `estatus_nuevo` | enum('disponible','ocupado','en_mantenimiento') | NN |  | Estado después del cambio. |
| `motivo` | varchar(255) |  |  | Descripción libre. |
| `fecha_cambio` | timestamp |  | now() | Cuándo ocurrió (hora del celular). |
| `fecha_recepcion` | timestamp |  | now() | Cuándo llegó al servidor (hora del servidor). |
| `foto_evidencia` | varchar(255) |  |  | Llave en MinIO de la foto de la falla o daño. |
| `tipo_motivo` | enum('liberacion','falla_mecanica','choque_o_golpe','servicio','emergencia','otro') |  |  | Clasificación del motivo (sirve como registro de percances). |

**Índices:** `ix_cambio_estatus_carro_empresa_id` (empresa_id); `ix_cambio_estatus_carro_empresa_id_carro_id` (empresa_id, carro_id); `ix_cambio_estatus_carro_empresa_id_empleado_id` (empresa_id, empleado_id); `ix_cambio_estatus_carro_empresa_id_tipo_motivo` (empresa_id, tipo_motivo); `ix_cambio_estatus_carro_empresa_id_viaje_afectado_id` (empresa_id, viaje_afectado_id); `uq_cambio_estatus_carro_empresa_id_cambio_estatus_carro_id` (empresa_id, cambio_estatus_carro_id) **único**

**Llaves foráneas:** (empresa_id, carro_id) → `carro`(empresa_id, carro_id) ON DELETE RESTRICT; (empresa_id, empleado_id) → `empleado`(empresa_id, empleado_id) ON DELETE RESTRICT; (empresa_id) → `empresa`(empresa_id) ON DELETE RESTRICT; (empresa_id, viaje_afectado_id) → `viaje`(empresa_id, viaje_id) ON DELETE RESTRICT

### Reservas

#### `solicitud_reserva`

Solicitud de un empleado para usar un carro; se aprueba o rechaza.

| Columna | Tipo | Llaves / restricciones | Default | Descripción |
|---|---|---|---|---|
| `solicitud_reserva_id` | binary(16) | PK |  | Identificador (UUID v7 generado por la app). |
| `empresa_id` | binary(16) | NN, FK → carro (compuesta), FK → empleado (compuesta), FK → empresa |  | Empresa (tenant). |
| `carro_id` | binary(16) | NN, FK → carro (compuesta) |  | Referencia a carro. |
| `empleado_solicitante_id` | binary(16) | NN, FK → empleado (compuesta) |  | Quién pide el carro. |
| `fecha_solicitud` | timestamp |  | now() | Cuándo se pidió. |
| `estado` | enum('pendiente','aprobada','rechazada','cancelada','finalizada') | NN | pendiente | pendiente → aprobada → finalizada; o rechazada / cancelada. |
| `empleado_confirmador_id` | binary(16) | FK → empleado (compuesta) |  | Quién la aprobó o rechazó. |
| `fecha_resolucion` | timestamp |  |  | Cuándo se aprobó o rechazó. |
| `motivo_resolucion` | varchar(255) |  |  | Comentario de quien resolvió (obligatorio al rechazar). |
| `fecha_liberacion` | timestamp |  |  | Cuándo se devolvió el carro. |
| `observaciones_liberacion` | varchar(500) |  |  | Comentarios al devolver el carro. |
| `motivo_solicitud` | varchar(255) |  |  | v8. Para qué necesita el carro. La API lo exige en solicitudes nuevas. |

**Índices:** `ix_solicitud_reserva_empresa_id` (empresa_id); `ix_solicitud_reserva_empresa_id_carro_id` (empresa_id, carro_id); `ix_solicitud_reserva_empresa_id_carro_id_estado` (empresa_id, carro_id, estado); `ix_solicitud_reserva_empresa_id_empleado_confirmador_id` (empresa_id, empleado_confirmador_id); `ix_solicitud_reserva_empresa_id_empleado_solicitante_id` (empresa_id, empleado_solicitante_id); `uq_solicitud_reserva_empresa_id_solicitud_reserva_id` (empresa_id, solicitud_reserva_id) **único**

**Llaves foráneas:** (empresa_id, carro_id) → `carro`(empresa_id, carro_id) ON DELETE RESTRICT; (empresa_id, empleado_confirmador_id) → `empleado`(empresa_id, empleado_id) ON DELETE RESTRICT; (empresa_id, empleado_solicitante_id) → `empleado`(empresa_id, empleado_id) ON DELETE RESTRICT; (empresa_id) → `empresa`(empresa_id) ON DELETE RESTRICT

### Viajes

#### `viaje`

Cada uso del carro: km inicial/final, fotos del tablero, estado.

| Columna | Tipo | Llaves / restricciones | Default | Descripción |
|---|---|---|---|---|
| `viaje_id` | binary(16) | PK |  | Identificador (UUID v7, lo genera el celular: el viaje existe aunque no haya red). |
| `empresa_id` | binary(16) | NN, FK → carro (compuesta), FK → centro_costo (compuesta), FK → empleado (compuesta), FK → empresa, FK → solicitud_reserva (compuesta) |  | Empresa (tenant). |
| `carro_id` | binary(16) | NN, FK → carro (compuesta) |  | Referencia a carro. |
| `solicitud_reserva_id` | binary(16) | FK → solicitud_reserva (compuesta) |  | Reserva aprobada que permite el viaje. |
| `empleado_operador_id` | binary(16) | NN, FK → empleado (compuesta) |  | Quién maneja. |
| `fecha_inicio` | timestamp | NN |  | Inicio (hora del celular). |
| `ubicacion_inicio_lat` | decimal(9,6) |  |  | Latitud. |
| `ubicacion_inicio_lng` | decimal(9,6) |  |  | Longitud. |
| `kilometraje_inicial` | decimal(10,2) | NN |  | Odómetro al iniciar. |
| `fecha_fin` | timestamp |  |  | Fin del viaje. |
| `ubicacion_fin_lat` | decimal(9,6) |  |  | Latitud. |
| `ubicacion_fin_lng` | decimal(9,6) |  |  | Longitud. |
| `kilometraje_final` | decimal(10,2) |  |  | No puede ser menor al inicial. |
| `distancia_recorrida` | decimal(10,2) |  |  | kilometraje_final − kilometraje_inicial. |
| `estado` | enum('en_curso','finalizado','interrumpido') | NN | en_curso | en_curso → finalizado o interrumpido (la pausa no es estado: es una viaje_pausa abierta). |
| `motivo_interrupcion` | varchar(255) |  |  | Por qué se interrumpió. |
| `corregido` | tinyint(1) | NN | 0 | Se corrigió el kilometraje después de capturarlo (detalle en auditoria_evento). |
| `fecha_recepcion` | timestamp |  | now() | Cuándo llegó al servidor (hora del servidor). |
| `foto_tablero_final` | varchar(255) |  |  | Llave en MinIO de la foto del odómetro al terminar. |
| `foto_tablero_inicial` | varchar(255) |  |  | Llave en MinIO de la foto del odómetro al iniciar. |
| `centro_costo_id` | binary(16) | FK → centro_costo (compuesta) |  | v6. Área a la que se carga el viaje (opcional). |

**Índices:** `ix_viaje_empresa_id` (empresa_id); `ix_viaje_empresa_id_carro_id` (empresa_id, carro_id); `ix_viaje_empresa_id_carro_id_fecha_inicio` (empresa_id, carro_id, fecha_inicio); `ix_viaje_empresa_id_centro_costo_id` (empresa_id, centro_costo_id); `ix_viaje_empresa_id_empleado_operador_id` (empresa_id, empleado_operador_id); `ix_viaje_empresa_id_solicitud_reserva_id` (empresa_id, solicitud_reserva_id); `uq_viaje_carro_id_en_curso` (carro_id) **único filtrado** `WHERE estado = 'en_curso'` (un viaje en curso por carro, F010); `uq_viaje_empresa_id_viaje_id` (empresa_id, viaje_id) **único**

**Llaves foráneas:** (empresa_id, carro_id) → `carro`(empresa_id, carro_id) ON DELETE RESTRICT; (empresa_id, centro_costo_id) → `centro_costo`(empresa_id, centro_costo_id) ON DELETE RESTRICT; (empresa_id, empleado_operador_id) → `empleado`(empresa_id, empleado_id) ON DELETE RESTRICT; (empresa_id) → `empresa`(empresa_id) ON DELETE RESTRICT; (empresa_id, solicitud_reserva_id) → `solicitud_reserva`(empresa_id, solicitud_reserva_id) ON DELETE RESTRICT

#### `viaje_pausa`

Pausas de un viaje (no cuentan como tiempo ni recorrido).

| Columna | Tipo | Llaves / restricciones | Default | Descripción |
|---|---|---|---|---|
| `viaje_pausa_id` | binary(16) | PK |  | Identificador (UUID v7 generado por la app). |
| `empresa_id` | binary(16) | NN, FK → empresa, FK → viaje (compuesta) |  | Empresa (tenant). |
| `viaje_id` | binary(16) | NN, FK → viaje (compuesta) |  | Referencia a viaje. |
| `fecha_inicio` | timestamp | NN |  | Cuándo se pausó. |
| `fecha_fin` | timestamp |  |  | NULL = la pausa sigue abierta (solo una abierta por viaje). |
| `motivo` | varchar(255) |  |  | Motivo opcional (comida, espera...). |
| `ubicacion_lat` | decimal(9,6) |  |  | Latitud. |
| `ubicacion_lng` | decimal(9,6) |  |  | Longitud. |

**Índices:** `ix_viaje_pausa_empresa_id` (empresa_id); `ix_viaje_pausa_empresa_id_viaje_id` (empresa_id, viaje_id); `ix_viaje_pausa_viaje_id_fecha_inicio` (viaje_id, fecha_inicio); `uq_viaje_pausa_viaje_id_abierta` (viaje_id) **único filtrado** `WHERE fecha_fin IS NULL` (una pausa abierta por viaje, F010)

**Llaves foráneas:** (empresa_id) → `empresa`(empresa_id) ON DELETE RESTRICT; (empresa_id, viaje_id) → `viaje`(empresa_id, viaje_id) ON DELETE CASCADE

#### `punto_ruta`

Puntos GPS crudos capturados por el celular (nunca se modifican).

| Columna | Tipo | Llaves / restricciones | Default | Descripción |
|---|---|---|---|---|
| `punto_ruta_id` | binary(16) | PK |  | UUID v7 generado por el celular: reenviar un lote no duplica puntos. |
| `empresa_id` | binary(16) | NN, FK → empresa, FK → viaje (compuesta) |  | Empresa (tenant). |
| `viaje_id` | binary(16) | NN, FK → viaje (compuesta) |  | Referencia a viaje. |
| `latitud` | decimal(9,6) | NN |  | Latitud. |
| `longitud` | decimal(9,6) | NN |  | Longitud. |
| `precision_metros` | decimal(6,2) |  |  | Radio de error del GPS (los de más de 50 m se descartan al procesar). |
| `altitud` | decimal(7,2) |  |  | Metros sobre el nivel del mar. |
| `velocidad` | decimal(6,2) |  |  | Velocidad instantánea (m/s). Solo para descartar puntos erróneos. |
| `timestamp_captura` | timestamp | NN |  | Hora del celular. |
| `fecha_recepcion` | timestamp |  | now() | Cuándo llegó al servidor (hora del servidor). |

**Índices:** `ix_punto_ruta_empresa_id` (empresa_id); `ix_punto_ruta_empresa_id_viaje_id` (empresa_id, viaje_id); `uq_punto_ruta_viaje_id_timestamp_captura` (viaje_id, timestamp_captura) **único** (F010); `uq_punto_ruta_empresa_id_punto_ruta_id` (empresa_id, punto_ruta_id) **único**

**Llaves foráneas:** (empresa_id) → `empresa`(empresa_id) ON DELETE RESTRICT; (empresa_id, viaje_id) → `viaje`(empresa_id, viaje_id) ON DELETE CASCADE

#### `ruta_procesada`

Resumen calculado de la ruta (km GPS, polyline, km/L, alertas). 1:1 con viaje.

| Columna | Tipo | Llaves / restricciones | Default | Descripción |
|---|---|---|---|---|
| `ruta_procesada_id` | binary(16) | PK |  | Identificador (UUID v7 generado por la app). |
| `empresa_id` | binary(16) | NN, FK → empresa, FK → viaje (compuesta) |  | Empresa (tenant). |
| `viaje_id` | binary(16) | NN, único, FK → viaje (compuesta) |  | Viaje (1:1, único). Al recalcular se actualiza la misma fila. |
| `puntos_recibidos` | int | NN | 0 | Puntos crudos del viaje. |
| `puntos_descartados` | int | NN | 0 | Mala precisión, saltos imposibles, duplicados o dentro de una pausa. |
| `distancia_gps_km` | decimal(10,3) | NN | 0.000 | Km según el GPS. |
| `tiempo_movimiento_seg` | int | NN | 0 | Tiempo a más de 3 km/h. |
| `tiempo_detenido_seg` | int | NN | 0 | Tiempo detenido. |
| `velocidad_promedio_kmh` | decimal(6,2) |  |  | Uso interno. |
| `velocidad_maxima_kmh` | decimal(6,2) |  |  | Velocidad máxima. |
| `distancia_odometro_km` | decimal(10,2) |  |  | kilometraje_final − kilometraje_inicial del viaje. |
| `diferencia_km` | decimal(10,2) |  |  | Odómetro − GPS. Positivo = se declararon más km. |
| `diferencia_porcentaje` | decimal(7,2) |  |  | Diferencia en %. |
| `alerta_kilometraje` | tinyint(1) | NN | 0 | Diferencia GPS vs odómetro mayor al 10 % y a 2 km. |
| `litros_cargados` | decimal(8,2) | NN | 0.00 | Suma de litros de las cargas del viaje. |
| `rendimiento_km_por_litro` | decimal(6,2) |  |  | Uso interno y reportes. |
| `polyline` | text |  |  | Ruta simplificada (Douglas-Peucker, Google Polyline) para el mapa. |
| `version_algoritmo` | varchar(20) | NN |  | Versión del cálculo (para reprocesar si cambia). |
| `fecha_proceso` | timestamp | NN | now() | Cuándo se calculó. |

**Índices:** `ix_ruta_procesada_empresa_id` (empresa_id); `ix_ruta_procesada_empresa_id_alerta_kilometraje` (empresa_id, alerta_kilometraje); `ix_ruta_procesada_empresa_id_viaje_id` (empresa_id, viaje_id); `uq_ruta_procesada_empresa_id_ruta_procesada_id` (empresa_id, ruta_procesada_id) **único**; `uq_ruta_procesada_viaje_id` (viaje_id) **único**

**Llaves foráneas:** (empresa_id) → `empresa`(empresa_id) ON DELETE RESTRICT; (empresa_id, viaje_id) → `viaje`(empresa_id, viaje_id) ON DELETE CASCADE

### Combustible

#### `carga_combustible`

Cada carga de gasolina: litros, monto y foto del ticket.

| Columna | Tipo | Llaves / restricciones | Default | Descripción |
|---|---|---|---|---|
| `carga_combustible_id` | binary(16) | PK |  | Identificador (UUID v7, lo genera el celular). |
| `empresa_id` | binary(16) | NN, FK → centro_costo (compuesta), FK → empresa, FK → viaje (compuesta) |  | Empresa (tenant). |
| `viaje_id` | binary(16) | NN, FK → viaje (compuesta) |  | Referencia a viaje. |
| `litros` | decimal(8,2) | NN |  | Litros cargados (dato principal). |
| `monto` | decimal(10,2) |  |  | Importe pagado. |
| `foto_comprobante` | varchar(255) | NN |  | Llave en MinIO del ticket (obligatoria). |
| `fecha_carga` | timestamp | NN |  | Cuándo se cargó (hora del celular). |
| `ubicacion_lat` | decimal(9,6) |  |  | Latitud. |
| `ubicacion_lng` | decimal(9,6) |  |  | Longitud. |
| `corregido` | tinyint(1) | NN | 0 | Se corrigieron litros o monto (detalle en auditoria_evento). |
| `fecha_recepcion` | timestamp |  | now() | Cuándo llegó al servidor (hora del servidor). |
| `fecha_subida_comprobante` | timestamp |  |  | NULL = los datos llegaron pero la foto todavía no. |
| `centro_costo_id` | binary(16) | FK → centro_costo (compuesta) |  | v6. Área a la que se carga el gasto (opcional). |

**Índices:** `ix_carga_combustible_empresa_id` (empresa_id); `ix_carga_combustible_empresa_id_centro_costo_id` (empresa_id, centro_costo_id); `ix_carga_combustible_empresa_id_viaje_id` (empresa_id, viaje_id); `uq_carga_combustible_empresa_id_carga_combustible_id` (empresa_id, carga_combustible_id) **único**

**Llaves foráneas:** (empresa_id, centro_costo_id) → `centro_costo`(empresa_id, centro_costo_id) ON DELETE RESTRICT; (empresa_id) → `empresa`(empresa_id) ON DELETE RESTRICT; (empresa_id, viaje_id) → `viaje`(empresa_id, viaje_id) ON DELETE RESTRICT

### Notificaciones

#### `notificacion`

Bandeja de avisos de cada empleado.

| Columna | Tipo | Llaves / restricciones | Default | Descripción |
|---|---|---|---|---|
| `notificacion_id` | binary(16) | PK |  | Identificador (UUID v7 generado por la app). |
| `empresa_id` | binary(16) | NN, FK → empleado (compuesta), FK → empresa |  | Empresa (tenant). |
| `empleado_destinatario_id` | binary(16) | NN, FK → empleado (compuesta) |  | A quién va. |
| `tipo` | enum('nueva_solicitud','resultado_solicitud','cambio_estatus_forzado','reporte_mantenimiento') | NN |  | Tipo de aviso. |
| `referencia_id` | binary(16) |  |  | Registro relacionado según el tipo. Sin FK. |
| `mensaje` | varchar(255) |  |  | Texto. |
| `leida` | tinyint(1) | NN | 0 | Si ya la leyó. |
| `fecha_envio` | timestamp |  | now() | Cuándo se creó (el envío push está en outbox_mensaje). |

**Índices:** `ix_notificacion_empleado_destinatario_id_leida` (empleado_destinatario_id, leida); `ix_notificacion_empresa_id` (empresa_id); `ix_notificacion_empresa_id_empleado_destinatario_id` (empresa_id, empleado_destinatario_id); `uq_notificacion_empresa_id_notificacion_id` (empresa_id, notificacion_id) **único**

**Llaves foráneas:** (empresa_id, empleado_destinatario_id) → `empleado`(empresa_id, empleado_id) ON DELETE RESTRICT; (empresa_id) → `empresa`(empresa_id) ON DELETE RESTRICT

#### `outbox_mensaje`

Cola de mensajes push pendientes (patrón Outbox).

| Columna | Tipo | Llaves / restricciones | Default | Descripción |
|---|---|---|---|---|
| `outbox_mensaje_id` | binary(16) | PK |  | Identificador (UUID v7 generado por la app). |
| `empresa_id` | binary(16) | NN, FK → empresa |  | Empresa (tenant). |
| `tipo` | varchar(100) | NN |  | Tipo de mensaje (ej. notificacion.push). |
| `agregado_id` | binary(16) | NN |  | Registro de negocio que lo originó. Sin FK. |
| `payload` | json | NN |  | Datos del push (título, cuerpo, destino). |
| `estado` | enum('pendiente','procesando','enviado','fallido') | NN | pendiente | pendiente → procesando → enviado / fallido. |
| `intentos` | int | NN | 0 | Cuántas veces se intentó. |
| `proximo_intento` | timestamp | NN | now() | Cuándo se vuelve a intentar (backoff). |
| `ultimo_error` | varchar(500) |  |  | Último error de Firebase. |
| `fecha_creacion` | timestamp | NN | now() | Cuándo se creó. |
| `fecha_procesado` | timestamp |  |  | Cuándo quedó enviado o fallido definitivamente. |

**Índices:** `ix_outbox_mensaje_agregado_id` (agregado_id); `ix_outbox_mensaje_empresa_id` (empresa_id); `ix_outbox_mensaje_estado_proximo_intento` (estado, proximo_intento); `uq_outbox_mensaje_empresa_id_outbox_mensaje_id` (empresa_id, outbox_mensaje_id) **único**

**Llaves foráneas:** (empresa_id) → `empresa`(empresa_id) ON DELETE RESTRICT

### Auditoría

#### `auditoria_evento`

Historial de quién cambió qué, cuándo y por qué (solo inserción).

| Columna | Tipo | Llaves / restricciones | Default | Descripción |
|---|---|---|---|---|
| `auditoria_evento_id` | binary(16) | PK |  | Identificador (UUID v7 generado por la app). |
| `empresa_id` | binary(16) | FK → empresa |  | Empresa. NULL en acciones de plataforma. |
| `usuario_id` | binary(16) | FK → usuario |  | Quién hizo la acción (del token). NULL si fue un proceso automático. |
| `accion` | enum('crear','actualizar','eliminar','aprobar','rechazar','cancelar','cambiar_estado','corregir') | NN |  | Qué se hizo. |
| `entidad` | varchar(60) | NN |  | Tabla afectada. |
| `entidad_id` | binary(16) | NN |  | Registro afectado. Sin FK (depende de entidad). |
| `valores_anteriores` | json |  |  | Solo los campos que cambiaron, con su valor previo. |
| `valores_nuevos` | json |  |  | Solo los campos que cambiaron, con su valor nuevo. |
| `motivo` | varchar(500) |  |  | Obligatorio en correcciones, rechazos y bajas. |
| `origen` | enum('movil','web','sistema') | NN |  | De dónde vino (header X-Origen). |
| `fecha_hora` | timestamp |  | now() | Hora del servidor. |

**Índices:** `ix_auditoria_evento_empresa_id_entidad_entidad_id` (empresa_id, entidad, entidad_id); `ix_auditoria_evento_empresa_id_fecha_hora` (empresa_id, fecha_hora); `ix_auditoria_evento_usuario_id_fecha_hora` (usuario_id, fecha_hora)

**Llaves foráneas:** (empresa_id) → `empresa`(empresa_id) ON DELETE RESTRICT; (usuario_id) → `usuario`(usuario_id) ON DELETE RESTRICT

#### `revision_auditoria`

Revisiones de un responsable sobre un viaje o un carro en un periodo.

| Columna | Tipo | Llaves / restricciones | Default | Descripción |
|---|---|---|---|---|
| `revision_auditoria_id` | binary(16) | PK |  | Identificador (UUID v7 generado por la app). |
| `empresa_id` | binary(16) | NN, FK → carro (compuesta), FK → empleado (compuesta), FK → empresa, FK → viaje (compuesta) |  | Empresa (tenant). |
| `viaje_id` | binary(16) | FK → viaje (compuesta) |  | Alcance 'un viaje'. |
| `carro_id` | binary(16) | FK → carro (compuesta) |  | Alcance 'un carro en un periodo' (con periodo_desde y periodo_hasta). |
| `periodo_desde` | date |  |  | Inicio del periodo revisado. |
| `periodo_hasta` | date |  |  | Fin del periodo revisado. |
| `empleado_revisor_id` | binary(16) | NN, FK → empleado (compuesta) |  | Quién revisó. |
| `fecha_revision` | timestamp |  | now() | Cuándo se revisó. |
| `estado_revision` | enum('pendiente_informacion','revisado','con_observaciones') | NN | pendiente_informacion | Resultado de la revisión. |
| `observaciones` | varchar(1000) |  |  | Comentarios del revisor. |
| `fecha_cierre` | timestamp |  |  | NULL = la revisión sigue abierta. |

**Índices:** `ix_revision_auditoria_empresa_id` (empresa_id); `ix_revision_auditoria_empresa_id_carro_id_periodo_desde` (empresa_id, carro_id, periodo_desde); `ix_revision_auditoria_empresa_id_empleado_revisor_id` (empresa_id, empleado_revisor_id); `ix_revision_auditoria_empresa_id_estado_revision` (empresa_id, estado_revision); `ix_revision_auditoria_empresa_id_viaje_id` (empresa_id, viaje_id); `uq_revision_auditoria_empresa_id_revision_auditoria_id` (empresa_id, revision_auditoria_id) **único**

**Llaves foráneas:** (empresa_id, carro_id) → `carro`(empresa_id, carro_id) ON DELETE RESTRICT; (empresa_id, empleado_revisor_id) → `empleado`(empresa_id, empleado_id) ON DELETE RESTRICT; (empresa_id) → `empresa`(empresa_id) ON DELETE RESTRICT; (empresa_id, viaje_id) → `viaje`(empresa_id, viaje_id) ON DELETE RESTRICT

**CHECK** `ck_revision_auditoria_alcance`: `(viaje_id IS NOT NULL) OR (carro_id IS NOT NULL AND periodo_desde IS NOT NULL AND periodo_hasta IS NOT NULL)`

---

## 8. Reglas que aplica la API (no la BD)

- **Reservas:** para aprobar, el carro debe estar `disponible`, no dado de baja y sin otra solicitud aprobada. Al aprobar, el carro pasa a `ocupado` y las demás pendientes del mismo carro se rechazan. Un empleado no puede tener dos pendientes del mismo carro, y sin licencia (`empleado.fecha_carga_licencia IS NULL`) no puede solicitar.
- **Viajes:** un carro solo puede tener un viaje `en_curso`. El kilometraje final no puede ser menor al inicial. Al finalizar se actualiza `carro.kilometraje_actual`, se cierra la pausa abierta y se recalcula `ruta_procesada`.
- **Pausas:** solo una abierta por viaje, sin traslaparse y dentro del viaje.
- **Cargas:** los litros no pueden superar `carro.capacidad_tanque`. Editar o borrar una carga ya enviada es una corrección: pide motivo y queda en `auditoria_evento` (con el viaje cerrado además se necesita `viaje.corregir_cerrado`).
- **Auditoría:** el `AuditoriaInterceptor` de EF escribe `auditoria_evento` en la misma transacción que el cambio. Nunca guarda contraseñas, hashes ni tokens.
- **Outbox:** el push se guarda en `outbox_mensaje` en la misma transacción que el cambio de negocio. `OutboxWorker` lo toma con `FOR UPDATE SKIP LOCKED`, así varias instancias de la API no envían el mismo mensaje.
- **Alertas y reportes** (`/api/auditoria/revision-combustible`, `/api/reportes/flota`): se calculan al consultar, no tienen tabla.

---

## 9. Diferencias encontradas contra la documentación v9 (30-oct-2026)

| # | Documentación v9 | Lo que hay en la API / BD real |
|---|---|---|
| 1 | 23 tablas. | **24 tablas**: falta `sesion_usuario` (migración `20261106120000_SesionesUsuario`, 6-nov). Es la versión 10 del modelo. |
| 2 | `auditoria_evento` es "solo inserción: el usuario de MySQL de la API no tiene UPDATE ni DELETE". | Ninguna migración ni `docker-compose` aplica ese `REVOKE`; solo aparece como instrucción en `docs/CambiosV4-Auditoria.md`. Con el usuario `root` de los compose, la tabla sí se puede modificar. Hoy lo garantiza solo la API (no expone endpoints para editarla). |
| 3 | `fecha_recepcion` `not null` en `viaje`, `punto_ruta`, `carga_combustible`, `cambio_estatus_carro`. | Son **nullable** con default `now()`. En la práctica nunca quedan en NULL porque la API las llena. |
| 4 | `rol.empresa_clave` `not null`. | Es nullable en el esquema (columna generada). Nunca vale NULL por el `COALESCE`. |
| 5 | Intercalación `utf8mb4_unicode_ci` (por el `docker-compose`). | Las tablas quedan en **`utf8mb4_0900_ai_ci`** (ver la sección 2). Ambas ignoran mayúsculas y acentos, pero ordenan distinto en algunos casos. |
| 6 | "Las relaciones de negocio validan siempre `(empresa_id, entidad_id)`". | Cierto para las 21 FKs compuestas, pero `usuario_rol`, `rol_privilegio` y `sesion_usuario.empresa_id` no lo tienen en la BD (sección 3.2). |
| 7 | No menciona ON DELETE de seguridad. | `usuario_rol`, `rol_privilegio` y `sesion_usuario` tienen **CASCADE** (sección 5). |
| 8 | Índices listados de forma parcial. | El esquema tiene 88 índices además de las PK (todos en el diccionario, sección 7). |
| 9 | `corregido` es una columna nueva de la v4. | La migración v4 hizo `RENAME COLUMN sincronizado TO corregido` en `viaje` y `carga_combustible` (EF lo interpretó como renombre). Si había datos anteriores a la v4 con `sincronizado = 1`, ahora aparecen como `corregido = 1` sin tener evento de auditoría. En una BD nueva no afecta. |

Todo lo demás de la v9 (columnas, tipos, enums, FKs compuestas, CHECK de `revision_auditoria`, convenciones de MinIO, reglas de negocio) **coincide** con el código.

---

## 10. Historial de versiones del modelo

| Versión | Migración | Cambios principales |
|---|---|---|
| 1–3 | `20261009162833_Inicial` | 18 tablas base: usuario, rol, privilegio, usuario_rol, rol_privilegio, corporativo, division, empresa, centro_costo, empleado, carro, cambio_estatus_carro, solicitud_reserva, viaje, punto_ruta, ruta_procesada, carga_combustible, notificacion. UUID v7 en `binary(16)`. |
| 3 | `20261009181101_OutboxYDispositivos` | `dispositivo_empleado` (tokens FCM) y `outbox_mensaje`. |
| 4 | `20261013170116_AuditoriaYTrazabilidadV4` | `auditoria_evento`, `revision_auditoria` (+ CHECK de alcance), fotos de tablero, `fecha_subida_comprobante`, `tipo_motivo` y foto en cambios de estatus, cierre de reserva, baja de carro, empresa activa, fecha de carga de licencia, `fecha_recepcion`. En `viaje` y `carga_combustible` la columna `sincronizado` se **renombró** a `corregido` (ver diferencia #9). |
| 5 | `20261015180000_RbacPrivilegiosV5` | `privilegio.clave/modulo/ambito`; `rol.empresa_clave` (columna generada). |
| 6 | `20261020120000_CentroCostoEnViajeYCarga` | `centro_costo_id` en `viaje` y `carga_combustible` (FK compuesta). |
| 7 | `20261021120000_PausasDeViaje` | `viaje_pausa`. |
| 8 | `20261022120000_MotivoSolicitud` | `solicitud_reserva.motivo_solicitud`. |
| 9 | `20261030120000_TipoVehiculo` | `carro.tipo_vehiculo`. |
| 10 | `20261106120000_SesionesUsuario` | **`sesion_usuario`**: refresh tokens con rotación, detección de reuso, expiración deslizante y absoluta, revocación por motivo. |
| SQL Server | `20261111182911_InicialSqlServer` (fase 1) | El modelo de la v10 recreado para SQL Server (tipos de la sección 12; se elimina `rol.empresa_clave`). Las migraciones de MySQL se retiraron. |
| SQL Server | `20261111211442_AgregarEmpresaAUsuario` (fase 3) | `usuario.empresa_id`: de qué empresa es cada cuenta. |
| SQL Server | `20261111222631_SeguridadPorFilas` (fase 6) | Row-Level Security: esquema `rls`, funciones de predicado y política `rls.aislamiento_empresa`. |
| 11 | `EstructuraOrganizacional` (fase 8) | La empresa es la raíz: `corporativo.empresa_id` y `division.empresa_id`; **`corporativo_division`** (N:M); `centro_costo.corporativo_id`/`division_id` opcionales; `empresa` pierde `corporativo_id`/`division_id`; las tres tablas entran a la política de RLS; `corporativo.gestionar` pasa a ámbito empresa. Datos existentes: cada corporativo queda en la primera empresa que lo usaba (los que no usaba ninguna se borran con sus divisiones) y la relación división → corporativo se vuelve liga. |
| 12 | `EmpresaDesactivada` (fase 9A) | Nuevo motivo de cierre de sesión `empresa_desactivada`: al desactivar una empresa se cierran sus sesiones y ya no puede iniciar sesión ni renovar el token. |
| **13** | `IndicesUnicosConcurrencia` (fase 9B) | Índices únicos (F010): un viaje en curso por carro y una pausa abierta por viaje (filtrados) y `(viaje_id, timestamp_captura)` en `punto_ruta`. Antes de crearlos se limpian duplicados: puntos repetidos se borran, pausas abiertas de más se cierran y viajes en curso de más quedan interrumpidos. |

---

## 11. Cómo regenerar esta carpeta

Cuando se agregue una migración (desde la raíz del repo):

```bash
# 1. Esquema completo (todas las migraciones en orden). Después se le vuelve a poner el encabezado.
dotnet ef migrations script -p APIGasoline.Infrastructure -s APIGasoline -o BD/esquema-sqlserver.sql

# 2. Script idempotente para aplicar a mano en un servidor (NO se sube: está en .gitignore)
dotnet ef migrations script --idempotent -p APIGasoline.Infrastructure -s APIGasoline -o migraciones.sql
```

Después se actualizan a mano este README (tablas, índices, historial de la sección 10) y `BaseDatos.dbml`.

---

## 12. Puntos que importaron para la migración a SQL Server (fase 1, ya hecha)

Referencia histórica: partes del esquema que eran propias de MySQL y cómo se tradujeron:

| MySQL | Dónde | En SQL Server |
|---|---|---|
| `binary(16)` + `UUID_TO_BIN` | Todas las llaves | `uniqueidentifier`. Al migrar datos, exportar los GUID como texto, porque el orden de bytes cambia. |
| `enum(...)` (17 columnas) | Estados, tipos y motivos | `nvarchar(n)` + `CHECK (col IN (...))`, o solo `nvarchar` y que valide el convertidor de EF. |
| `json` | `auditoria_evento.valores_*`, `outbox_mensaje.payload` | `nvarchar(max)` (o `json` en SQL Server 2025). |
| Columna generada `rol.empresa_clave` | `rol` | No se necesita: en SQL Server un índice único trata los NULL como iguales. Usar `UNIQUE(empresa_id, nombre)` con `HasFilter(null)` en EF. |
| `FOR UPDATE SKIP LOCKED` | `OutboxMensajeRepository` | `WITH (UPDLOCK, READPAST, ROWLOCK)`. |
| `timestamp` | Todas las fechas | `datetime2`. |
| `tinyint(1)` | Booleanos | `bit`. |
| `utf8mb4_0900_ai_ci` | Toda la BD | `Modern_Spanish_CI_AI` para conservar "sin mayúsculas ni acentos". |
| FK compuestas `(empresa_id, x_id)` | 21 relaciones | Se conservan igual (SQL Server las soporta). Ojo con las rutas múltiples de CASCADE (`viaje` → hijos): SQL Server rechaza ciclos o rutas múltiples de cascada y puede pedir cambiar alguna a `NO ACTION`. |
