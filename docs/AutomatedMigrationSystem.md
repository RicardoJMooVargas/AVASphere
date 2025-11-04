# Sistema Automatizado de Migraciones - DbToolsController

## Resumen

Se ha implementado un sistema completo de gestión automática de migraciones que detecta, elimina y crea nuevas migraciones sin intervención manual.

## Nuevos Endpoints

### 1. **GET** `/api/system/DbTools/check-migrations`
**Descripción:** Verifica si existen archivos de migración en la carpeta `System/Migrations`

**Respuesta:**
```json
{
  "hasMigrations": true,
  "migrationCount": 3,
  "migrationFiles": [
    "20251103214605_Initial.cs",
    "20251103214605_Initial.Designer.cs",
    "MasterDbContextModelSnapshot.cs"
  ]
}
```

### 2. **DELETE** `/api/system/DbTools/delete-migrations`
**Descripción:** Elimina todos los archivos de migración existentes (incluyendo el snapshot)

**Respuesta:**
```json
{
  "result": "🗑️ 3 archivos de migración eliminados exitosamente"
}
```

### 3. **POST** `/api/system/DbTools/full-migration` ⭐
**Descripción:** Proceso completo automatizado de migración

**Query Parameters:**
- `name` (opcional, default: "Initial"): Nombre de la migración

**Proceso que ejecuta:**
1. ✅ Verificar conexión a la base de datos
2. 🗑️ Eliminar tablas existentes (si hay datos)
3. 🗑️ Eliminar archivos de migración antiguos (si existen)
4. 📝 Crear nueva migración con el nombre especificado
5. ⚙️ Aplicar migración a la base de datos
6. ✅ Verificar estado final

**Respuesta:**
```json
{
  "result": "✅ Conexión verificada: Conexión OK. No existen tablas (base vacía o sin migrar).\n🗑️ 3 archivos de migración eliminados exitosamente\n📝 Creando nueva migración: Initial...\n✅ Migración creada exitosamente.\n⚙️ Aplicando migración a la base de datos...\n✅ Migraciones aplicadas correctamente.\n✅ Estado final: Conexión OK. Existen tablas en la base de datos.",
  "success": true
}
```

### 4. **GET** `/api/system/DbTools/check`
**Descripción:** Verifica la conexión y estado de la base de datos

**Respuesta:**
```json
{
  "isConnected": true,
  "hasData": false,
  "message": "Conexión OK. No existen tablas (base vacía o sin migrar)."
}
```

### 5. **POST** `/api/system/DbTools/create-migration`
**Descripción:** Crea una nueva migración (solo si DB está vacía)

**Query Parameters:**
- `name` (requerido): Nombre de la migración

**Respuesta:**
```json
{
  "result": "✅ Migración creada exitosamente."
}
```

### 6. **POST** `/api/system/DbTools/apply-migration`
**Descripción:** Aplica las migraciones pendientes (solo si DB está vacía)

**Respuesta:**
```json
{
  "result": "✅ Migraciones aplicadas correctamente."
}
```

### 7. **DELETE** `/api/system/DbTools/drop`
**Descripción:** Elimina todas las tablas de la base de datos

**Respuesta:**
```json
{
  "result": "🗑️ 5 tablas eliminadas exitosamente"
}
```

## Flujos de Trabajo

### Flujo 1: Proceso Manual (Tradicional)
```bash
# 1. Eliminar archivos manualmente
DELETE /api/system/DbTools/delete-migrations

# 2. Crear migración
POST /api/system/DbTools/create-migration?name=AddSalesTable

# 3. Aplicar migración
POST /api/system/DbTools/apply-migration
```

### Flujo 2: Proceso Automático ⭐ (Recomendado)
```bash
# Un solo endpoint hace todo el trabajo
POST /api/system/DbTools/full-migration?name=Initial
```

### Flujo 3: Reset Completo de Base de Datos
```bash
# 1. Eliminar todas las tablas
DELETE /api/system/DbTools/drop

# 2. Eliminar archivos de migración
DELETE /api/system/DbTools/delete-migrations

# 3. Crear y aplicar nueva migración
POST /api/system/DbTools/full-migration?name=Fresh
```

## Casos de Uso

### Caso 1: Primera Instalación
```http
POST /api/system/DbTools/full-migration?name=Initial
```
**Resultado:** Crea la primera migración y la aplica a la DB vacía

### Caso 2: Actualización de Modelo (Nueva Entidad)
```http
# Primero verificar migraciones existentes
GET /api/system/DbTools/check-migrations

# Proceso automático completo
POST /api/system/DbTools/full-migration?name=AddSalesModule
```
**Resultado:** Elimina migraciones antiguas, crea nueva con los cambios, aplica a la DB

### Caso 3: Reset de Desarrollo
```http
DELETE /api/system/DbTools/drop
POST /api/system/DbTools/full-migration?name=DevReset
```
**Resultado:** Base de datos limpia con esquema actualizado

### Caso 4: Verificar Estado Actual
```http
GET /api/system/DbTools/check
GET /api/system/DbTools/check-migrations
```
**Resultado:** Información sobre conexión, datos y migraciones existentes

## Comparación: Manual vs Automático

### ❌ Proceso Manual Anterior
```bash
# 1. Abrir carpeta manualmente
C:\Users\...\AVASphere\src\AVASphere.Infrastructure\System\Migrations

# 2. Eliminar archivos manualmente
# (3 archivos: .cs, .Designer.cs, Snapshot.cs)

# 3. Abrir terminal

# 4. Ejecutar comando de creación
dotnet ef migrations add Initial `
  --project src/AVASphere.Infrastructure `
  --startup-project src/AVASphere.Infrastructure `
  --context MasterDbContext `
  --output-dir System/Migrations

# 5. Ejecutar comando de aplicación
dotnet ef database update `
  --project src/AVASphere.Infrastructure `
  --startup-project src/AVASphere.Infrastructure `
  --context MasterDbContext
```

**Tiempo estimado:** 3-5 minutos  
**Pasos:** 5 pasos manuales  
**Errores potenciales:** Olvidar eliminar archivos, errores de path

### ✅ Proceso Automático Nuevo
```bash
POST /api/system/DbTools/full-migration?name=Initial
```

**Tiempo estimado:** 10-30 segundos  
**Pasos:** 1 llamada HTTP  
**Errores potenciales:** Mínimos, todo automatizado

## Características del Sistema

### ✅ Detección Inteligente
- Detecta automáticamente si existen migraciones
- Verifica el estado de la base de datos
- Encuentra automáticamente los proyectos .csproj

### ✅ Limpieza Automática
- Elimina archivos de migración antiguos
- Elimina el ModelSnapshot automáticamente
- Limpia tablas existentes si es necesario

### ✅ Proceso Completo
- Ejecuta todo el flujo en un solo endpoint
- Logs detallados de cada paso
- Respuesta con resumen completo

### ✅ Seguridad
- Solo permite crear migraciones si la DB está vacía (o se eliminó)
- Confirma cada paso antes de continuar
- Manejo robusto de errores

## Configuración Requerida

### appsettings.json
```json
{
  "EfTools": {
    "InfrastructureProjectPath": "src/AVASphere.Infrastructure/AVASphere.Infrastructure.csproj",
    "StartupProjectPath": "src/AVASphere.WebApi/AVASphere.WebApi.csproj",
    "MigrationsFolder": "System/Migrations"
  },
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=AVASphereDB;Username=postgres;Password=postgres;"
  }
}
```

## Logs del Proceso

El servicio genera logs detallados en cada paso:

```
🚀 Iniciando proceso completo de migración...
✅ Conexión verificada: Conexión OK. No existen tablas (base vacía o sin migrar).
ℹ️ No hay migraciones previas que eliminar
📝 Creando nueva migración: Initial...
Ejecutando comando EF: dotnet ef migrations add Initial --project "C:\...\AVASphere.Infrastructure.csproj" --startup-project "C:\...\AVASphere.WebApi.csproj" --output-dir "System/Migrations"
✅ Migración creada exitosamente.
⚙️ Aplicando migración a la base de datos...
✅ Migraciones aplicadas correctamente.
✅ Estado final: Conexión OK. Existen tablas en la base de datos.
```

## Ejemplo de Uso en Frontend

### JavaScript/TypeScript
```typescript
async function applyFullMigration(migrationName: string = 'Initial') {
  try {
    const response = await fetch(
      `/api/system/DbTools/full-migration?name=${migrationName}`,
      { method: 'POST' }
    );
    
    const data = await response.json();
    
    if (data.success) {
      console.log('✅ Migración completada exitosamente');
      console.log(data.result);
    } else {
      console.error('❌ Error en migración:', data.result);
    }
  } catch (error) {
    console.error('Error:', error);
  }
}

// Uso
applyFullMigration('AddExternalNoteData');
```

### cURL
```bash
# Proceso completo
curl -X POST "http://localhost:5000/api/system/DbTools/full-migration?name=Initial"

# Verificar migraciones
curl -X GET "http://localhost:5000/api/system/DbTools/check-migrations"

# Eliminar migraciones
curl -X DELETE "http://localhost:5000/api/system/DbTools/delete-migrations"
```

## Ventajas del Sistema

### 🚀 Velocidad
- Reduce el tiempo de migración de minutos a segundos
- Un solo clic/llamada para todo el proceso

### 🎯 Precisión
- Elimina errores humanos
- Proceso consistente y repetible

### 📊 Transparencia
- Respuesta detallada de cada paso
- Logs completos para debugging

### 🔄 Automatización
- Perfecto para CI/CD pipelines
- Integración fácil con scripts de deployment

### 🛡️ Seguridad
- Validaciones en cada paso
- Previene operaciones destructivas sin confirmación

## Casos de Error Comunes

### Error 1: Base de datos tiene datos
```json
{
  "result": "❌ La base contiene datos. Ejecute primero el drop de tablas.",
  "success": false
}
```
**Solución:** Ejecutar `DELETE /api/system/DbTools/drop` primero

### Error 2: No se encuentran proyectos .csproj
```json
{
  "result": "❌ No se encuentra el proyecto de infraestructura.",
  "success": false
}
```
**Solución:** Verificar la configuración de paths en appsettings.json

### Error 3: Error de EF Tools
```json
{
  "result": "❌ Error en EF: Build failed",
  "success": false
}
```
**Solución:** Verificar que el proyecto compile correctamente

## Recomendaciones

### ✅ Para Desarrollo
- Usar `full-migration` con nombres descriptivos: `AddSalesModule`, `UpdateCustomerFields`
- Mantener un log de nombres de migraciones aplicadas

### ✅ Para Producción
- **NO** usar estos endpoints en producción
- Implementar un sistema de versionado de BD separado
- Aplicar migraciones manualmente con validación

### ✅ Para Testing
- Crear script que ejecute `full-migration` antes de tests
- Reset automático de DB entre test suites

## Próximas Mejoras Sugeridas

1. **Backup automático** antes de aplicar migraciones
2. **Rollback** a migración anterior
3. **Preview** de cambios SQL antes de aplicar
4. **Historial** de migraciones aplicadas
5. **Validación** de integridad post-migración

---

**Fecha de Implementación:** 2025-11-04  
**Estado:** ✅ Completado  
**Endpoint Principal:** `POST /api/system/DbTools/full-migration?name={nombre}`

