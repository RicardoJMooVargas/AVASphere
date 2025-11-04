# Guía de Endpoints para Migraciones - Equivalentes a Comandos Manuales

## 🎯 Lo que acabas de hacer manualmente:

```bash
# 1. Crear migración
dotnet ef migrations add Initial `
  --project src/AVASphere.Infrastructure `
  --startup-project src/AVASphere.Infrastructure `
  --context MasterDbContext `
  --output-dir System/Migrations

# 2. Aplicar migración  
dotnet ef database update `
  --project src/AVASphere.Infrastructure `
  --startup-project src/AVASphere.Infrastructure `
  --context MasterDbContext
```

## 🚀 Equivalentes con Endpoints

### 📋 **Endpoint Completo (Todo en Uno)**
Hace exactamente el mismo proceso que acabas de ejecutar manualmente:

```http
POST http://localhost:5000/api/system/DbTools/full-migration?name=Initial
```

**O usando cURL:**
```bash
curl -X POST "http://localhost:5000/api/system/DbTools/full-migration?name=Initial"
```

### 📋 **Endpoints Paso a Paso**

#### 1. Verificar estado actual:
```http
GET http://localhost:5000/api/system/DbTools/detailed-status
```

#### 2. Crear migración:
```http
POST http://localhost:5000/api/system/DbTools/create-migration?name=Initial
```

#### 3. Aplicar migración:
```http
POST http://localhost:5000/api/system/DbTools/apply-migration
```

#### 3b. **Si hay error "pending changes", usar CLI (equivale exactamente al comando manual):**
```http
POST http://localhost:5000/api/system/DbTools/apply-migration-cli
```

#### 4. Verificar resultado:
```http
GET http://localhost:5000/api/system/DbTools/check
```

## 🔄 **Para Futuras Migraciones (Cuando Agregues Campos)**

### Si agregas nuevos campos (como ExternalNoteData, Products):

#### Opción A - Recreación Completa (Recomendado):
```http
POST http://localhost:5000/api/system/DbTools/force-recreate-model?name=AddNewFields
```

#### Opción B - Proceso Completo Normal:
```http
POST http://localhost:5000/api/system/DbTools/full-migration?name=AddNewFields
```

## 📱 **Usando Swagger UI (Más Fácil)**

1. Navega a: `https://localhost:5001/swagger`
2. Busca: **"System - Database Tools"**
3. Expande: `POST /api/system/DbTools/full-migration`
4. Click: **"Try it out"**
5. En `name`: escribe `"Initial"` o el nombre que quieras
6. Click: **"Execute"**

## 🧪 **Respuestas Esperadas**

### ✅ **Éxito:**
```json
{
  "result": "✅ Conexión verificada: Conexión OK. Base de datos válida con 8 tablas y 0 registro(s) en ConfigSys.\nℹ️ No hay migraciones previas que eliminar\n✅ Migración creada exitosamente.\n✅ Migraciones aplicadas correctamente.\n✅ Estado final: Conexión OK. Base de datos válida con 8 tablas y 0 registro(s) en ConfigSys.",
  "success": true
}
```

### ❌ **Si hay problema:**
```json
{
  "result": "❌ Error en EF: ...",
  "success": false
}
```

## 🎯 **Ventajas de Usar Endpoints**

| Aspecto | Manual | Endpoint |
|---------|--------|----------|
| **Tiempo** | 2-3 minutos | 30 segundos |
| **Pasos** | 4-5 comandos | 1 llamada HTTP |
| **Errores** | Posibles typos | Automatizado |
| **Logs** | Terminal | Respuesta JSON estructurada |
| **Integración** | Manual | Puede automatizarse en CI/CD |

## 🚀 **Prueba Inmediata**

Ya que tu migración manual funcionó, puedes probar que el endpoint automático funcione para futuras migraciones:

### Test 1: Verificar que detecta tu migración actual
```http
GET http://localhost:5000/api/system/DbTools/check-migrations
```

**Debería mostrar:**
```json
{
  "hasMigrations": true,
  "migrationCount": 2,
  "migrationFiles": ["20251104190733_Initial.cs", "20251104190733_Initial.Designer.cs"]
}
```

### Test 2: Verificar estado de la DB
```http
GET http://localhost:5000/api/system/DbTools/detailed-status
```

**Debería mostrar algo como:**
```json
{
  "isConnected": true,
  "hasTables": true,
  "hasConfigSys": true,
  "configSysRecords": 0,
  "totalTables": 8,
  "databaseState": "NotInitialized"
}
```

## 🚨 **Solución al Error "Pending Model Changes"**

Si recibes este error:
```
An error was generated for warning 'Microsoft.EntityFrameworkCore.Migrations.PendingModelChangesWarning': The model for context 'MasterDbContext' has pending changes.
```

**Usa el endpoint CLI que hace exactamente lo mismo que tu comando manual:**
```http
POST http://localhost:5000/api/system/DbTools/apply-migration-cli
```

Este endpoint ejecuta:
```bash
dotnet ef database update --project Infrastructure --startup-project Infrastructure --context MasterDbContext
```

## ⚠️ **Notas Importantes**

1. **Primera vez:** Como ya aplicaste la migración manualmente, el sistema detectará que ya hay datos
2. **Error "pending changes":** Usa `/apply-migration-cli` que funciona igual que el comando manual
3. **Desarrollo:** Los endpoints son perfectos para desarrollo iterativo
4. **Producción:** Para producción, sigue usando el proceso manual por seguridad

## 🎬 **Demo Rápida**

Ejecuta esto para ver que todo funciona:

```bash
# Usando cURL (desde PowerShell)
curl -X GET "http://localhost:5000/api/system/DbTools/detailed-status"
```

O desde el navegador:
```
http://localhost:5000/api/system/DbTools/detailed-status
```

---

**¡Ya tienes todo listo!** El proceso manual que acabas de hacer funcionó perfectamente, y ahora los endpoints automáticos están configurados para hacer exactamente lo mismo. 🎉
