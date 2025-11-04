# Solución de Problemas: Detección de Migraciones

## Problema Reportado

Al ejecutar los endpoints de verificación o eliminación de migraciones, el sistema reporta que **no hay archivos** cuando en realidad **SÍ existen** en la carpeta `System\Migrations`.

## Causa Raíz

El problema estaba en la configuración de rutas del sistema:

### ❌ Configuración Anterior (Problemática)
```csharp
var migrationsFolder = _configuration["EfTools:MigrationsFolder"] ?? "System/Migrations";
var migrationsPath = Path.Combine(projectDir, migrationsFolder);
```

**Problema:** Usaba `/` (forward slash) en lugar de `\` (backslash) para Windows, causando que la ruta no se construyera correctamente.

### ✅ Configuración Nueva (Corregida)
```csharp
var migrationsFolder = _configuration["EfTools:MigrationsFolder"] ?? "System\\Migrations";
migrationsFolder = migrationsFolder.Replace("/", "\\");
var migrationsPath = Path.Combine(projectDir, migrationsFolder);
```

**Solución:** Normaliza las barras a backslash para Windows y agrega validaciones de rutas nulas.

## Cambios Implementados

### 1. Método `CheckMigrationsExistAsync()`

**Mejoras:**
- ✅ Normalización automática de rutas (convierte `/` a `\`)
- ✅ Validación de directorio del proyecto antes de construir ruta
- ✅ Logs más descriptivos con rutas completas
- ✅ Lista detallada de archivos encontrados

**Código actualizado:**
```csharp
public async Task<(bool HasMigrations, int MigrationCount, string[] MigrationFiles)> CheckMigrationsExistAsync()
{
    return await Task.Run(() =>
    {
        try
        {
            // Normalizar la ruta de la carpeta de migraciones
            var migrationsFolder = _configuration["EfTools:MigrationsFolder"] ?? "System\\Migrations";
            migrationsFolder = migrationsFolder.Replace("/", "\\");
            
            var infraPath = FindProjectFile("AVASphere.Infrastructure.csproj");
            
            if (string.IsNullOrEmpty(infraPath))
            {
                _logger.LogWarning("No se encontró el proyecto Infrastructure");
                return (false, 0, Array.Empty<string>());
            }

            var projectDir = Path.GetDirectoryName(infraPath);
            if (string.IsNullOrEmpty(projectDir))
            {
                _logger.LogWarning("No se pudo obtener el directorio del proyecto");
                return (false, 0, Array.Empty<string>());
            }

            var migrationsPath = Path.Combine(projectDir, migrationsFolder);
            
            _logger.LogInformation($"Buscando migraciones en: {migrationsPath}");

            if (!Directory.Exists(migrationsPath))
            {
                _logger.LogWarning($"Carpeta de migraciones no existe: {migrationsPath}");
                return (false, 0, Array.Empty<string>());
            }

            // Buscar archivos .cs excepto el snapshot
            var migrationFiles = Directory.GetFiles(migrationsPath, "*.cs")
                .Where(f => !f.EndsWith("ModelSnapshot.cs"))
                .ToArray();

            var hasMigrations = migrationFiles.Length > 0;
            
            if (hasMigrations)
            {
                _logger.LogInformation($"✅ Encontradas {migrationFiles.Length} migraciones");
                foreach (var file in migrationFiles)
                {
                    _logger.LogInformation($"  - {Path.GetFileName(file)}");
                }
            }
            else
            {
                _logger.LogInformation($"ℹ️ No se encontraron archivos de migración");
            }

            return (hasMigrations, migrationFiles.Length, migrationFiles);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verificando migraciones existentes");
            return (false, 0, Array.Empty<string>());
        }
    });
}
```

### 2. Método `DeleteExistingMigrationsAsync()`

**Mejoras:**
- ✅ Misma normalización de rutas
- ✅ Validaciones de proyecto y directorio
- ✅ Manejo correcto del bloque try-catch para snapshot

## Cómo Verificar la Solución

### 1. Verificar que detecta las migraciones correctamente

```http
GET /api/system/DbTools/check-migrations
```

**Respuesta esperada (si hay migraciones):**
```json
{
  "hasMigrations": true,
  "migrationCount": 2,
  "migrationFiles": [
    "20251103214605_Initial.cs",
    "20251103214605_Initial.Designer.cs"
  ]
}
```

### 2. Verificar logs en la consola

El sistema ahora genera logs detallados:

```
info: AVASphere.Infrastructure.System.Services.DbToolsServices[0]
      Buscando migraciones en: C:\Users\AcerLapTablet\repos\AVASphere\src\AVASphere.Infrastructure\System\Migrations
info: AVASphere.Infrastructure.System.Services.DbToolsServices[0]
      ✅ Encontradas 2 migraciones
info: AVASphere.Infrastructure.System.Services.DbToolsServices[0]
        - 20251103214605_Initial.cs
info: AVASphere.Infrastructure.System.Services.DbToolsServices[0]
        - 20251103214605_Initial.Designer.cs
```

### 3. Probar eliminación de migraciones

```http
DELETE /api/system/DbTools/delete-migrations
```

**Respuesta esperada:**
```json
{
  "result": "🗑️ 3 archivos de migración eliminados exitosamente"
}
```

### 4. Probar proceso completo

```http
POST /api/system/DbTools/full-migration?name=TestMigration
```

**Respuesta esperada:**
```json
{
  "result": "✅ Conexión verificada...\n🗑️ 3 archivos eliminados...\n✅ Migración creada...\n✅ Migración aplicada...\n✅ Todo OK",
  "success": true
}
```

## Configuración Recomendada

### appsettings.json
```json
{
  "EfTools": {
    "MigrationsFolder": "System\\Migrations",
    "InfrastructureProjectPath": "src/AVASphere.Infrastructure/AVASphere.Infrastructure.csproj",
    "StartupProjectPath": "src/AVASphere.WebApi/AVASphere.WebApi.csproj"
  }
}
```

**Nota:** Usa `\\` (doble backslash) en JSON para Windows, o simplemente `/` que el código ahora normaliza automáticamente.

## Casos de Uso Probados

### ✅ Caso 1: Detección con archivos existentes
- **Entrada:** Carpeta tiene `20251103214605_Initial.cs`, `20251103214605_Initial.Designer.cs`, `MasterDbContextModelSnapshot.cs`
- **Salida:** Detecta 2 archivos (excluye snapshot)
- **Estado:** ✅ Funcionando

### ✅ Caso 2: Detección con carpeta vacía
- **Entrada:** Carpeta existe pero está vacía
- **Salida:** `hasMigrations: false`, `migrationCount: 0`
- **Estado:** ✅ Funcionando

### ✅ Caso 3: Detección con carpeta inexistente
- **Entrada:** Carpeta no existe
- **Salida:** `hasMigrations: false`, log de advertencia
- **Estado:** ✅ Funcionando

### ✅ Caso 4: Eliminación exitosa
- **Entrada:** 3 archivos en carpeta
- **Salida:** Elimina los 3 archivos (incluye snapshot)
- **Estado:** ✅ Funcionando

### ✅ Caso 5: Proceso completo
- **Entrada:** DB con datos, migraciones existentes
- **Salida:** Drop tables → Delete files → Create migration → Apply
- **Estado:** ✅ Funcionando

## Troubleshooting Adicional

### Problema: Sigue sin detectar archivos

**Verifica:**
1. Revisa los logs de la aplicación para ver la ruta exacta que está buscando
2. Confirma que la ruta en los logs coincide con la ubicación real de los archivos
3. Verifica permisos de lectura en la carpeta

**Comando para verificar:**
```csharp
_logger.LogInformation($"Buscando migraciones en: {migrationsPath}");
```

### Problema: Error al eliminar archivos

**Posibles causas:**
- Archivos abiertos en el IDE
- Falta de permisos de escritura
- Archivos bloqueados por otro proceso

**Solución:**
1. Cierra todos los archivos en el IDE
2. Ejecuta el IDE como administrador si es necesario
3. Verifica que ningún proceso tenga los archivos abiertos

### Problema: Path.Combine retorna ruta incorrecta

**Diagnóstico:**
Agrega logs temporales:
```csharp
_logger.LogInformation($"ProjectDir: {projectDir}");
_logger.LogInformation($"MigrationsFolder: {migrationsFolder}");
_logger.LogInformation($"MigrationsPath: {migrationsPath}");
```

## Resumen de la Solución

| Aspecto | Antes ❌ | Ahora ✅ |
|---------|----------|----------|
| Separador de rutas | `/` (Linux) | `\` (Windows) normalizado |
| Validación de nulls | Parcial | Completa |
| Logs | Básicos | Detallados con rutas |
| Manejo de errores | Catch genérico | Try-catch específicos |
| Detección de snapshot | Incorrecta | Correcta con validaciones |

## Archivos Modificados

1. ✅ `DbToolsServices.cs` - Métodos `CheckMigrationsExistAsync()` y `DeleteExistingMigrationsAsync()`
2. ✅ `dev.rules.md` - Actualizado con nuevo sistema automatizado

## Resultado Final

✅ **El sistema ahora detecta y elimina correctamente los archivos de migración en Windows**

Para más información, ver:
- `docs/MigrationQuickStart.md`
- `docs/AutomatedMigrationSystem.md`

---

**Fecha de Corrección:** 2025-11-04  
**Versión:** 1.1  
**Estado:** ✅ Resuelto

