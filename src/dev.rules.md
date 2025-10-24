# REGLAS DE DESARROLLO

## Convenciones de Nombramiento Basicas

| Tipo            | regla                  |
| --------------- | ---------------------- |
| Class           | PascaleCase            |
| Func            | PascaleCase            |
| Var             | camelCase              |
| Const           | UPPER_SNAKE            |
| Proyect Folders | nameProject.PascalCase |
| Modules Folders | Modulename             |
| Sub Modules     | Modulename             |
| Files           | PascaleCase            |
| Branch names    | type/kebab-case        |

### Convencion de ramas

- feature/feature-name : para nuevas funcionalidades
- bugfix/bug-name : para correccion de errores
- hotfix/hotfix-name : para correcciones urgentes en produccion
- release/release-name : para preparar una nueva version
- chore/chore-name : para tareas de mantenimiento
- docs/docs-name : para cambios en la documentacion
- test/test-name : para agregar o modificar pruebas
- refactor/refactor-name : para reestructurar codigo sin cambiar funcionalidad

## Reglas de migración

```bash
dotnet ef migrations add Initial `
  --project src/AVASphere.Infrastructure `
  --startup-project src/AVASphere.Infrastructure `
  --context MasterDbContext `
  --output-dir System/Migrations

```

```bash
dotnet ef database update `
  --project src/AVASphere.Infrastructure `
  --startup-project src/AVASphere.Infrastructure `
  --context MasterDbContext

```

## COMANDOS PARA REALIZAR UN MERGE (BAJAR CAMBIOS A TU PROPIA RAMA)
### ✅ Actualizar mi rama con los cambios de otra rama

Este flujo permite traer los cambios más recientes de la rama `feature/postgresql-implementation` hacia mi rama de trabajo `feature/create-module-inventory`.

```bash
# 1. Ir a mi rama actual
git checkout feature/create-module-inventory

# 2. (Opcional pero recomendado) Actualizar referencias del remoto
git fetch origin

# 3. Traer los cambios de la otra rama hacia la mía
git merge origin/feature/postgresql-implementation
# → Esto mezcla los cambios más recientes de esa rama dentro de la mía

# 4. Subir mi rama ya actualizada al repositorio remoto
git push origin feature/create-module-inventory

