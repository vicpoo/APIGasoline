# Plan de pruebas manual en Postman — APIGasoline v4

Flujo completo de la API (modelo v4): usuarios → organización → viaje tipo Strava con combustible → correcciones → auditoría y trazabilidad.
Las pruebas van **en orden**: cada una usa datos que crearon las anteriores (ids, tokens).
La numeración es la misma que la colección automática `postman/APIGasoline.postman_collection.json`.

## 1. Antes de empezar

1. Aplica la migración `AuditoriaYTrazabilidadV4` (ver `docs/CambiosV4-Auditoria.md`) y levanta la API.
2. Levanta MinIO (las fotos y documentos se suben ahí).
3. En Postman: **Settings → General → Working directory** = carpeta `APIGasoline\pruebas`. Ahí están `ticket.jpg`, `carro.jpg`, `perfil.png`, `licencia.pdf`, `tablero.jpg` y `falla.jpg`.
4. Crea un environment (o usa las variables de una colección vacía) con las variables de la sección 2.

### Cómo leer cada prueba

- **Ruta**: método y URL. Lo que va entre `{{ }}` es una variable de Postman.
- **Auth**: pestaña *Authorization* → *Bearer Token* con el token indicado. "Sin token" = *No Auth*.
- **Headers**: además de `Content-Type: application/json` (Postman lo pone solo con body *raw → JSON*).
- **Body**: *raw → JSON* o *form-data* (en form-data, las filas de tipo *File* se eligen con *Select Files*).
- **Espero**: el código HTTP correcto. Un 400/401/403/404/409 esperado es una prueba **exitosa**.
- **Guarda**: copia ese campo de la respuesta en la variable indicada antes de seguir.

## 2. Variables

### 2.1. Las que llenas tú al inicio

| Variable | Valor |
|---|---|
| `host` | `http://localhost:5234` |
| `run` | Un sufijo nuevo en cada corrida: `001`, `002`, … |

### 2.2. Las que se llenan con las respuestas

| Variable | Se obtiene en | Campo de la respuesta |
|---|---|---|
| `usuarioOperadorId` | 01.1 | `usuarioId` |
| `usuarioConfirmadorId` | 01.2 | `usuarioId` |
| `usuarioConsultorId` | 01.3 | `usuarioId` |
| `rolConfirmadorId` | 02.1 | `rolId` |
| `rolConsultorId` | 02.2 | `rolId` |
| `privilegioId` | 02.3 | `privilegioId` |
| `corporativoId` | 03.1 | `corporativoId` |
| `divisionId` | 03.2 | `divisionId` |
| `empresaId` | 03.3 | `empresaId` |
| `centroCostoId` | 03.6 | `centroCostoId` |
| `empleadoOperadorId` | 04.1 | `empleadoId` |
| `empleadoConfirmadorId` | 04.2 | `empleadoId` |
| `empleadoConsultorId` | 04.3 | `empleadoId` |
| `fotoPerfilUrl` | 04.5 | `fotoPerfilUrl` |
| `tokenOperador` | 05.1 | `token` |
| `tokenConfirmador` | 05.2 | `token` |
| `tokenConsultor` | 05.3 | `token` |
| `carroId` | 06.1 | `carroId` |
| `solicitudReservaId` | 07.1 | `solicitudReservaId` |
| `viajeId` | 08.1 | `viajeId` |
| `cargaCombustibleId` | 08.6 | `cargaCombustibleId` |
| `fotoComprobanteUrl` | 08.6 | `fotoComprobanteUrl` |
| `cambioEstatusCarroId` | 11.1 | `cambioEstatusCarroId` |
| `revisionViajeId` | 12.7 | `revisionAuditoriaId` |
| `revisionCarroId` | 12.11 | `revisionAuditoriaId` |
| `carroBajaId` | 13.1 | `carroId` |

### 2.3. Fechas (la API usa la hora local del servidor, sin zona horaria)

Formato `AAAA-MM-DDTHH:MM:SS`. Usa la fecha de **hoy**; ninguna puede estar en el futuro. Ejemplo si empiezas el viaje a las 10:00 del 13 de octubre de 2026:

| Variable | Qué es | Ejemplo |
|---|---|---|
| `fechaInicio` | Inicio del viaje | `2026-10-13T10:00:00` |
| `punto1` | Primer punto GPS (igual al inicio) | `2026-10-13T10:00:00` |
| `punto2` | 30 s después | `2026-10-13T10:00:30` |
| `punto3` | 60 s después | `2026-10-13T10:01:00` |
| `fechaCarga` | Hora de la carga | `2026-10-13T10:01:30` |
| `fechaFin` | Fin del viaje (hora actual al enviar 08.10) | `2026-10-13T10:05:00` |
| `revisionDesde` / `revisionHasta` | Periodo de la revisión de combustible | ayer / mañana |
| `periodoDesde` / `periodoHasta` | Periodo de la revisión del carro (solo fecha) | `2026-10-01` / `2026-10-13` |

## 3. Resumen

| # | Prueba | Ruta | Espero |
|---|---|---|---|
| 01.1 | Crear usuario operador | `POST /api/usuarios` | **201** |
| 01.2 | Crear usuario confirmador | `POST /api/usuarios` | **201** |
| 01.3 | Crear usuario consultor (auditoría) | `POST /api/usuarios` | **201** |
| 01.4 | Listar usuarios | `GET /api/usuarios` | **200** |
| 01.5 | Obtener usuario por id | `GET /api/usuarios/{{usuarioOperadorId}}` | **200** |
| 02.1 | Crear rol Confirmador (o reutilizarlo) | `POST /api/roles` | **201 o 409** |
| 02.2 | Crear rol Consultor/Reportes (o reutilizarlo) | `POST /api/roles` | **201 o 409** |
| 02.3 | Crear privilegio | `POST /api/privilegios` | **201** |
| 02.4 | Asignar privilegio al rol Consultor | `POST /api/rol-privilegios` | **201** |
| 02.5 | Asignar rol Confirmador al usuario confirmador | `POST /api/usuario-roles` | **201** |
| 02.6 | Asignar rol Consultor al usuario consultor | `POST /api/usuario-roles` | **201** |
| 02.7 | Roles del usuario confirmador | `GET /api/usuario-roles?usuarioId={{usuarioConfirmadorId}}` | **200** |
| 03.1 | Crear corporativo | `POST /api/corporativos` | **201** |
| 03.2 | Crear división | `POST /api/divisiones` | **201** |
| 03.3 | Crear empresa | `POST /api/empresas` | **201** |
| 03.4 | Desactivar empresa | `PUT /api/empresas/{{empresaId}}` | **200** |
| 03.5 | Reactivar empresa | `PUT /api/empresas/{{empresaId}}` | **200** |
| 03.6 | Crear centro de costo | `POST /api/centros-costo` | **201** |
| 03.7 | Empresas del corporativo | `GET /api/empresas?corporativoId={{corporativoId}}` | **200** |
| 04.1 | Crear empleado operador | `POST /api/empleados` | **201** |
| 04.2 | Crear empleado confirmador | `POST /api/empleados` | **201** |
| 04.3 | Crear empleado consultor | `POST /api/empleados` | **201** |
| 04.4 | Subir licencia del operador (PDF) | `PUT /api/empleados/{{empleadoOperadorId}}/foto-licencia` | **200** |
| 04.5 | Subir foto de perfil del operador | `PUT /api/empleados/{{empleadoOperadorId}}/foto-perfil` | **200** |
| 04.6 | Descargar la foto de perfil desde MinIO (URL prefirmada) | `GET {{fotoPerfilUrl}}` | **200** |
| 04.7 | Empleado por usuario | `GET /api/empleados/por-usuario/{{usuarioOperadorId}}` | **200** |
| 05.1 | Login del operador | `POST /api/auth/login` | **200** |
| 05.2 | Login del confirmador | `POST /api/auth/login` | **200** |
| 05.3 | Login del consultor | `POST /api/auth/login` | **200** |
| 05.4 | Perfil con token (Bearer) | `GET /api/auth/perfil` | **200** |
| 05.5 | Perfil sin token (401) | `GET /api/auth/perfil` | **401** |
| 05.6 | Login con contraseña incorrecta (401) | `POST /api/auth/login` | **401** |
| 06.1 | Crear carro | `POST /api/carros` | **201** |
| 06.2 | Subir foto del carro | `PUT /api/carros/{{carroId}}/foto` | **200** |
| 06.3 | Obtener carro | `GET /api/carros/{{carroId}}` | **200** |
| 06.4 | Carros disponibles de la empresa | `GET /api/carros?empresaId={{empresaId}}&estado=disponible` | **200** |
| 07.1 | Crear solicitud de reserva | `POST /api/solicitudes-reserva` | **201** |
| 07.2 | Cola de pendientes de la empresa | `GET /api/solicitudes-reserva?empresaId={{empresaId}}&estado=pendiente` | **200** |
| 07.3 | Aprobar solicitud | `POST /api/solicitudes-reserva/{{solicitudReservaId}}/aprobar` | **200** |
| 07.4 | El carro quedó ocupado | `GET /api/carros/{{carroId}}` | **200** |
| 08.1 | Iniciar viaje | `POST /api/viajes` | **201** |
| 08.2 | Foto del tablero al iniciar | `PUT /api/viajes/{{viajeId}}/foto-tablero/inicial` | **200** |
| 08.3 | Registrar lote de puntos GPS | `POST /api/viajes/{{viajeId}}/puntos-ruta` | **200 o 201** |
| 08.4 | Reenviar el mismo lote (no duplica) | `POST /api/viajes/{{viajeId}}/puntos-ruta` | **200 o 201** |
| 08.5 | Ruta del viaje | `GET /api/viajes/{{viajeId}}/puntos-ruta` | **200** |
| 08.6 | Registrar carga de combustible con ticket | `POST /api/cargas-combustible` | **201** |
| 08.7 | Ver el ticket desde MinIO (URL prefirmada) | `GET {{fotoComprobanteUrl}}` | **200** |
| 08.8 | El operador corrige los litros (viaje en curso) | `PUT /api/cargas-combustible/{{cargaCombustibleId}}` | **200** |
| 08.9 | Cargas del viaje | `GET /api/cargas-combustible?viajeId={{viajeId}}` | **200** |
| 08.10 | Finalizar viaje | `POST /api/viajes/{{viajeId}}/finalizar` | **200** |
| 08.11 | Foto del tablero al terminar | `PUT /api/viajes/{{viajeId}}/foto-tablero/final` | **200** |
| 08.12 | El carro actualizó su kilometraje | `GET /api/carros/{{carroId}}` | **200** |
| 08.13 | Ruta procesada (GPS contra odómetro) | `GET /api/viajes/{{viajeId}}/ruta-procesada` | **200** |
| 09.1 | El operador ya no puede corregir (403) | `POST /api/viajes/{{viajeId}}/corregir-kilometraje` | **403** |
| 09.2 | Corregir sin token (401) | `POST /api/viajes/{{viajeId}}/corregir-kilometraje` | **401** |
| 09.3 | Corregir sin motivo (400) | `POST /api/viajes/{{viajeId}}/corregir-kilometraje` | **400** |
| 09.4 | El confirmador corrige el kilometraje final | `POST /api/viajes/{{viajeId}}/corregir-kilometraje` | **200** |
| 09.5 | El carro ajustó su kilometraje | `GET /api/carros/{{carroId}}` | **200** |
| 10.1 | Liberar la reserva | `POST /api/solicitudes-reserva/{{solicitudReservaId}}/liberar` | **200** |
| 10.2 | El carro volvió a estar disponible | `GET /api/carros/{{carroId}}` | **200** |
| 10.3 | La bitácora registra la liberación | `GET /api/cambios-estatus-carro?carroId={{carroId}}` | **200** |
| 10.4 | Liberar otra vez (400) | `POST /api/solicitudes-reserva/{{solicitudReservaId}}/liberar` | **400** |
| 11.1 | Mandar el carro a mantenimiento por una falla | `POST /api/cambios-estatus-carro` | **201** |
| 11.2 | Subir la foto de la falla | `PUT /api/cambios-estatus-carro/{{cambioEstatusCarroId}}/foto-evidencia` | **200** |
| 11.3 | Un carro en mantenimiento no se puede reservar (409) | `POST /api/solicitudes-reserva` | **409** |
| 11.4 | Regresar el carro a disponible | `POST /api/cambios-estatus-carro` | **201** |
| 11.5 | Percances del carro | `GET /api/cambios-estatus-carro?carroId={{carroId}}` | **200** |
| 12.1 | El operador no tiene acceso (403) | `GET /api/auditoria/eventos` | **403** |
| 12.2 | Sin token (401) | `GET /api/auditoria/eventos` | **401** |
| 12.3 | A. Historial de la carga corregida | `GET /api/auditoria/eventos?entidad=carga_combustible&entidadId={{cargaCombustibleId}}` | **200** |
| 12.4 | A. Historial del viaje (corrección del confirmador) | `GET /api/auditoria/eventos?entidad=viaje&entidadId={{viajeId}}&accion=corregir` | **200** |
| 12.5 | A. Historial del carro | `GET /api/auditoria/eventos?entidad=carro&entidadId={{carroId}}` | **200** |
| 12.6 | B. Revisión de combustible del carro | `GET /api/auditoria/revision-combustible?carroId={{carroId}}&desde={{revisionDesde}}&hasta={{revisionHasta}}` | **200** |
| 12.7 | C. Abrir revisión del viaje | `POST /api/auditoria/revisiones` | **201** |
| 12.8 | C. Con observaciones pero sin texto (400) | `PUT /api/auditoria/revisiones/{{revisionViajeId}}/resultado` | **400** |
| 12.9 | C. Registrar resultado y cerrar | `PUT /api/auditoria/revisiones/{{revisionViajeId}}/resultado` | **200** |
| 12.10 | C. Una revisión cerrada ya no cambia (400) | `PUT /api/auditoria/revisiones/{{revisionViajeId}}/resultado` | **400** |
| 12.11 | C. Abrir revisión del carro en el mes | `POST /api/auditoria/revisiones` | **201** |
| 12.12 | C. Revisión sin alcance (400) | `POST /api/auditoria/revisiones` | **400** |
| 12.13 | C. Revisiones abiertas | `GET /api/auditoria/revisiones?abiertas=true` | **200** |
| 12.14 | A. Historial de la revisión | `GET /api/auditoria/eventos?entidad=revision_auditoria&entidadId={{revisionViajeId}}` | **200** |
| 13.1 | Crear un segundo carro | `POST /api/carros` | **201** |
| 13.2 | Dar de baja sin motivo (400) | `POST /api/carros/{{carroBajaId}}/baja` | **400** |
| 13.3 | Dar de baja el carro | `POST /api/carros/{{carroBajaId}}/baja` | **200** |
| 13.4 | Un carro dado de baja no se puede reservar (409) | `POST /api/solicitudes-reserva` | **409** |
| 13.5 | Dar de baja otra vez (400) | `POST /api/carros/{{carroBajaId}}/baja` | **400** |
| 13.6 | La baja quedó en el historial | `GET /api/auditoria/eventos?entidad=carro&entidadId={{carroBajaId}}` | **200** |
| 14.1 | Bandeja del confirmador | `GET /api/notificaciones?empleadoDestinatarioId={{empleadoConfirmadorId}}` | **200** |
| 14.2 | Bandeja del operador | `GET /api/notificaciones?empleadoDestinatarioId={{empleadoOperadorId}}` | **200** |
| 14.3 | Contador de no leídas (badge) | `GET /api/notificaciones/no-leidas/{{empleadoConfirmadorId}}` | **200** |
| 14.4 | Marcar todas como leídas | `POST /api/notificaciones/marcar-todas-leidas/{{empleadoConfirmadorId}}` | **200 o 204** |
| 15.1 | Email repetido (409) | `POST /api/usuarios` | **409** |
| 15.2 | Email inválido (400) | `POST /api/usuarios` | **400** |
| 15.3 | Usuario inexistente (404) | `GET /api/usuarios/00000000-0000-7000-8000-000000000000` | **404** |
| 15.4 | Placas repetidas (409) | `POST /api/carros` | **409** |
| 15.5 | Kilometraje menor al actual (400) | `PUT /api/carros/{{carroId}}` | **400** |
| 15.6 | Foto de perfil en PDF (400) | `PUT /api/empleados/{{empleadoOperadorId}}/foto-perfil` | **400** |
| 15.7 | Carga sin comprobante (400) | `POST /api/cargas-combustible` | **400** |
| 15.8 | Más litros que la capacidad del tanque (400) | `POST /api/cargas-combustible` | **400** |
| 15.9 | Corregir una carga sin token (401) | `PUT /api/cargas-combustible/{{cargaCombustibleId}}` | **401** |
| 15.10 | Foto del tablero con momento inválido (400) | `PUT /api/viajes/{{viajeId}}/foto-tablero/intermedia` | **400** |
| 15.11 | Usuario ya asignado a otro empleado (409) | `POST /api/empleados` | **409** |

## 4. Pruebas

### 01 Usuarios

Alta de los tres usuarios del flujo: el operador (solicita el carro y maneja), el confirmador (aprueba y corrige registros cerrados) y el consultor (revisa la auditoría).

#### 01.1 Crear usuario operador

**Para qué sirve:** Da de alta la cuenta con la que el operador entra a la app móvil. Es el usuario que solicita el carro, maneja y registra el viaje y las cargas.

**Antes:** Antes de empezar, en la variable `run` escribe un número que no hayas usado (por ejemplo `001`; en la siguiente corrida `002`). Así los emails y placas no chocan con corridas anteriores.

**Ruta:** `POST {{host}}/api/usuarios`

**Auth:** Sin token (No Auth)

**Body (raw → JSON):**

```json
{
  "email": "operador.{{run}}@ejemplo.com",
  "password": "Gasolina2026",
  "estado": "activo"
}
```

**Espero:** **201**

**Revisa en la respuesta:**

- `usuarioId` es un UUID (36 caracteres)
- La respuesta no trae la contraseña

**Guarda:** `usuarioId` → `usuarioOperadorId`

#### 01.2 Crear usuario confirmador

**Para qué sirve:** Da de alta la cuenta del confirmador: aprueba las reservas, manda carros a mantenimiento y es el único que puede corregir un viaje ya cerrado.

**Ruta:** `POST {{host}}/api/usuarios`

**Auth:** Sin token (No Auth)

**Body (raw → JSON):**

```json
{
  "email": "confirmador.{{run}}@ejemplo.com",
  "password": "Gasolina2026",
  "estado": "activo"
}
```

**Espero:** **201**

**Guarda:** `usuarioId` → `usuarioConfirmadorId`

#### 01.3 Crear usuario consultor (auditoría)

**Para qué sirve:** Da de alta la cuenta del consultor. Tendrá el rol Consultor/Reportes para entrar al módulo de auditoría.

**Ruta:** `POST {{host}}/api/usuarios`

**Auth:** Sin token (No Auth)

**Body (raw → JSON):**

```json
{
  "email": "consultor.{{run}}@ejemplo.com",
  "password": "Gasolina2026",
  "estado": "activo"
}
```

**Espero:** **201**

**Guarda:** `usuarioId` → `usuarioConsultorId`

#### 01.4 Listar usuarios

**Para qué sirve:** Comprueba que la lista de usuarios responde y que aparecen los tres recién creados.

**Ruta:** `GET {{host}}/api/usuarios`

**Auth:** Sin token (No Auth)

**Espero:** **200**

**Revisa en la respuesta:**

- Regresa un arreglo `[ ... ]`

#### 01.5 Obtener usuario por id

**Para qué sirve:** Comprueba que se puede consultar un usuario por su UUID.

**Ruta:** `GET {{host}}/api/usuarios/{{usuarioOperadorId}}`

**Auth:** Sin token (No Auth)

**Espero:** **200**

**Revisa en la respuesta:**

- `usuarioId` = {{usuarioOperadorId}}

### 02 Roles y privilegios

Roles globales que usa la lógica: 'Confirmador' (aprueba reservas y corrige registros cerrados) y 'Consultor/Reportes' (consulta el módulo de auditoría).

#### 02.1 Crear rol Confirmador (o reutilizarlo)

**Para qué sirve:** Crea el rol global 'Confirmador'. El nombre debe ser exacto: la lógica de correcciones y de auditoría lo busca así.

**Ruta:** `POST {{host}}/api/roles`

**Auth:** Sin token (No Auth)

**Body (raw → JSON):**

```json
{
  "nombre": "Confirmador",
  "descripcion": "Aprueba o rechaza solicitudes y corrige registros cerrados",
  "activo": true
}
```

**Espero:** **201 o 409**

**Guarda:** `rolId` → `rolConfirmadorId`

Si responde **409** es porque el rol ya existe de una corrida anterior (es global). Haz `GET {{host}}/api/roles`, busca el que tenga ese `nombre` y `empresaId: null`, y copia su `rolId` en la variable.

#### 02.2 Crear rol Consultor/Reportes (o reutilizarlo)

**Para qué sirve:** Crea el rol global 'Consultor/Reportes', que da acceso a /api/auditoria.

**Ruta:** `POST {{host}}/api/roles`

**Auth:** Sin token (No Auth)

**Body (raw → JSON):**

```json
{
  "nombre": "Consultor/Reportes",
  "descripcion": "Consulta reportes y el módulo de auditoría y trazabilidad",
  "activo": true
}
```

**Espero:** **201 o 409**

**Guarda:** `rolId` → `rolConsultorId`

Si responde **409** es porque el rol ya existe de una corrida anterior (es global). Haz `GET {{host}}/api/roles`, busca el que tenga ese `nombre` y `empresaId: null`, y copia su `rolId` en la variable.

#### 02.3 Crear privilegio

**Para qué sirve:** Crea un privilegio para documentar qué puede hacer el rol consultor.

**Ruta:** `POST {{host}}/api/privilegios`

**Auth:** Sin token (No Auth)

**Body (raw → JSON):**

```json
{
  "clave": "auditoria.consultar_{{run}}",
  "nombre": "Consultar auditoría",
  "descripcion": "Consultar el historial de cambios y las revisiones",
  "ambito": "empresa",
  "activo": true
}
```

> v5: la `clave` (modulo.accion) es única e inmutable; el `modulo` se toma de la clave (`auditoria`).

**Espero:** **201**

**Guarda:** `privilegioId` → `privilegioId`

#### 02.4 Asignar privilegio al rol Consultor

**Para qué sirve:** Liga el privilegio al rol Consultor/Reportes.

**Ruta:** `POST {{host}}/api/rol-privilegios`

**Auth:** Sin token (No Auth)

**Body (raw → JSON):**

```json
{
  "rolId": "{{rolConsultorId}}",
  "privilegioId": "{{privilegioId}}"
}
```

**Espero:** **201**

#### 02.5 Asignar rol Confirmador al usuario confirmador

**Para qué sirve:** Le da el rol Confirmador al usuario confirmador. Debe hacerse ANTES del login para que el rol viaje dentro del token.

**Ruta:** `POST {{host}}/api/usuario-roles`

**Auth:** Sin token (No Auth)

**Body (raw → JSON):**

```json
{
  "usuarioId": "{{usuarioConfirmadorId}}",
  "rolId": "{{rolConfirmadorId}}"
}
```

**Espero:** **201**

#### 02.6 Asignar rol Consultor al usuario consultor

**Para qué sirve:** Le da el rol Consultor/Reportes al usuario consultor (también antes del login).

**Ruta:** `POST {{host}}/api/usuario-roles`

**Auth:** Sin token (No Auth)

**Body (raw → JSON):**

```json
{
  "usuarioId": "{{usuarioConsultorId}}",
  "rolId": "{{rolConsultorId}}"
}
```

**Espero:** **201**

#### 02.7 Roles del usuario confirmador

**Para qué sirve:** Comprueba que la asignación quedó guardada.

**Ruta:** `GET {{host}}/api/usuario-roles?usuarioId={{usuarioConfirmadorId}}`

**Auth:** Sin token (No Auth)

**Espero:** **200**

**Revisa en la respuesta:**

- La lista no está vacía

### 03 Organización

Corporativo → división → empresa → centro de costo. La empresa nace activa y se puede desactivar (HU-ADM-01).

#### 03.1 Crear corporativo

**Para qué sirve:** Crea el corporativo, raíz de la organización.

**Ruta:** `POST {{host}}/api/corporativos`

**Auth:** Sin token (No Auth)

**Body (raw → JSON):**

```json
{
  "nombre": "Grupo Corporativo del Sureste {{run}}",
  "activo": true
}
```

**Espero:** **201**

**Guarda:** `corporativoId` → `corporativoId`

#### 03.2 Crear división

**Para qué sirve:** Crea una división dentro del corporativo.

**Ruta:** `POST {{host}}/api/divisiones`

**Auth:** Sin token (No Auth)

**Body (raw → JSON):**

```json
{
  "corporativoId": "{{corporativoId}}",
  "nombre": "División Transporte {{run}}",
  "activo": true
}
```

**Espero:** **201**

**Guarda:** `divisionId` → `divisionId`

#### 03.3 Crear empresa

**Para qué sirve:** Crea la empresa (tenant). Todo lo demás (empleados, carros, viajes, auditoría) queda dentro de esta empresa.

**Ruta:** `POST {{host}}/api/empresas`

**Auth:** Sin token (No Auth)

**Body (raw → JSON):**

```json
{
  "nombre": "Gasolinera Tuxtla Centro {{run}}",
  "corporativoId": "{{corporativoId}}",
  "divisionId": "{{divisionId}}"
}
```

**Espero:** **201**

**Revisa en la respuesta:**

- `activo`: true

**Guarda:** `empresaId` → `empresaId`

#### 03.4 Desactivar empresa

**Para qué sirve:** v4 (HU-ADM-01): la empresa se puede desactivar con el campo 'activo' sin borrarla.

**Ruta:** `PUT {{host}}/api/empresas/{{empresaId}}`

**Auth:** Sin token (No Auth)

**Body (raw → JSON):**

```json
{
  "nombre": "Gasolinera Tuxtla Centro {{run}}",
  "corporativoId": "{{corporativoId}}",
  "divisionId": "{{divisionId}}",
  "activo": false
}
```

**Espero:** **200**

**Revisa en la respuesta:**

- `activo`: false

#### 03.5 Reactivar empresa

**Para qué sirve:** Vuelve a activar la empresa para poder seguir con el flujo.

**Ruta:** `PUT {{host}}/api/empresas/{{empresaId}}`

**Auth:** Sin token (No Auth)

**Body (raw → JSON):**

```json
{
  "nombre": "Gasolinera Tuxtla Centro {{run}}",
  "corporativoId": "{{corporativoId}}",
  "divisionId": "{{divisionId}}",
  "activo": true
}
```

**Espero:** **200**

**Revisa en la respuesta:**

- `activo`: true

#### 03.6 Crear centro de costo

**Para qué sirve:** Crea un centro de costo de la empresa.

**Ruta:** `POST {{host}}/api/centros-costo`

**Auth:** Sin token (No Auth)

**Body (raw → JSON):**

```json
{
  "empresaId": "{{empresaId}}",
  "nombre": "Logística",
  "clave": "CC-LOG-{{run}}",
  "activo": true
}
```

**Espero:** **201**

**Guarda:** `centroCostoId` → `centroCostoId`

#### 03.7 Empresas del corporativo

**Para qué sirve:** Comprueba el filtro de empresas por corporativo.

**Ruta:** `GET {{host}}/api/empresas?corporativoId={{corporativoId}}`

**Auth:** Sin token (No Auth)

**Espero:** **200**

**Revisa en la respuesta:**

- Aparece la empresa {{empresaId}}

### 04 Empleados y licencia (MinIO)

Empleados ligados a los usuarios. La licencia se sube a MinIO y queda la fecha de carga (HU-LOG-01).

#### 04.1 Crear empleado operador

**Para qué sirve:** Crea el empleado del operador (persona) y lo liga a su usuario y a la empresa. Nace sin licencia.

**Ruta:** `POST {{host}}/api/empleados`

**Auth:** Sin token (No Auth)

**Body (raw → JSON):**

```json
{
  "empresaId": "{{empresaId}}",
  "usuarioId": "{{usuarioOperadorId}}",
  "nombreCompleto": "Víctor Alejandro Pérez López"
}
```

**Espero:** **201**

**Revisa en la respuesta:**

- `tieneLicencia`: false
- `fechaCargaLicencia`: null

**Guarda:** `empleadoId` → `empleadoOperadorId`

#### 04.2 Crear empleado confirmador

**Para qué sirve:** Crea el empleado del confirmador.

**Ruta:** `POST {{host}}/api/empleados`

**Auth:** Sin token (No Auth)

**Body (raw → JSON):**

```json
{
  "empresaId": "{{empresaId}}",
  "usuarioId": "{{usuarioConfirmadorId}}",
  "nombreCompleto": "Katherine Romina Espinoza"
}
```

**Espero:** **201**

**Guarda:** `empleadoId` → `empleadoConfirmadorId`

#### 04.3 Crear empleado consultor

**Para qué sirve:** Crea el empleado del consultor. Es quien aparecerá como revisor en las revisiones de auditoría.

**Ruta:** `POST {{host}}/api/empleados`

**Auth:** Sin token (No Auth)

**Body (raw → JSON):**

```json
{
  "empresaId": "{{empresaId}}",
  "usuarioId": "{{usuarioConsultorId}}",
  "nombreCompleto": "Edwin de Jesús Díaz García"
}
```

**Espero:** **201**

**Guarda:** `empleadoId` → `empleadoConsultorId`

#### 04.4 Subir licencia del operador (PDF)

**Para qué sirve:** Sube la licencia a MinIO. v4 (HU-LOG-01): se guarda la fecha de carga de la licencia.

**Ruta:** `PUT {{host}}/api/empleados/{{empleadoOperadorId}}/foto-licencia`

**Auth:** Sin token (No Auth)

**Body (form-data):**

| Key | Tipo | Valor |
|---|---|---|
| `archivo` | File | `licencia.pdf` |

**Espero:** **200**

**Revisa en la respuesta:**

- `fotoLicencia` es solo la llave del archivo: `empresa/.../documento/...`
- `fotoLicenciaUrl` es una URL prefirmada de MinIO (contiene `X-Amz-Signature`)
- `tieneLicencia`: true
- `fechaCargaLicencia` no es null (HU-LOG-01)

#### 04.5 Subir foto de perfil del operador

**Para qué sirve:** Sube la foto de perfil del operador a MinIO.

**Ruta:** `PUT {{host}}/api/empleados/{{empleadoOperadorId}}/foto-perfil`

**Auth:** Sin token (No Auth)

**Body (form-data):**

| Key | Tipo | Valor |
|---|---|---|
| `archivo` | File | `perfil.png` |

**Espero:** **200**

**Revisa en la respuesta:**

- `fotoPerfil` es solo la llave del archivo: `empresa/.../perfil/...`
- `fotoPerfilUrl` es una URL prefirmada de MinIO (contiene `X-Amz-Signature`)

**Guarda:** `fotoPerfilUrl` → `fotoPerfilUrl`

#### 04.6 Descargar la foto de perfil desde MinIO (URL prefirmada)

**Para qué sirve:** Comprueba que la URL prefirmada de MinIO descarga la imagen sin credenciales.

**Ruta:** `GET {{fotoPerfilUrl}}`

**Auth:** Sin token (No Auth)

**Espero:** **200**

**Revisa en la respuesta:**

- Se ve la imagen (`Content-Type: image/png`)

#### 04.7 Empleado por usuario

**Para qué sirve:** La app móvil busca el empleado a partir del usuario que inició sesión.

**Ruta:** `GET {{host}}/api/empleados/por-usuario/{{usuarioOperadorId}}`

**Auth:** Sin token (No Auth)

**Espero:** **200**

**Revisa en la respuesta:**

- `empleadoId` = {{empleadoOperadorId}}

### 05 Autenticación (JWT)

Login de los tres usuarios. El token lleva el usuario, su empresa y sus roles: la auditoría y las correcciones lo usan (nunca se confía en ids que mande el cliente).

#### 05.1 Login del operador

**Para qué sirve:** Login del operador. El token trae usuario, empleado, empresa y roles; las peticiones del viaje lo usan.

**Ruta:** `POST {{host}}/api/auth/login`

**Auth:** Sin token (No Auth)

**Body (raw → JSON):**

```json
{
  "email": "operador.{{run}}@ejemplo.com",
  "password": "Gasolina2026"
}
```

**Espero:** **200**

**Revisa en la respuesta:**

- `token` es un JWT (tres partes separadas por punto)
- `perfil.empleado` no es null

**Guarda:** `token` → `tokenOperador`

#### 05.2 Login del confirmador

**Para qué sirve:** Login del confirmador. Su token debe traer el rol Confirmador.

**Ruta:** `POST {{host}}/api/auth/login`

**Auth:** Sin token (No Auth)

**Body (raw → JSON):**

```json
{
  "email": "confirmador.{{run}}@ejemplo.com",
  "password": "Gasolina2026"
}
```

**Espero:** **200**

**Revisa en la respuesta:**

- `token` es un JWT (tres partes separadas por punto)
- `perfil.empleado` no es null
- `perfil.roles` incluye "Confirmador"

**Guarda:** `token` → `tokenConfirmador`

#### 05.3 Login del consultor

**Para qué sirve:** Login del consultor. Su token debe traer el rol Consultor/Reportes.

**Ruta:** `POST {{host}}/api/auth/login`

**Auth:** Sin token (No Auth)

**Body (raw → JSON):**

```json
{
  "email": "consultor.{{run}}@ejemplo.com",
  "password": "Gasolina2026"
}
```

**Espero:** **200**

**Revisa en la respuesta:**

- `token` es un JWT (tres partes separadas por punto)
- `perfil.empleado` no es null
- `perfil.roles` incluye "Consultor/Reportes"

**Guarda:** `token` → `tokenConsultor`

#### 05.4 Perfil con token (Bearer)

**Para qué sirve:** Comprueba que un token válido abre el perfil.

**Ruta:** `GET {{host}}/api/auth/perfil`

**Auth:** Bearer {{tokenOperador}} (operador)

**Espero:** **200**

**Revisa en la respuesta:**

- `empleado.fotoPerfilUrl` es una URL de MinIO
- `privilegios` trae claves (ej. `auditoria.consultar_...`) y `versionPrivilegios` es un hash de 64 caracteres

#### 05.4a Privilegios del usuario (offline-first)

**Para qué sirve:** La app descarga las claves de privilegios efectivos del usuario y su versión para guardarlas en su BD local.

**Ruta:** `GET {{host}}/api/auth/privilegios`

**Auth:** Bearer {{tokenConsultor}} (consultor)

**Espero:** **200**

**Revisa en la respuesta:**

- `claves` incluye `auditoria.consultar_{{run}}` (asignado al rol Consultor en 02.4)
- `version` es un hash de 64 caracteres y el header `ETag` trae el mismo valor entre comillas

**Guarda:** `version` → `versionPrivilegios`

#### 05.4b Privilegios sin cambios (304)

**Para qué sirve:** Si la app manda la versión que ya tiene y los permisos no cambiaron, la API no repite los datos.

**Ruta:** `GET {{host}}/api/auth/privilegios`

**Auth:** Bearer {{tokenConsultor}} (consultor)

**Headers:** `If-None-Match: "{{versionPrivilegios}}"`

**Espero:** **304** (sin cuerpo)

#### 05.5 Perfil sin token (401)

**Para qué sirve:** Comprueba que sin token se rechaza la petición.

**Ruta:** `GET {{host}}/api/auth/perfil`

**Auth:** Sin token (No Auth)

**Espero:** **401**

#### 05.6 Login con contraseña incorrecta (401)

**Para qué sirve:** Comprueba que una contraseña incorrecta no da token.

**Ruta:** `POST {{host}}/api/auth/login`

**Auth:** Sin token (No Auth)

**Body (raw → JSON):**

```json
{
  "email": "operador.{{run}}@ejemplo.com",
  "password": "incorrecta123"
}
```

**Espero:** **401**

**Revisa en la respuesta:**

- El error viene en formato ProblemDetails (`title`, `detail`)

### 06 Carros

Alta del vehículo y su foto (MinIO).

#### 06.1 Crear carro

**Para qué sirve:** Da de alta el carro con kilometraje 15230.5 y tanque de 80 L. Esos números se usan en las alertas de auditoría.

**Ruta:** `POST {{host}}/api/carros`

**Auth:** Sin token (No Auth)

**Body (raw → JSON):**

```json
{
  "empresaId": "{{empresaId}}",
  "marca": "Nissan",
  "modelo": "NP300",
  "anio": 2023,
  "color": "Blanco",
  "placas": "TST-{{run}}",
  "capacidadTanque": 80.0,
  "kilometrajeActual": 15230.5,
  "estado": "disponible"
}
```

**Espero:** **201**

**Revisa en la respuesta:**

- `placas` en mayúsculas
- `dadoDeBaja`: false

**Guarda:** `carroId` → `carroId`

#### 06.2 Subir foto del carro

**Para qué sirve:** Sube la foto del vehículo a MinIO.

**Ruta:** `PUT {{host}}/api/carros/{{carroId}}/foto`

**Auth:** Sin token (No Auth)

**Body (form-data):**

| Key | Tipo | Valor |
|---|---|---|
| `archivo` | File | `carro.jpg` |

**Espero:** **200**

**Revisa en la respuesta:**

- `fotoVehiculo` es solo la llave del archivo: `empresa/.../foto/...`
- `fotoVehiculoUrl` es una URL prefirmada de MinIO (contiene `X-Amz-Signature`)

#### 06.3 Obtener carro

**Para qué sirve:** Consulta el carro con su URL de foto.

**Ruta:** `GET {{host}}/api/carros/{{carroId}}`

**Auth:** Sin token (No Auth)

**Espero:** **200**

**Revisa en la respuesta:**

- `fotoVehiculoUrl` es una URL prefirmada de MinIO (contiene `X-Amz-Signature`)

#### 06.4 Carros disponibles de la empresa

**Para qué sirve:** La app muestra al operador los carros disponibles de su empresa.

**Ruta:** `GET {{host}}/api/carros?empresaId={{empresaId}}&estado=disponible`

**Auth:** Sin token (No Auth)

**Espero:** **200**

**Revisa en la respuesta:**

- Aparece el carro {{carroId}}

### 07 Solicitudes de reserva

El operador pide el carro y el confirmador la aprueba (el carro pasa a 'ocupado').

#### 07.1 Crear solicitud de reserva

**Para qué sirve:** El operador solicita el carro. Genera una notificación al confirmador.

**Ruta:** `POST {{host}}/api/solicitudes-reserva`

**Auth:** Sin token (No Auth)

**Body (raw → JSON):**

```json
{
  "empresaId": "{{empresaId}}",
  "carroId": "{{carroId}}",
  "empleadoSolicitanteId": "{{empleadoOperadorId}}"
}
```

**Espero:** **201**

**Revisa en la respuesta:**

- `estado`: "pendiente"

**Guarda:** `solicitudReservaId` → `solicitudReservaId`

#### 07.2 Cola de pendientes de la empresa

**Para qué sirve:** Cola de solicitudes que ve el confirmador.

**Ruta:** `GET {{host}}/api/solicitudes-reserva?empresaId={{empresaId}}&estado=pendiente`

**Auth:** Sin token (No Auth)

**Espero:** **200**

**Revisa en la respuesta:**

- Aparece la solicitud {{solicitudReservaId}}

#### 07.3 Aprobar solicitud

**Para qué sirve:** El confirmador aprueba: la reserva queda aprobada y el carro pasa a ocupado.

**Ruta:** `POST {{host}}/api/solicitudes-reserva/{{solicitudReservaId}}/aprobar`

**Auth:** Sin token (No Auth)

**Body (raw → JSON):**

```json
{
  "empleadoConfirmadorId": "{{empleadoConfirmadorId}}",
  "motivo": "Viaje a sucursal Tapachula"
}
```

**Espero:** **200**

**Revisa en la respuesta:**

- `estado`: "aprobada"

#### 07.4 El carro quedó ocupado

**Para qué sirve:** Comprueba el efecto de la aprobación sobre el carro.

**Ruta:** `GET {{host}}/api/carros/{{carroId}}`

**Auth:** Sin token (No Auth)

**Espero:** **200**

**Revisa en la respuesta:**

- `estado`: "ocupado"

### 08 Viaje tipo Strava, combustible y foto del tablero

Inicia el viaje con foto del tablero, registra la ruta GPS, una carga con su ticket, el operador corrige un dato mientras el viaje sigue en curso y finaliza con foto del tablero.

#### 08.1 Iniciar viaje

**Para qué sirve:** El operador inicia el viaje desde el celular. v4: el servidor guarda fecha_recepcion (ya no existe 'sincronizado').

**Antes:** Antes de enviar, llena las variables de fechas del viaje (sección 2.3) con la fecha y hora de **hoy**, hace unos minutos.

**Ruta:** `POST {{host}}/api/viajes`

**Auth:** Bearer {{tokenOperador}} (operador)

**Headers:** `X-Origen: movil`

**Body (raw → JSON):**

```json
{
  "empresaId": "{{empresaId}}",
  "carroId": "{{carroId}}",
  "solicitudReservaId": "{{solicitudReservaId}}",
  "empleadoOperadorId": "{{empleadoOperadorId}}",
  "fechaInicio": "{{fechaInicio}}",
  "ubicacionInicioLat": 16.753056,
  "ubicacionInicioLng": -93.115556,
  "kilometrajeInicial": 15230.5
}
```

**Espero:** **201**

**Revisa en la respuesta:**

- `estado`: "enCurso"
- `corregido`: false
- `fechaRecepcion` no es null

**Guarda:** `viajeId` → `viajeId`

#### 08.2 Foto del tablero al iniciar

**Para qué sirve:** v4: foto del tablero (odómetro) al iniciar el viaje, como evidencia del kilometraje inicial.

**Ruta:** `PUT {{host}}/api/viajes/{{viajeId}}/foto-tablero/inicial`

**Auth:** Bearer {{tokenOperador}} (operador)

**Headers:** `X-Origen: movil`

**Body (form-data):**

| Key | Tipo | Valor |
|---|---|---|
| `archivo` | File | `tablero.jpg` |

**Espero:** **200**

**Revisa en la respuesta:**

- `fotoTableroInicial` es solo la llave del archivo: `empresa/.../tablero/...`
- `fotoTableroInicialUrl` es una URL prefirmada de MinIO (contiene `X-Amz-Signature`)
- `fotoTableroFinal`: null

#### 08.3 Registrar lote de puntos GPS

**Para qué sirve:** Envía un lote de puntos GPS (ruta tipo Strava).

**Ruta:** `POST {{host}}/api/viajes/{{viajeId}}/puntos-ruta`

**Auth:** Bearer {{tokenOperador}} (operador)

**Headers:** `X-Origen: movil`

**Body (raw → JSON):**

```json
{
  "puntos": [
    {
      "latitud": 16.753056,
      "longitud": -93.115556,
      "timestampCaptura": "{{punto1}}",
      "precisionMetros": 8,
      "velocidad": 9.5
    },
    {
      "latitud": 16.75012,
      "longitud": -93.10034,
      "timestampCaptura": "{{punto2}}",
      "precisionMetros": 6,
      "velocidad": 11.2
    },
    {
      "latitud": 16.74587,
      "longitud": -93.08521,
      "timestampCaptura": "{{punto3}}",
      "precisionMetros": 7,
      "velocidad": 10.1
    }
  ]
}
```

**Espero:** **200 o 201**

**Revisa en la respuesta:**

- `insertados`: 3

#### 08.4 Reenviar el mismo lote (no duplica)

**Para qué sirve:** Reenvía el mismo lote, como cuando el celular reintenta sin red: no debe duplicar puntos.

**Ruta:** `POST {{host}}/api/viajes/{{viajeId}}/puntos-ruta`

**Auth:** Bearer {{tokenOperador}} (operador)

**Headers:** `X-Origen: movil`

**Body (raw → JSON):**

```json
{
  "puntos": [
    {
      "latitud": 16.753056,
      "longitud": -93.115556,
      "timestampCaptura": "{{punto1}}",
      "precisionMetros": 8,
      "velocidad": 9.5
    },
    {
      "latitud": 16.75012,
      "longitud": -93.10034,
      "timestampCaptura": "{{punto2}}",
      "precisionMetros": 6,
      "velocidad": 11.2
    },
    {
      "latitud": 16.74587,
      "longitud": -93.08521,
      "timestampCaptura": "{{punto3}}",
      "precisionMetros": 7,
      "velocidad": 10.1
    }
  ]
}
```

**Espero:** **200 o 201**

**Revisa en la respuesta:**

- `insertados`: 0 y `duplicados`: 3

#### 08.5 Ruta del viaje

**Para qué sirve:** Consulta la ruta. Cada punto trae su fecha de recepción en el servidor.

**Ruta:** `GET {{host}}/api/viajes/{{viajeId}}/puntos-ruta`

**Auth:** Sin token (No Auth)

**Espero:** **200**

**Revisa en la respuesta:**

- La lista trae 3 puntos
- Cada punto trae `fechaRecepcion` (la pone el servidor)

#### 08.6 Registrar carga de combustible con ticket

**Para qué sirve:** Registra una carga de 45.30 L con la foto del ticket. v4: guarda fecha_subida_comprobante.

**Ruta:** `POST {{host}}/api/cargas-combustible`

**Auth:** Bearer {{tokenOperador}} (operador)

**Headers:** `X-Origen: movil`

**Body (form-data):**

| Key | Tipo | Valor |
|---|---|---|
| `viajeId` | Text | `{{viajeId}}` |
| `litros` | Text | `45.30` |
| `monto` | Text | `1087.20` |
| `fechaCarga` | Text | `{{fechaCarga}}` |
| `ubicacionLat` | Text | `16.750120` |
| `ubicacionLng` | Text | `-93.100340` |
| `fotoComprobante` | File | `ticket.jpg` |

**Espero:** **201**

**Revisa en la respuesta:**

- `fotoComprobante` es solo la llave del archivo: `empresa/.../carga/...`
- `fotoComprobanteUrl` es una URL prefirmada de MinIO (contiene `X-Amz-Signature`)
- `precioPorLitro`: 24 (1087.20 / 45.30)
- `fechaSubidaComprobante` no es null
- `corregido`: false

**Guarda:** `cargaCombustibleId` → `cargaCombustibleId`, `fotoComprobanteUrl` → `fotoComprobanteUrl`

#### 08.7 Ver el ticket desde MinIO (URL prefirmada)

**Para qué sirve:** Comprueba que el ticket se puede ver desde MinIO.

**Ruta:** `GET {{fotoComprobanteUrl}}`

**Auth:** Sin token (No Auth)

**Espero:** **200**

**Revisa en la respuesta:**

- Se ve la imagen (`Content-Type: image/jpeg`)

#### 08.8 El operador corrige los litros (viaje en curso)

**Para qué sirve:** v4: el operador corrige los litros (45.30 → 44.80) mientras el viaje sigue en curso. Exige motivo, marca corregido = true y deja el valor anterior en el historial.

**Ruta:** `PUT {{host}}/api/cargas-combustible/{{cargaCombustibleId}}`

**Auth:** Bearer {{tokenOperador}} (operador)

**Headers:** `X-Origen: movil`

**Body (form-data):**

| Key | Tipo | Valor |
|---|---|---|
| `litros` | Text | `44.80` |
| `monto` | Text | `1075.20` |
| `fechaCarga` | Text | `{{fechaCarga}}` |
| `ubicacionLat` | Text | `16.750120` |
| `ubicacionLng` | Text | `-93.100340` |
| `motivo` | Text | `Escribí 45.30 en vez de 44.80; el ticket dice 44.80 L` |

**Espero:** **200**

**Revisa en la respuesta:**

- `corregido`: true
- `litros`: 44.8
- `fotoComprobante` sigue siendo la misma llave (empieza con `empresa/`)

#### 08.9 Cargas del viaje

**Para qué sirve:** Lista las cargas del viaje.

**Ruta:** `GET {{host}}/api/cargas-combustible?viajeId={{viajeId}}`

**Auth:** Sin token (No Auth)

**Espero:** **200**

**Revisa en la respuesta:**

- La lista trae 1 carga

#### 08.10 Finalizar viaje

**Para qué sirve:** Finaliza el viaje con kilometraje 15310 (a propósito mal: luego se corrige a 15300).

**Antes:** Antes de enviar, pon en `fechaFin` la hora actual (debe ser posterior a `fechaCarga`).

**Ruta:** `POST {{host}}/api/viajes/{{viajeId}}/finalizar`

**Auth:** Bearer {{tokenOperador}} (operador)

**Headers:** `X-Origen: movil`

**Body (raw → JSON):**

```json
{
  "fechaFin": "{{fechaFin}}",
  "ubicacionFinLat": 16.74587,
  "ubicacionFinLng": -93.08521,
  "kilometrajeFinal": 15310.0
}
```

**Espero:** **200**

**Revisa en la respuesta:**

- `estado`: "finalizado"
- `distanciaRecorrida`: 79.5 (15310 − 15230.5)

#### 08.11 Foto del tablero al terminar

**Para qué sirve:** v4: foto del tablero al terminar. No borra la foto inicial.

**Ruta:** `PUT {{host}}/api/viajes/{{viajeId}}/foto-tablero/final`

**Auth:** Bearer {{tokenOperador}} (operador)

**Headers:** `X-Origen: movil`

**Body (form-data):**

| Key | Tipo | Valor |
|---|---|---|
| `archivo` | File | `tablero.jpg` |

**Espero:** **200**

**Revisa en la respuesta:**

- `fotoTableroFinal` es solo la llave del archivo: `empresa/.../tablero/...`
- `fotoTableroFinalUrl` es una URL prefirmada de MinIO (contiene `X-Amz-Signature`)
- `fotoTableroInicial` sigue igual (contiene `inicial-`)

#### 08.12 El carro actualizó su kilometraje

**Para qué sirve:** Comprueba que el carro tomó el kilometraje final del viaje.

**Ruta:** `GET {{host}}/api/carros/{{carroId}}`

**Auth:** Sin token (No Auth)

**Espero:** **200**

**Revisa en la respuesta:**

- `kilometrajeActual`: 15310

#### 08.13 Ruta procesada (GPS contra odómetro)

**Para qué sirve:** Compara la distancia GPS con la del odómetro. Debe marcar alerta porque 79.5 km declarados contra ~3.5 km de GPS no cuadran.

**Ruta:** `GET {{host}}/api/viajes/{{viajeId}}/ruta-procesada`

**Auth:** Sin token (No Auth)

**Espero:** **200**

**Revisa en la respuesta:**

- `distanciaGpsKm` mayor a 0
- `alertaKilometraje`: true (el odómetro dice 79.5 km y el GPS ~3.5 km)

### 09 Correcciones de un viaje cerrado

Con el viaje cerrado, solo un Confirmador o Gestor de activos puede corregir, siempre con motivo. El valor anterior queda en el historial de auditoría.

#### 09.1 El operador ya no puede corregir (403)

**Para qué sirve:** v4: con el viaje cerrado, el operador ya NO puede corregir.

**Ruta:** `POST {{host}}/api/viajes/{{viajeId}}/corregir-kilometraje`

**Auth:** Bearer {{tokenOperador}} (operador)

**Headers:** `X-Origen: movil`

**Body (raw → JSON):**

```json
{
  "kilometrajeInicial": 15230.5,
  "kilometrajeFinal": 15300.0,
  "motivo": "El operador tecleó 15310; la foto del tablero dice 15300"
}
```

**Espero:** **403**

**Revisa en la respuesta:**

- El error viene en formato ProblemDetails (`title`, `detail`)

#### 09.2 Corregir sin token (401)

**Para qué sirve:** Corregir siempre requiere token (para saber quién corrigió).

**Ruta:** `POST {{host}}/api/viajes/{{viajeId}}/corregir-kilometraje`

**Auth:** Sin token (No Auth)

**Body (raw → JSON):**

```json
{
  "kilometrajeInicial": 15230.5,
  "kilometrajeFinal": 15300.0,
  "motivo": "El operador tecleó 15310; la foto del tablero dice 15300"
}
```

**Espero:** **401**

**Revisa en la respuesta:**

- El error viene en formato ProblemDetails (`title`, `detail`)

#### 09.3 Corregir sin motivo (400)

**Para qué sirve:** Corregir siempre requiere motivo.

**Ruta:** `POST {{host}}/api/viajes/{{viajeId}}/corregir-kilometraje`

**Auth:** Bearer {{tokenConfirmador}} (confirmador)

**Body (raw → JSON):**

```json
{
  "kilometrajeInicial": 15230.5,
  "kilometrajeFinal": 15300.0
}
```

**Espero:** **400**

#### 09.4 El confirmador corrige el kilometraje final

**Para qué sirve:** v4: el confirmador corrige el kilometraje final (15310 → 15300) con motivo. Recalcula la distancia.

**Ruta:** `POST {{host}}/api/viajes/{{viajeId}}/corregir-kilometraje`

**Auth:** Bearer {{tokenConfirmador}} (confirmador)

**Body (raw → JSON):**

```json
{
  "kilometrajeInicial": 15230.5,
  "kilometrajeFinal": 15300.0,
  "motivo": "El operador tecleó 15310; la foto del tablero dice 15300"
}
```

**Espero:** **200**

**Revisa en la respuesta:**

- `corregido`: true
- `kilometrajeFinal`: 15300
- `distanciaRecorrida`: 69.5

#### 09.5 El carro ajustó su kilometraje

**Para qué sirve:** Comprueba que la corrección también ajustó el kilometraje del carro.

**Ruta:** `GET {{host}}/api/carros/{{carroId}}`

**Auth:** Sin token (No Auth)

**Espero:** **200**

**Revisa en la respuesta:**

- `kilometrajeActual`: 15300 (antes de la corrección era 15310)

### 10 Liberar el carro (HU-USO-04)

El operador termina sus viajes y libera el carro: la reserva queda 'finalizada' y el carro 'disponible'.

#### 10.1 Liberar la reserva

**Para qué sirve:** v4 (HU-USO-04): el operador libera el carro. La reserva queda finalizada.

**Ruta:** `POST {{host}}/api/solicitudes-reserva/{{solicitudReservaId}}/liberar`

**Auth:** Bearer {{tokenOperador}} (operador)

**Headers:** `X-Origen: movil`

**Body (raw → JSON):**

```json
{
  "observaciones": "Se entrega limpio, con medio tanque"
}
```

**Espero:** **200**

**Revisa en la respuesta:**

- `estado`: "finalizada"
- `fechaLiberacion` no es null y `observacionesLiberacion` contiene "medio tanque"

#### 10.2 El carro volvió a estar disponible

**Para qué sirve:** Comprueba que el carro volvió a disponible.

**Ruta:** `GET {{host}}/api/carros/{{carroId}}`

**Auth:** Sin token (No Auth)

**Espero:** **200**

**Revisa en la respuesta:**

- `estado`: "disponible"

#### 10.3 La bitácora registra la liberación

**Para qué sirve:** La liberación queda en la bitácora de cambios de estatus con tipoMotivo 'liberacion'.

**Ruta:** `GET {{host}}/api/cambios-estatus-carro?carroId={{carroId}}`

**Auth:** Sin token (No Auth)

**Espero:** **200**

**Revisa en la respuesta:**

- Hay un registro con `tipoMotivo`: "liberacion"

#### 10.4 Liberar otra vez (400)

**Para qué sirve:** No se puede liberar dos veces la misma reserva.

**Ruta:** `POST {{host}}/api/solicitudes-reserva/{{solicitudReservaId}}/liberar`

**Auth:** Bearer {{tokenOperador}} (operador)

**Body (raw → JSON):**

```json
{}
```

**Espero:** **400**

**Revisa en la respuesta:**

- El error viene en formato ProblemDetails (`title`, `detail`)

### 11 Cambios de estatus y percances (HU-EST)

Una falla manda el carro a mantenimiento con tipoMotivo y foto de evidencia. Las fallas y golpes forman el registro de percances, sin tabla aparte.

#### 11.1 Mandar el carro a mantenimiento por una falla

**Para qué sirve:** v4: registra un percance. El carro pasa a mantenimiento con tipoMotivo 'fallaMecanica'.

**Ruta:** `POST {{host}}/api/cambios-estatus-carro`

**Auth:** Bearer {{tokenConfirmador}} (confirmador)

**Body (raw → JSON):**

```json
{
  "carroId": "{{carroId}}",
  "empleadoId": "{{empleadoConfirmadorId}}",
  "estatusNuevo": "enMantenimiento",
  "tipoMotivo": "fallaMecanica",
  "motivo": "Ruido en los frenos delanteros"
}
```

**Espero:** **201**

**Revisa en la respuesta:**

- `tipoMotivo`: "fallaMecanica"

**Guarda:** `cambioEstatusCarroId` → `cambioEstatusCarroId`

#### 11.2 Subir la foto de la falla

**Para qué sirve:** v4: sube la foto de evidencia de la falla.

**Ruta:** `PUT {{host}}/api/cambios-estatus-carro/{{cambioEstatusCarroId}}/foto-evidencia`

**Auth:** Bearer {{tokenConfirmador}} (confirmador)

**Body (form-data):**

| Key | Tipo | Valor |
|---|---|---|
| `archivo` | File | `falla.jpg` |

**Espero:** **200**

**Revisa en la respuesta:**

- `fotoEvidencia` es solo la llave del archivo: `empresa/.../estatus/...`
- `fotoEvidenciaUrl` es una URL prefirmada de MinIO (contiene `X-Amz-Signature`)

#### 11.3 Un carro en mantenimiento no se puede reservar (409)

**Para qué sirve:** Comprueba que un carro en mantenimiento no se puede reservar.

**Ruta:** `POST {{host}}/api/solicitudes-reserva`

**Auth:** Sin token (No Auth)

**Body (raw → JSON):**

```json
{
  "empresaId": "{{empresaId}}",
  "carroId": "{{carroId}}",
  "empleadoSolicitanteId": "{{empleadoOperadorId}}"
}
```

**Espero:** **409**

**Revisa en la respuesta:**

- El error viene en formato ProblemDetails (`title`, `detail`)

#### 11.4 Regresar el carro a disponible

**Para qué sirve:** Regresa el carro a disponible después del servicio.

**Ruta:** `POST {{host}}/api/cambios-estatus-carro`

**Auth:** Bearer {{tokenConfirmador}} (confirmador)

**Body (raw → JSON):**

```json
{
  "carroId": "{{carroId}}",
  "empleadoId": "{{empleadoConfirmadorId}}",
  "estatusNuevo": "disponible",
  "tipoMotivo": "servicio",
  "motivo": "Se cambiaron las balatas"
}
```

**Espero:** **201**

#### 11.5 Percances del carro

**Para qué sirve:** Los percances del carro se obtienen filtrando la bitácora por tipoMotivo (no hay tabla aparte).

**Ruta:** `GET {{host}}/api/cambios-estatus-carro?carroId={{carroId}}`

**Auth:** Sin token (No Auth)

**Espero:** **200**

**Revisa en la respuesta:**

- Hay 1 registro con `tipoMotivo`: "fallaMecanica"
- Ese registro trae `fotoEvidenciaUrl` (URL de MinIO)

### 12 Auditoría y trazabilidad

Módulo secundario. Requiere token con rol Administrador, Consultor/Reportes, Confirmador o Gestor de activos. A. Historial de cambios, B. Revisión de combustible, C. Revisiones.

#### 12.1 El operador no tiene acceso (403)

**Para qué sirve:** El operador no tiene rol de auditoría: no entra.

**Ruta:** `GET {{host}}/api/auditoria/eventos`

**Auth:** Bearer {{tokenOperador}} (operador)

**Espero:** **403**

**Revisa en la respuesta:**

- El error viene en formato ProblemDetails (`title`, `detail`)

#### 12.2 Sin token (401)

**Para qué sirve:** Auditoría siempre requiere token.

**Ruta:** `GET {{host}}/api/auditoria/eventos`

**Auth:** Sin token (No Auth)

**Espero:** **401**

#### 12.3 A. Historial de la carga corregida

**Para qué sirve:** Historial (auditoria_evento) de la carga: muestra la creación y la corrección con valores antes/después, motivo, usuario y origen móvil.

**Ruta:** `GET {{host}}/api/auditoria/eventos?entidad=carga_combustible&entidadId={{cargaCombustibleId}}`

**Auth:** Bearer {{tokenConsultor}} (consultor)

**Espero:** **200**

**Revisa en la respuesta:**

- Hay eventos con `accion`: "crear" y "corregir"
- En el evento `corregir`: `valoresAnteriores.litros` = 45.3 y `valoresNuevos.litros` = 44.8
- Ese evento trae `motivo` con el texto de la corrección, `usuarioId` = {{usuarioOperadorId}} y `origen`: "movil"

#### 12.4 A. Historial del viaje (corrección del confirmador)

**Para qué sirve:** Historial del viaje: la corrección del kilometraje hecha por el confirmador.

**Ruta:** `GET {{host}}/api/auditoria/eventos?entidad=viaje&entidadId={{viajeId}}&accion=corregir`

**Auth:** Bearer {{tokenConsultor}} (consultor)

**Espero:** **200**

**Revisa en la respuesta:**

- `valoresAnteriores.kilometraje_final` = 15310 y `valoresNuevos.kilometraje_final` = 15300
- `usuarioId` = {{usuarioConfirmadorId}}

#### 12.5 A. Historial del carro

**Para qué sirve:** Historial del carro: sus cambios de estado. Solo muestra datos de la empresa del token.

**Ruta:** `GET {{host}}/api/auditoria/eventos?entidad=carro&entidadId={{carroId}}`

**Auth:** Bearer {{tokenConsultor}} (consultor)

**Espero:** **200**

**Revisa en la respuesta:**

- Hay eventos con `accion`: "cambiarEstado"
- Todos los eventos tienen `empresaId` = {{empresaId}}

#### 12.6 B. Revisión de combustible del carro

**Para qué sirve:** Revisión de combustible del carro en un periodo: totales del periodo y alertas calculadas al momento.

**Antes:** Antes de enviar, pon `revisionDesde` = ayer y `revisionHasta` = mañana (formato `AAAA-MM-DDTHH:MM:SS`).

**Ruta:** `GET {{host}}/api/auditoria/revision-combustible?carroId={{carroId}}&desde={{revisionDesde}}&hasta={{revisionHasta}}`

**Auth:** Bearer {{tokenConsultor}} (consultor)

**Espero:** **200**

**Revisa en la respuesta:**

- `viajesRegistrados`: 1 y `cargasRegistradas`: 1
- `litrosRegistrados`: 44.8
- `cargasConComprobante`: 1
- En `alertas` hay una con `tipo`: "registroCorregido"
- En `alertas` hay una con `tipo`: "kilometrajeContraGps"
- NO hay alerta `comprobantePendiente`

#### 12.7 C. Abrir revisión del viaje

**Para qué sirve:** Abre una revisión del viaje. El revisor sale del token, no del body.

**Ruta:** `POST {{host}}/api/auditoria/revisiones`

**Auth:** Bearer {{tokenConsultor}} (consultor)

**Body (raw → JSON):**

```json
{
  "viajeId": "{{viajeId}}",
  "observaciones": "Revisar diferencia entre odómetro y GPS"
}
```

**Espero:** **201**

**Revisa en la respuesta:**

- `estadoRevision`: "pendienteInformacion"
- `empleadoRevisorId` = {{empleadoConsultorId}}

**Guarda:** `revisionAuditoriaId` → `revisionViajeId`

#### 12.8 C. Con observaciones pero sin texto (400)

**Para qué sirve:** Si el resultado es 'conObservaciones', las observaciones son obligatorias.

**Ruta:** `PUT {{host}}/api/auditoria/revisiones/{{revisionViajeId}}/resultado`

**Auth:** Bearer {{tokenConsultor}} (consultor)

**Body (raw → JSON):**

```json
{
  "estadoRevision": "conObservaciones",
  "cerrar": false
}
```

**Espero:** **400**

**Revisa en la respuesta:**

- El error viene en formato ProblemDetails (`title`, `detail`)

#### 12.9 C. Registrar resultado y cerrar

**Para qué sirve:** Registra el resultado de la revisión y la cierra.

**Ruta:** `PUT {{host}}/api/auditoria/revisiones/{{revisionViajeId}}/resultado`

**Auth:** Bearer {{tokenConsultor}} (consultor)

**Body (raw → JSON):**

```json
{
  "estadoRevision": "conObservaciones",
  "observaciones": "El GPS registró 3.5 km y el odómetro 69.5 km. Se pidió aclaración al operador.",
  "cerrar": true
}
```

**Espero:** **200**

**Revisa en la respuesta:**

- `estadoRevision`: "conObservaciones" y `estaCerrada`: true

#### 12.10 C. Una revisión cerrada ya no cambia (400)

**Para qué sirve:** Una revisión cerrada ya no se puede modificar.

**Ruta:** `PUT {{host}}/api/auditoria/revisiones/{{revisionViajeId}}/resultado`

**Auth:** Bearer {{tokenConsultor}} (consultor)

**Body (raw → JSON):**

```json
{
  "estadoRevision": "revisado",
  "cerrar": false
}
```

**Espero:** **400**

**Revisa en la respuesta:**

- El error viene en formato ProblemDetails (`title`, `detail`)

#### 12.11 C. Abrir revisión del carro en el mes

**Para qué sirve:** Abre una revisión de un carro en un periodo (el mes actual).

**Antes:** Antes de enviar, pon `periodoDesde` = día 1 del mes actual y `periodoHasta` = hoy (formato `AAAA-MM-DD`).

**Ruta:** `POST {{host}}/api/auditoria/revisiones`

**Auth:** Bearer {{tokenConsultor}} (consultor)

**Body (raw → JSON):**

```json
{
  "carroId": "{{carroId}}",
  "periodoDesde": "{{periodoDesde}}",
  "periodoHasta": "{{periodoHasta}}",
  "observaciones": "Gasto de combustible del mes"
}
```

**Espero:** **201**

**Revisa en la respuesta:**

- `carroId` = {{carroId}} y `viajeId`: null

**Guarda:** `revisionAuditoriaId` → `revisionCarroId`

#### 12.12 C. Revisión sin alcance (400)

**Para qué sirve:** Una revisión debe tener alcance: un viaje, o un carro con su periodo.

**Ruta:** `POST {{host}}/api/auditoria/revisiones`

**Auth:** Bearer {{tokenConsultor}} (consultor)

**Body (raw → JSON):**

```json
{
  "observaciones": "Sin viaje ni carro"
}
```

**Espero:** **400**

**Revisa en la respuesta:**

- El error viene en formato ProblemDetails (`title`, `detail`)

#### 12.13 C. Revisiones abiertas

**Para qué sirve:** Lista las revisiones abiertas: aparece la del carro y no la del viaje (ya cerrada).

**Ruta:** `GET {{host}}/api/auditoria/revisiones?abiertas=true`

**Auth:** Bearer {{tokenConsultor}} (consultor)

**Espero:** **200**

**Revisa en la respuesta:**

- Aparece la revisión {{revisionCarroId}}
- NO aparece la revisión {{revisionViajeId}} (ya está cerrada)

#### 12.14 A. Historial de la revisión

**Para qué sirve:** Las propias revisiones también quedan en el historial.

**Ruta:** `GET {{host}}/api/auditoria/eventos?entidad=revision_auditoria&entidadId={{revisionViajeId}}`

**Auth:** Bearer {{tokenConsultor}} (consultor)

**Espero:** **200**

**Revisa en la respuesta:**

- Hay eventos con `accion`: "crear" y "cambiarEstado"

### 13 Baja de carro (HU-AST-03)

El carro se da de baja sin borrarlo: su historial se conserva y ya no se puede reservar.

#### 13.1 Crear un segundo carro

**Para qué sirve:** Crea un segundo carro solo para probar la baja.

**Ruta:** `POST {{host}}/api/carros`

**Auth:** Sin token (No Auth)

**Body (raw → JSON):**

```json
{
  "empresaId": "{{empresaId}}",
  "marca": "Chevrolet",
  "modelo": "Aveo",
  "anio": 2018,
  "color": "Gris",
  "placas": "BAJ-{{run}}",
  "capacidadTanque": 45.0,
  "kilometrajeActual": 98000,
  "estado": "disponible"
}
```

**Espero:** **201**

**Guarda:** `carroId` → `carroBajaId`

#### 13.2 Dar de baja sin motivo (400)

**Para qué sirve:** Dar de baja exige motivo.

**Ruta:** `POST {{host}}/api/carros/{{carroBajaId}}/baja`

**Auth:** Sin token (No Auth)

**Body (raw → JSON):**

```json
{}
```

**Espero:** **400**

#### 13.3 Dar de baja el carro

**Para qué sirve:** v4 (HU-AST-03): da de baja el carro sin borrarlo. Guarda fecha y motivo.

**Ruta:** `POST {{host}}/api/carros/{{carroBajaId}}/baja`

**Auth:** Bearer {{tokenConfirmador}} (confirmador)

**Body (raw → JSON):**

```json
{
  "motivo": "Venta de la unidad"
}
```

**Espero:** **200**

**Revisa en la respuesta:**

- `dadoDeBaja`: true, `fechaBaja` no es null y `motivoBaja`: "Venta de la unidad"

#### 13.4 Un carro dado de baja no se puede reservar (409)

**Para qué sirve:** Un carro dado de baja no se puede reservar.

**Ruta:** `POST {{host}}/api/solicitudes-reserva`

**Auth:** Sin token (No Auth)

**Body (raw → JSON):**

```json
{
  "empresaId": "{{empresaId}}",
  "carroId": "{{carroBajaId}}",
  "empleadoSolicitanteId": "{{empleadoOperadorId}}"
}
```

**Espero:** **409**

**Revisa en la respuesta:**

- El error viene en formato ProblemDetails (`title`, `detail`)

#### 13.5 Dar de baja otra vez (400)

**Para qué sirve:** No se puede dar de baja dos veces.

**Ruta:** `POST {{host}}/api/carros/{{carroBajaId}}/baja`

**Auth:** Sin token (No Auth)

**Body (raw → JSON):**

```json
{
  "motivo": "Otra vez"
}
```

**Espero:** **400**

**Revisa en la respuesta:**

- El error viene en formato ProblemDetails (`title`, `detail`)

#### 13.6 La baja quedó en el historial

**Para qué sirve:** La baja queda en el historial de auditoría con su motivo.

**Ruta:** `GET {{host}}/api/auditoria/eventos?entidad=carro&entidadId={{carroBajaId}}`

**Auth:** Bearer {{tokenConsultor}} (consultor)

**Espero:** **200**

**Revisa en la respuesta:**

- Hay un evento con `accion`: "cambiarEstado" y `motivo`: "Venta de la unidad"

### 14 Notificaciones

Generadas automáticamente: nueva solicitud (al confirmador) y resultado (al operador).

#### 14.1 Bandeja del confirmador

**Para qué sirve:** El confirmador recibió el aviso de la nueva solicitud (07.1).

**Ruta:** `GET {{host}}/api/notificaciones?empleadoDestinatarioId={{empleadoConfirmadorId}}`

**Auth:** Sin token (No Auth)

**Espero:** **200**

**Revisa en la respuesta:**

- Hay una notificación con `tipo`: "nuevaSolicitud"

#### 14.2 Bandeja del operador

**Para qué sirve:** El operador recibió el resultado de su solicitud (07.3).

**Ruta:** `GET {{host}}/api/notificaciones?empleadoDestinatarioId={{empleadoOperadorId}}`

**Auth:** Sin token (No Auth)

**Espero:** **200**

**Revisa en la respuesta:**

- Hay una notificación con `tipo`: "resultadoSolicitud"

#### 14.3 Contador de no leídas (badge)

**Para qué sirve:** Contador de no leídas para el badge de la app.

**Ruta:** `GET {{host}}/api/notificaciones/no-leidas/{{empleadoConfirmadorId}}`

**Auth:** Sin token (No Auth)

**Espero:** **200**

#### 14.4 Marcar todas como leídas

**Para qué sirve:** Marca todas las notificaciones del confirmador como leídas.

**Ruta:** `POST {{host}}/api/notificaciones/marcar-todas-leidas/{{empleadoConfirmadorId}}`

**Auth:** Sin token (No Auth)

**Espero:** **200 o 204**

### 15 Validaciones y errores

Evidencia de que la API rechaza datos inválidos con el código correcto.

#### 15.1 Email repetido (409)

**Para qué sirve:** No se permiten dos usuarios con el mismo email.

**Ruta:** `POST {{host}}/api/usuarios`

**Auth:** Sin token (No Auth)

**Body (raw → JSON):**

```json
{
  "email": "operador.{{run}}@ejemplo.com",
  "password": "OtraClave123",
  "estado": "activo"
}
```

**Espero:** **409**

**Revisa en la respuesta:**

- El error viene en formato ProblemDetails (`title`, `detail`)

#### 15.2 Email inválido (400)

**Para qué sirve:** El email debe tener formato válido.

**Ruta:** `POST {{host}}/api/usuarios`

**Auth:** Sin token (No Auth)

**Body (raw → JSON):**

```json
{
  "email": "esto-no-es-email",
  "password": "Gasolina2026",
  "estado": "activo"
}
```

**Espero:** **400**

#### 15.3 Usuario inexistente (404)

**Para qué sirve:** Un UUID que no existe da 404.

**Ruta:** `GET {{host}}/api/usuarios/00000000-0000-7000-8000-000000000000`

**Auth:** Sin token (No Auth)

**Espero:** **404**

**Revisa en la respuesta:**

- El error viene en formato ProblemDetails (`title`, `detail`)

#### 15.4 Placas repetidas (409)

**Para qué sirve:** No se permiten placas repetidas.

**Ruta:** `POST {{host}}/api/carros`

**Auth:** Sin token (No Auth)

**Body (raw → JSON):**

```json
{
  "empresaId": "{{empresaId}}",
  "marca": "Toyota",
  "modelo": "Hilux",
  "placas": "TST-{{run}}",
  "kilometrajeActual": 0
}
```

**Espero:** **409**

**Revisa en la respuesta:**

- El error viene en formato ProblemDetails (`title`, `detail`)

#### 15.5 Kilometraje menor al actual (400)

**Para qué sirve:** El kilometraje de un carro no puede bajar al editarlo (para eso existe la corrección con motivo).

**Ruta:** `PUT {{host}}/api/carros/{{carroId}}`

**Auth:** Sin token (No Auth)

**Body (raw → JSON):**

```json
{
  "empresaId": "{{empresaId}}",
  "marca": "Nissan",
  "modelo": "NP300",
  "placas": "TST-{{run}}",
  "kilometrajeActual": 100
}
```

**Espero:** **400**

**Revisa en la respuesta:**

- El error viene en formato ProblemDetails (`title`, `detail`)

#### 15.6 Foto de perfil en PDF (400)

**Para qué sirve:** La foto de perfil solo acepta imágenes.

**Ruta:** `PUT {{host}}/api/empleados/{{empleadoOperadorId}}/foto-perfil`

**Auth:** Sin token (No Auth)

**Body (form-data):**

| Key | Tipo | Valor |
|---|---|---|
| `archivo` | File | `licencia.pdf` |

**Espero:** **400**

**Revisa en la respuesta:**

- El error viene en formato ProblemDetails (`title`, `detail`)

#### 15.7 Carga sin comprobante (400)

**Para qué sirve:** Una carga sin foto del ticket se rechaza.

**Ruta:** `POST {{host}}/api/cargas-combustible`

**Auth:** Sin token (No Auth)

**Body (form-data):**

| Key | Tipo | Valor |
|---|---|---|
| `viajeId` | Text | `{{viajeId}}` |
| `litros` | Text | `20` |
| `fechaCarga` | Text | `{{fechaCarga}}` |

**Espero:** **400**

#### 15.8 Más litros que la capacidad del tanque (400)

**Para qué sirve:** Más litros que la capacidad del tanque se rechaza.

**Ruta:** `POST {{host}}/api/cargas-combustible`

**Auth:** Sin token (No Auth)

**Body (form-data):**

| Key | Tipo | Valor |
|---|---|---|
| `viajeId` | Text | `{{viajeId}}` |
| `litros` | Text | `500` |
| `fechaCarga` | Text | `{{fechaInicio}}` |
| `fotoComprobante` | File | `ticket.jpg` |

**Espero:** **400**

**Revisa en la respuesta:**

- El error viene en formato ProblemDetails (`title`, `detail`)

#### 15.9 Corregir una carga sin token (401)

**Para qué sirve:** Corregir una carga requiere token.

**Ruta:** `PUT {{host}}/api/cargas-combustible/{{cargaCombustibleId}}`

**Auth:** Sin token (No Auth)

**Body (form-data):**

| Key | Tipo | Valor |
|---|---|---|
| `litros` | Text | `40` |
| `fechaCarga` | Text | `{{fechaCarga}}` |
| `motivo` | Text | `Prueba` |

**Espero:** **401**

**Revisa en la respuesta:**

- El error viene en formato ProblemDetails (`title`, `detail`)

#### 15.10 Foto del tablero con momento inválido (400)

**Para qué sirve:** El momento de la foto del tablero solo puede ser 'inicial' o 'final'.

**Ruta:** `PUT {{host}}/api/viajes/{{viajeId}}/foto-tablero/intermedia`

**Auth:** Sin token (No Auth)

**Body (form-data):**

| Key | Tipo | Valor |
|---|---|---|
| `archivo` | File | `tablero.jpg` |

**Espero:** **400**

#### 15.11 Usuario ya asignado a otro empleado (409)

**Para qué sirve:** Un usuario solo puede estar ligado a un empleado.

**Ruta:** `POST {{host}}/api/empleados`

**Auth:** Sin token (No Auth)

**Body (raw → JSON):**

```json
{
  "empresaId": "{{empresaId}}",
  "usuarioId": "{{usuarioOperadorId}}",
  "nombreCompleto": "Otra Persona"
}
```

**Espero:** **409**

**Revisa en la respuesta:**

- El error viene en formato ProblemDetails (`title`, `detail`)
