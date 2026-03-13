# Documentación de Endpoints - ConfigSys Controller

## **Descripción General**
Controlador para la gestión de la configuración del sistema. Permite configurar datos de la empresa, colores del tema y módulos deshabilitados.

---

## **GET /api/common/ConfigSys/get**

### **Descripción**
Obtiene la configuración actual del sistema.

### **Respuestas**

#### **200 OK - Configuración encontrada**
```json
{
  "success": true,
  "message": "System configuration retrieved successfully",
  "data": {
    "idConfigSys": 1,
    "companyName": "AGUA",
    "branchName": "DOS",
    "logoUrl": "",
    "colors": [
      {
        "index": 0,
        "nameColor": "Primary",
        "colorCode": "#007bff",
        "colorRgb": "0, 123, 255"
      },
      {
        "index": 1,
        "nameColor": "Secondary",
        "colorCode": "#6c757d",
        "colorRgb": "108, 117, 125"
      }
    ],
    "notUseModules": [
      {
        "index": 0,
        "nameModule": "Inventory"
      }
    ],
    "createdAt": "2025-11-25T10:30:00Z"
  },
  "statusCode": 200,
  "timestamp": "2026-01-02T15:30:00Z"
}
```

#### **404 Not Found - Configuración no encontrada**
```json
{
  "success": false,
  "message": "System configuration not found",
  "data": null,
  "statusCode": 404,
  "timestamp": "2026-01-02T15:30:00Z"
}
```

#### **500 Internal Server Error**
```json
{
  "success": false,
  "message": "Internal server error",
  "data": null,
  "statusCode": 500,
  "timestamp": "2026-01-02T15:30:00Z"
}
```

---

## **POST /api/common/ConfigSys/save**

### **Descripción**
Guarda una nueva configuración del sistema (configuración inicial).

### **Request Body**
```json
{
  "companyName": "Mi Empresa S.A.",
  "branchName": "Sucursal Principal",
  "logoUrl": "https://example.com/logo.png",
  "colors": [
    {
      "index": 0,
      "nameColor": "Primary",
      "colorCode": "#007bff",
      "colorRgb": "0, 123, 255"
    },
    {
      "index": 1,
      "nameColor": "Secondary", 
      "colorCode": "#6c757d",
      "colorRgb": "108, 117, 125"
    }
  ],
  "notUseModules": [
    {
      "index": 0,
      "nameModule": "Inventory"
    },
    {
      "index": 1,
      "nameModule": "Reports"
    }
  ]
}
```

### **Validaciones**
- `companyName`: Requerido, máximo 200 caracteres
- `branchName`: Opcional, máximo 200 caracteres
- `logoUrl`: Opcional, máximo 500 caracteres
- `colors`: Lista opcional de colores del sistema
- `notUseModules`: Lista opcional de módulos deshabilitados

### **Respuesta exitosa - 200 OK**
```json
{
  "success": true,
  "message": "System configuration saved successfully",
  "data": {
    "idConfigSys": 1,
    "companyName": "Mi Empresa S.A.",
    "branchName": "Sucursal Principal",
    "logoUrl": "https://example.com/logo.png",
    "colors": [
      {
        "index": 0,
        "nameColor": "Primary",
        "colorCode": "#007bff",
        "colorRgb": "0, 123, 255"
      },
      {
        "index": 1,
        "nameColor": "Secondary",
        "colorCode": "#6c757d", 
        "colorRgb": "108, 117, 125"
      }
    ],
    "notUseModules": [
      {
        "index": 0,
        "nameModule": "Inventory"
      },
      {
        "index": 1,
        "nameModule": "Reports"
      }
    ],
    "createdAt": "2026-01-02T15:30:00Z"
  },
  "statusCode": 200,
  "timestamp": "2026-01-02T15:30:00Z"
}
```

#### **400 Bad Request - Datos inválidos**
```json
{
  "success": false,
  "message": "Configuration cannot be null",
  "data": null,
  "statusCode": 400,
  "timestamp": "2026-01-02T15:30:00Z"
}
```

#### **400 Bad Request - Validación de modelo**
```json
{
  "success": false,
  "message": "Invalid model state",
  "data": {
    "CompanyName": [
      "El nombre de la compañía es requerido"
    ]
  },
  "statusCode": 400,
  "timestamp": "2026-01-02T15:30:00Z"
}
```

#### **400 Bad Request - Regla de negocio**
```json
{
  "success": false,
  "message": "Configuration already exists. Use update endpoint instead",
  "data": null,
  "statusCode": 400,
  "timestamp": "2026-01-02T15:30:00Z"
}
```

---

## **PUT /api/common/ConfigSys/update**

### **Descripción**
Actualiza la configuración existente del sistema.

### **Request Body**
```json
{
  "companyName": "Mi Empresa Actualizada S.A.",
  "branchName": "Sucursal Central",
  "logoUrl": "https://example.com/new-logo.png",
  "colors": [
    {
      "index": 0,
      "nameColor": "Primary",
      "colorCode": "#28a745",
      "colorRgb": "40, 167, 69"
    },
    {
      "index": 1,
      "nameColor": "Secondary",
      "colorCode": "#dc3545",
      "colorRgb": "220, 53, 69"
    }
  ],
  "notUseModules": [
    {
      "index": 0,
      "nameModule": "Analytics"
    }
  ]
}
```

### **Validaciones**
Mismas validaciones que el endpoint de save.

### **Respuesta exitosa - 200 OK**
```json
{
  "success": true,
  "message": "System configuration updated successfully",
  "data": {
    "idConfigSys": 1,
    "companyName": "Mi Empresa Actualizada S.A.",
    "branchName": "Sucursal Central",
    "logoUrl": "https://example.com/new-logo.png",
    "colors": [
      {
        "index": 0,
        "nameColor": "Primary",
        "colorCode": "#28a745",
        "colorRgb": "40, 167, 69"
      },
      {
        "index": 1,
        "nameColor": "Secondary",
        "colorCode": "#dc3545",
        "colorRgb": "220, 53, 69"
      }
    ],
    "notUseModules": [
      {
        "index": 0,
        "nameModule": "Analytics"
      }
    ],
    "createdAt": "2025-11-25T10:30:00Z"
  },
  "statusCode": 200,
  "timestamp": "2026-01-02T15:30:00Z"
}
```

#### **Respuestas de error**
Mismas respuestas de error que el endpoint de save.

---

## **GET /api/common/ConfigSys/check-initial**

### **Descripción**
Verifica si existe configuración inicial del sistema y si las tablas de la base de datos están creadas.

### **Respuestas**

#### **200 OK - Sin configuración inicial**
```json
{
  "success": true,
  "message": "No data",
  "data": {
    "hasConfig": false,
    "tablesExist": true,
    "message": "No initial configuration found"
  },
  "statusCode": 200,
  "timestamp": "2026-01-02T15:30:00Z"
}
```

#### **200 OK - Con configuración inicial**
```json
{
  "success": true,
  "message": "Configuration found",
  "data": {
    "hasConfig": true,
    "tablesExist": true,
    "config": {
      "idConfigSys": 1,
      "companyName": "AGUA",
      "branchName": "DOS",
      "logoUrl": "",
      "colors": [
        {
          "index": 0,
          "nameColor": "Primary",
          "colorCode": "#007bff",
          "colorRgb": "0, 123, 255"
        }
      ],
      "notUseModules": [],
      "createdAt": "2025-11-25T10:30:00Z"
    }
  },
  "statusCode": 200,
  "timestamp": "2026-01-02T15:30:00Z"
}
```

#### **500 Internal Server Error**
```json
{
  "success": false,
  "message": "Error checking initial configuration: Database connection failed",
  "data": null,
  "statusCode": 500,
  "timestamp": "2026-01-02T15:30:00Z"
}
```

---

## **Modelos de Datos**

### **ConfigSysRequestDto** - Crear/Actualizar Configuración
```json
{
  "companyName": "string (requerido, máx 200 caracteres)",
  "branchName": "string (opcional, máx 200 caracteres)",
  "logoUrl": "string (opcional, máx 500 caracteres)",
  "colors": [
    {
      "index": "int (requerido)",
      "nameColor": "string (requerido, máx 100 caracteres)",
      "colorCode": "string (requerido, máx 20 caracteres)",
      "colorRgb": "string (opcional, máx 50 caracteres)"
    }
  ],
  "notUseModules": [
    {
      "index": "int (requerido)",
      "nameModule": "string (requerido, máx 100 caracteres)"
    }
  ]
}
```

### **ConfigSysResponseDto** - Respuesta de Configuración
```json
{
  "idConfigSys": "int",
  "companyName": "string",
  "branchName": "string",
  "logoUrl": "string",
  "colors": [
    {
      "index": "int",
      "nameColor": "string",
      "colorCode": "string",
      "colorRgb": "string"
    }
  ],
  "notUseModules": [
    {
      "index": "int",
      "nameModule": "string"
    }
  ],
  "createdAt": "ISO 8601 string"
}
```

### **ColorRequestDto/ColorResponseDto** - Colores del Sistema
```json
{
  "index": "int",
  "nameColor": "string",
  "colorCode": "string (formato hexadecimal ej: #007bff)",
  "colorRgb": "string (formato RGB ej: 0, 123, 255)"
}
```

### **NotUseModuleRequestDto/NotUseModuleResponseDto** - Módulos Deshabilitados
```json
{
  "index": "int",
  "nameModule": "string"
}
```

---

## **Códigos de Estado Comunes**

### **200 - OK**
Operación exitosa (GET, POST, PUT)

### **400 - Bad Request**
- Configuración nula
- Validación de modelo fallida
- Violación de reglas de negocio

### **404 - Not Found**
- Configuración del sistema no encontrada

### **500 - Internal Server Error**
Error interno del servidor

---

## **Estructura de Respuesta Estándar**

Todas las respuestas siguen el formato `ApiResponse`:

```json
{
  "success": "boolean",
  "message": "string",
  "data": "object | null",
  "statusCode": "number",
  "timestamp": "ISO 8601 string"
}
```

---

## **Notas Importantes**

1. **Configuración única**: El sistema solo permite una configuración activa. Use `save` para crear la inicial y `update` para modificarla.

2. **Validaciones de campo**:
   - `companyName`: Campo obligatorio
   - Límites de caracteres estrictos en todos los campos de texto
   - Colores deben seguir formato hexadecimal estándar

3. **Colores del sistema**: Se usan para personalizar la interfaz de usuario. Cada color tiene:
   - `index`: Orden/prioridad del color
   - `nameColor`: Nombre descriptivo (ej: "Primary", "Secondary")
   - `colorCode`: Código hexadecimal (ej: "#007bff")
   - `colorRgb`: Valores RGB opcionales

4. **Módulos deshabilitados**: Lista de módulos que no deben mostrarse en la interfaz:
   - `index`: Orden en la lista
   - `nameModule`: Nombre del módulo a deshabilitar

5. **Check inicial**: El endpoint `check-initial` es útil para:
   - Verificar si el sistema está configurado
   - Validar conectividad de base de datos
   - Configuración inicial de la aplicación

6. **Persistencia**: La configuración se guarda inmediatamente y afecta a todos los usuarios del sistema.

7. **Campos de fecha**: Se devuelven en formato ISO 8601 (UTC).

8. **Manejo de errores**: Errores de validación incluyen detalles específicos del campo problemático.
