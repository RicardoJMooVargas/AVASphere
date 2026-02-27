# 🚀 Configuración de Migración "Initial" para AVASphere

## ⚠️ IMPORTANTE: Nombre de Migración Obligatorio

**La migración principal del sistema DEBE llamarse "Initial"** para garantizar el funcionamiento correcto en instalaciones nuevas.

## 🎯 Endpoints Principales para Instalación

### 1. Migración Completa Automática (Recomendado)

```http
POST /api/system/DbTools/full-migration?name=Initial
```

**¿Qué hace?**
1. ✅ Verifica conexión a la base de datos
2. 🗑️ Elimina tablas existentes (si las hay)
3. 🗑️ Elimina archivos de migración antiguos
4. 📝 Crea nueva migración llamada "Initial"
5. ⚙️ Aplica la migración a la base de datos
6. ✅ Verifica estado final

**Respuesta exitosa:**
```json
{
  "success": true,
  "data": {
    "result": "✅ Conexión verificada: Conexión OK...\n🗑️ 3 archivos de migración eliminados exitosamente\n📝 Creando nueva migración: Initial...\n✅ Migración creada exitosamente.\n⚙️ Aplicando migración a la base de datos...\n✅ Migraciones aplicadas correctamente.\n✅ Estado final: Conexión OK. Existen tablas en la base de datos."
  },
  "message": "Proceso completo de migración ejecutado exitosamente",
  "statusCode": 200
}
```

### 2. Endpoint Específico para Instalación

```http
POST /api/system/Config/prepare-initial-migration
```

**Características especiales:**
- ✅ Siempre usa "Initial" como nombre (no permite otro nombre)
- ✅ Optimizado para instalaciones nuevas del sistema
- ✅ Incluye validaciones adicionales para configuración inicial

## 🔧 Endpoints de Soporte

### Verificar Estado Actual
```http
GET /api/system/DbTools/check
```

### Verificar Migraciones Existentes
```http
GET /api/system/DbTools/check-migrations
```

### Eliminar Migraciones Antiguas
```http
DELETE /api/system/DbTools/delete-migrations
```

### Crear Migración Manual
```http
POST /api/system/DbTools/create-migration?name=Initial
```

### Aplicar Migración
```http
POST /api/system/DbTools/apply-migration
```

## 🏗️ Flujo Completo de Instalación

### Para Instalación Nueva:

1. **Ejecutar migración inicial:**
   ```http
   POST /api/system/Config/prepare-initial-migration
   ```

2. **Verificar configuración del sistema:**
   ```http
   GET /api/system/Config/check-initial-config
   ```

3. **Si no hay configuración, crear sistema:**
   ```http
   POST /api/system/Config/configure-system
   ```

4. **Crear usuario administrador:**
   ```http
   POST /api/system/Config/configure-admin
   ```

### Para Desarrollo (Reset Completo):

1. **Recrear base de datos:**
   ```http
   POST /api/system/DbTools/full-migration?name=Initial
   ```

2. **Cargar datos de catálogos por defecto:**
   ```http
   POST /api/system/Config/load-default-catalogs
   ```

## 🎯 Validación del Proceso

Después de ejecutar la migración "Initial", puedes verificar el estado con:

```http
GET /api/system/DbTools/check
```

**Respuesta esperada:**
```json
{
  "success": true,
  "data": {
    "isConnected": true,
    "hasData": true
  },
  "message": "Conexión OK. Base de datos válida con X tablas y N registro(s) en ConfigSys.",
  "statusCode": 200
}
```

## 📂 Ubicación de Archivos

- **Migraciones:** `/src/AVASphere.Infrastructure/System/Migrations/`
- **Archivo principal:** `YYYYMMDDHHMMSS_Initial.cs`
- **Snapshot:** `MasterDbContextModelSnapshot.cs`

## 🚨 Solución de Problemas

### Error: "Column does not exist"
Si aparecen errores como `column s.AreaIdArea does not exist`, ejecuta:

```http
POST /api/system/Tools/cleanup-duplicate-fields
```

Luego vuelve a ejecutar la migración inicial.

### Error: "Build failed"
1. Ejecuta `dotnet build` en la terminal para ver errores específicos
2. Corrige errores de compilación
3. Vuelve a ejecutar la migración

### Error: "Migration already exists"
1. Elimina migraciones existentes:
   ```http
   DELETE /api/system/DbTools/delete-migrations
   ```
2. Vuelve a crear la migración "Initial"

## ✅ Verificación Final

Una instalación exitosa debe tener:
- ✅ Migración "Initial" en directorio de migraciones
- ✅ Base de datos con todas las tablas creadas
- ✅ Tabla ConfigSys existente (puede estar vacía inicialmente)
- ✅ Sin errores de conexión o migración

---

**💡 Tip:** Para instalaciones de producción, usa siempre el endpoint `/api/system/Config/prepare-initial-migration` que garantiza el uso del nombre "Initial".
