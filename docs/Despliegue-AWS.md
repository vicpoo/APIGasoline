# Despliegue en AWS (EC2) — versión 2.0.0

Guía para actualizar el servidor que ya corre APIGasoline con `docker-compose.prod.yml` (en `~/APIGasoline`).
Todo se ejecuta en el servidor, por SSH, salvo donde se indique.

> **Antes de empezar:** el CI de `main` debe estar en verde (pestaña **Actions** de GitHub) y el tag `v2.0.0` publicado.

## Qué cambia en el servidor

| Cambio | Qué hay que hacer |
|---|---|
| La API ya no usa `sa`: usa `apigasoline_app` y migra con `apigasoline_migrador`. | Agregar `MSSQL_APP_PASSWORD` y `MSSQL_MIGRADOR_PASSWORD` al `.env` (paso 2). |
| Contenedor nuevo `db-init` que crea la base y los logins. | Nada: arranca solo con `up`. |
| 3 migraciones nuevas: `EstructuraOrganizacional`, `EmpresaDesactivada` e `IndicesUnicosConcurrencia`. | Se aplican solas al arrancar. Antes se respalda la base (paso 1). |

## 1. Respaldar la base y anotar la versión actual

```bash
cd ~/APIGasoline
git rev-parse --short HEAD | tee ~/version-anterior.txt

docker compose -f docker-compose.prod.yml exec db mkdir -p /var/opt/mssql/backup
docker compose -f docker-compose.prod.yml exec db sh -c '/opt/mssql-tools18/bin/sqlcmd -C -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -b -Q "BACKUP DATABASE [apigasoline] TO DISK = N'"'"'/var/opt/mssql/backup/antes-v2.bak'"'"' WITH INIT, CHECKSUM"'
docker compose -f docker-compose.prod.yml cp db:/var/opt/mssql/backup/antes-v2.bak ~/respaldo-antes-v2-$(date +%F).bak
ls -lh ~/respaldo-antes-v2-*.bak
```

Debe decir `BACKUP DATABASE successfully processed` y el archivo no debe pesar 0.

## 2. Agregar las contraseñas nuevas al `.env`

Generadas al azar (32 caracteres + mayúscula, número y símbolo, como pide SQL Server):

```bash
grep -q '^MSSQL_APP_PASSWORD=' .env || echo "MSSQL_APP_PASSWORD=$(openssl rand -hex 16)Aa1-" >> .env
grep -q '^MSSQL_MIGRADOR_PASSWORD=' .env || echo "MSSQL_MIGRADOR_PASSWORD=$(openssl rand -hex 16)Aa1-" >> .env
chmod 600 .env
grep -E '^MSSQL_(APP|MIGRADOR)_PASSWORD=' .env | sed 's/=.*/=<definida>/'
```

## 3. Traer la versión 2.0.0 y levantar

```bash
git pull origin main
git describe --tags      # debe decir v2.0.0
docker compose -f docker-compose.prod.yml up -d --build
```

## 4. Verificar

```bash
docker compose -f docker-compose.prod.yml ps
docker compose -f docker-compose.prod.yml logs db-init | tail -3
docker compose -f docker-compose.prod.yml logs api | grep -E "Migraci|Seguridad|listening|Exception|fail"
curl -s http://localhost/salud
```

Lo esperado:

- `db-init` terminó (`Exited (0)`) y su log dice **"Logins listos"**.
- La API dice `Aplicando 3 migración(es) pendiente(s): ...EstructuraOrganizacional, ...EmpresaDesactivada, ...IndicesUnicosConcurrencia`, luego `Migraciones aplicadas correctamente` y `Now listening on: http://[::]:8080`, sin `Exception`.
- `/salud` responde `{"estado":"ok","version":"2.0.0"}`.

Que la API ya no use `sa`:

```bash
docker compose -f docker-compose.prod.yml exec db sh -c '/opt/mssql-tools18/bin/sqlcmd -C -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -Q "SELECT login_name, COUNT(*) AS conexiones FROM sys.dm_exec_sessions WHERE database_id = DB_ID(N'"'"'apigasoline'"'"') GROUP BY login_name"'
```

Debe aparecer `apigasoline_app` (y no `sa`).

Desde tu computadora: entra a la app o a Postman con el administrador y prueba login, lista de carros y un reporte.

## 5. Después del despliegue

- **Roles de empresa:** `corporativo.gestionar` ahora es de ámbito empresa. Si los administradores de cada empresa deben administrar sus corporativos y divisiones, dales ese privilegio a su rol (`POST /api/rol-privilegios`).
- **Apps:** los listados ahora llegan paginados (50 por defecto) y los reportes sin fechas son del último mes. Revisa que la app móvil y el panel web pidan `?pagina=` o usen `X-Total-Count`.
- Guarda el respaldo `~/respaldo-antes-v2-*.bak` fuera del servidor (por ejemplo con `scp`).

## Si algo sale mal (volver atrás)

Las migraciones cambian la base, así que volver solo el código no basta: hay que restaurar el respaldo.

```bash
cd ~/APIGasoline
docker compose -f docker-compose.prod.yml stop api
docker compose -f docker-compose.prod.yml cp ~/respaldo-antes-v2-AAAA-MM-DD.bak db:/var/opt/mssql/backup/restaurar.bak
docker compose -f docker-compose.prod.yml exec db sh -c '/opt/mssql-tools18/bin/sqlcmd -C -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -b -Q "ALTER DATABASE [apigasoline] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; RESTORE DATABASE [apigasoline] FROM DISK = N'"'"'/var/opt/mssql/backup/restaurar.bak'"'"' WITH REPLACE; ALTER DATABASE [apigasoline] SET MULTI_USER;"'
git checkout $(cat ~/version-anterior.txt)
docker compose -f docker-compose.prod.yml up -d --build
```

La versión anterior usa `sa`, así que funciona sin `db-init`. Cuando se corrija el problema: `git switch main && git pull` y repetir desde el paso 3.
