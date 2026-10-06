# Pruebas de carga y estrés (k6)

## Requisitos
1. Instalar k6: `winget install k6 --source winget`  (o `choco install k6`)
2. `docker compose up -d` (MySQL + MinIO)
3. Encender la API en **Release** (Debug es mucho más lento y falsea los resultados):
   `dotnet run --project APIGasoline -c Release --launch-profile http`

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
k6 run pruebas-carga/3-estres.js
```

## Cómo leer el resultado
- `http_req_duration p(95)`: el 95 % de las peticiones tardó menos que esto. Meta: < 500 ms.
- `http_req_failed`: % de errores. Meta: < 1 %.
- `http_reqs`: peticiones por segundo que procesó la API.
- **Capacidad = el último escalón donde se cumplen ambas metas.**
