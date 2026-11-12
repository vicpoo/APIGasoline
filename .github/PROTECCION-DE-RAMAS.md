# Proteger `main` y `develop` (que no se pueda mergear sin CI en verde)

El workflow `.github/workflows/ci.yml` crea dos revisiones en cada pull request:

| Revisión | Qué hace |
|---|---|
| **Compilación y pruebas unitarias** | Compila en Release; pruebas de dominio, aplicación y arquitectura; revisa que no falten migraciones; genera `migraciones.sql`; avisa de paquetes vulnerables. |
| **Pruebas de integración** | La API completa contra SQL Server en Docker: matriz de fugas entre empresas, lo propio, Row-Level Security, empresa inactiva, concurrencia y paginación. |

La regla que obliga a pasarlas **no vive en el repositorio**: se configura una vez en GitHub (necesitas ser administrador del repo).

## Opción A: desde la página de GitHub

1. Repositorio → **Settings** → **Rules** → **Rulesets** → **New ruleset** → **New branch ruleset**.
2. **Ruleset name:** `CI obligatorio`. **Enforcement status:** `Active`.
3. **Target branches** → **Add target** → **Include by pattern**: `main` y otra vez `develop`.
4. Marca:
   - **Restrict deletions**.
   - **Require a pull request before merging** (aprobaciones requeridas: 0 si trabajas solo, 1 con equipo).
   - **Require status checks to pass** → **Add checks** → escribe y agrega `Compilación y pruebas unitarias` y `Pruebas de integración`. Activa también **Require branches to be up to date before merging**.
   - **Block force pushes**.
5. **Create**.

> Los nombres de las revisiones solo aparecen para elegirlos después de que el workflow corrió al menos una vez (haz push de la rama primero).

## Opción B: con GitHub CLI (`gh`)

Desde la carpeta del repo, ya con `gh auth login` hecho (PowerShell):

```powershell
@'
{
  "name": "CI obligatorio",
  "target": "branch",
  "enforcement": "active",
  "conditions": { "ref_name": { "include": ["refs/heads/main", "refs/heads/develop"], "exclude": [] } },
  "rules": [
    { "type": "deletion" },
    { "type": "non_fast_forward" },
    { "type": "pull_request", "parameters": {
        "required_approving_review_count": 0, "dismiss_stale_reviews_on_push": false,
        "require_code_owner_review": false, "require_last_push_approval": false,
        "required_review_thread_resolution": false } },
    { "type": "required_status_checks", "parameters": {
        "strict_required_status_checks_policy": true,
        "required_status_checks": [
          { "context": "Compilación y pruebas unitarias" },
          { "context": "Pruebas de integración" } ] } }
  ]
}
'@ | Set-Content -Encoding utf8 ruleset.json

gh api repos/vicpoo/APIGasoline/rulesets --method POST --input ruleset.json
Remove-Item ruleset.json
```

## Flujo de trabajo desde ahora

1. Rama desde `develop`: `git switch develop; git pull; git switch -c feat/mi-cambio`.
2. Commits con Conventional Commits y `git push -u origin feat/mi-cambio`.
3. Pull request a `develop`. El CI corre solo; con las dos revisiones en verde se puede mergear.
4. Para publicar: pull request de `develop` a `main`.

El script `migraciones.sql` de cada corrida se descarga en la pestaña **Actions** → la corrida → **Artifacts**.
