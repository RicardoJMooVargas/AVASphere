# REGLAS DE DESARROLLO

## Convenciones de Nombramiento Basicas
| Tipo            | regla                  |
|-----------------|------------------------|
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

### ✅ NUEVO: Sistema Automatizado (Recomendado)
Usa el endpoint que hace todo automáticamente:
```http
POST /api/system/DbTools/full-migration?name=Initial
```
**⚠️ IMPORTANTE:** La migración principal DEBE llamarse "Initial" para que las instalaciones nuevas funcionen correctamente.

Este endpoint automáticamente:
1. Verifica la conexión
2. Elimina tablas existentes
3. Elimina archivos de migración antiguos en `System\Migrations` (ubicación correcta)
4. Crea nueva migración con nombre "Initial"
5. Aplica la migración

### ❌ Sistema Manual (Antiguo - No recomendado)
Si necesitas hacerlo manualmente, sigue estos pasos:

1. Elimina los archivos de migración dentro de la carpeta `System\Migrations` manualmente (ubicación correcta)

2. Crea la migración:
```bash
dotnet ef migrations add Initial `
  --project src/AVASphere.Infrastructure `
  --startup-project src/AVASphere.Infrastructure `
  --context MasterDbContext `
  --output-dir System/Migrations
```
En Linux
```bash
# DESDE EL DIRECTORIO RAÍZ DEL PROYECTO (/home/ricardomogas/RiderProjects/AVASphere):
dotnet ef migrations add Initial \
  --project src/AVASphere.Infrastructure \
  --startup-project src/AVASphere.Infrastructure \
  --context MasterDbContext \
  --output-dir System/Migrations
            
# DESDE EL DIRECTORIO WEBAPI (/home/ricardomogas/RiderProjects/AVASphere/src/AVASphere.WebApi

# OPCIÓN CON PATHS ABSOLUTOS (desde cualquier directorio):
dotnet ef migrations add Initial \
  --project /home/ricardomogas/RiderProjects/AVASphere/src/AVASphere.Infrastructure \
  --startup-project /home/ricardomogas/RiderProjects/AVASphere/src/AVASphere.Infrastructure \
  --context MasterDbContext \
  --output-dir System/Migrations
```

3. Aplica la migración:
```bash
# DESDE EL DIRECTORIO RAÍZ DEL PROYECTO:
dotnet ef database update \
  --project src/AVASphere.Infrastructure \
  --startup-project src/AVASphere.Infrastructure \
  --context MasterDbContext

# DESDE EL DIRECTORIO WEBAPI:
dotnet ef database update \
  --project ../AVASphere.Infrastructure \
  --startup-project ../AVASphere.Infrastructure \
  --context MasterDbContext

# CON PATHS ABSOLUTOS (desde cualquier directorio):
dotnet ef database update \
  --project /home/ricardomogas/RiderProjects/AVASphere/src/AVASphere.Infrastructure \
  --startup-project /home/ricardomogas/RiderProjects/AVASphere/src/AVASphere.Infrastructure \
  --context MasterDbContext
```

**Nota:** Es necesario usar `Infrastructure` como startup project debido al `IDesignTimeDbContextFactory`.

## 🚀 Equivalentes con Endpoints (Recomendado para Desarrollo)

### Proceso Completo Automático (Equivalente a comandos manuales):
```http
POST http://localhost:5000/api/system/DbTools/full-migration?name=NombreMigracion
```

### Endpoints Paso a Paso:
```http
# 1. Verificar estado
GET http://localhost:5000/api/system/DbTools/detailed-status

# 2. Crear migración (equivale a: dotnet ef migrations add)
POST http://localhost:5000/api/system/DbTools/create-migration?name=Initial

# 3. Aplicar migración (equivale a: dotnet ef database update)
POST http://localhost:5000/api/system/DbTools/apply-migration

# 4. Verificar resultado
GET http://localhost:5000/api/system/DbTools/check
```

### Para Recreación Completa (Cuando hay cambios grandes en el modelo):
```http
POST http://localhost:5000/api/system/DbTools/force-recreate-model?name=ModelUpdate
```

### Usando Swagger UI:
1. Navega a: `https://localhost:5001/swagger`
2. Busca: **"System - Database Tools"**
3. Usa: `POST /api/system/DbTools/full-migration`

### 📚 Documentación Completa
Ver: `docs/MigrationQuickStart.md`, `docs/AutomatedMigrationSystem.md` y `docs/EndpointEquivalentCommands.md`
