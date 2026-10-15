# Plan de pruebas manual en Postman — APIGasoline v4

Este plan reemplaza al anterior. Empieza desde usuarios y sigue el flujo real de la app: organización, empleados, login, carro, reserva, viaje tipo Strava con carga de combustible, correcciones, liberación, percances, auditoría y baja del carro.

Cada paso usa datos que se crearon en los pasos anteriores, así que hay que hacerlos **en orden**. La colección `postman/APIGasoline.postman_collection.json` tiene exactamente estas mismas peticiones; si la importas, cada una ya trae el body y las pruebas.

## 0. Antes de empezar

1. Aplica la migración `AuditoriaYTrazabilidadV4` (ver `docs/CambiosV4-Auditoria.md`) y levanta la API.
2. En Postman crea un environment **APIGasoline v4** con estas variables:
   - `host` = `http://localhost:5234`
   - `run` = un sufijo cualquiera, por ejemplo `001`. **Cámbialo cada vez que repitas el plan** (`002`, `003`…) para que los correos, placas y nombres no choquen con los de la vez anterior (409).
   - Todas las demás variables de la tabla del final, vacías. Cuando un paso diga **Guardar**, copia ese valor de la respuesta y pégalo en la variable del environment.
3. Las fechas van en hora **local** del servidor, sin zona: `2026-10-13T10:15:00`.
4. Los archivos para las subidas están en `APIGasoline/pruebas/` (ticket.jpg, carro.jpg, perfil.png, licencia.pdf, tablero.jpg, falla.jpg).
5. **Token:** en la pestaña *Authorization* elige *Bearer Token* y pon la variable indicada (`{{tokenOperador}}`, `{{tokenConfirmador}}` o `{{tokenConsultor}}`). Si dice *Ninguno*, elige *No Auth*.
6. **X-Origen: movil:** cuando un paso lo indique, agrega ese header. Así el historial de auditoría registra que el cambio vino del celular.

Contraseña de todos los usuarios de prueba: `Gasolina2026`.


## 01 Usuarios

Alta de los tres usuarios del flujo: el operador (solicita el carro y maneja), el confirmador (aprueba y corrige registros cerrados) y el consultor (revisa la auditoría).


### 01.1 Crear usuario operador

`POST {{host}}/api/usuarios`

- **Token:** Ninguno
- **Antes de enviar:** asegúrate de que `run` tenga un valor nuevo.
- **Body** (raw, JSON):

```json
{
  "email": "operador.{{run}}@ejemplo.com",
  "password": "Gasolina2026",
  "estado": "activo"
}
```

- **Resultado esperado:**
  - Código 201
  - El id es un UUID
  - No devuelve la contraseña
- **Guardar:** `usuarioOperadorId` ← `usuarioId` de la respuesta


### 01.2 Crear usuario confirmador

`POST {{host}}/api/usuarios`

- **Token:** Ninguno
- **Body** (raw, JSON):

```json
{
  "email": "confirmador.{{run}}@ejemplo.com",
  "password": "Gasolina2026",
  "estado": "activo"
}
```

- **Resultado esperado:**
  - Código 201
- **Guardar:** `usuarioConfirmadorId` ← `usuarioId` de la respuesta


### 01.3 Crear usuario consultor (auditoría)

`POST {{host}}/api/usuarios`

- **Token:** Ninguno
- **Body** (raw, JSON):

```json
{
  "email": "consultor.{{run}}@ejemplo.com",
  "password": "Gasolina2026",
  "estado": "activo"
}
```

- **Resultado esperado:**
  - Código 201
- **Guardar:** `usuarioConsultorId` ← `usuarioId` de la respuesta


### 01.4 Listar usuarios

`GET {{host}}/api/usuarios`

- **Token:** Ninguno
- **Resultado esperado:**
  - Código 200
  - Es una lista


### 01.5 Obtener usuario por id

`GET {{host}}/api/usuarios/{{usuarioOperadorId}}`

- **Token:** Ninguno
- **Resultado esperado:**
  - Código 200
  - Es el usuario solicitado


## 02 Roles y privilegios

Roles globales que usa la lógica: 'Confirmador' (aprueba reservas y corrige registros cerrados) y 'Consultor/Reportes' (consulta el módulo de auditoría).


### 02.1 Crear rol Confirmador (o reutilizarlo)

`POST {{host}}/api/roles`

- **Token:** Ninguno
- **Body** (raw, JSON):

```json
{
  "nombre": "Confirmador",
  "descripcion": "Aprueba o rechaza solicitudes y corrige registros cerrados",
  "activo": true
}
```

- **Resultado esperado:**
  - Código 201 o 409
- **Guardar:** `rolConfirmadorId` ← `rolId` de la respuesta. Si responde **409** (el rol ya existía de una vez anterior), haz `GET {{host}}/api/roles`, busca el rol `Confirmador` que no tiene `empresaId` y guarda su `rolId`.


### 02.2 Crear rol Consultor/Reportes (o reutilizarlo)

`POST {{host}}/api/roles`

- **Token:** Ninguno
- **Body** (raw, JSON):

```json
{
  "nombre": "Consultor/Reportes",
  "descripcion": "Consulta reportes y el módulo de auditoría y trazabilidad",
  "activo": true
}
```

- **Resultado esperado:**
  - Código 201 o 409
- **Guardar:** `rolConsultorId` ← `rolId` de la respuesta. Si responde **409** (el rol ya existía de una vez anterior), haz `GET {{host}}/api/roles`, busca el rol `Consultor/Reportes` que no tiene `empresaId` y guarda su `rolId`.


### 02.3 Crear privilegio

`POST {{host}}/api/privilegios`

- **Token:** Ninguno
- **Body** (raw, JSON):

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

- **Resultado esperado:**
  - Código 201
- **Guardar:** `privilegioId` ← `privilegioId` de la respuesta


### 02.4 Asignar privilegio al rol Consultor

`POST {{host}}/api/rol-privilegios`

- **Token:** Ninguno
- **Body** (raw, JSON):

```json
{
  "rolId": "{{rolConsultorId}}",
  "privilegioId": "{{privilegioId}}"
}
```

- **Resultado esperado:**
  - Código 201


### 02.5 Asignar rol Confirmador al usuario confirmador

`POST {{host}}/api/usuario-roles`

- **Token:** Ninguno
- **Body** (raw, JSON):

```json
{
  "usuarioId": "{{usuarioConfirmadorId}}",
  "rolId": "{{rolConfirmadorId}}"
}
```

- **Resultado esperado:**
  - Código 201


### 02.6 Asignar rol Consultor al usuario consultor

`POST {{host}}/api/usuario-roles`

- **Token:** Ninguno
- **Body** (raw, JSON):

```json
{
  "usuarioId": "{{usuarioConsultorId}}",
  "rolId": "{{rolConsultorId}}"
}
```

- **Resultado esperado:**
  - Código 201


### 02.7 Roles del usuario confirmador

`GET {{host}}/api/usuario-roles?usuarioId={{usuarioConfirmadorId}}`

- **Token:** Ninguno
- **Resultado esperado:**
  - Código 200
  - Tiene el rol asignado


## 03 Organización

Corporativo → división → empresa → centro de costo. La empresa nace activa y se puede desactivar (HU-ADM-01).


### 03.1 Crear corporativo

`POST {{host}}/api/corporativos`

- **Token:** Ninguno
- **Body** (raw, JSON):

```json
{
  "nombre": "Grupo Corporativo del Sureste {{run}}",
  "activo": true
}
```

- **Resultado esperado:**
  - Código 201
- **Guardar:** `corporativoId` ← `corporativoId` de la respuesta


### 03.2 Crear división

`POST {{host}}/api/divisiones`

- **Token:** Ninguno
- **Body** (raw, JSON):

```json
{
  "corporativoId": "{{corporativoId}}",
  "nombre": "División Transporte {{run}}",
  "activo": true
}
```

- **Resultado esperado:**
  - Código 201
- **Guardar:** `divisionId` ← `divisionId` de la respuesta


### 03.3 Crear empresa

`POST {{host}}/api/empresas`

- **Token:** Ninguno
- **Body** (raw, JSON):

```json
{
  "nombre": "Gasolinera Tuxtla Centro {{run}}",
  "corporativoId": "{{corporativoId}}",
  "divisionId": "{{divisionId}}"
}
```

- **Resultado esperado:**
  - Código 201
  - Nace activa
- **Guardar:** `empresaId` ← `empresaId` de la respuesta


### 03.4 Desactivar empresa

`PUT {{host}}/api/empresas/{{empresaId}}`

- **Token:** Ninguno
- **Body** (raw, JSON):

```json
{
  "nombre": "Gasolinera Tuxtla Centro {{run}}",
  "corporativoId": "{{corporativoId}}",
  "divisionId": "{{divisionId}}",
  "activo": false
}
```

- **Resultado esperado:**
  - Código 200
  - Queda inactiva


### 03.5 Reactivar empresa

`PUT {{host}}/api/empresas/{{empresaId}}`

- **Token:** Ninguno
- **Body** (raw, JSON):

```json
{
  "nombre": "Gasolinera Tuxtla Centro {{run}}",
  "corporativoId": "{{corporativoId}}",
  "divisionId": "{{divisionId}}",
  "activo": true
}
```

- **Resultado esperado:**
  - Código 200
  - Queda activa otra vez


### 03.6 Crear centro de costo

`POST {{host}}/api/centros-costo`

- **Token:** Ninguno
- **Body** (raw, JSON):

```json
{
  "empresaId": "{{empresaId}}",
  "nombre": "Logística",
  "clave": "CC-LOG-{{run}}",
  "activo": true
}
```

- **Resultado esperado:**
  - Código 201
- **Guardar:** `centroCostoId` ← `centroCostoId` de la respuesta


### 03.7 Empresas del corporativo

`GET {{host}}/api/empresas?corporativoId={{corporativoId}}`

- **Token:** Ninguno
- **Resultado esperado:**
  - Código 200
  - Aparece la empresa creada


## 04 Empleados y licencia (MinIO)

Empleados ligados a los usuarios. La licencia se sube a MinIO y queda la fecha de carga (HU-LOG-01).


### 04.1 Crear empleado operador

`POST {{host}}/api/empleados`

- **Token:** Ninguno
- **Body** (raw, JSON):

```json
{
  "empresaId": "{{empresaId}}",
  "usuarioId": "{{usuarioOperadorId}}",
  "nombreCompleto": "Víctor Alejandro Pérez López"
}
```

- **Resultado esperado:**
  - Código 201
  - Todavía no tiene licencia
  - Sin fecha de carga de licencia
- **Guardar:** `empleadoOperadorId` ← `empleadoId` de la respuesta


### 04.2 Crear empleado confirmador

`POST {{host}}/api/empleados`

- **Token:** Ninguno
- **Body** (raw, JSON):

```json
{
  "empresaId": "{{empresaId}}",
  "usuarioId": "{{usuarioConfirmadorId}}",
  "nombreCompleto": "Katherine Romina Espinoza"
}
```

- **Resultado esperado:**
  - Código 201
- **Guardar:** `empleadoConfirmadorId` ← `empleadoId` de la respuesta


### 04.3 Crear empleado consultor

`POST {{host}}/api/empleados`

- **Token:** Ninguno
- **Body** (raw, JSON):

```json
{
  "empresaId": "{{empresaId}}",
  "usuarioId": "{{usuarioConsultorId}}",
  "nombreCompleto": "Edwin de Jesús Díaz García"
}
```

- **Resultado esperado:**
  - Código 201
- **Guardar:** `empleadoConsultorId` ← `empleadoId` de la respuesta


### 04.4 Subir licencia del operador (PDF)

`PUT {{host}}/api/empleados/{{empleadoOperadorId}}/foto-licencia`

- **Token:** Ninguno
- **Body** (form-data):
  - `archivo` (tipo *File*) = `licencia.pdf`
- **Resultado esperado:**
  - Código 200
  - fotoLicencia: en BD solo se guarda la llave (empresa/.../documento/...)
  - fotoLicenciaUrl: URL prefirmada de MinIO
  - Ahora sí tiene licencia
  - Guarda la fecha de carga (HU-LOG-01)


### 04.5 Subir foto de perfil del operador

`PUT {{host}}/api/empleados/{{empleadoOperadorId}}/foto-perfil`

- **Token:** Ninguno
- **Body** (form-data):
  - `archivo` (tipo *File*) = `perfil.png`
- **Resultado esperado:**
  - Código 200
  - fotoPerfil: en BD solo se guarda la llave (empresa/.../perfil/...)
  - fotoPerfilUrl: URL prefirmada de MinIO
- **Guardar:** `fotoPerfilUrl` ← `fotoPerfilUrl` de la respuesta


### 04.6 Descargar la foto de perfil desde MinIO (URL prefirmada)

`GET {{fotoPerfilUrl}}`

- **Token:** Ninguno
- **Resultado esperado:**
  - Código 200
  - MinIO devuelve una imagen PNG


### 04.7 Empleado por usuario

`GET {{host}}/api/empleados/por-usuario/{{usuarioOperadorId}}`

- **Token:** Ninguno
- **Resultado esperado:**
  - Código 200
  - Es el empleado del operador


## 05 Autenticación (JWT)

Login de los tres usuarios. El token lleva el usuario, su empresa y sus roles: la auditoría y las correcciones lo usan (nunca se confía en ids que mande el cliente).


### 05.1 Login del operador

`POST {{host}}/api/auth/login`

- **Token:** Ninguno
- **Body** (raw, JSON):

```json
{
  "email": "operador.{{run}}@ejemplo.com",
  "password": "Gasolina2026"
}
```

- **Resultado esperado:**
  - Código 200
  - Devuelve un token JWT
  - El token trae al empleado (su empresa va en el token)
- **Guardar:** `tokenOperador` ← `token` de la respuesta


### 05.2 Login del confirmador

`POST {{host}}/api/auth/login`

- **Token:** Ninguno
- **Body** (raw, JSON):

```json
{
  "email": "confirmador.{{run}}@ejemplo.com",
  "password": "Gasolina2026"
}
```

- **Resultado esperado:**
  - Código 200
  - Devuelve un token JWT
  - El token trae al empleado (su empresa va en el token)
  - Tiene el rol Confirmador
- **Guardar:** `tokenConfirmador` ← `token` de la respuesta


### 05.3 Login del consultor

`POST {{host}}/api/auth/login`

- **Token:** Ninguno
- **Body** (raw, JSON):

```json
{
  "email": "consultor.{{run}}@ejemplo.com",
  "password": "Gasolina2026"
}
```

- **Resultado esperado:**
  - Código 200
  - Devuelve un token JWT
  - El token trae al empleado (su empresa va en el token)
  - Tiene el rol Consultor/Reportes
- **Guardar:** `tokenConsultor` ← `token` de la respuesta


### 05.4 Perfil con token (Bearer)

`GET {{host}}/api/auth/perfil`

- **Token:** `{{tokenOperador}}`
- **Resultado esperado:**
  - Código 200
  - Incluye el empleado con su foto


### 05.5 Perfil sin token (401)

`GET {{host}}/api/auth/perfil`

- **Token:** Ninguno
- **Resultado esperado:**
  - Código 401


### 05.6 Login con contraseña incorrecta (401)

`POST {{host}}/api/auth/login`

- **Token:** Ninguno
- **Body** (raw, JSON):

```json
{
  "email": "operador.{{run}}@ejemplo.com",
  "password": "incorrecta123"
}
```

- **Resultado esperado:**
  - Código 401
  - Respuesta con formato ProblemDetails


## 06 Carros

Alta del vehículo y su foto (MinIO).


### 06.1 Crear carro

`POST {{host}}/api/carros`

- **Token:** Ninguno
- **Body** (raw, JSON):

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

- **Resultado esperado:**
  - Código 201
  - Placas en mayúsculas
  - No está dado de baja
- **Guardar:** `carroId` ← `carroId` de la respuesta


### 06.2 Subir foto del carro

`PUT {{host}}/api/carros/{{carroId}}/foto`

- **Token:** Ninguno
- **Body** (form-data):
  - `archivo` (tipo *File*) = `carro.jpg`
- **Resultado esperado:**
  - Código 200
  - fotoVehiculo: en BD solo se guarda la llave (empresa/.../foto/...)
  - fotoVehiculoUrl: URL prefirmada de MinIO


### 06.3 Obtener carro

`GET {{host}}/api/carros/{{carroId}}`

- **Token:** Ninguno
- **Resultado esperado:**
  - Código 200
  - fotoVehiculoUrl: URL prefirmada de MinIO


### 06.4 Carros disponibles de la empresa

`GET {{host}}/api/carros?empresaId={{empresaId}}&estado=disponible`

- **Token:** Ninguno
- **Resultado esperado:**
  - Código 200
  - El carro está disponible


## 07 Solicitudes de reserva

El operador pide el carro y el confirmador la aprueba (el carro pasa a 'ocupado').


### 07.1 Crear solicitud de reserva

`POST {{host}}/api/solicitudes-reserva`

- **Token:** Ninguno
- **Body** (raw, JSON):

```json
{
  "empresaId": "{{empresaId}}",
  "carroId": "{{carroId}}",
  "empleadoSolicitanteId": "{{empleadoOperadorId}}"
}
```

- **Resultado esperado:**
  - Código 201
  - Queda pendiente
- **Guardar:** `solicitudReservaId` ← `solicitudReservaId` de la respuesta


### 07.2 Cola de pendientes de la empresa

`GET {{host}}/api/solicitudes-reserva?empresaId={{empresaId}}&estado=pendiente`

- **Token:** Ninguno
- **Resultado esperado:**
  - Código 200
  - Aparece la solicitud


### 07.3 Aprobar solicitud

`POST {{host}}/api/solicitudes-reserva/{{solicitudReservaId}}/aprobar`

- **Token:** Ninguno
- **Body** (raw, JSON):

```json
{
  "empleadoConfirmadorId": "{{empleadoConfirmadorId}}",
  "motivo": "Viaje a sucursal Tapachula"
}
```

- **Resultado esperado:**
  - Código 200
  - Queda aprobada


### 07.4 El carro quedó ocupado

`GET {{host}}/api/carros/{{carroId}}`

- **Token:** Ninguno
- **Resultado esperado:**
  - Código 200
  - Estado ocupado


## 08 Viaje tipo Strava, combustible y foto del tablero

Inicia el viaje con foto del tablero, registra la ruta GPS, una carga con su ticket, el operador corrige un dato mientras el viaje sigue en curso y finaliza con foto del tablero.


### 08.1 Iniciar viaje

`POST {{host}}/api/viajes`

- **Token:** `{{tokenOperador}}`
- **Header:** `X-Origen: movil`
- **Antes de enviar, pon estas fechas:** `fechaInicio` = hora actual menos 3 minutos; `punto1` = igual que fechaInicio; `punto2` = fechaInicio + 30 s; `punto3` = fechaInicio + 60 s; `fechaCarga` = fechaInicio + 90 s
- **Body** (raw, JSON):

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

- **Resultado esperado:**
  - Código 201
  - Viaje en curso
  - No está corregido
  - El servidor guardó la hora de recepción
- **Guardar:** `viajeId` ← `viajeId` de la respuesta


### 08.2 Foto del tablero al iniciar

`PUT {{host}}/api/viajes/{{viajeId}}/foto-tablero/inicial`

- **Token:** `{{tokenOperador}}`
- **Header:** `X-Origen: movil`
- **Body** (form-data):
  - `archivo` (tipo *File*) = `tablero.jpg`
- **Resultado esperado:**
  - Código 200
  - fotoTableroInicial: en BD solo se guarda la llave (empresa/.../tablero/...)
  - fotoTableroInicialUrl: URL prefirmada de MinIO
  - Todavía no hay foto final


### 08.3 Registrar lote de puntos GPS

`POST {{host}}/api/viajes/{{viajeId}}/puntos-ruta`

- **Token:** `{{tokenOperador}}`
- **Header:** `X-Origen: movil`
- **Body** (raw, JSON):

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

- **Resultado esperado:**
  - Código 200 o 201
  - Se insertaron 3 puntos


### 08.4 Reenviar el mismo lote (no duplica)

`POST {{host}}/api/viajes/{{viajeId}}/puntos-ruta`

- **Token:** `{{tokenOperador}}`
- **Header:** `X-Origen: movil`
- **Body** (raw, JSON):

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

- **Resultado esperado:**
  - Código 200 o 201
  - 0 insertados, 3 duplicados


### 08.5 Ruta del viaje

`GET {{host}}/api/viajes/{{viajeId}}/puntos-ruta`

- **Token:** Ninguno
- **Resultado esperado:**
  - Código 200
  - 3 puntos en la ruta
  - Cada punto tiene su hora de recepción


### 08.6 Registrar carga de combustible con ticket

`POST {{host}}/api/cargas-combustible`

- **Token:** `{{tokenOperador}}`
- **Header:** `X-Origen: movil`
- **Body** (form-data):
  - `viajeId` = `{{viajeId}}`
  - `litros` = `45.30`
  - `monto` = `1087.20`
  - `fechaCarga` = `{{fechaCarga}}`
  - `ubicacionLat` = `16.750120`
  - `ubicacionLng` = `-93.100340`
  - `fotoComprobante` (tipo *File*) = `ticket.jpg`
- **Resultado esperado:**
  - Código 201
  - fotoComprobante: en BD solo se guarda la llave (empresa/.../carga/...)
  - fotoComprobanteUrl: URL prefirmada de MinIO
  - Calcula el precio por litro
  - El comprobante ya está en el servidor
  - No está corregida
- **Guardar:** `cargaCombustibleId` ← `cargaCombustibleId` de la respuesta, `fotoComprobanteUrl` ← `fotoComprobanteUrl` de la respuesta


### 08.7 Ver el ticket desde MinIO (URL prefirmada)

`GET {{fotoComprobanteUrl}}`

- **Token:** Ninguno
- **Resultado esperado:**
  - Código 200
  - MinIO devuelve una imagen JPEG


### 08.8 El operador corrige los litros (viaje en curso)

`PUT {{host}}/api/cargas-combustible/{{cargaCombustibleId}}`

- **Token:** `{{tokenOperador}}`
- **Header:** `X-Origen: movil`
- **Body** (form-data):
  - `litros` = `44.80`
  - `monto` = `1075.20`
  - `fechaCarga` = `{{fechaCarga}}`
  - `ubicacionLat` = `16.750120`
  - `ubicacionLng` = `-93.100340`
  - `motivo` = `Escribí 45.30 en vez de 44.80; el ticket dice 44.80 L`
- **Resultado esperado:**
  - Código 200
  - Queda marcada como corregida
  - Litros corregidos
  - Conserva el comprobante


### 08.9 Cargas del viaje

`GET {{host}}/api/cargas-combustible?viajeId={{viajeId}}`

- **Token:** Ninguno
- **Resultado esperado:**
  - Código 200
  - Hay 1 carga


### 08.10 Finalizar viaje

`POST {{host}}/api/viajes/{{viajeId}}/finalizar`

- **Token:** `{{tokenOperador}}`
- **Header:** `X-Origen: movil`
- **Antes de enviar, pon estas fechas:** `fechaFin` = hora actual
- **Body** (raw, JSON):

```json
{
  "fechaFin": "{{fechaFin}}",
  "ubicacionFinLat": 16.74587,
  "ubicacionFinLng": -93.08521,
  "kilometrajeFinal": 15310.0
}
```

- **Resultado esperado:**
  - Código 200
  - Viaje finalizado
  - Distancia por odómetro = 79.5 km


### 08.11 Foto del tablero al terminar

`PUT {{host}}/api/viajes/{{viajeId}}/foto-tablero/final`

- **Token:** `{{tokenOperador}}`
- **Header:** `X-Origen: movil`
- **Body** (form-data):
  - `archivo` (tipo *File*) = `tablero.jpg`
- **Resultado esperado:**
  - Código 200
  - fotoTableroFinal: en BD solo se guarda la llave (empresa/.../tablero/...)
  - fotoTableroFinalUrl: URL prefirmada de MinIO
  - Conserva la foto inicial


### 08.12 El carro actualizó su kilometraje

`GET {{host}}/api/carros/{{carroId}}`

- **Token:** Ninguno
- **Resultado esperado:**
  - Código 200
  - Kilometraje = 15310


### 08.13 Ruta procesada (GPS contra odómetro)

`GET {{host}}/api/viajes/{{viajeId}}/ruta-procesada`

- **Token:** Ninguno
- **Resultado esperado:**
  - Código 200
  - Calculó la distancia GPS
  - Alerta: el odómetro declara mucho más que el GPS (79.5 km vs ~3.5 km)


## 09 Correcciones de un viaje cerrado

Con el viaje cerrado, solo un Confirmador o Gestor de activos puede corregir, siempre con motivo. El valor anterior queda en el historial de auditoría.


### 09.1 El operador ya no puede corregir (403)

`POST {{host}}/api/viajes/{{viajeId}}/corregir-kilometraje`

- **Token:** `{{tokenOperador}}`
- **Header:** `X-Origen: movil`
- **Body** (raw, JSON):

```json
{
  "kilometrajeInicial": 15230.5,
  "kilometrajeFinal": 15300.0,
  "motivo": "El operador tecleó 15310; la foto del tablero dice 15300"
}
```

- **Resultado esperado:**
  - Código 403
  - Respuesta con formato ProblemDetails


### 09.2 Corregir sin token (401)

`POST {{host}}/api/viajes/{{viajeId}}/corregir-kilometraje`

- **Token:** Ninguno
- **Body** (raw, JSON):

```json
{
  "kilometrajeInicial": 15230.5,
  "kilometrajeFinal": 15300.0,
  "motivo": "El operador tecleó 15310; la foto del tablero dice 15300"
}
```

- **Resultado esperado:**
  - Código 401
  - Respuesta con formato ProblemDetails


### 09.3 Corregir sin motivo (400)

`POST {{host}}/api/viajes/{{viajeId}}/corregir-kilometraje`

- **Token:** `{{tokenConfirmador}}`
- **Body** (raw, JSON):

```json
{
  "kilometrajeInicial": 15230.5,
  "kilometrajeFinal": 15300.0
}
```

- **Resultado esperado:**
  - Código 400


### 09.4 El confirmador corrige el kilometraje final

`POST {{host}}/api/viajes/{{viajeId}}/corregir-kilometraje`

- **Token:** `{{tokenConfirmador}}`
- **Body** (raw, JSON):

```json
{
  "kilometrajeInicial": 15230.5,
  "kilometrajeFinal": 15300.0,
  "motivo": "El operador tecleó 15310; la foto del tablero dice 15300"
}
```

- **Resultado esperado:**
  - Código 200
  - Queda marcado como corregido
  - Kilometraje final corregido
  - Recalcula la distancia (69.5 km)


### 09.5 El carro ajustó su kilometraje

`GET {{host}}/api/carros/{{carroId}}`

- **Token:** Ninguno
- **Resultado esperado:**
  - Código 200
  - Kilometraje = 15300 (este viaje lo había dejado en 15310)


## 10 Liberar el carro (HU-USO-04)

El operador termina sus viajes y libera el carro: la reserva queda 'finalizada' y el carro 'disponible'.


### 10.1 Liberar la reserva

`POST {{host}}/api/solicitudes-reserva/{{solicitudReservaId}}/liberar`

- **Token:** `{{tokenOperador}}`
- **Header:** `X-Origen: movil`
- **Body** (raw, JSON):

```json
{
  "observaciones": "Se entrega limpio, con medio tanque"
}
```

- **Resultado esperado:**
  - Código 200
  - Reserva finalizada
  - Guarda fecha y observaciones de liberación


### 10.2 El carro volvió a estar disponible

`GET {{host}}/api/carros/{{carroId}}`

- **Token:** Ninguno
- **Resultado esperado:**
  - Código 200
  - Estado disponible


### 10.3 La bitácora registra la liberación

`GET {{host}}/api/cambios-estatus-carro?carroId={{carroId}}`

- **Token:** Ninguno
- **Resultado esperado:**
  - Código 200
  - Hay un cambio con tipoMotivo liberacion


### 10.4 Liberar otra vez (400)

`POST {{host}}/api/solicitudes-reserva/{{solicitudReservaId}}/liberar`

- **Token:** `{{tokenOperador}}`
- **Body** (raw, JSON):

```json
{}
```

- **Resultado esperado:**
  - Código 400
  - Respuesta con formato ProblemDetails


## 11 Cambios de estatus y percances (HU-EST)

Una falla manda el carro a mantenimiento con tipoMotivo y foto de evidencia. Las fallas y golpes forman el registro de percances, sin tabla aparte.


### 11.1 Mandar el carro a mantenimiento por una falla

`POST {{host}}/api/cambios-estatus-carro`

- **Token:** `{{tokenConfirmador}}`
- **Body** (raw, JSON):

```json
{
  "carroId": "{{carroId}}",
  "empleadoId": "{{empleadoConfirmadorId}}",
  "estatusNuevo": "enMantenimiento",
  "tipoMotivo": "fallaMecanica",
  "motivo": "Ruido en los frenos delanteros"
}
```

- **Resultado esperado:**
  - Código 201
  - Clasificado como falla mecánica
- **Guardar:** `cambioEstatusCarroId` ← `cambioEstatusCarroId` de la respuesta


### 11.2 Subir la foto de la falla

`PUT {{host}}/api/cambios-estatus-carro/{{cambioEstatusCarroId}}/foto-evidencia`

- **Token:** `{{tokenConfirmador}}`
- **Body** (form-data):
  - `archivo` (tipo *File*) = `falla.jpg`
- **Resultado esperado:**
  - Código 200
  - fotoEvidencia: en BD solo se guarda la llave (empresa/.../estatus/...)
  - fotoEvidenciaUrl: URL prefirmada de MinIO


### 11.3 Un carro en mantenimiento no se puede reservar (409)

`POST {{host}}/api/solicitudes-reserva`

- **Token:** Ninguno
- **Body** (raw, JSON):

```json
{
  "empresaId": "{{empresaId}}",
  "carroId": "{{carroId}}",
  "empleadoSolicitanteId": "{{empleadoOperadorId}}"
}
```

- **Resultado esperado:**
  - Código 409
  - Respuesta con formato ProblemDetails


### 11.4 Regresar el carro a disponible

`POST {{host}}/api/cambios-estatus-carro`

- **Token:** `{{tokenConfirmador}}`
- **Body** (raw, JSON):

```json
{
  "carroId": "{{carroId}}",
  "empleadoId": "{{empleadoConfirmadorId}}",
  "estatusNuevo": "disponible",
  "tipoMotivo": "servicio",
  "motivo": "Se cambiaron las balatas"
}
```

- **Resultado esperado:**
  - Código 201


### 11.5 Percances del carro

`GET {{host}}/api/cambios-estatus-carro?carroId={{carroId}}`

- **Token:** Ninguno
- **Resultado esperado:**
  - Código 200
  - Aparece la falla registrada
  - Con su foto de evidencia


## 12 Auditoría y trazabilidad

Módulo secundario. Requiere token con rol Administrador, Consultor/Reportes, Confirmador o Gestor de activos. A. Historial de cambios, B. Revisión de combustible, C. Revisiones.


### 12.1 El operador no tiene acceso (403)

`GET {{host}}/api/auditoria/eventos`

- **Token:** `{{tokenOperador}}`
- **Resultado esperado:**
  - Código 403
  - Respuesta con formato ProblemDetails


### 12.2 Sin token (401)

`GET {{host}}/api/auditoria/eventos`

- **Token:** Ninguno
- **Resultado esperado:**
  - Código 401


### 12.3 A. Historial de la carga corregida

`GET {{host}}/api/auditoria/eventos?entidad=carga_combustible&entidadId={{cargaCombustibleId}}`

- **Token:** `{{tokenConsultor}}`
- **Resultado esperado:**
  - Código 200
  - Registra la creación y la corrección
  - Conserva el valor anterior y el nuevo
  - Con motivo, usuario y origen móvil


### 12.4 A. Historial del viaje (corrección del confirmador)

`GET {{host}}/api/auditoria/eventos?entidad=viaje&entidadId={{viajeId}}&accion=corregir`

- **Token:** `{{tokenConsultor}}`
- **Resultado esperado:**
  - Código 200
  - Kilometraje final: 15310 → 15300
  - Lo corrigió el confirmador


### 12.5 A. Historial del carro

`GET {{host}}/api/auditoria/eventos?entidad=carro&entidadId={{carroId}}`

- **Token:** `{{tokenConsultor}}`
- **Resultado esperado:**
  - Código 200
  - Registra los cambios de estado del carro
  - Nunca muestra datos de otra empresa


### 12.6 B. Revisión de combustible del carro

`GET {{host}}/api/auditoria/revision-combustible?carroId={{carroId}}&desde={{revisionDesde}}&hasta={{revisionHasta}}`

- **Token:** `{{tokenConsultor}}`
- **Antes de enviar, pon estas fechas:** `revisionDesde` = ayer a esta hora; `revisionHasta` = mañana a esta hora
- **Resultado esperado:**
  - Código 200
  - 1 viaje y 1 carga en el periodo
  - Consumo = suma de litros (44.8 L)
  - La carga tiene comprobante
  - Alerta: registros corregidos
  - Alerta: kilometraje contra GPS
  - Sin comprobantes pendientes


### 12.7 C. Abrir revisión del viaje

`POST {{host}}/api/auditoria/revisiones`

- **Token:** `{{tokenConsultor}}`
- **Body** (raw, JSON):

```json
{
  "viajeId": "{{viajeId}}",
  "observaciones": "Revisar diferencia entre odómetro y GPS"
}
```

- **Resultado esperado:**
  - Código 201
  - Nace pendiente de información
  - El revisor es el empleado del token
- **Guardar:** `revisionViajeId` ← `revisionAuditoriaId` de la respuesta


### 12.8 C. Con observaciones pero sin texto (400)

`PUT {{host}}/api/auditoria/revisiones/{{revisionViajeId}}/resultado`

- **Token:** `{{tokenConsultor}}`
- **Body** (raw, JSON):

```json
{
  "estadoRevision": "conObservaciones",
  "cerrar": false
}
```

- **Resultado esperado:**
  - Código 400
  - Respuesta con formato ProblemDetails


### 12.9 C. Registrar resultado y cerrar

`PUT {{host}}/api/auditoria/revisiones/{{revisionViajeId}}/resultado`

- **Token:** `{{tokenConsultor}}`
- **Body** (raw, JSON):

```json
{
  "estadoRevision": "conObservaciones",
  "observaciones": "El GPS registró 3.5 km y el odómetro 69.5 km. Se pidió aclaración al operador.",
  "cerrar": true
}
```

- **Resultado esperado:**
  - Código 200
  - Queda con observaciones y cerrada


### 12.10 C. Una revisión cerrada ya no cambia (400)

`PUT {{host}}/api/auditoria/revisiones/{{revisionViajeId}}/resultado`

- **Token:** `{{tokenConsultor}}`
- **Body** (raw, JSON):

```json
{
  "estadoRevision": "revisado",
  "cerrar": false
}
```

- **Resultado esperado:**
  - Código 400
  - Respuesta con formato ProblemDetails


### 12.11 C. Abrir revisión del carro en el mes

`POST {{host}}/api/auditoria/revisiones`

- **Token:** `{{tokenConsultor}}`
- **Antes de enviar, pon estas fechas:** `periodoDesde` = día 1 del mes actual (yyyy-MM-dd); `periodoHasta` = hoy (yyyy-MM-dd)
- **Body** (raw, JSON):

```json
{
  "carroId": "{{carroId}}",
  "periodoDesde": "{{periodoDesde}}",
  "periodoHasta": "{{periodoHasta}}",
  "observaciones": "Gasto de combustible del mes"
}
```

- **Resultado esperado:**
  - Código 201
  - Alcance: carro en un periodo
- **Guardar:** `revisionCarroId` ← `revisionAuditoriaId` de la respuesta


### 12.12 C. Revisión sin alcance (400)

`POST {{host}}/api/auditoria/revisiones`

- **Token:** `{{tokenConsultor}}`
- **Body** (raw, JSON):

```json
{
  "observaciones": "Sin viaje ni carro"
}
```

- **Resultado esperado:**
  - Código 400
  - Respuesta con formato ProblemDetails


### 12.13 C. Revisiones abiertas

`GET {{host}}/api/auditoria/revisiones?abiertas=true`

- **Token:** `{{tokenConsultor}}`
- **Resultado esperado:**
  - Código 200
  - Aparece la del carro
  - No aparece la del viaje (ya se cerró)


### 12.14 A. Historial de la revisión

`GET {{host}}/api/auditoria/eventos?entidad=revision_auditoria&entidadId={{revisionViajeId}}`

- **Token:** `{{tokenConsultor}}`
- **Resultado esperado:**
  - Código 200
  - Registra la creación y el cambio de estado


## 13 Baja de carro (HU-AST-03)

El carro se da de baja sin borrarlo: su historial se conserva y ya no se puede reservar.


### 13.1 Crear un segundo carro

`POST {{host}}/api/carros`

- **Token:** Ninguno
- **Body** (raw, JSON):

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

- **Resultado esperado:**
  - Código 201
- **Guardar:** `carroBajaId` ← `carroId` de la respuesta


### 13.2 Dar de baja sin motivo (400)

`POST {{host}}/api/carros/{{carroBajaId}}/baja`

- **Token:** Ninguno
- **Body** (raw, JSON):

```json
{}
```

- **Resultado esperado:**
  - Código 400


### 13.3 Dar de baja el carro

`POST {{host}}/api/carros/{{carroBajaId}}/baja`

- **Token:** `{{tokenConfirmador}}`
- **Body** (raw, JSON):

```json
{
  "motivo": "Venta de la unidad"
}
```

- **Resultado esperado:**
  - Código 200
  - Queda dado de baja con fecha y motivo


### 13.4 Un carro dado de baja no se puede reservar (409)

`POST {{host}}/api/solicitudes-reserva`

- **Token:** Ninguno
- **Body** (raw, JSON):

```json
{
  "empresaId": "{{empresaId}}",
  "carroId": "{{carroBajaId}}",
  "empleadoSolicitanteId": "{{empleadoOperadorId}}"
}
```

- **Resultado esperado:**
  - Código 409
  - Respuesta con formato ProblemDetails


### 13.5 Dar de baja otra vez (400)

`POST {{host}}/api/carros/{{carroBajaId}}/baja`

- **Token:** Ninguno
- **Body** (raw, JSON):

```json
{
  "motivo": "Otra vez"
}
```

- **Resultado esperado:**
  - Código 400
  - Respuesta con formato ProblemDetails


### 13.6 La baja quedó en el historial

`GET {{host}}/api/auditoria/eventos?entidad=carro&entidadId={{carroBajaId}}`

- **Token:** `{{tokenConsultor}}`
- **Resultado esperado:**
  - Código 200
  - Cambio de estado con el motivo de la baja


## 14 Notificaciones

Generadas automáticamente: nueva solicitud (al confirmador) y resultado (al operador).


### 14.1 Bandeja del confirmador

`GET {{host}}/api/notificaciones?empleadoDestinatarioId={{empleadoConfirmadorId}}`

- **Token:** Ninguno
- **Resultado esperado:**
  - Código 200
  - Recibió la notificación de nueva solicitud


### 14.2 Bandeja del operador

`GET {{host}}/api/notificaciones?empleadoDestinatarioId={{empleadoOperadorId}}`

- **Token:** Ninguno
- **Resultado esperado:**
  - Código 200
  - Recibió el resultado de su solicitud


### 14.3 Contador de no leídas (badge)

`GET {{host}}/api/notificaciones/no-leidas/{{empleadoConfirmadorId}}`

- **Token:** Ninguno
- **Resultado esperado:**
  - Código 200


### 14.4 Marcar todas como leídas

`POST {{host}}/api/notificaciones/marcar-todas-leidas/{{empleadoConfirmadorId}}`

- **Token:** Ninguno
- **Resultado esperado:**
  - Código 200 o 204


## 15 Validaciones y errores

Evidencia de que la API rechaza datos inválidos con el código correcto.


### 15.1 Email repetido (409)

`POST {{host}}/api/usuarios`

- **Token:** Ninguno
- **Body** (raw, JSON):

```json
{
  "email": "operador.{{run}}@ejemplo.com",
  "password": "OtraClave123",
  "estado": "activo"
}
```

- **Resultado esperado:**
  - Código 409
  - Respuesta con formato ProblemDetails


### 15.2 Email inválido (400)

`POST {{host}}/api/usuarios`

- **Token:** Ninguno
- **Body** (raw, JSON):

```json
{
  "email": "esto-no-es-email",
  "password": "Gasolina2026",
  "estado": "activo"
}
```

- **Resultado esperado:**
  - Código 400


### 15.3 Usuario inexistente (404)

`GET {{host}}/api/usuarios/00000000-0000-7000-8000-000000000000`

- **Token:** Ninguno
- **Resultado esperado:**
  - Código 404
  - Respuesta con formato ProblemDetails


### 15.4 Placas repetidas (409)

`POST {{host}}/api/carros`

- **Token:** Ninguno
- **Body** (raw, JSON):

```json
{
  "empresaId": "{{empresaId}}",
  "marca": "Toyota",
  "modelo": "Hilux",
  "placas": "TST-{{run}}",
  "kilometrajeActual": 0
}
```

- **Resultado esperado:**
  - Código 409
  - Respuesta con formato ProblemDetails


### 15.5 Kilometraje menor al actual (400)

`PUT {{host}}/api/carros/{{carroId}}`

- **Token:** Ninguno
- **Body** (raw, JSON):

```json
{
  "empresaId": "{{empresaId}}",
  "marca": "Nissan",
  "modelo": "NP300",
  "placas": "TST-{{run}}",
  "kilometrajeActual": 100
}
```

- **Resultado esperado:**
  - Código 400
  - Respuesta con formato ProblemDetails


### 15.6 Foto de perfil en PDF (400)

`PUT {{host}}/api/empleados/{{empleadoOperadorId}}/foto-perfil`

- **Token:** Ninguno
- **Body** (form-data):
  - `archivo` (tipo *File*) = `licencia.pdf`
- **Resultado esperado:**
  - Código 400
  - Respuesta con formato ProblemDetails


### 15.7 Carga sin comprobante (400)

`POST {{host}}/api/cargas-combustible`

- **Token:** Ninguno
- **Body** (form-data):
  - `viajeId` = `{{viajeId}}`
  - `litros` = `20`
  - `fechaCarga` = `{{fechaCarga}}`
- **Resultado esperado:**
  - Código 400


### 15.8 Más litros que la capacidad del tanque (400)

`POST {{host}}/api/cargas-combustible`

- **Token:** Ninguno
- **Body** (form-data):
  - `viajeId` = `{{viajeId}}`
  - `litros` = `500`
  - `fechaCarga` = `{{fechaInicio}}`
  - `fotoComprobante` (tipo *File*) = `ticket.jpg`
- **Resultado esperado:**
  - Código 400
  - Respuesta con formato ProblemDetails


### 15.9 Corregir una carga sin token (401)

`PUT {{host}}/api/cargas-combustible/{{cargaCombustibleId}}`

- **Token:** Ninguno
- **Body** (form-data):
  - `litros` = `40`
  - `fechaCarga` = `{{fechaCarga}}`
  - `motivo` = `Prueba`
- **Resultado esperado:**
  - Código 401
  - Respuesta con formato ProblemDetails


### 15.10 Foto del tablero con momento inválido (400)

`PUT {{host}}/api/viajes/{{viajeId}}/foto-tablero/intermedia`

- **Token:** Ninguno
- **Body** (form-data):
  - `archivo` (tipo *File*) = `tablero.jpg`
- **Resultado esperado:**
  - Código 400


### 15.11 Usuario ya asignado a otro empleado (409)

`POST {{host}}/api/empleados`

- **Token:** Ninguno
- **Body** (raw, JSON):

```json
{
  "empresaId": "{{empresaId}}",
  "usuarioId": "{{usuarioOperadorId}}",
  "nombreCompleto": "Otra Persona"
}
```

- **Resultado esperado:**
  - Código 409
  - Respuesta con formato ProblemDetails


## Variables del environment

| Variable | Se llena en |
|---|---|
| `host` | inicio |
| `run` | inicio (cámbiala en cada repetición) |
| `usuarioOperadorId` | 01.1 |
| `usuarioConfirmadorId` | 01.2 |
| `usuarioConsultorId` | 01.3 |
| `rolConfirmadorId` | 02.1 |
| `rolConsultorId` | 02.2 |
| `privilegioId` | 02.3 |
| `corporativoId` | 03.1 |
| `divisionId` | 03.2 |
| `empresaId` | 03.3 |
| `centroCostoId` | 03.6 |
| `empleadoOperadorId` | 04.1 |
| `empleadoConfirmadorId` | 04.2 |
| `empleadoConsultorId` | 04.3 |
| `fotoPerfilUrl` | 04.5 |
| `tokenOperador` | 05.1 |
| `tokenConfirmador` | 05.2 |
| `tokenConsultor` | 05.3 |
| `carroId` | 06.1 |
| `solicitudReservaId` | 07.1 |
| `fechaCarga` | 08.1 (fecha que tú escribes) |
| `fechaInicio` | 08.1 (fecha que tú escribes) |
| `punto1` | 08.1 (fecha que tú escribes) |
| `punto2` | 08.1 (fecha que tú escribes) |
| `punto3` | 08.1 (fecha que tú escribes) |
| `viajeId` | 08.1 |
| `cargaCombustibleId` | 08.6 |
| `fotoComprobanteUrl` | 08.6 |
| `fechaFin` | 08.10 (fecha que tú escribes) |
| `cambioEstatusCarroId` | 11.1 |
| `revisionDesde` | 12.6 (fecha que tú escribes) |
| `revisionHasta` | 12.6 (fecha que tú escribes) |
| `revisionViajeId` | 12.7 |
| `periodoDesde` | 12.11 (fecha que tú escribes) |
| `periodoHasta` | 12.11 (fecha que tú escribes) |
| `revisionCarroId` | 12.11 |
| `carroBajaId` | 13.1 |
