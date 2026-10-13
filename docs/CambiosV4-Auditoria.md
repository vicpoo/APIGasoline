# Cambios v4 — Auditoría y trazabilidad

Este documento resume lo que cambió en la API y en la base de datos al pasar del modelo v3 al v4.
El modelo completo está en `docs/BaseDatos.dbml`.

El núcleo de la app sigue siendo **viajes (ruta tipo Strava) y cargas de combustible**.
La auditoría es un **módulo secundario**: no duplica viajes ni cargas, solo registra los eventos
sobre ellos y las revisiones que hace un responsable.

---

## 1. Cómo aplicar los cambios

### 1.1. Compilar y probar

```powershell
dotnet build
dotnet test
```

### 1.2. Crear la migración

Desde la carpeta de la solución:

```powershell
dotnet ef migrations add AuditoriaYTrazabilidadV4 --project APIGasoline.Infrastructure --startup-project APIGasoline
```

Antes de aplicarla, abre el archivo de la migración generada y, **al final del método `Up`**, agrega estas
líneas para que los datos que ya existen no generen alertas falsas:

```csharp
// Las cargas registradas antes de v4 siempre subían el comprobante junto con los datos
migrationBuilder.Sql("UPDATE carga_combustible SET fecha_subida_comprobante = fecha_carga WHERE fecha_subida_comprobante IS NULL;");

// Los empleados que ya tenían licencia: se toma su fecha de alta como fecha de carga
migrationBuilder.Sql("UPDATE empleado SET fecha_carga_licencia = fecha_alta WHERE foto_licencia IS NOT NULL AND fecha_carga_licencia IS NULL;");
```

Revisa también que la migración **renombre** (`RenameColumn`) la columna `delete` a `eliminado` en
`empresa` y `empleado`, y que no la borre y la vuelva a crear (eso perdería los datos).

Aplica la migración:

```powershell
dotnet ef database update --project APIGasoline.Infrastructure --startup-project APIGasoline
```

### 1.3. Historial de solo inserción (recomendado)

El historial no debe poder modificarse. Si la API se conecta con un usuario de MySQL propio,
quítale los permisos de modificar y borrar sobre esa tabla:

```sql
REVOKE UPDATE, DELETE ON apigasoline.auditoria_evento FROM 'usuario_api'@'%';
```

(Cambia `usuario_api` por el usuario de MySQL con el que se conecta la API. Con `root` no aplica: crea un usuario propio para la API.)

---

## 2. Cambios en la base de datos

| Tabla | Cambio |
|---|---|
| `empresa` | `activo` (HU-ADM-01). La columna `delete` se renombra a `eliminado`. |
| `empleado` | `fecha_carga_licencia` (HU-LOG-01). La columna `delete` se renombra a `eliminado`. |
| `carro` | `dado_de_baja`, `fecha_baja`, `motivo_baja` (HU-AST-03). |
| `solicitud_reserva` | Estado `finalizada`; `fecha_liberacion` y `observaciones_liberacion` (HU-USO-04). |
| `viaje` | `foto_tablero_inicial`, `foto_tablero_final`, `corregido`. `fecha_recepcion` reemplaza a `sincronizado`. |
| `punto_ruta` | `fecha_recepcion` reemplaza a `sincronizado`. |
| `carga_combustible` | `fecha_subida_comprobante`, `corregido`. `fecha_recepcion` reemplaza a `sincronizado`. |
| `cambio_estatus_carro` | `tipo_motivo` (registro de percances) y `foto_evidencia`. `fecha_recepcion` reemplaza a `sincronizado`. |
| `notificacion` | Tipo `reporte_mantenimiento` (HU-EST-02). |
| `auditoria_evento` | **Nueva.** Historial de cambios: quién, qué, cuándo, valores anteriores y nuevos, motivo y origen. |
| `revision_auditoria` | **Nueva.** Revisión de un viaje, o de un carro en un periodo, con su resultado. |

`fecha_recepcion` la pone el servidor. Comparada con la fecha del celular (`fecha_inicio`, `fecha_carga`,
`timestamp_captura`, `fecha_cambio`) indica cuánto tiempo estuvo el dato sin sincronizar.

---

## 3. Historial de cambios automático

`AuditoriaInterceptor` (Infrastructure/Auditoria) se ejecuta en cada `SaveChanges` y agrega un
`auditoria_evento` por cada registro creado, modificado o eliminado, **en la misma transacción**.

- El usuario y el origen salen del token (`IUsuarioActual`), nunca del cuerpo de la petición.
- El origen se lee del header `X-Origen: movil` (la app móvil debe mandarlo). Sin header es `web`;
  sin petición HTTP (workers) es `sistema`.
- La acción se deduce: crear, actualizar, eliminar (incluye el borrado lógico), aprobar / rechazar /
  cancelar (solicitudes) o cambiar_estado. Un caso de uso puede indicarla con `IContextoAuditoria`
  (por ejemplo, `corregir` con su motivo).
- No se auditan: `punto_ruta` (alto volumen, nunca se modifica), `ruta_procesada` (calculada),
  `notificacion`, `outbox_mensaje` y `dispositivo_empleado`.
- `password_hash` y `token_fcm` se guardan como `***`.

---

## 4. Endpoints nuevos y modificados

### 4.1. Viajes

| Método | Ruta | Descripción |
|---|---|---|
| PUT | `/api/viajes/{id}/foto-tablero/inicial` | Foto del tablero al iniciar (multipart, campo `archivo`). |
| PUT | `/api/viajes/{id}/foto-tablero/final` | Foto del tablero al terminar. La foto anterior no se borra. |
| POST | `/api/viajes/{id}/corregir-kilometraje` | Corrige el kilometraje con motivo. Requiere token. |

### 4.2. Cargas de combustible

| Método | Ruta | Cambio |
|---|---|---|
| POST | `/api/cargas-combustible` | Ya no recibe `sincronizado`. Guarda `fecha_subida_comprobante`. |
| PUT | `/api/cargas-combustible/{id}` | Ahora es una **corrección**: requiere token y el campo `motivo`. El comprobante anterior ya no se borra. |

### 4.3. Reglas de corrección

| Situación | Quién puede corregir |
|---|---|
| Antes de sincronizar | El operador, libremente en el celular (el dato aún no existe en el servidor). |
| Viaje en curso | El operador del viaje, o un Confirmador / Gestor de activos. |
| Viaje cerrado | Solo un Confirmador o un Gestor de activos (o el Administrador). |

Siempre con motivo. El registro queda con `corregido = true` y el valor anterior en `auditoria_evento`.
Nunca se corrigen `punto_ruta` ni `auditoria_evento`.

### 4.4. Reservas, carros, cambios de estatus y empresas

| Método | Ruta | Descripción |
|---|---|---|
| POST | `/api/solicitudes-reserva/{id}/liberar` | HU-USO-04: la reserva queda `finalizada` y el carro `disponible`. |
| POST | `/api/carros/{id}/baja` | HU-AST-03: da de baja el carro (no lo borra). Cancela sus solicitudes pendientes. |
| POST | `/api/cambios-estatus-carro` | Ya no recibe `sincronizado`; recibe `tipoMotivo` (`fallaMecanica`, `choqueOGolpe`, etc.). Si el operador que tiene el carro lo manda a mantenimiento, se avisa a los confirmadores. |
| PUT | `/api/cambios-estatus-carro/{id}/foto-evidencia` | Foto de la falla o del daño (multipart, campo `archivo`). |
| PUT | `/api/empresas/{id}` | Acepta `activo` (opcional). |
| POST | `/api/viajes/{id}/puntos-ruta` | Ya no recibe `sincronizado` (si lo manda, se ignora). |

### 4.5. Auditoría y trazabilidad (`/api/auditoria`)

Requieren token y uno de estos roles: `Administrador`, `Consultor/Reportes`, `Confirmador` o
`Gestor de activos`. Un empleado solo ve la información de su empresa.

| Método | Ruta | Descripción |
|---|---|---|
| GET | `/api/auditoria/eventos` | Historial. Filtros: `entidad`, `entidadId`, `usuarioId`, `accion`, `desde`, `hasta`, `maximo`. |
| GET | `/api/auditoria/eventos/{id}` | Un evento. |
| GET | `/api/auditoria/revision-combustible?carroId=&desde=&hasta=` | Resumen del periodo y alertas calculadas. |
| GET | `/api/auditoria/revisiones` | Revisiones. Filtros: `viajeId`, `carroId`, `estado`, `abiertas`. |
| GET | `/api/auditoria/revisiones/{id}` | Una revisión. |
| POST | `/api/auditoria/revisiones` | Abre una revisión de un viaje, o de un carro con su periodo. El revisor es el empleado del token. |
| PUT | `/api/auditoria/revisiones/{id}/resultado` | Registra el resultado y opcionalmente la cierra (`cerrar: true`). |

Alertas de la revisión de combustible (se calculan al consultar, no se guardan):

| Alerta | Qué detecta |
|---|---|
| `comprobantePendiente` | La carga está en el servidor pero su foto todavía no. |
| `kilometrajeInconsistente` | Final menor que el inicial, o un viaje que no empieza donde terminó el anterior del mismo carro. |
| `kilometrajeContraGps` | El odómetro declara bastantes más km que la ruta GPS. |
| `registroCorregido` | Viaje o carga corregidos después de registrarse. |
| `viajeInterrumpido` | Un Confirmador cerró el viaje anticipadamente. |
| `cargaInusual` | Más litros que la capacidad del tanque. |
| `posibleDuplicado` | Dos cargas iguales en el mismo viaje con menos de 10 minutos de diferencia. |

Una alerta no significa fraude ni error: indica que alguien debería revisar el registro.

---

## 5. Nombres de roles que usa la lógica

Están en `NombresRol` (Application/Common/Constants). Los roles en la tabla `rol` deben llamarse igual:

| Constante | Nombre en la BD |
|---|---|
| `Confirmador` | `Confirmador` |
| `GestorActivos` | `Gestor de activos` |
| `Administrador` | `Administrador` |
| `ConsultorReportes` | `Consultor/Reportes` |
