# Cambios v5 — RBAC (roles y privilegios) listo para el front

Este documento resume lo que cambió en la API y en la base de datos al pasar del modelo v4 al v5.
El modelo completo está en `docs/BaseDatos.dbml` (los cambios están marcados con 🔐 v5).

La idea: el front (Kotlin Multiplatform, offline-first) decide qué botones mostrar con las
**claves de privilegios** del usuario (`carro.crear`, `viaje.ver_todos`...), nunca con el nombre del rol.

---

## 1. Cómo aplicar los cambios

### 1.1. Compilar y probar

```powershell
dotnet build
dotnet test
```

### 1.2. Revisar y aplicar la migración

La migración ya viene escrita: `APIGasoline.Infrastructure/Migrations/20261015180000_RbacPrivilegiosV5.cs`
(con su `.Designer.cs` y el `AppDbContextModelSnapshot.cs` actualizado). Antes de aplicarla, confirma que
el modelo y el snapshot coinciden (debe decir que **no** hay cambios pendientes):

```powershell
dotnet ef migrations has-pending-model-changes --project APIGasoline.Infrastructure --startup-project APIGasoline
```

Si dijera que sí hay cambios, borra los 2 archivos `20261015180000_RbacPrivilegiosV5*`, regresa el snapshot con
`git checkout -- APIGasoline.Infrastructure/Migrations/AppDbContextModelSnapshot.cs` y genérala con
`dotnet ef migrations add RbacPrivilegiosV5 ...`; después copia el bloque `migrationBuilder.Sql(...)` del
archivo original **entre** los `AddColumn` de privilegio y el `CreateIndex` de `uq_privilegio_clave`.

Aplicar:

```powershell
dotnet ef database update --project APIGasoline.Infrastructure --startup-project APIGasoline
```

### 1.3. Qué hace la migración con los datos que ya existen

- **Privilegios existentes**: reciben una clave temporal única `legado.p<id en hex>` y módulo `legado`
  (si no, el índice único de `clave` fallaría). Como la clave es inmutable, para usar la definitiva
  elimina el privilegio legado y créalo otra vez con `POST /api/privilegios`.
- **Roles globales repetidos**: si ya hubiera dos roles globales con el mismo nombre, el índice
  `uq_rol_empresa_clave_nombre` no se podrá crear. Renombra o elimina uno antes de migrar.

---

## 2. Cambios en la base de datos

| Tabla | Cambio | Para qué |
|---|---|---|
| `privilegio` | 🆕 `clave` varchar(100), única | Código técnico inmutable `modulo.accion`. Lo usan la API y el front. |
| `privilegio` | 🆕 `modulo` varchar(50), con índice | Agrupa los privilegios en la pantalla de roles. |
| `privilegio` | 🆕 `ambito` enum('plataforma','empresa') | `plataforma` solo se asigna a roles globales (ej. `empresa.crear`). |
| `privilegio` | ✏️ `nombre` deja de ser único | Ahora es solo el texto visible; renombrarlo no rompe permisos. |
| `rol` | 🆕 `empresa_clave` binary(16), columna generada | `COALESCE(empresa_id, 0x00…)`: con ella el índice único funciona también con roles globales. |
| `rol` | ✏️ índice único `(empresa_clave, nombre)` | Reemplaza `(empresa_id, nombre)`, que permitía roles globales repetidos (MySQL no compara NULL). |

`empresa_clave` la calcula MySQL; la API nunca la escribe y no aparece en el historial de auditoría.

---

## 3. Cambios en la API

### 3.1. Privilegios (`/api/privilegios`)

- `POST` recibe `clave` (obligatoria, formato `modulo.accion`, se guarda en minúsculas), `modulo` (opcional:
  si no se manda se toma el primer segmento de la clave), `nombre`, `descripcion`, `ambito` (`empresa` por
  defecto) y `activo`. Clave repetida → **409**. Formato inválido → **400**.
- `PUT` **no** recibe la clave (es inmutable). Cambia módulo, nombre, descripción, ámbito y activo.
  Pasar a `plataforma` un privilegio que ya está en roles de empresa → **409**.
- `GET` devuelve el catálogo ordenado por módulo y clave. Filtro opcional: `?modulo=carro`.

Ejemplo:

```json
{
  "clave": "carro.crear",
  "nombre": "Agregar carro",
  "descripcion": "Registrar un vehículo nuevo en la flotilla",
  "ambito": "empresa",
  "activo": true
}
```

### 3.2. Asignar privilegios a roles (`/api/rol-privilegios`)

Nueva regla: un privilegio con `ambito = plataforma` solo se asigna a roles globales. Si el rol es de una
empresa → **400**. (La regla de `usuario_rol` — rol global o de la empresa del empleado — ya existía.)

### 3.3. Perfil y login

`perfil.privilegios` ahora trae **claves** (antes traía nombres) y hay un campo nuevo `perfil.versionPrivilegios`.

### 3.4. 🆕 `GET /api/auth/privilegios` (offline-first)

Requiere token. Devuelve los privilegios efectivos del usuario: la unión de los privilegios activos de sus
roles activos que sean globales o de su empresa (un rol de otra empresa nunca da permisos).

```json
{
  "claves": ["carro.crear", "carro.ver", "viaje.ver_propios"],
  "version": "3f1c…(SHA-256 de las claves ordenadas, 64 caracteres)"
}
```

- La versión también va en el header `ETag`.
- La app guarda `claves` + `version` en su BD local. En la siguiente sincronización manda
  `If-None-Match: "<version>"`: si no cambió responde **304** sin cuerpo; si cambió, **200** con las claves nuevas.
- La versión cambia también cuando se **quita** un privilegio o un rol (filas borradas), sin columnas de fecha.

---

## 4. Archivos tocados

- Dominio: `Privilegio` (clave, módulo, ámbito, `AsignableA(rol)`), nuevo enum `AmbitoPrivilegio`,
  `IPrivilegioRepository`, `IRolPrivilegioRepository`.
- Application: comandos/DTO/consulta de privilegios, comandos de rol-privilegio, `PerfilUsuarioBuilder`,
  `PerfilUsuarioDto`, nuevos `PrivilegiosUsuarioDto`, `GetPrivilegiosUsuarioQuery` y `VersionPrivilegios`.
- Infrastructure: configuraciones de `privilegio` y `rol`, repositorios, interceptor de auditoría
  (ignora columnas generadas) y la migración `RbacPrivilegiosV5`.
- API: contratos y controlador de privilegios, `AuthController` (`GET /api/auth/privilegios`).
- Pruebas: `PrivilegioTests` (reescritas) y `VersionPrivilegiosTests` (nuevas).
- Postman: 02.3 con `clave`; nuevas 05.4a (200) y 05.4b (304).

---

## 5. Pendiente (siguiente paso)

La API todavía autoriza por **nombre de rol** en estos puntos: `PermisosAuditoria`, `ReglasCorreccion` y la
búsqueda de confirmadores para notificaciones (`NombresRol`). Para cerrar el RBAC conviene:

1. Definir el catálogo de claves (ej. `auditoria.consultar`, `viaje.corregir_cerrado`, `solicitud.aprobar`)
   y cargarlo con un seed.
2. Cambiar esas validaciones a `usuario.TienePrivilegio("...")` (incluyendo las claves en el JWT o
   consultándolas en la BD).
