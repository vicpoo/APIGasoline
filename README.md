# APIGasoline

API (.NET 10) para controlar el uso de vehículos y el consumo de combustible de un grupo corporativo: viajes con ruta GPS, cargas de gasolina, reservas, auditoría y reportes. Multiempresa (cada empresa es un tenant).

**Stack:** ASP.NET Core 10 · EF Core 10 · SQL Server 2022 · MinIO (fotos) · Firebase Cloud Messaging (push) · MediatR · Clean Architecture.

---

## Correrlo en tu máquina (un comando)

**Requisitos:** [.NET 10 SDK](https://dotnet.microsoft.com/download) y [Docker Desktop](https://www.docker.com/products/docker-desktop/) abierto.

```bash
git clone https://github.com/vicpoo/APIGasoline.git
cd APIGasoline
dotnet run --project APIGasoline.AppHost
```

Eso es todo. [Aspire](https://aspire.dev) levanta SQL Server y MinIO en Docker, arranca la API y abre su panel (la URL sale en la consola). Al arrancar, la API:

1. Crea la base de datos `apigasoline` y aplica las migraciones.
2. Siembra el catálogo de privilegios, el rol **Administrador** y el usuario administrador.
3. Siembra los **datos demo**: dos empresas aisladas para probar.

La primera vez tarda unos minutos (descarga las imágenes de Docker). Después, los contenedores se quedan vivos y los datos se conservan entre ejecuciones.

### Usuarios de prueba

| Usuario | Contraseña | Qué puede hacer |
|---|---|---|
| `admin@gasoline.local` | `Admin#Demo2026` | Administrador de plataforma: ve y gestiona **todas** las empresas. |
| `admin.a@demo.local` | `Demo#Gasolina2026` | Administrador de **Empresa Demo A**. |
| `operador.a@demo.local` | `Demo#Gasolina2026` | Operador de la Empresa Demo A (tiene licencia: puede pedir carros). |
| `admin.b@demo.local` | `Demo#Gasolina2026` | Administrador de **Empresa Demo B**. |
| `operador.b@demo.local` | `Demo#Gasolina2026` | Operador de la Empresa Demo B. |

Cada empresa demo trae un centro de costo y dos vehículos (`DEMO-A01`, `DEMO-A02` / `DEMO-B01`, `DEMO-B02`).

### Sin Aspire (alternativa)

```bash
cp .env.example .env                 # en Windows: copy .env.example .env
docker compose up -d                 # SQL Server + MinIO
dotnet run --project APIGasoline
```

Los valores de `.env.example` coinciden con `APIGasoline/appsettings.Development.json`, así que funciona sin editar nada.

> No corras las dos opciones a la vez: usan los mismos puertos de MinIO (9000 y 9001). Si cambias de una a otra, primero `docker compose down`.

---

## Configuración

| Archivo | ¿Se sube a git? | Para qué |
|---|---|---|
| `APIGasoline/appsettings.json` | Sí | Valores base de todos los ambientes. |
| `APIGasoline/appsettings.Development.json` | Sí | Desarrollo, **sin secretos reales**: sirve a cualquiera que clone el repo. |
| `APIGasoline/appsettings.Local.json` | **No** | Tus valores personales: tu llave JWT, Firebase, otra BD... Le gana a `Development.json`. |
| `.env` | **No** | Contraseñas de los contenedores de `docker compose`. |

Prioridad (el de abajo gana): `appsettings.json` → `appsettings.Development.json` → `appsettings.Local.json` → variables de entorno (Aspire / Docker).

Ejemplo de `APIGasoline/appsettings.Local.json`:

```json
{
  "Jwt": { "Key": "una-llave-de-al-menos-32-caracteres-para-ti" },
  "Push": {
    "Proveedor": "Firebase",
    "Firebase": { "CredencialesPath": "C:\\ruta\\a\\tu\\apigasoline-firebase.json" }
  }
}
```

- Si `Jwt:Key` está vacía en desarrollo, la API genera una llave temporal en cada arranque (los access tokens dejan de servir al reiniciar; los refresh tokens siguen funcionando).
- Las notificaciones push están en `NoOp` (simuladas) por defecto.

---

## Comandos útiles

```bash
dotnet test                                   # pruebas (dominio y aplicación)
dotnet tool restore                           # instala dotnet-ef (versión fijada en .config/dotnet-tools.json)
dotnet ef migrations add NombreDelCambio -p APIGasoline.Infrastructure -s APIGasoline
dotnet ef database update -p APIGasoline.Infrastructure -s APIGasoline
```

Para borrar la base de datos de Aspire y empezar de cero: detén el AppHost y en Docker Desktop elimina el contenedor `sql-...` y el volumen `apigasoline-sql`.

---

## Estructura

```text
APIGasoline/                 API: controladores, autenticación, Program.cs
APIGasoline.Application/     Casos de uso (Commands / Queries con MediatR), DTOs, reglas
APIGasoline.Domain/          Entidades y reglas de negocio
APIGasoline.Infrastructure/  EF Core (SQL Server), repositorios, migraciones, MinIO, Firebase, reportes
APIGasoline.AppHost/         Orquestador de desarrollo (Aspire)
tests/                       Pruebas unitarias
BD/                          Esquema y documentación de la base de datos
docs/                        Documentación de la API
postman/                     Colecciones de Postman
pruebas-carga/               Pruebas de carga (k6)
```

## Producción

Ver `docker-compose.prod.yml` y `.env.production.example` (API + SQL Server Express + MinIO). SQL Server necesita un servidor x64 con al menos 2 GB de RAM.
