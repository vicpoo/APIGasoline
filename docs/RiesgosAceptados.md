# Riesgos aceptados

Hallazgos de las revisiones de seguridad que **no se corrigen por ahora**, con el motivo y cuándo se vuelven a revisar.
Aceptar un riesgo no lo cierra: si cambia la condición que lo justifica, se reabre.

| ID | Riesgo | Severidad | Aceptado el | Revisar |
|---|---|---|---|---|
| A004 | La API y MinIO se sirven sin HTTPS | P2 | 2026-11-19 | En cuanto haya dominio y certificado, y antes de operar con una segunda empresa real |
| A006 | El correo es único en toda la BD | P3 | 2026-11-19 | Si se agrega login por empresa |
| N006 | `GetAsignacionesQuery` no pagina | P3 | 2026-11-19 | Si una empresa pasa de ~300 carros o empleados |

## A004: sin HTTPS

**Qué pasa:** el despliegue en EC2 publica la API en el puerto 80 y MinIO en el 9000, sin TLS. Viajan en claro las contraseñas del login, los tokens de acceso y refresh, y las URLs prefirmadas de fotos (válidas 60 minutos sin autenticación).

**Por qué se acepta por ahora:** no hay dominio ni certificado disponibles en este momento. Hoy el sistema opera con una sola empresa en fase de pruebas.

**Mitigaciones vigentes:**

- Tokens de acceso de 15 minutos; los privilegios se vuelven a leer cada 30 segundos.
- URLs de fotos con caducidad de 60 minutos y llaves que salen de la BD, nunca del cliente.
- Límite de 10 intentos de login por minuto por IP. Como no hay proxy delante, la IP es la del cliente real (no aplica el problema de límite global descrito en A004).
- `MINIO_PUBLIC_ENDPOINT` y `ALLOWED_HOSTS` fijos en el `.env` del servidor (A003).

**Para cerrarlo:**

1. Dominio apuntando a la IP elástica de EC2.
2. Proxy con TLS (Caddy o nginx en `docker-compose.prod.yml`) delante de la API y de MinIO.
3. `UseForwardedHeaders` en `Program.cs` con el proxy como único conocido, para que el límite de login siga siendo por IP.
4. `Minio__UseSsl: "true"` o MinIO detrás del mismo proxy; `MINIO_PUBLIC_ENDPOINT` y `ALLOWED_HOSTS` con el dominio.
5. App móvil: `BASE_URL` con `https://` y quitar `usesCleartextTraffic` del `AndroidManifest.xml`.

## A006: correo único global

**Qué pasa:** `uq_usuario_email` es único en toda la base. Un administrador de la empresa A que intente crear un usuario con un correo de la empresa B recibe 409, lo que confirma que ese correo existe en otro lado (el mensaje no dice dónde).

**Por qué se acepta:** el inicio de sesión es solo con correo y contraseña. Si el correo se repitiera entre empresas, el login no sabría a qué cuenta entrar. Volverlo único por empresa obligaría a pedir la empresa al iniciar sesión, lo que cambia la app y la experiencia de todos los usuarios.

**Lo que sí se corrigió de A006:** las placas son únicas por empresa (migración `PlacasUnicasPorEmpresa`) y el mensaje de dispositivo ya no dice "de otra empresa".

## N006: asignaciones sin paginar

**Qué pasa:** `GetAsignacionesQuery` carga en una sola respuesta todos los carros, empleados, solicitudes aprobadas y viajes en curso de la empresa.

**Por qué se acepta:** el volumen depende del tamaño de la flota de una empresa, no del historial, y la pantalla necesita la vista completa para asignar. Son 4 consultas sin N+1.

**Cuándo se reabre:** si una empresa pasa de unos 300 carros o empleados, o si el tiempo de respuesta de `GET /api/asignaciones` pasa de 1 segundo.
