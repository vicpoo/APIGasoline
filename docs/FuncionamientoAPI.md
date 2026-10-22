# Funcionamiento de la API — APIGasoline

API REST en **.NET 10** para la app de control de combustible y vehículos (GasolineProject).

- **URL base en desarrollo:** `http://localhost:5234`
- **Formato:** JSON. Los envíos con archivos (fotos, comprobantes, licencia) son `multipart/form-data`.
- **Enums:** se envían y se reciben en camelCase (por ejemplo `enCurso`, `enMantenimiento`, `pendiente`).

---

## 1. Arquitectura

| Capa | Proyecto | Qué contiene |
|---|---|---|
| **API** | `APIGasoline` | Todo lo de HTTP. |
| **Application** | `APIGasoline.Application` | La lógica de negocio y la orquestación entre capas. |
| **Infrastructure** | `APIGasoline.Infrastructure` | Los servicios externos y librerías de terceros. |
| **Domain** | `APIGasoline.Domain` | La lógica pura de negocio, sin dependencias de ningún paquete. |

**Detalle de cada capa:**

- **API:**
  - Controladores (solo hacen `_sender.Send(...)`).
  - Contratos de entrada (requests).
  - Middleware de errores.
  - Atributo `[RequierePrivilegio]`.
  - Lectura del usuario del token (`UsuarioActual`).
- **Application:**
  - **CQRS con MediatR:** Commands y Queries por módulo (`Features/<Modulo>/Commands|Queries`).
  - DTOs y perfiles de **AutoMapper**.
  - Validadores y reglas de corrección.
  - Catálogo de privilegios (`Permisos`).
  - Pipeline `AccesoPropioBehavior`.
- **Infrastructure:**
  - **Entity Framework Core** con MySQL.
  - **Unit of Work** y sus repositorios.
  - Migraciones.
  - **MinIO** (archivos).
  - **Firebase** (push), con el worker del Outbox.
  - JWT, hash de contraseñas e interceptor de auditoría.
- **Domain:**
  - Entidades con sus reglas (por ejemplo `Viaje.Pausar`, `Carro.AvanzarKilometraje`).
  - Enums y excepciones de dominio.
  - Interfaces de repositorios e `IUnitOfWork`.
  - Modelos de lectura.

**Patrones:**

- **Unit of Work:** todo el acceso a datos pasa por `IUnitOfWork` y sus 24 repositorios. El Unit of Work también controla las transacciones (`BeginTransactionAsync`, `CommitTransactionAsync`, `RollbackTransactionAsync`).
- **CQRS:** los Commands cambian datos y las Queries solo leen.
- **Autorización basada en permisos:** se decide por las claves de privilegio del usuario, nunca por el nombre de su rol.
- **Outbox:** las notificaciones push se guardan en la misma transacción que el cambio de negocio, y un worker las envía después.
- **Multiempresa (multi-tenant):** casi todas las tablas llevan `empresa_id`, y las llaves foráneas compuestas `(empresa_id, id)` impiden mezclar datos de dos empresas.

---

## 2. Flujo de una petición

```
App / Postman
   │  HTTP + Authorization: Bearer <token>  (+ X-Origen: movil | web)
   ▼
ExceptionHandlingMiddleware ── convierte excepciones en respuestas JSON (ProblemDetails)
   ▼
Autenticación JWT ── valida el token (emisor, audiencia, firma, expiración)
   ▼
Autorización ── [RequierePrivilegio("modulo.accion")]
   │            los privilegios se leen de la BD (caché de 30 s) → 401 sin sesión / 403 sin privilegio
   ▼
Controlador ── arma el Command/Query con los datos del request
   ▼
MediatR ── AccesoPropioBehavior: "lo tuyo, o lo de otros con privilegio"
   ▼
Handler (Application) ── orquesta: alcance (empresa / propios), validaciones y reglas de negocio
   ▼
Dominio ── la entidad aplica sus reglas (lanza DomainException si no se cumplen)
   ▼
Unit of Work ── repositorios → EF Core → MySQL   (+ MinIO para archivos)
   │            SaveChanges: el interceptor escribe auditoria_evento en la MISMA transacción
   ▼
Notificaciones MediatR (por ejemplo, recalcular la ruta procesada) / Outbox (push)
   ▼
DTO (AutoMapper) → respuesta JSON
```

### Respuestas de error

| Código | Cuándo |
|---|---|
| 400 | Regla de negocio no cumplida (`DomainException`) o datos inválidos (`BadRequestException`). |
| 401 | Sin sesión o token inválido. |
| 403 | Sin el privilegio, o el registro es de otro empleado u otra empresa. El mensaje dice qué privilegio falta. |
| 404 | El registro no existe. |
| 409 | Conflicto: duplicado, registro en uso, o dos personas modificaron lo mismo al mismo tiempo. |
| 413 | El archivo excede el tamaño permitido (5 MB). |
| 503 | El almacenamiento de archivos (MinIO) no está disponible. |

Todas las respuestas de error usan este formato:

```json
{ "status": 403, "title": "Acceso denegado", "detail": "Necesitas el privilegio 'carro.crear'." }
```

---

## 3. Autenticación y autorización

1. **Login:** `POST /api/auth/login` con email y contraseña. Devuelve:
   - el token JWT;
   - el usuario y su empleado;
   - sus roles;
   - sus privilegios y la `versionPrivilegios`.
2. **En cada petición** se manda el header `Authorization: Bearer <token>`. El token lleva:
   - `sub`: el usuario;
   - `empleado_id`;
   - `empresa_id`;
   - `role`: solo informativo.
3. **Privilegios efectivos:** son los privilegios activos de los roles activos del usuario, globales o de su empresa.
   - **No van en el token.** Se consultan en la BD con una caché de 30 segundos, así un cambio aplica sin volver a iniciar sesión.
4. **Endpoints sin atributo:** todo endpoint que no lleva `[RequierePrivilegio]` igual pide sesión. Solo el login es público.
5. **Alcance de los datos:**
   - Sin `plataforma.multiempresa`, cada quien ve solo la empresa de su token.
   - Sin el privilegio de "ver todos" (`viaje.ver_todos`, `solicitud.ver_todas`, `notificacion.enviar`), la API filtra para devolver solo lo propio.
   - **Autoservicio:** perfil, licencia, contraseña, celular y notificaciones propias no llevan privilegio. Para hacerlo por otro empleado se necesita `empleado.gestionar` o `usuario.gestionar`.
6. **Siembra al arrancar** (`SembrarSeguridadCommand`):
   - Crea los privilegios del catálogo que falten.
   - Crea el rol global **Administrador** con todos los privilegios.
   - Crea el usuario admin configurado en `Seguridad:AdminEmail` / `AdminPassword`.
7. **Sincronización de privilegios en la app:** `GET /api/auth/privilegios` con la versión que tiene la app. Si no cambió responde `304`, y si cambió manda la lista nueva.

El catálogo completo de privilegios está en `docs/CatalogoPrivilegios.md`.

---

## 4. Endpoints

**Columna "Privilegio":**
- **Separados por coma:** basta con tener uno de ellos.
- **"sesión":** solo pide iniciar sesión (autoservicio, solo sobre lo propio).
- **"público":** no pide nada.

### 4.1 Auth

| Método | Ruta | Privilegio | Qué hace |
|---|---|---|---|
| POST | `/api/auth/login` | público | Inicia sesión. Devuelve el token, el perfil y los privilegios. |
| GET | `/api/auth/perfil` | sesión | Perfil del usuario del token. |
| GET | `/api/auth/privilegios` | sesión | Privilegios y versión. Responde `304` si no cambiaron. |

### 4.2 Vehículos (carros)

| Método | Ruta | Privilegio | Qué hace |
|---|---|---|---|
| GET | `/api/carros` | `carro.ver` | Lista. Filtros: `?empresaId=`, `?estado=disponible`. |
| GET | `/api/carros/{id}` | `carro.ver_detalle`, `viaje.registrar` | Detalle de un vehículo. |
| POST | `/api/carros` | `carro.crear` | Agrega un vehículo. La foto se sube después con `PUT …/foto`. |
| PUT | `/api/carros/{id}` | `carro.editar` | Edita los datos. |
| PUT | `/api/carros/{id}/foto` | `carro.editar` | Sube o reemplaza la foto (campo `archivo`: jpg, png o webp, máx. 5 MB). |
| DELETE | `/api/carros/{id}/foto` | `carro.editar` | Quita la foto. |
| POST | `/api/carros/{id}/baja` | `carro.eliminar` | Lo da de baja sin borrar su historial. No se puede si está en uso. |
| DELETE | `/api/carros/{id}` | `carro.eliminar` | Lo borra solo si no tiene historial; si tiene, responde `409` y hay que usar `/baja`. |

### 4.3 Cambios de estatus del carro

| Método | Ruta | Privilegio | Qué hace |
|---|---|---|---|
| GET | `/api/cambios-estatus-carro` | `carro.ver`, `carro.cambiar_estatus` | Bitácora. Filtros: `?empresaId=`, `?carroId=`, `?desde=`, `?hasta=`. |
| GET | `/api/cambios-estatus-carro/{id}` | `carro.ver`, `carro.cambiar_estatus` | Un cambio. |
| POST | `/api/cambios-estatus-carro` | `carro.cambiar_estatus`, `viaje.registrar` | Cambia el estatus a disponible o en mantenimiento, y lo registra en la bitácora. |
| PUT | `/api/cambios-estatus-carro/{id}/foto-evidencia` | `carro.cambiar_estatus`, `viaje.registrar` | Foto de la falla o del daño. |

### 4.4 Solicitudes de reserva

| Método | Ruta | Privilegio | Qué hace |
|---|---|---|---|
| GET | `/api/solicitudes-reserva` | `solicitud.ver_propias`, `solicitud.ver_todas`, `viaje.registrar` | Lista. Sin `ver_todas`, solo las propias. Filtros: `?carroId=`, `?empleadoSolicitanteId=`, `?estado=`. Cada solicitud trae `solicitante` (nombre y foto) y `carro` (marca, modelo, placas y foto) para la tarjeta. |
| GET | `/api/solicitudes-reserva/{id}` | mismos | Detalle: `motivoSolicitud`, `solicitante` (nombre, foto y licencia), `carro` (datos, estado y foto) y `confirmador`. |
| POST | `/api/solicitudes-reserva` | `solicitud.crear` | Envía una solicitud con su `motivo` (obligatorio, máx. 255; requiere licencia registrada). |
| POST | `/api/solicitudes-reserva/{id}/aprobar` | `solicitud.aprobar` | La aprueba: ocupa el carro y rechaza las demás pendientes de ese carro. |
| POST | `/api/solicitudes-reserva/{id}/rechazar` | `solicitud.rechazar` | La rechaza. |
| POST | `/api/solicitudes-reserva/{id}/cancelar` | `solicitud.crear`, `solicitud.aprobar` | La cancela. Si estaba aprobada, libera el carro. |
| POST | `/api/solicitudes-reserva/{id}/liberar` | `viaje.registrar`, `solicitud.aprobar` | El operador entrega el carro: la reserva queda finalizada y el carro, disponible. |
| DELETE | `/api/solicitudes-reserva/{id}` | `solicitud.aprobar` | La borra. |

### 4.5 Viajes

| Método | Ruta | Privilegio | Qué hace |
|---|---|---|---|
| GET | `/api/viajes` | `viaje.ver_propios`, `viaje.ver_todos` | Historial. Sin `ver_todos`, solo los propios. Filtros: `?carroId=`, `?empleadoOperadorId=`, `?estado=`. Cada viaje trae `operador` (nombre y foto) para la tarjeta. |
| GET | `/api/viajes/{id}` | `viaje.ver_detalle`, `viaje.registrar` | Detalle con `estaPausado`, `segundosEnPausa` y `pausas`. |
| POST | `/api/viajes` | `viaje.registrar` | Inicia un viaje en línea (requiere reserva aprobada; el centro de costos es opcional). |
| POST | `/api/viajes/{id}/pausar` | `viaje.registrar` | Pone el viaje en pausa. Cuerpo opcional: `fecha`, `motivo`, `ubicacionLat`, `ubicacionLng`. |
| POST | `/api/viajes/{id}/reanudar` | `viaje.registrar` | Reanuda. Cuerpo opcional: `fecha`. |
| POST | `/api/viajes/{id}/finalizar` | `viaje.registrar` | Finaliza con la fecha y el kilometraje final. Cierra la pausa si estaba en pausa. |
| POST | `/api/viajes/{id}/interrumpir` | `viaje.registrar`, `viaje.corregir_cerrado` | Lo corta antes de terminar, con motivo obligatorio. |
| POST | `/api/viajes/sincronizar` | `viaje.registrar` | Sube un viaje completo hecho sin conexión, con sus `pausas`. |
| PUT | `/api/viajes/{id}/foto-tablero/{momento}` | `viaje.registrar` | Foto del odómetro; `momento` = `inicial` o `final`. |
| POST | `/api/viajes/{id}/corregir-kilometraje` | `viaje.registrar`, `viaje.corregir_cerrado` | Corrige el kilometraje, con motivo, y queda auditado. |
| DELETE | `/api/viajes/{id}` | `viaje.eliminar` | Borra un viaje cerrado sin cargas ni revisiones. |

### 4.6 Puntos GPS y ruta procesada

| Método | Ruta | Privilegio | Qué hace |
|---|---|---|---|
| POST | `/api/viajes/{id}/puntos-ruta` | `viaje.registrar` | Lote de hasta 1000 puntos. Se puede reenviar sin duplicar; descarta los que caen en una pausa. |
| GET | `/api/viajes/{id}/puntos-ruta` | `viaje.ver_detalle`, `viaje.registrar` | Ruta cruda en orden de captura. |
| GET | `/api/puntos-ruta/{id}` | `viaje.ver_detalle`, `viaje.registrar` | Un punto. |
| DELETE | `/api/puntos-ruta/{id}` | `viaje.eliminar` | Borra un punto. |
| GET | `/api/viajes/{id}/ruta-procesada` | `viaje.ver_detalle` | Resumen calculado: kilómetros por GPS, comparación contra el odómetro y rendimiento en km/L. |
| GET | `/api/viajes/{id}/ruta-procesada/mapa` | `viaje.ver_detalle` | Ruta limpia lista para dibujar en el mapa. |
| POST | `/api/viajes/{id}/ruta-procesada/recalcular` | `viaje.ver_todos` | Vuelve a calcular la ruta. |
| GET | `/api/rutas-procesadas/alertas` | `viaje.ver_todos`, `auditoria.ver` | Viajes cuyo odómetro no cuadra con el GPS (más del 10 % y más de 2 km de diferencia). |

### 4.7 Cargas de gasolina

| Método | Ruta | Privilegio | Qué hace |
|---|---|---|---|
| GET | `/api/cargas-combustible?viajeId=` | `viaje.ver_detalle`, `auditoria.ver` | Cargas de un viaje. Sin `viajeId`, necesita `viaje.ver_todos` o `auditoria.ver`. |
| GET | `/api/cargas-combustible/{id}` | `viaje.ver_detalle`, `carga.editar`, `carga.eliminar`, `auditoria.ver` | Una carga. |
| POST | `/api/cargas-combustible` | `carga.registrar` | Registra una carga (multipart con `fotoComprobante`). |
| PUT | `/api/cargas-combustible/{id}` | `carga.editar` | La corrige, con `motivo` obligatorio, y queda auditada. |
| DELETE | `/api/cargas-combustible/{id}?motivo=` | `carga.eliminar` | La elimina, con motivo obligatorio, y queda auditada. |

### 4.8 Empleados, perfil y licencia

| Método | Ruta | Privilegio | Qué hace |
|---|---|---|---|
| GET | `/api/empleados` | `empleado.ver`, `empleado.gestionar`, `viaje.ver_todos`, `solicitud.aprobar` | Lista de su empresa. |
| GET | `/api/empleados/{id}` | sesión (el propio) o `empleado.ver` | Un empleado. |
| GET | `/api/empleados/por-usuario/{usuarioId}` | sesión (el propio) o `empleado.ver` | El empleado de un usuario. |
| POST | `/api/empleados` | `empleado.gestionar` | Lo da de alta. |
| PUT | `/api/empleados/{id}` | sesión (el propio) o `empleado.gestionar` | Cambia el nombre. El propio empleado no puede cambiarse de empresa ni de usuario. |
| PUT | `/api/empleados/{id}/foto-perfil` | sesión (el propio) o `empleado.gestionar` | Foto de perfil. |
| PUT | `/api/empleados/{id}/foto-licencia` | sesión (el propio) o `empleado.gestionar` | Licencia (jpg, png, webp o pdf). |
| DELETE | `/api/empleados/{id}/foto-perfil` · `/foto-licencia` | sesión (el propio) o `empleado.gestionar` | Quita la foto. |
| DELETE | `/api/empleados/{id}` | `empleado.gestionar` | Lo elimina (borrado lógico: queda marcado como eliminado). |

### 4.9 Notificaciones y celulares

| Método | Ruta | Privilegio | Qué hace |
|---|---|---|---|
| GET | `/api/notificaciones` | sesión (las propias) o `notificacion.enviar` | Bandeja. Filtros: `?soloNoLeidas=true`, `?tipo=`. |
| GET | `/api/notificaciones/{id}` | sesión | Una notificación. |
| GET | `/api/notificaciones/no-leidas/{empleadoId}` | sesión (el propio) o `notificacion.enviar` | Cuántas no leídas tiene (número para la campana). |
| POST | `/api/notificaciones/{id}/marcar-leida` | sesión | Marca una como leída. |
| POST | `/api/notificaciones/marcar-todas-leidas/{empleadoId}` | sesión (el propio) o `notificacion.enviar` | Marca todas como leídas. |
| POST | `/api/notificaciones` | `notificacion.enviar` | Notificación manual. |
| DELETE | `/api/notificaciones/{id}` | sesión | La borra. |
| POST | `/api/dispositivos` | sesión (el propio) o `empleado.gestionar` | Registra el token de Firebase del celular. |
| DELETE | `/api/dispositivos/{id}` | sesión | Desactiva el celular al cerrar sesión. |
| GET | `/api/empleados/{id}/dispositivos` | sesión (el propio) o `empleado.ver` | Celulares del empleado. |

### 4.10 Administración (panel web)

| Módulo | Rutas | Ver | Gestionar |
|---|---|---|---|
| Usuarios | `/api/usuarios` (+ `/{id}/cambiar-password`) | `usuario.ver` | `usuario.gestionar` (la contraseña propia: sesión) |
| Roles | `/api/roles` | `rol.ver`, `rol.asignar` | `rol.gestionar` |
| Privilegios | `/api/privilegios` | `privilegio.ver`, `rol.gestionar` | `privilegio.gestionar` |
| Privilegios de un rol | `/api/rol-privilegios` | `rol.ver` | `rol.gestionar` |
| Roles de un usuario | `/api/usuario-roles` | `rol.ver` | `rol.asignar` |
| Corporativos y divisiones | `/api/corporativos`, `/api/divisiones` | `corporativo.ver` | `corporativo.gestionar` |
| Empresas | `/api/empresas` | `empresa.ver` | `empresa.editar` / `empresa.gestionar` |
| Centros de costo | `/api/centros-costo` | `centro_costo.ver`, `viaje.registrar`, `carga.registrar` | `centro_costo.gestionar` |
| Outbox | `GET /api/outbox`, `POST /api/outbox/{id}/reintentar` | `outbox.gestionar` | `outbox.gestionar` |

Todos estos módulos siguen el CRUD estándar: `GET` (lista), `GET /{id}`, `POST`, `PUT /{id}` y `DELETE /{id}`.

### 4.11 Auditoría

| Método | Ruta | Privilegio | Qué hace |
|---|---|---|---|
| GET | `/api/auditoria/eventos` · `/eventos/{id}` | `auditoria.ver` | Historial de cambios: qué cambió, quién, cuándo y con qué motivo. |
| GET | `/api/auditoria/revision-combustible` | `auditoria.ver` | Resumen de un periodo (viajes, litros, monto y comprobantes) con las observaciones detectadas. |
| GET | `/api/auditoria/revisiones` · `/revisiones/{id}` | `auditoria.ver` | Revisiones de auditoría. |
| POST | `/api/auditoria/revisiones` | `auditoria.revisar` | Abre una revisión. |
| PUT | `/api/auditoria/revisiones/{id}/resultado` | `auditoria.revisar` | Registra el resultado y la cierra. |

---

## 5. Flujos de negocio

### 5.1 Puesta en marcha

1. Aplicar las migraciones: `dotnet ef database update --project APIGasoline.Infrastructure --startup-project APIGasoline`.
2. Arrancar la API. La siembra crea los privilegios, el rol Administrador y el usuario admin.
3. Con el admin, configurar en este orden:
   1. Corporativo, división y empresa.
   2. Centros de costo.
   3. Usuarios y empleados.
   4. Roles de la empresa y sus privilegios.
   5. Asignar roles a los usuarios.

### 5.2 Iniciar sesión (app)

1. `POST /auth/login`. La app guarda el token, el perfil y los privilegios.
2. Si el empleado no tiene licencia, la app pide subirla con `PUT /empleados/{id}/foto-licencia`.
3. La app muestra solo las pestañas y botones de los privilegios que tiene el usuario.
4. Cada cierto tiempo, la app revisa `GET /auth/privilegios` con su versión. Si responde `304`, sus privilegios no cambiaron.

### 5.3 Solicitar y usar un vehículo (flujo principal)

```
Operador                         API                                  Supervisor
   │ POST /solicitudes-reserva  ─▶ valida motivo, licencia y carro disponible
   │                               estado = pendiente
   │                               notificación + push ─────────────▶ (quien tenga solicitud.aprobar)
   │                                                                   POST …/aprobar  (o …/rechazar)
   │ ◀─ notificación + push ─────── aprobada: carro = ocupado,
   │                               las demás pendientes de ese carro se rechazan
   │ POST /viajes (iniciar) ──────▶ viaje en curso (km inicial, centro de costos)
   │ PUT  …/foto-tablero/inicial
   │ POST …/puntos-ruta (lotes) ──▶ guarda el GPS sin duplicar
   │ POST …/pausar / …/reanudar ──▶ pausas (no cuenta el GPS en pausa)
   │ POST /cargas-combustible ────▶ litros, monto, ticket (MinIO)
   │ POST …/finalizar ────────────▶ km final, distancia, km del carro;
   │ PUT  …/foto-tablero/final       cierra la pausa abierta;
   │                               calcula la ruta procesada (GPS vs odómetro, km/L)
   │ POST /solicitudes-reserva/{id}/liberar ─▶ reserva finalizada, carro = disponible
```

**Reglas de este flujo:**

- **Un carro, un viaje:** no puede tener dos viajes en curso a la vez.
- **Kilometraje:** el final no puede ser menor al inicial.
- **A nombre de quién:** cada operador inicia, pausa y finaliza sus propios viajes. Para el de otro se necesita `viaje.corregir_cerrado`.
- **Una pausa a la vez:** no se puede pausar dos veces seguidas, ni reanudar si el viaje no está en pausa.

### 5.4 Sin conexión (offline-first)

1. La app guarda en el celular el viaje, sus puntos GPS, sus pausas y sus cargas (con la foto del ticket).
2. Al recuperar la red, la app sincroniza en este orden:
   1. `POST /viajes/sincronizar`, con las pausas.
   2. Los puntos en lotes de 500.
   3. Las cargas.
   4. Finalizar, si el viaje ya terminó.
3. **Reintentos:** los lotes de puntos se pueden reenviar sin duplicarse. Las pausas se validan juntas: no se enciman y deben caer dentro del viaje.
4. **Fecha de recepción:** la BD guarda en `fecha_recepcion` cuándo llegó cada registro. Así se sabe cuánto tiempo estuvo sin sincronizar.

### 5.5 Correcciones (kilometraje y cargas)

**Quién puede corregir:**
- Con el viaje **en curso:** el operador del viaje.
- Con el viaje **cerrado:** solo quien tenga `viaje.corregir_cerrado`.

**Reglas que aplican siempre:**
- Hace falta motivo.
- El interceptor guarda en `auditoria_evento` el valor anterior, el nuevo, quién lo cambió y el motivo.
- Al eliminar una carga se aplican las mismas reglas.
- Si el viaje ya estaba cerrado, la ruta procesada (km/L) se recalcula sola.

### 5.6 Mantenimiento y fallas

1. `POST /cambios-estatus-carro` con `enMantenimiento` y un motivo (falla mecánica, golpe, servicio, etc.).
   - Se puede agregar foto de evidencia.
2. Si el carro tenía un viaje en curso, el viaje se **interrumpe** solo. Si estaba en pausa, la pausa se cierra.
3. **Avisos:**
   - Si fue el propio operador quien reportó la falla, se avisa a quien tenga `solicitud.aprobar`.
   - Si el cambio lo hizo otra persona, se avisa al empleado que tenía el carro.
4. Al terminar el mantenimiento, otro cambio de estatus regresa el carro a `disponible`.

### 5.7 Notificaciones push (patrón Outbox)

1. Una acción de negocio (solicitud nueva, aprobación, rechazo, cambio de estatus) crea la `notificacion` y su mensaje en `outbox_mensaje`, en la **misma transacción**.
2. El `OutboxWorker` revisa cada 5 segundos los pendientes y los envía por Firebase a los celulares registrados del empleado.
   - Estados del mensaje: pendiente → procesando → enviado o fallido.
3. Los mensajes fallidos se reintentan con `POST /outbox/{id}/reintentar`.
4. La bandeja siempre está disponible con `GET /notificaciones`, aunque el push no llegue.

---

## 6. Procesos en segundo plano

| Proceso | Cuándo corre | Qué hace |
|---|---|---|
| Siembra de seguridad | Al arrancar la API | Crea el catálogo de privilegios, el rol Administrador y el usuario admin (sin duplicar). |
| `MinioBucketInitializer` | Al arrancar la API | Crea el bucket de archivos si no existe. |
| `OutboxWorker` | Cada 5 s (configurable) | Envía los push pendientes. |
| `RutaViajeModificadaNotification` | Al finalizar, interrumpir o sincronizar un viaje; al llegar puntos o cargas a un viaje cerrado; al corregir | Recalcula la ruta procesada (km por GPS, diferencia contra el odómetro, km/L). |
| Interceptor de auditoría | En cada `SaveChanges` | Escribe `auditoria_evento` por cada cambio, en la misma transacción. |

---

## 7. Estados

| Entidad | Estados |
|---|---|
| Carro | `disponible`, `ocupado`, `enMantenimiento` (o dado de baja) |
| Solicitud de reserva | `pendiente`, `aprobada`, `rechazada`, `cancelada`, `finalizada` |
| Viaje | `enCurso` (puede estar en pausa), `finalizado`, `interrumpido` |
| Pausa de viaje | abierta (`fechaFin` = null) o cerrada |
| Mensaje del outbox | `pendiente`, `procesando`, `enviado`, `fallido` |
| Revisión de auditoría | `pendienteInformacion`, `revisado`, `conObservaciones` |
| Motivo de cambio de estatus | `liberacion`, `fallaMecanica`, `choqueOGolpe`, `servicio`, `emergencia`, `otro` |

---

## 8. Configuración (appsettings)

| Sección | Para qué |
|---|---|
| `ConnectionStrings:DefaultConnection` | MySQL. |
| `Jwt` | Emisor, audiencia, clave (mínimo 32 caracteres) y expiración (480 min). |
| `Minio` | Endpoint, credenciales y bucket de archivos. |
| `Outbox` | Intervalo, tamaño de lote y bloqueo del worker. |
| `Push` | Proveedor `NoOp` (simulado) o `Firebase`, con su archivo de credenciales. |
| `Seguridad` | `SembrarAlIniciar`, `AdminEmail` y `AdminPassword` del administrador inicial. |

---

## 9. Pruebas

- **Pruebas unitarias:** `dotnet test`. Cubren las reglas de dominio y de Application.
- **Postman:**
  - Carpeta `postman/collections/APIGAsoline`: pruebas por módulo. Empieza con `00_sesion`, que inicia sesión como admin.
  - Colección "GasolineApp - Datos de prueba": crea los datos y usuarios para probar la app.
    - Usuarios: `admin.app`, `operador.app` y `supervisor.app` @prueba.com, contraseña `Gasolina2026`.

**Documentos relacionados** (en la carpeta `docs/` de la API):

- `CatalogoPrivilegios.md`: los privilegios de la app y sus reglas.
- `BaseDatos.dbml`: el diagrama de la base de datos, versión 7.
