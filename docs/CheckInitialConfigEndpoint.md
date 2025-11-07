# Endpoint de Verificación de Configuración Inicial - ConfigSys

## Descripción
Se ha agregado un nuevo endpoint al controlador `DbToolsController` que permite verificar si existe alguna configuración inicial (ConfigSys) en el sistema.

## Endpoint
```
GET api/system/dbtools/check-initial-config
```

## Funcionalidad
Este endpoint verifica la existencia de configuración inicial del sistema y devuelve:

### Respuesta cuando NO se puede conectar a la base de datos
```json
{
    "hasConfiguration": false,
    "tableExists": false,
    "requiresMigration": true,
    "message": "No se puede conectar a la base de datos",
    "data": null
}
```

### Respuesta cuando la tabla ConfigSys NO existe
```json
{
    "hasConfiguration": false,
    "tableExists": false,
    "requiresMigration": true,
    "message": "La tabla ConfigSys no existe. Se requiere ejecutar migraciones.",
    "data": null
}
```

### Respuesta cuando la tabla existe pero NO hay configuración
```json
{
    "hasConfiguration": false,
    "tableExists": true,
    "requiresMigration": false,
    "message": "La tabla ConfigSys existe pero no hay configuración inicial del sistema",
    "data": null
}
```

### Respuesta cuando SÍ existe configuración
```json
{
    "hasConfiguration": true,
    "tableExists": true,
    "requiresMigration": false,
    "message": "Configuración inicial encontrada y tabla ConfigSys existe",
    "data": {
        "idConfigSys": 1,
        "companyName": "Mi Empresa",
        "branchName": "Sucursal Principal",
        "logoUrl": "https://ejemplo.com/logo.png",
        "colorsCount": 5,
        "notUseModulesCount": 2,
        "createdAt": "2024-11-04T10:30:00Z"
    }
}
```

### Respuesta en caso de error
```json
{
    "hasConfiguration": false,
    "tableExists": false,
    "requiresMigration": true,
    "message": "Error al verificar la configuración inicial: [mensaje de error]",
    "data": null
}
```

## Campos de Respuesta
- **`hasConfiguration`**: Indica si existe configuración inicial en el sistema
- **`tableExists`**: Indica si la tabla ConfigSys existe en la base de datos
- **`requiresMigration`**: Indica si se requiere ejecutar migraciones (true cuando la tabla no existe)
- **`message`**: Mensaje descriptivo del estado actual
- **`data`**: Información resumida de la configuración (null si no existe)

## Casos de Uso
1. **Verificación de estado del sistema**: Permite saber si el sistema ya ha sido configurado inicialmente
2. **Validación de instalación**: Útil para verificar que la configuración básica existe
3. **Detección de migraciones pendientes**: Identifica si se requiere ejecutar migraciones de base de datos
4. **Debugging**: Ayuda a diagnosticar problemas relacionados con la configuración del sistema

## Diferencia con otros endpoints
- **`api/common/configsys/get`**: Devuelve la configuración completa con todos los detalles (404 si no existe)
- **`api/system/dbtools/check-initial-config`**: Devuelve un resumen de verificación con información básica (siempre 200 OK)

## Implementación
El endpoint utiliza el `MasterDbContext` directamente para evitar problemas de conexión cerrada que pueden ocurrir al usar múltiples servicios que manejan conexiones de base de datos.

**Servicios utilizados:**
- **`MasterDbContext`**: Para verificar conectividad y consultar la configuración directamente
- **Manejo de errores PostgreSQL**: Detecta específicamente errores de tabla inexistente

**Maneja cinco escenarios diferentes:**
1. **Sin conexión a BD**: No se puede conectar a la base de datos
2. **Tabla no existe**: La tabla ConfigSys no existe (requiere migración)
3. **Tabla vacía**: La tabla existe pero no hay configuración inicial
4. **Configuración existe**: Devuelve información resumida de la configuración
5. **Error en consulta**: Devuelve error con código 500

**Solución a problemas de conexión:**
- Evita el error "Cannot access a disposed object - NpgsqlConnection" usando una única instancia de DbContext
- Maneja específicamente códigos de error PostgreSQL para detectar tablas inexistentes

## Documentación API
El endpoint está documentado en Swagger bajo el grupo "System - Database Tools".
