# Guía Rápida: Sistema de Migraciones Automatizado

## 🎯 Resumen Ejecutivo

Se ha implementado un sistema que **automatiza completamente** el proceso de migraciones. Ya **NO necesitas** eliminar archivos manualmente ni ejecutar comandos en terminal.

## 🚀 Uso Rápido

### Antes (Proceso Manual) ❌
```bash
# 1. Eliminar archivos en: src/AVASphere.Infrastructure/System/Migrations
# 2. Abrir terminal
# 3. Ejecutar:
dotnet ef migrations add Initial --project ... --startup-project ... --output-dir ...
# 4. Ejecutar:
dotnet ef database update --project ... --startup-project ...
```

### Ahora (Proceso Automático) ✅
```http
POST /api/system/DbTools/full-migration?name=Initial
```

**¡Un solo endpoint hace todo el trabajo!**

## 📋 Endpoints Disponibles

### 1️⃣ Proceso Completo (Recomendado) ⭐
```http
POST /api/system/DbTools/full-migration?name=MiNombreDeMigracion
```
**Hace:**
- ✅ Verifica conexión
- 🗑️ Elimina tablas existentes
- 🗑️ Elimina archivos de migración antiguos
- 📝 Crea nueva migración
- ⚙️ Aplica migración a la DB
- ✅ Verifica resultado

### 2️⃣ Verificar Migraciones Existentes
```http
GET /api/system/DbTools/check-migrations
```
**Respuesta:**
```json
{
  "hasMigrations": true,
  "migrationCount": 3,
  "migrationFiles": ["20251103214605_Initial.cs", "..."]
}
```

### 3️⃣ Eliminar Migraciones
```http
DELETE /api/system/DbTools/delete-migrations
```

### 4️⃣ Verificar Estado de DB
```http
GET /api/system/DbTools/check
```

### 5️⃣ Eliminar Todas las Tablas
```http
DELETE /api/system/DbTools/drop
```

## 💡 Flujos de Trabajo Comunes

### Escenario 1: Agregar Nueva Entidad (ej: ExternalNoteData)
```http
# Opción A: Un solo paso
POST /api/system/DbTools/full-migration?name=AddExternalNoteData

# Opción B: Paso a paso
1. DELETE /api/system/DbTools/drop              # Eliminar tablas
2. DELETE /api/system/DbTools/delete-migrations # Eliminar archivos
3. POST /api/system/DbTools/create-migration?name=AddExternalNoteData
4. POST /api/system/DbTools/apply-migration
```

### Escenario 2: Reset Completo de Desarrollo
```http
POST /api/system/DbTools/full-migration?name=DevReset
```

### Escenario 3: Verificar Estado Actual
```http
GET /api/system/DbTools/check
GET /api/system/DbTools/check-migrations
```

## 🎬 Ejemplo Real

### Situación: Acabas de agregar el campo `ExternalNoteData` a `Sale`

**Paso 1:** Verificar estado actual
```http
GET /api/system/DbTools/check-migrations
```

**Respuesta:**
```json
{
  "hasMigrations": true,
  "migrationCount": 3,
  "migrationFiles": ["20251103214605_Initial.cs", "20251103214605_Initial.Designer.cs", "MasterDbContextModelSnapshot.cs"]
}
```

**Paso 2:** Aplicar migración completa
```http
POST /api/system/DbTools/full-migration?name=AddExternalNoteToSale
```

**Respuesta:**
```json
{
  "result": "✅ Conexión verificada: Conexión OK. Existen tablas en la base de datos.\n🗑️ 5 tablas eliminadas exitosamente\n🗑️ 3 archivos de migración eliminados exitosamente\n✅ Migración creada exitosamente.\n✅ Migraciones aplicadas correctamente.\n✅ Estado final: Conexión OK. Existen tablas en la base de datos.",
  "success": true
}
```

**¡Listo!** ✅ Tu base de datos ahora tiene el campo `ExternalNoteData`

## 📱 Usar desde Swagger/Postman

### Swagger UI
1. Navega a: `https://localhost:5001/swagger`
2. Busca: **System - Database Tools**
3. Expande: `POST /api/system/DbTools/full-migration`
4. Click: **Try it out**
5. Ingresa: `name` = "MiMigracion"
6. Click: **Execute**

### Postman
```
POST http://localhost:5000/api/system/DbTools/full-migration?name=MiMigracion
```

### cURL
```bash
curl -X POST "http://localhost:5000/api/system/DbTools/full-migration?name=MiMigracion"
```

## ⚙️ Configuración (appsettings.json)

```json
{
  "EfTools": {
    "MigrationsFolder": "System/Migrations"
  },
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=AVASphereDB;Username=postgres;Password=postgres;"
  }
}
```

## ✅ Ventajas

| Antes | Ahora |
|-------|-------|
| 5 pasos manuales | 1 llamada HTTP |
| 3-5 minutos | 10-30 segundos |
| Propenso a errores | Automatizado |
| Buscar carpeta manualmente | Todo automático |
| Copiar comandos largos | Un endpoint simple |

## ⚠️ Notas Importantes

### ✅ Desarrollo
- **Perfecto** para desarrollo local
- Usa nombres descriptivos: `AddSalesModule`, `UpdateCustomer`

### ❌ Producción
- **NO usar** en producción
- Aplicar migraciones manualmente con validación

### 🔄 CI/CD
- Ideal para pipelines de integración continua
- Script automático antes de tests

## 🆘 Solución de Problemas

### Problema: "Base contiene datos"
```
⚠️ La base contiene datos. No se puede crear una nueva migración.
```
**Solución:**
```http
DELETE /api/system/DbTools/drop
POST /api/system/DbTools/full-migration?name=MiMigracion
```

### Problema: "No se encuentra el proyecto"
```
❌ No se encuentra el proyecto de infraestructura
```
**Solución:** El sistema busca automáticamente, pero verifica que el proyecto compile

### Problema: "Build failed"
```
❌ Error en EF: Build failed
```
**Solución:** Asegúrate que el proyecto compile sin errores primero

## 📚 Documentación Completa

Para más detalles, ver: `docs/AutomatedMigrationSystem.md`

---

**TL;DR:** Usa `POST /api/system/DbTools/full-migration?name=MiNombre` y olvídate de todo lo demás. 🚀

