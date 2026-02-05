# Endpoints de Backup y Restore de Catálogos

## Descripción
Los endpoints de backup y restore permiten hacer copias de seguridad de los catálogos principales del sistema (Properties, Suppliers, PropertyValues) y restaurarlos posteriormente.

## Endpoints Disponibles

### 1. Crear Backup de Catálogos
**POST** `/api/system/config/backup-catalogs`

Crea un backup de los catálogos seleccionados.

**Request Body:**
```json
{
  "includeProperties": true,
  "includeSuppliers": true,
  "includePropertyValues": true,
  "backupName": "Backup_2026_02_04",
  "description": "Backup completo de catálogos del sistema"
}
```

**Response:**
```json
{
  "success": true,
  "message": "Backup de catálogos creado exitosamente. Total de registros: 150",
  "data": {
    "backupId": "uuid-here",
    "backupName": "Backup_2026_02_04",
    "createdAt": "2026-02-04T10:30:00Z",
    "description": "Backup completo de catálogos del sistema",
    "stats": {
      "propertiesCount": 3,
      "suppliersCount": 36,
      "propertyValuesCount": 111,
      "totalRecords": 150
    }
  },
  "statusCode": 200
}
```

### 2. Restaurar Catálogos
**POST** `/api/system/config/restore-catalogs`

Restaura catálogos desde un backup.

**Request Body:**
```json
{
  "data": {
    "properties": [...],
    "suppliers": [...],
    "propertyValues": [...]
  },
  "options": {
    "clearExistingData": false,
    "skipDuplicates": true,
    "updateExisting": false,
    "restoreProperties": true,
    "restoreSuppliers": true,
    "restorePropertyValues": true
  }
}
```

### 3. Exportar Catálogos a JSON
**POST** `/api/system/config/export-catalogs-json`

Exporta los catálogos a un archivo JSON descargable.

**Request Body:**
```json
{
  "includeProperties": true,
  "includeSuppliers": true,
  "includePropertyValues": true,
  "backupName": "Export_Catalogs",
  "description": "Exportación de catálogos"
}
```

**Response:** Archivo JSON descargable

### 4. Importar Catálogos desde JSON
**POST** `/api/system/config/import-catalogs-json`

Importa catálogos desde un archivo JSON.

**Form Data:**
- `jsonFile`: Archivo JSON con el backup
- `options` (opcional): JSON con opciones de restore

### 5. Cargar Catálogos por Defecto
**POST** `/api/system/config/load-default-catalogs`

Carga los catálogos por defecto proporcionados en el sistema.

**Response:**
```json
{
  "success": true,
  "message": "Catálogos por defecto cargados exitosamente...",
  "data": {
    "success": true,
    "stats": {
      "propertiesInserted": 3,
      "suppliersInserted": 36,
      "propertyValuesInserted": 108,
      "totalInserted": 147
    }
  },
  "statusCode": 200
}
```

## Opciones de Restore

### RestoreOptionsDto
- **`clearExistingData`**: Si es `true`, elimina todos los datos existentes antes de restaurar
- **`skipDuplicates`**: Si es `true`, omite registros que ya existen
- **`updateExisting`**: Si es `true`, actualiza registros existentes con los datos del backup
- **`restoreProperties`**: Incluir Properties en el restore
- **`restoreSuppliers`**: Incluir Suppliers en el restore  
- **`restorePropertyValues`**: Incluir PropertyValues en el restore

## Datos por Defecto Incluidos

### Properties
1. **Familia** (Family) - IdProperty: 1
2. **Clase** (Class) - IdProperty: 2  
3. **Línea** (Line) - IdProperty: 3

### Suppliers (36 proveedores)
Incluye todos los proveedores principales como:
- LA VIGA EXTRUSIONES
- LA VIGA CUPRUM
- LA VIGA INDALUM
- CASA FERNANDEZ DEL SURESTE, S.A. DE C.V.
- HERRALUM INDUSTRIAL, S.A. DE C.V.
- Y más...

### PropertyValues (108+ valores)
**Familias (IdProperty=1)**: 59 valores incluyendo ACRILICOS, ALUMINIO BEIGE Y ORO, etc.
**Clases (IdProperty=2)**: 35 valores incluyendo ACRILICOS, ANGULOS, BATIENTES, etc.  
**Líneas (IdProperty=3)**: 96 valores incluyendo ACRILICOS, ACRILICOS DECORADOS, ANGULOS, etc.

## Casos de Uso

### Escenario 1: Backup de Seguridad
1. Usar `backup-catalogs` para crear un backup antes de hacer cambios
2. Usar `export-catalogs-json` para descargar el backup como archivo

### Escenario 2: Migración de Datos
1. Usar `export-catalogs-json` en el sistema origen
2. Usar `import-catalogs-json` en el sistema destino

### Escenario 3: Restauración después de Error
1. Usar `restore-catalogs` con un backup previo
2. Configurar `clearExistingData: true` si se necesita restauración completa

### Escenario 4: Inicialización de Sistema Nuevo
1. Usar `load-default-catalogs` para cargar datos iniciales
2. Posteriormente importar datos específicos del cliente

## Notas Importantes

- Los backups incluyen integridad referencial (PropertyValues dependen de Properties)
- El sistema valida que las Properties existan antes de restaurar PropertyValues
- Se usa transacciones para garantizar consistencia de datos
- Los logs detallan el progreso y errores durante las operaciones
