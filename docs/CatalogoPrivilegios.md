# Catálogo de privilegios — App de control de combustible (GasolineProject)

## 1. Modelo de autorización

La API (APIGasoline) y la app móvil (GasolineProject) usan **autorización basada en permisos** (*permission-based authorization*):

- Un **privilegio** es una acción concreta del sistema. Se identifica con una **clave** inmutable con formato `modulo.accion` (por ejemplo, `carro.crear`).
- Un **rol** es solo un nombre que agrupa uno o varios privilegios. Un mismo privilegio puede estar en cualquier rol, y cada empresa puede llamar a sus roles como quiera.
- La API y la app **nunca deciden por el nombre del rol**, solo por las claves de privilegio que tiene el usuario.
- Los privilegios efectivos del usuario son la unión de los privilegios activos de sus roles activos.
- **Comportamiento según el privilegio:**
  - **API:** si el usuario no tiene el privilegio, el endpoint responde `403 Forbidden` e indica qué privilegio falta.
  - **App:** la pantalla, el botón o la pestaña de esa función no se muestra.

## 2. Catálogo de privilegios de la app (17)

| # | Módulo | Función de la app | Clave del privilegio | Endpoint de la API |
|---|---|---|---|---|
| 1 | Vehículos | Ver vehículos (lista) | `carro.ver` | `GET /api/carros` |
| 2 | Vehículos | Ver detalles de un vehículo (por ID) | `carro.ver_detalle` | `GET /api/carros/{id}` |
| 3 | Vehículos | Agregar vehículo (con foto del vehículo) | `carro.crear` | `POST /api/carros` · `PUT /api/carros/{id}/foto` |
| 4 | Vehículos | Editar vehículo (incluye cambiar o quitar la foto) | `carro.editar` | `PUT /api/carros/{id}` · `PUT /api/carros/{id}/foto` · `DELETE /api/carros/{id}/foto` |
| 5 | Vehículos | Eliminar vehículo | `carro.eliminar` | `DELETE /api/carros/{id}` |
| 6 | Solicitudes | Enviar solicitud de un vehículo | `solicitud.crear` | `POST /api/solicitudes-reserva` |
| 7 | Solicitudes | Ver el status de mis solicitudes | `solicitud.ver_propias` | `GET /api/solicitudes-reserva` (solo las del usuario) |
| 8 | Solicitudes | Ver las solicitudes de la empresa | `solicitud.ver_todas` | `GET /api/solicitudes-reserva` |
| 9 | Solicitudes | Aceptar solicitud | `solicitud.aprobar` | `POST /api/solicitudes-reserva/{id}/aprobar` |
| 10 | Solicitudes | Rechazar solicitud | `solicitud.rechazar` | `POST /api/solicitudes-reserva/{id}/rechazar` |
| 11 | Viajes | Ver historial de mis viajes | `viaje.ver_propios` | `GET /api/viajes` (solo los del usuario) |
| 12 | Viajes | Ver historial de viajes de la empresa | `viaje.ver_todos` | `GET /api/viajes` |
| 13 | Viajes | Ver detalles de un viaje (datos, pausas, mapa y cargas) | `viaje.ver_detalle` | `GET /api/viajes/{id}` · `GET /api/viajes/{id}/ruta-procesada/mapa` · `GET /api/cargas-combustible?viajeId={id}` |
| 14 | Viajes | Iniciar, pausar, reanudar y finalizar un viaje | `viaje.registrar` | `POST /api/viajes` · `POST /api/viajes/{id}/pausar` · `POST /api/viajes/{id}/reanudar` · `POST /api/viajes/sincronizar` · `POST /api/viajes/{id}/puntos-ruta` · `POST /api/viajes/{id}/finalizar` · `PUT /api/viajes/{id}/foto-tablero/{momento}` |
| 15 | Cargas de gasolina | Registrar carga de gasolina | `carga.registrar` | `POST /api/cargas-combustible` |
| 16 | Cargas de gasolina | Editar carga de gasolina | `carga.editar` | `PUT /api/cargas-combustible/{id}` |
| 17 | Cargas de gasolina | Eliminar carga de gasolina | `carga.eliminar` | `DELETE /api/cargas-combustible/{id}?motivo=...` |

### Reglas de cada privilegio

- **`carro.eliminar`:** si el vehículo ya tiene historial (solicitudes, viajes, revisiones o cambios de estatus), la API no lo borra y responde `409 Conflict`.
- **Ver mis solicitudes (`solicitud.ver_propias`) o todas (`solicitud.ver_todas`):**
  - Con `solicitud.ver_propias`, la API filtra automáticamente para que solo se vean las del usuario.
  - Con `solicitud.ver_todas`, se ven las de toda su empresa.
- **`solicitud.crear`:** el usuario debe tener su licencia de conducir registrada. Cada usuario solicita solo para sí mismo.
- **`solicitud.aprobar`:**
  - El vehículo queda asignado al solicitante.
  - Las demás solicitudes pendientes de ese vehículo se rechazan.
  - El solicitante recibe una notificación.
- **Notificación de solicitud nueva:** cuando alguien envía una solicitud, la notificación les llega a los usuarios que tienen `solicitud.aprobar`.
- **Ver mis viajes (`viaje.ver_propios`) o todos (`viaje.ver_todos`):** funciona igual que con las solicitudes. El detalle (`viaje.ver_detalle`) también respeta esto: sin `viaje.ver_todos` solo se ve el detalle de los viajes propios.
  - Con `viaje.ver_propios`, solo se ven los viajes del usuario.
  - Con `viaje.ver_todos`, se ven los de toda su empresa.
- **`viaje.registrar`:**
  - Requiere una solicitud aprobada para el vehículo.
  - Funciona sin conexión: el viaje se guarda en el celular y se sincroniza al recuperar la red.
  - Solo el operador del viaje lo pausa, reanuda o finaliza; para el de otro se necesita `viaje.corregir_cerrado`.
  - **Pausar:** solo un viaje en curso y sin otra pausa abierta. Mientras está en pausa, la API descarta los puntos GPS que caigan dentro de la pausa.
  - **Reanudar:** cierra la pausa abierta. Si se finaliza o interrumpe estando en pausa, la pausa se cierra sola a la hora de fin.
  - **Sin conexión:** las pausas viajan en `POST /api/viajes/sincronizar` (campo `pausas`). No se pueden encimar ni quedar fuera del viaje.
  - El detalle del viaje trae `estaPausado`, `segundosEnPausa` y la lista de `pausas`.
- **`carga.editar`:** editar una carga ya enviada es una corrección.
  - Siempre pide un motivo y queda en la auditoría.
  - Con el viaje cerrado, además se necesita `viaje.corregir_cerrado`.
- **`carga.eliminar`:** sigue las mismas reglas que editar: motivo obligatorio (`?motivo=`), queda en la auditoría y, con el viaje cerrado, además se necesita `viaje.corregir_cerrado`.

## 3. Funciones sin privilegio (5)

Son funciones de **autoservicio**. Las puede usar cualquier usuario con sesión iniciada, pero **solo sobre su propia información**.

Para hacerlas sobre la información de otro empleado se necesita un privilegio de administración: `empleado.gestionar`, o `usuario.gestionar` en el caso de la contraseña.

| # | Función de la app | Endpoint de la API | Por qué no lleva privilegio |
|---|---|---|---|
| 1 | Iniciar sesión | `POST /api/auth/login` | Es público: antes de iniciar sesión todavía no hay privilegios que revisar. |
| 2 | Subir o actualizar la licencia de conducir | `PUT /api/empleados/{id}/foto-licencia` | Es obligatoria para enviar solicitudes. Si se restringiera, el usuario nunca podría pedir un vehículo. |
| 3 | Subir foto de perfil | `PUT /api/empleados/{id}/foto-perfil` | Es información propia del usuario. |
| 4 | Cambiar nombre | `PUT /api/empleados/{id}` | Es información propia. El usuario no puede cambiarse de empresa ni de usuario. |
| 5 | Recibir notificaciones | `GET /api/notificaciones` · notificaciones push | Cada usuario recibe y consulta solo las suyas. |

## 4. Resumen

| Concepto | Cantidad |
|---|---|
| Privilegios de la app | 17 |
| Privilegios que ya existen en la API | 10 |
| Privilegios nuevos por agregar | 7: `carro.ver_detalle`, `solicitud.ver_propias`, `solicitud.ver_todas`, `solicitud.rechazar`, `viaje.ver_detalle`, `carga.editar`, `carga.eliminar` |
| Funciones sin privilegio (autoservicio) | 5 |

Las claves son las mismas en la API (`APIGasoline.Application/Common/Security/Permisos.cs`) y en la app (`core/rbac/Privileges.kt`). Si se agrega una clave en un lado, se agrega también en el otro.

## 5. Dependencias entre funciones

Algunas funciones necesitan leer datos de otro módulo. En esos casos el endpoint acepta también el privilegio de la función que depende de él:

| Endpoint | Privilegio propio | También lo acepta | Por qué |
|---|---|---|---|
| `GET /api/carros/{id}` | `carro.ver_detalle` | `viaje.registrar` | La pestaña Registrar muestra el vehículo asignado. |
| `GET /api/solicitudes-reserva` | `solicitud.ver_propias` / `solicitud.ver_todas` | `viaje.registrar` | La pestaña Registrar busca las reservas aprobadas del operador. |
| `GET /api/viajes/{id}` · `GET /api/viajes/{id}/puntos-ruta` | `viaje.ver_detalle` | `viaje.registrar` | La app consulta el viaje en curso. |
| `GET /api/centros-costo` | `centro_costo.ver` | `viaje.registrar`, `carga.registrar` | Se elige el centro de costos al iniciar el viaje y en cada carga. |
