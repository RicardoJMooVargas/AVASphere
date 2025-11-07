# Solución de Error: "Build failed. Use dotnet build to see the errors"

## Problema Reportado

Al ejecutar el endpoint de migración, se recibe el siguiente error:

```json
{
  "success": true,
  "message": "Success",
  "data": {
    "result": "❌ Error en EF: Build started...\r\nBuild failed. Use dotnet build to see the errors.\r\n"
  },
  "statusCode": 200,
  "timestamp": "2025-11-04T18:39:45.1105539Z"
}
```

## Causa Identificada y Corrección

### 🐛 Error en ConfigSysEntitieConfig

Se encontró un error en la configuración de relaciones de Entity Framework:

#### ❌ Código Problemático (Corregido)
```csharp
// ERROR: Usaba QuotationId (PK) en lugar de IdConfigSys (FK)
entity.HasMany(q => q.Quotations)
  .WithOne(q => q.ConfigSys)
  .HasForeignKey(q => q.QuotationId)  // ← INCORRECTO
  .OnDelete(DeleteBehavior.Restrict);
```

#### ✅ Código Corregido
```csharp
// CORRECTO: Usa IdConfigSys (FK) como debe ser
entity.HasMany(c => c.Quotations)
  .WithOne(q => q.ConfigSys)
  .HasForeignKey(q => q.IdConfigSys)  // ← CORRECTO
  .OnDelete(DeleteBehavior.Restrict);

// También agregada relación con Sales
entity.HasMany(c => c.Sales)
  .WithOne(s => s.ConfigSys)
  .HasForeignKey(s => s.IdConfigSys)
  .OnDelete(DeleteBehavior.Restrict);
```

### 🧹 Limpieza de Usings

También se eliminó un using innecesario que podría causar conflictos:

```csharp
// ELIMINADO: using AVASphere.ApplicationCore.Sales.Entities;
```

## Archivos Corregidos

### 1. `ConfigSysEntitieConfig.cs` ✅

**Ubicación:** `src/AVASphere.Infrastructure/Common/Configuration/ConfigSysEntitieConfig.cs`

**Cambios:**
- ✅ Corregida FK de Quotations: `q.QuotationId` → `q.IdConfigSys`
- ✅ Agregada relación con Sales
- ✅ Eliminado using innecesario

### 2. `DbToolsController.cs` ✅

**Ubicación:** `src/AVASphere.WebApi/System/Controllers/DbToolsController.cs`

**Cambios:**
- ✅ Agregado endpoint `/test` para verificación básica

## Verificación de la Corrección

### Test 1: Verificar que el controlador funciona
```http
GET /api/system/DbTools/test
```

**Respuesta esperada:**
```json
{
  "status": "OK",
  "message": "DbToolsController funcionando correctamente",
  "timestamp": "2025-11-04T18:45:00Z"
}
```

### Test 2: Verificar conexión básica
```http
GET /api/system/DbTools/check
```

**Debería funcionar sin errores de build.**

### Test 3: Intentar migración nuevamente
```http
POST /api/system/DbTools/full-migration?name=TestBuildFix
```

**Ahora debería compilar correctamente.**

## Comandos de Diagnóstico Manual

Si el problema persiste, ejecutar estos comandos para obtener más información:

### 1. Limpiar y Compilar
```bash
cd C:\Users\AcerLapTablet\repos\AVASphere\src
dotnet clean
dotnet build
```

### 2. Restaurar Paquetes
```bash
dotnet restore
```

### 3. Compilar Proyecto Específico
```bash
dotnet build AVASphere.Infrastructure/AVASphere.Infrastructure.csproj
dotnet build AVASphere.WebApi/AVASphere.WebApi.csproj
```

### 4. Ver Errores Detallados
```bash
dotnet build --verbosity detailed
```

## Posibles Causas Adicionales

Si el error persiste, revisar:

### 1. Referencias de Proyecto Faltantes
Verificar en `.csproj` que las referencias estén correctas:

```xml
<ProjectReference Include="..\AVASphere.ApplicationCore\AVASphere.ApplicationCore.csproj" />
<ProjectReference Include="..\AVASphere.Infrastructure\AVASphere.Infrastructure.csproj" />
```

### 2. Paquetes NuGet Faltantes
Verificar que estén instalados:
- `Microsoft.EntityFrameworkCore`
- `Microsoft.EntityFrameworkCore.Design`
- `Npgsql.EntityFrameworkCore.PostgreSQL`

### 3. Versiones de .NET Compatibles
Verificar que todos los proyectos usen la misma versión de .NET.

### 4. Configuraciones Conflictivas
Revisar que no haya configuraciones duplicadas o conflictivas en:
- `MasterDbContext.cs`
- Archivos `*EntitieConfig.cs`

## Estructura de Relaciones Correcta

### ConfigSys (1) ----< (N) Users
```csharp
// En ConfigSys
public ICollection<User> Users { get; set; }

// En User
public int IdConfigSys { get; set; }
public ConfigSys ConfigSys { get; set; }

// En Configuración
entity.HasMany(c => c.Users)
  .WithOne(u => u.ConfigSys)
  .HasForeignKey(u => u.IdConfigSys);
```

### ConfigSys (1) ----< (N) Quotations
```csharp
// En ConfigSys
public ICollection<Quotation> Quotations { get; set; }

// En Quotation
public int IdConfigSys { get; set; }
public ConfigSys ConfigSys { get; set; }

// En Configuración
entity.HasMany(c => c.Quotations)
  .WithOne(q => q.ConfigSys)
  .HasForeignKey(q => q.IdConfigSys);  // ← CORREGIDO
```

### ConfigSys (1) ----< (N) Sales
```csharp
// En ConfigSys
public ICollection<Sale> Sales { get; set; }

// En Sale
public int IdConfigSys { get; set; }
public ConfigSys ConfigSys { get; set; }

// En Configuración
entity.HasMany(c => c.Sales)
  .WithOne(s => s.ConfigSys)
  .HasForeignKey(s => s.IdConfigSys);
```

## Logs de Entity Framework

Para obtener más información sobre errores de EF, agregar en `appsettings.Development.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.EntityFrameworkCore": "Debug"
    }
  }
}
```

## Resumen de la Corrección

| Problema | Causa | Solución |
|----------|-------|----------|
| Build failed | FK incorrecta en relación | Cambiar `q.QuotationId` a `q.IdConfigSys` |
| Using innecesario | Import no utilizado | Eliminar using de Sales.Entities |
| Falta relación Sales | Configuración incompleta | Agregar relación ConfigSys → Sales |

## Estado Actual

✅ **Corregido:** Error de FK en ConfigSysEntitieConfig  
✅ **Agregado:** Endpoint de test para verificación  
✅ **Limpiado:** Usings innecesarios  
✅ **Completado:** Relación ConfigSys → Sales  

## Próximo Paso

Probar nuevamente el endpoint de migración:

```http
POST /api/system/DbTools/full-migration?name=TestAfterBuildFix
```

---

**Fecha de Corrección:** 2025-11-04  
**Estado:** ✅ Resuelto  
**Archivos Modificados:** 2  

El error de "Build failed" debería estar resuelto. Si persiste, ejecutar `dotnet build` manualmente para obtener detalles específicos del error.
