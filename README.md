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

Cada empresa demo trae un corporativo y una división ligados entre sí, un centro de costo colgado de ellos y dos vehículos (`DEMO-A01`, `DEMO-A02` / `DEMO-B01`, `DEMO-B02`).

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
dotnet test                                   # todas las pruebas (las de integración requieren Docker abierto)
dotnet test tests/APIGasoline.Domain.Tests                                     # unitarias de dominio, sin Docker
dotnet test tests/APIGasoline.Application.Tests                                # unitarias de casos de uso, sin Docker
dotnet test tests/APIGasoline.Architecture.Tests                               # reglas de arquitectura (CQRS, capas)
dotnet test tests/APIGasoline.Api.IntegrationTests                              # matriz de fugas entre empresas
dotnet tool restore                           # instala dotnet-ef (versión fijada en .config/dotnet-tools.json)
dotnet ef migrations add NombreDelCambio -p APIGasoline.Infrastructure -s APIGasoline
dotnet ef database update -p APIGasoline.Infrastructure -s APIGasoline
```

Para borrar la base de datos de Aspire y empezar de cero: detén el AppHost y en Docker Desktop elimina el contenedor `sql-...` y el volumen `apigasoline-sql`.

---

## Pruebas de fugas entre empresas

`tests/APIGasoline.Api.IntegrationTests` levanta la API completa contra un SQL Server real en Docker (Testcontainers) con dos empresas, **A** y **B**, cada una con operación real (solicitud, viaje con GPS, carga, ruta, revisión, dispositivo...). Después, el administrador de A intenta, endpoint por endpoint:

| Prueba | Qué intenta A | Debe pasar |
|---|---|---|
| `FugasLecturaTests` | Ver un registro de B, o listas filtrando por B | 403/404, y ningún id de B en la respuesta |
| `FugasEscrituraTests` | Editar, borrar o mover registros de B (o pasar los suyos a B) | Rechazo, y el registro de B queda igual |
| `FugasCreacionTests` | Crear cosas dentro de B o colgadas de registros de B | Rechazo, y no aparece nada nuevo en B |
| `SoloPlataformaTests` | Acciones de plataforma (crear y borrar empresas, privilegios, outbox) | 403 |

Cada caso tiene un **control** (`ControlesDeLaMatrizTests`): el dueño sí puede hacer lo mismo. Si un control falla, el caso está mal armado. `CoberturaEndpointsTests` falla si algún endpoint de la API no está en la matriz (`Matriz/CatalogoFugas.cs`).

Un fallo que empieza con **`FUGA:`** es un hueco de seguridad real. La matriz debe estar siempre en verde.

### Dentro de la misma empresa: lo propio y el techo de privilegios

`LoPropioYTechoTests` (catálogo en `Matriz/CatalogoLoPropio.cs`) prueba lo que un empleado no debe hacer con lo de **otro empleado de su misma empresa**, y que nadie da más privilegios de los que tiene:

| Qué | Lo puede hacer | Con privilegio, también |
|---|---|---|
| Ver un viaje, sus puntos GPS, GPX, ruta procesada, mapa o una carga | El operador del viaje | `viaje.ver_todos`, `auditoria.ver` (y para la carga, `carga.editar` / `carga.eliminar`) |
| Mandar puntos GPS, subir la foto del tablero, pausar, reanudar, finalizar | El operador del viaje | `viaje.corregir_cerrado` |
| Ver, marcar leída o borrar una notificación | El destinatario | `notificacion.enviar` |
| Desactivar un celular | Su dueño | `empleado.gestionar` |
| Cancelar una solicitud | Quien la pidió | `solicitud.aprobar` |
| Dar o quitar un privilegio a un rol | Solo si tú tienes ese privilegio | |
| Asignar, quitar, editar o borrar un rol | Solo si tú tienes **todos** sus privilegios | |

El atacante es un segundo operador de la Empresa A (o un gestor de roles con pocos privilegios); los controles comprueban que el dueño y el administrador sí pueden. Un fallo **`FUGA DENTRO DE LA EMPRESA:`** o **`ESCALADA:`** es un hueco real.

### Cómo se aíslan las empresas

- **Lecturas:** cada consulta de EF Core lleva el filtro global `Empresa` (`AppDbContext.AplicarFiltroPorEmpresa`). Para un usuario de una empresa, lo de otra "no existe": 404 en el detalle y fuera de las listas.
- **Escrituras:** `AislamientoEmpresaInterceptor` revisa todo lo que va a guardar cada `SaveChanges` y responde **403** si se crea, modifica o borra algo de otra empresa (también roles globales y cambios de `empresaId`).
- **De qué empresa es la petición:** `IContextoEmpresa`, que `ContextoEmpresaMiddleware` inicializa justo después de autenticar con la empresa del token. Con `plataforma.multiempresa`, o sin usuario (login, workers, siembra), no hay filtro. Si hay usuario y no se inicializó, falla cerrado.
- **Usuarios:** `usuario.empresa_id` dice de qué empresa es la cuenta. Un administrador de empresa crea usuarios solo en la suya; la plataforma puede mandar `empresaId` (o ninguno: se asigna al crearle su empleado).
- **Estructura organizacional (fase 8):** la empresa es la raíz. Cada empresa tiene sus corporativos y divisiones (ligados N:M con `POST/DELETE /api/corporativos/{id}/divisiones/{divisionId}`) y sus centros de costo pueden colgar de un corporativo y una división ligados. Todo se aísla igual que lo demás (filtro, guardián y RLS) y se administra con `corporativo.gestionar` (ámbito empresa).
- `AislamientoEmpresaTests` (pruebas de arquitectura) falla si una tabla nueva queda sin filtro o sin regla en el guardián, o si alguien usa `IgnoreQueryFilters()` sin nombre.
- **Row-Level Security (segundo candado, en SQL Server):** cada conexión de EF le manda a SQL Server la empresa de la petición (`SESSION_CONTEXT`) y la política `rls.aislamiento_empresa` filtra y bloquea filas aunque la consulta se salte el filtro de EF. `SeguridadPorFilasTests` lo prueba hablando con SQL Server directo. Detalle en `BD/README.md` (sección 3.4).

> **Consultar la BD a mano** (SSMS, Azure Data Studio, scripts): sin contexto de sesión no se ve ninguna fila. Ejecuta primero
> `EXEC sys.sp_set_session_context N'apigasoline.ve_todas', 1;`

---

## Commands y Queries

Todo caso de uso en `APIGasoline.Application/Features/<Modulo>/` es un **Command** (cambia algo) o una **Query** (solo lee), con las interfaces de `Common/Messaging`:

```csharp
// Features/Carros/Commands/DeleteCarroCommand.cs
public sealed record DeleteCarroCommand(Guid CarroId) : ICommand;                 // sin resultado
public sealed class DeleteCarroCommandHandler : ICommandHandler<DeleteCarroCommand> { ... }

// Features/Carros/Queries/GetCarroByIdQuery.cs
public sealed record GetCarroByIdQuery(Guid CarroId) : IQuery<CarroDto>;
public sealed class GetCarroByIdQueryHandler : IQueryHandler<GetCarroByIdQuery, CarroDto> { ... }
```

`tests/APIGasoline.Architecture.Tests` hace cumplir las reglas (no necesita Docker):

| Regla | Prueba |
|---|---|
| Todo request es `ICommand`, `ICommand<T>` o `IQuery<T>` (nunca `IRequest` a secas) | `CommandsYQueriesTests` |
| Es `sealed record` inmutable, se llama `XxxCommand`/`XxxQuery` y vive en `Commands`/`Queries` | `CommandsYQueriesTests` |
| Tiene exactamente un handler `sealed`, llamado igual + `Handler`, que implementa `ICommandHandler`/`IQueryHandler` | `CommandsYQueriesTests` |
| Una Query no guarda, no abre transacciones, no publica eventos, no sube ni borra archivos y no crea Commands (se revisa el IL compilado) | `QueriesSoloLeenTests` |
| Cada tabla tiene el filtro por empresa (o está en la lista de exentas) y el guardián sabe aislarla; nadie quita todos los filtros sin revisión | `AislamientoEmpresaTests` |
| Los controladores solo reciben `ISender` | `ControladoresTests` |
| Domain ← Application ← Infrastructure ← API (nadie depende hacia afuera) | `CapasTests` |

---

## Estructura

```text
APIGasoline/                 API: controladores, autenticación, Program.cs
APIGasoline.Application/     Casos de uso (Commands / Queries con MediatR), DTOs, reglas
APIGasoline.Domain/          Entidades y reglas de negocio
APIGasoline.Infrastructure/  EF Core (SQL Server), repositorios, migraciones, MinIO, Firebase, reportes
APIGasoline.AppHost/         Orquestador de desarrollo (Aspire)
tests/                       Pruebas unitarias, de arquitectura y de integración (matriz de fugas)
BD/                          Esquema y documentación de la base de datos
docs/                        Documentación de la API
postman/                     Colecciones de Postman
pruebas-carga/               Pruebas de carga (k6)
```

## Producción

Ver `docker-compose.prod.yml` y `.env.production.example` (API + SQL Server Express + MinIO). SQL Server necesita un servidor x64 con al menos 2 GB de RAM.
