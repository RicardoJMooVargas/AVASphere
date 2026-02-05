# BackupService - Sistema Generalizado de Backup y Restore

## Descripción
El `BackupService` es un servicio generalizado que permite hacer backup y restore de cualquier tabla de la base de datos usando consultas SQL, con reglas específicas de manejo de errores y compatibilidad.

## 🎯 Funcionalidades Principales

### 1. **Obtener Tablas Disponibles** (GET)
**Endpoint:** `GET /api/system/config/available-tables`

Obtiene una lista de todas las tablas disponibles en la base de datos.

**Response:**
```json
{
  "success": true,
  "data": {
    "tables": ["Property", "Supplier", "PropertyValue", "User", "..."],
    "totalCount": 25,
    "retrievedAt": "2026-02-04T10:30:00Z"
  },
  "message": "Se encontraron 25 tablas disponibles",
  "statusCode": 200
}
```

### 2. **Exportar Tablas** (Función Principal de Exportación)
**Endpoint:** `POST /api/system/config/export-tables`

Exporta tablas seleccionadas o todas las tablas como archivo SQL descargable.

**Request Body:**
```json
{
  "tableNames": ["Property", "Supplier", "PropertyValue"],
  "exportAllTables": false,
  "backupName": "MiBackup_2026_02_04",
  "description": "Backup de catálogos principales",
  "format": "SQL"
}
```

**Options:**
- `tableNames`: Lista de tablas específicas a exportar
- `exportAllTables`: Si es `true`, exporta todas las tablas (ignora `tableNames`)
- `backupName`: Nombre del archivo de backup
- `description`: Descripción opcional
- `format`: Formato del backup (actualmente solo "SQL")

**Response:** Archivo `.sql` descargable con todas las consultas INSERT

### 3. **Importar desde SQL** (Función Principal de Importación)
**Endpoint:** `POST /api/system/config/import-sql`

Importa datos desde un archivo SQL aplicando las reglas de negocio especificadas.

**Form Data:**
- `sqlFile`: Archivo SQL con las consultas INSERT
- `overwrite`: `true`/`false` - Permite sobrescribir datos existentes

**Response:**
```json
{
  "success": true,
  "data": {
    "success": true,
    "message": "Importación completada. Ejecutadas: 45, Omitidas: 3, Errores: 0",
    "overwriteMode": false,
    "executedStatements": ["Tabla 'Property': INSERT INTO...", "..."],
    "skippedStatements": ["Tabla 'OldTable' no existe en la base de datos"],
    "errors": [],
    "warnings": ["Tabla 'Property' omitida - columnas faltantes"],
    "processedAt": "2026-02-04T10:35:00Z"
  },
  "statusCode": 200
}
```

## 🔧 Reglas de Importación Implementadas

### **Regla 1: Tabla No Existe**
- **Condición:** La tabla existe en el archivo SQL pero no en la base de datos actual
- **Acción:** Se omite la importación de esa tabla
- **Resultado:** Se agrega a `skippedStatements` con mensaje explicativo

### **Regla 2: Columna No Existe**
- **Condición:** Una columna existe en el archivo SQL pero no en la tabla actual de la DB
- **Acción:** Se omite solo esa columna, se mantienen las columnas válidas
- **Resultado:** Se agrega warning y se ejecuta la declaración ajustada

### **Regla 3: Conflictos de Datos (Parámetro `overwrite`)**
- **`overwrite = false`:** Si hay conflicto por ID duplicado, el INSERT falla y se reporta error
- **`overwrite = true`:** Se convierte el INSERT a UPSERT (INSERT ... ON CONFLICT DO UPDATE)

## 🚀 Casos de Uso

### **Escenario 1: Backup Completo**
```json
{
  "exportAllTables": true,
  "backupName": "FullBackup_2026_02_04"
}
```

### **Escenario 2: Backup Selectivo**
```json
{
  "tableNames": ["Property", "PropertyValue", "Supplier"],
  "exportAllTables": false,
  "backupName": "Catalogs_Backup"
}
```

### **Escenario 3: Migración Entre Entornos**
1. Exportar desde entorno origen con `export-tables`
2. Importar en entorno destino con `import-sql` y `overwrite=false`
3. Revisar warnings y errores para ajustes necesarios

### **Escenario 4: Sincronización de Datos**
1. Exportar cambios específicos
2. Importar con `overwrite=true` para actualizar registros existentes

## ⚙️ Características Técnicas

### **Formato SQL Generado**
```sql
-- BACKUP DE TABLAS SELECCIONADAS
-- Generado el: 2026-02-04 10:30:00 UTC
-- Tablas solicitadas: Property, Supplier

-- TABLA: Property
INSERT INTO "Property"("IdProperty","Name","NormalizedName") VALUES(1,'Familia','Family');
INSERT INTO "Property"("IdProperty","Name","NormalizedName") VALUES(2,'Clase','Class');

-- TABLA: Supplier
INSERT INTO "Supplier"("IdSupplier","Name","RegistrationDate") VALUES(1,'LA VIGA EXTRUSIONES','2026-02-04');
```

### **Manejo de Tipos de Datos**
- **Strings:** Escapado automático de comillas simples
- **Fechas:** Formato ISO (yyyy-MM-dd HH:mm:ss)
- **Booleans:** `true`/`false` para PostgreSQL
- **NULL:** Se mantiene como `NULL`
- **Binarios:** Formato hexadecimal `\x...`

### **Transacciones**
- Todas las operaciones de importación usan transacciones
- Rollback automático si hay errores críticos
- Commit solo si toda la importación es exitosa

### **Logging Detallado**
- Log de inicio y fin de operaciones
- Warnings para tablas/columnas no encontradas
- Errores detallados con contexto

## 📋 Endpoint Legacy (Compatibilidad)

### **Cargar Catálogos por Defecto**
**Endpoint:** `POST /api/system/config/load-default-catalogs?overwrite=false`

Carga los catálogos por defecto proporcionados (Properties, Suppliers, PropertyValues).

**Response:** Igual formato que import-sql

## 🔍 Ejemplos de Respuestas

### **Importación Exitosa**
```json
{
  "success": true,
  "data": {
    "success": true,
    "message": "Importación completada. Ejecutadas: 150, Omitidas: 0, Errores: 0",
    "executedStatements": ["Tabla 'Property': INSERT INTO...", "..."],
    "skippedStatements": [],
    "errors": [],
    "warnings": []
  }
}
```

### **Importación con Advertencias**
```json
{
  "success": true,
  "data": {
    "success": true,
    "message": "Importación completada. Ejecutadas: 140, Omitidas: 10, Errores: 0",
    "warnings": [
      "Tabla 'OldTable' omitida - no existe en la DB actual",
      "Declaración ajustada para tabla 'Property' - columnas faltantes omitidas"
    ]
  }
}
```

### **Importación con Errores**
```json
{
  "success": false,
  "data": {
    "success": false,
    "message": "Error en importación: Violación de clave única",
    "errors": [
      "Error ejecutando declaración en tabla 'Property': duplicate key value violates unique constraint"
    ]
  }
}
```

## ✅ Ventajas del Nuevo Sistema

1. **Flexibilidad Total:** Backup/restore de cualquier tabla
2. **Reglas Inteligentes:** Manejo automático de incompatibilidades
3. **Transparencia:** Reportes detallados de cada operación
4. **Seguridad:** Transacciones y rollback automático
5. **Compatibilidad:** Funciona con estructuras de DB en evolución
6. **Simplicidad:** Archivos SQL estándar, fáciles de leer y editar

## 🔄 Migración desde CatalogBackupService

El nuevo `BackupService` reemplaza al `CatalogBackupService` anterior, ofreciendo:
- Mayor flexibilidad (cualquier tabla vs. solo catálogos)
- Mejor manejo de errores
- Reglas de negocio más robustas
- Formato SQL estándar vs. JSON propietario
