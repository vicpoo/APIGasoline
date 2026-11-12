-- ============================================================
--  APIGasoline - Logins de SQL Server para producción (F013)
--  Lo ejecuta el contenedor db-init de docker-compose.prod.yml en cada arranque (es idempotente).
--
--  apigasoline_migrador  db_owner SOLO de la base de la API. Lo usa la API únicamente para aplicar
--                        migraciones (ConnectionStrings__Migraciones).
--  apigasoline_app       Solo lee y escribe datos (db_datareader + db_datawriter). Es con el que
--                        opera la API (ConnectionStrings__DefaultConnection). No puede cambiar el
--                        esquema ni quitar la política de Row-Level Security.
--
--  Ninguno es sysadmin: sa solo lo usan el contenedor de la BD y este script.
--  Variables (sqlcmd -v): MSSQL_DATABASE, MSSQL_APP_PASSWORD, MSSQL_MIGRADOR_PASSWORD.
--  Si la contraseña de un login cambia en el .env, aquí se actualiza.
-- ============================================================

SET NOCOUNT ON;

-- La base: si no existe se crea con la intercalación de la API (la misma que pondría EF)
IF DB_ID(N'$(MSSQL_DATABASE)') IS NULL
BEGIN
    EXEC(N'CREATE DATABASE [$(MSSQL_DATABASE)] COLLATE Modern_Spanish_CI_AI');
    PRINT N'Base $(MSSQL_DATABASE) creada.';
END
GO

-- Logins del servidor
IF SUSER_ID(N'apigasoline_migrador') IS NULL
    CREATE LOGIN [apigasoline_migrador] WITH PASSWORD = N'$(MSSQL_MIGRADOR_PASSWORD)', DEFAULT_DATABASE = [$(MSSQL_DATABASE)], CHECK_POLICY = ON;
ELSE
    ALTER LOGIN [apigasoline_migrador] WITH PASSWORD = N'$(MSSQL_MIGRADOR_PASSWORD)';

IF SUSER_ID(N'apigasoline_app') IS NULL
    CREATE LOGIN [apigasoline_app] WITH PASSWORD = N'$(MSSQL_APP_PASSWORD)', DEFAULT_DATABASE = [$(MSSQL_DATABASE)], CHECK_POLICY = ON;
ELSE
    ALTER LOGIN [apigasoline_app] WITH PASSWORD = N'$(MSSQL_APP_PASSWORD)';
GO

USE [$(MSSQL_DATABASE)];
GO

-- Usuarios dentro de la base y sus permisos
IF USER_ID(N'apigasoline_migrador') IS NULL
    CREATE USER [apigasoline_migrador] FOR LOGIN [apigasoline_migrador];
ALTER ROLE db_owner ADD MEMBER [apigasoline_migrador];

IF USER_ID(N'apigasoline_app') IS NULL
    CREATE USER [apigasoline_app] FOR LOGIN [apigasoline_app];
ALTER ROLE db_datareader ADD MEMBER [apigasoline_app];
ALTER ROLE db_datawriter ADD MEMBER [apigasoline_app];
GO

PRINT N'Logins listos: apigasoline_migrador (db_owner de $(MSSQL_DATABASE)) y apigasoline_app (solo datos).';
GO
