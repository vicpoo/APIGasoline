# Pruebas de carga y estrés (k6)

## Requisitos
1. Instalar k6: `winget install k6 --source winget`  (o `choco install k6`)
2. `docker compose up -d` (SQL Server + MinIO)
3. Encender la API en **Release** (Debug es mucho más lento y falsea los resultados):
   `dotnet run --project APIGasoline -c Release --launch-profile http`
4. Un usuario para las pruebas (por ejemplo el administrador de la Empresa Demo A). Las credenciales
   **no van en los scripts**: se pasan con `-e` (o variables de entorno `K6_EMAIL` / `K6_PASSWORD`):
   ```powershell
   k6 run -e K6_EMAIL=admin.a@demo.local -e K6_PASSWORD=<su contraseña> pruebas-carga/1-humo.js
   ```
   Otra API: `-e BASE_URL=http://3.140.10.20`.

## Opciones
- `-e PROBAR_LOGIN=1` mide también el login (1 de cada 10 iteraciones). Enciende la API con
  `$env:LimiteAuth__Habilitado="false"`, si no el límite de intentos por IP responde 429.
- Si un usuario virtual recibe 401 (el token de acceso venció, dura `Jwt:ExpiracionMinutos`), vuelve a
  iniciar sesión y reintenta. En pruebas largas conviene subirlo: `$env:Jwt__ExpiracionMinutos="60"`.
- **No corras estas pruebas contra producción.**

## Orden recomendado
| Script | Qué mide | Duración |
|---|---|---|
| `1-humo.js` | Que todo responde con 1 usuario | 30 s |
| `2-carga.js` | Uso normal: 50 usuarios | 7 min |
| `3-estres.js` | Punto de quiebre: 50 → 800 usuarios | 17 min |
| `4-picos.js` | Pico repentino de 300 usuarios | 3 min |

## Ejecutar con reporte HTML (evidencia)
```powershell
$env:K6_WEB_DASHBOARD="true"
$env:K6_WEB_DASHBOARD_EXPORT="pruebas-carga/reporte-estres.html"
k6 run -e K6_EMAIL=admin.a@demo.local -e K6_PASSWORD=<su contraseña> pruebas-carga/3-estres.js
```

## Cómo leer el resultado
- `http_req_duration p(95)`: el 95 % de las peticiones tardó menos que esto. Meta: < 500 ms.
- `http_req_failed`: % de errores. Meta: < 1 %.
- `http_reqs`: peticiones por segundo que procesó la API.
- **Capacidad = el último escalón donde se cumplen ambas metas.**
