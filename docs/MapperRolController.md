# Documentación de Endpoints - Rol Controller

## **Descripción General**
Controlador para la gestión de roles del sistema. Los roles definen permisos y módulos accesibles para los usuarios.

---

## **GET /api/common/Rol/get**

### **Descripción**
Obtiene roles del sistema con filtros opcionales. Permite buscar por ID, nombre o listar todos los roles.

### **Query Parameters**
- `id` (int, opcional): ID específico del rol
- `name` (string, opcional): Nombre específico del rol

**Nota:** Si no se especifica ningún parámetro, devuelve todos los roles.

### **Ejemplos de uso**
- `GET /api/common/Rol/get` - Obtener todos los roles
- `GET /api/common/Rol/get?id=1` - Obtener rol por ID
- `GET /api/common/Rol/get?name=Administrador` - Obtener rol por nombre

### **Respuestas**

#### **200 OK - Todos los roles**
```json
{
  "success": true,
  "message": "Roles retrieved successfully",
  "data": [
    {
      "idRol": 1,
      "name": "Administrador",
      "normalizedName": "admin",
      "idArea": 1,
      "areaName": "Área Administrativa",
      "userCount": 3,
      "permissions": [
        {
          "index": 0,
          "name": "Acceso Total",
          "normalized": "FULL_ACCESS",
          "type": "SUPER_ADMIN",
          "status": "active"
        }
      ],
      "modules": [
        {
          "index": 999,
          "name": "Admin",
          "normalized": "/"
        }
      ]
    },
    {
      "idRol": 2,
      "name": "Ventas",
      "normalizedName": "sales",
      "idArea": 2,
      "areaName": "Área Comercial",
      "userCount": 5,
      "permissions": [
        {
          "index": 0,
          "name": "Gestión de Ventas",
          "normalized": "SALES_MANAGEMENT",
          "type": "SALES",
          "status": "active"
        }
      ],
      "modules": [
        {
          "index": 0,
          "name": "Ventas",
          "normalized": "/main-sales"
        }
      ]
    }
  ],
  "statusCode": 200,
  "timestamp": "2026-01-02T15:30:00Z"
}
```

#### **200 OK - Rol específico**
```json
{
  "success": true,
  "message": "Rol retrieved successfully",
  "data": {
    "idRol": 1,
    "name": "Administrador",
    "normalizedName": "admin",
    "idArea": 1,
    "areaName": "Área Administrativa",
    "userCount": 3,
    "permissions": [
      {
        "index": 0,
        "name": "Acceso Total",
        "normalized": "FULL_ACCESS",
        "type": "SUPER_ADMIN",
        "status": "active"
      }
    ],
    "modules": [
      {
        "index": 999,
        "name": "Admin",
        "normalized": "/"
      }
    ]
  },
  "statusCode": 200,
  "timestamp": "2026-01-02T15:30:00Z"
}
```

#### **404 Not Found**
```json
{
  "success": false,
  "message": "Rol with ID 999 not found",
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

## **POST /api/common/Rol/new**

### **Descripción**
Crea un nuevo rol en el sistema con permisos y módulos asignados.

### **Request Body**
```json
{
  "name": "Supervisor",
  "normalizedName": "supervisor",
  "idArea": 1,
  "permissions": [
    {
      "index": 0,
      "name": "Gestión de Equipos",
      "normalized": "TEAM_MANAGEMENT",
      "type": "MANAGEMENT",
      "status": "active"
    }
  ],
  "modules": [
    {
      "index": 0,
      "name": "Supervisión",
      "normalized": "/supervision"
    }
  ]
}
```

### **Validaciones**
- `name`: Requerido, debe ser único
- `idArea`: Requerido, debe existir en el sistema
- `permissions`: Opcional, lista de permisos del rol
- `modules`: Opcional, lista de módulos accesibles

### **Respuesta exitosa - 201 Created**
```json
{
  "success": true,
  "message": "Rol created successfully",
  "data": {
    "idRol": 3,
    "name": "Supervisor",
    "normalizedName": "supervisor",
    "idArea": 1,
    "areaName": "Área Administrativa",
    "userCount": 0,
    "permissions": [
      {
        "index": 0,
        "name": "Gestión de Equipos",
        "normalized": "TEAM_MANAGEMENT",
        "type": "MANAGEMENT",
        "status": "active"
      }
    ],
    "modules": [
      {
        "index": 0,
        "name": "Supervisión",
        "normalized": "/supervision"
      }
    ]
  },
  "statusCode": 201,
  "timestamp": "2026-01-02T15:30:00Z"
}
```

#### **400 Bad Request - Área no encontrada**
```json
{
  "success": false,
  "message": "Area with ID 999 not found",
  "data": null,
  "statusCode": 400,
  "timestamp": "2026-01-02T15:30:00Z"
}
```

#### **400 Bad Request - Nombre duplicado**
```json
{
  "success": false,
  "message": "A rol with the name 'Administrador' already exists",
  "data": null,
  "statusCode": 400,
  "timestamp": "2026-01-02T15:30:00Z"
}
```

---

## **PUT /api/common/Rol/edit/{id}**

### **Descripción**
Actualiza un rol existente en el sistema.

### **Request Body**
```json
{
  "name": "Supervisor Actualizado",
  "normalizedName": "supervisor_updated",
  "idArea": 2,
  "permissions": [
    {
      "index": 0,
      "name": "Gestión de Equipos Avanzada",
      "normalized": "ADVANCED_TEAM_MANAGEMENT",
      "type": "MANAGEMENT",
      "status": "active"
    }
  ],
  "modules": [
    {
      "index": 0,
      "name": "Supervisión Avanzada",
      "normalized": "/advanced-supervision"
    }
  ]
}
```

### **Validaciones**
- El rol debe existir
- `name`: Si se cambia, debe ser único
- `idArea`: Debe existir en el sistema
- No se puede actualizar si tiene usuarios asignados (según reglas de negocio)

### **Respuesta exitosa - 200 OK**
```json
{
  "success": true,
  "message": "Rol updated successfully",
  "data": {
    "idRol": 3,
    "name": "Supervisor Actualizado",
    "normalizedName": "supervisor_updated",
    "idArea": 2,
    "areaName": "Área Comercial",
    "userCount": 0,
    "permissions": [
      {
        "index": 0,
        "name": "Gestión de Equipos Avanzada",
        "normalized": "ADVANCED_TEAM_MANAGEMENT",
        "type": "MANAGEMENT",
        "status": "active"
      }
    ],
    "modules": [
      {
        "index": 0,
        "name": "Supervisión Avanzada",
        "normalized": "/advanced-supervision"
      }
    ]
  },
  "statusCode": 200,
  "timestamp": "2026-01-02T15:30:00Z"
}
```

#### **404 Not Found - Rol no existe**
```json
{
  "success": false,
  "message": "Rol with ID 999 not found",
  "data": null,
  "statusCode": 404,
  "timestamp": "2026-01-02T15:30:00Z"
}
```

#### **400 Bad Request - Regla de negocio**
```json
{
  "success": false,
  "message": "Cannot update rol because it has active users assigned",
  "data": null,
  "statusCode": 400,
  "timestamp": "2026-01-02T15:30:00Z"
}
```

---

## **DELETE /api/common/Rol/{id}**

### **Descripción**
Elimina un rol del sistema por su ID.

### **Validaciones**
- El rol debe existir
- No debe tener usuarios asignados
- No puede ser un rol del sistema (ej: Administrador predeterminado)

### **Respuesta exitosa - 200 OK**
```json
{
  "success": true,
  "message": "Rol deleted successfully",
  "data": null,
  "statusCode": 200,
  "timestamp": "2026-01-02T15:30:00Z"
}
```

#### **404 Not Found**
```json
{
  "success": false,
  "message": "Rol with ID 999 not found",
  "data": null,
  "statusCode": 404,
  "timestamp": "2026-01-02T15:30:00Z"
}
```

#### **400 Bad Request - Tiene usuarios asignados**
```json
{
  "success": false,
  "message": "Cannot delete rol because it has active users assigned",
  "data": null,
  "statusCode": 400,
  "timestamp": "2026-01-02T15:30:00Z"
}
```

#### **400 Bad Request - Rol del sistema**
```json
{
  "success": false,
  "message": "Cannot delete system default rol",
  "data": null,
  "statusCode": 400,
  "timestamp": "2026-01-02T15:30:00Z"
}
```

---

## **Modelos de Datos**

### **RolRequestDto** - Crear/Actualizar Rol
```json
{
  "name": "string",
  "normalizedName": "string (opcional)",
  "idArea": "int",
  "permissions": [
    {
      "index": "int",
      "name": "string",
      "normalized": "string",
      "type": "string",
      "status": "string"
    }
  ],
  "modules": [
    {
      "index": "int", 
      "name": "string",
      "normalized": "string"
    }
  ]
}
```

### **RolResponseDto** - Respuesta de Rol
```json
{
  "idRol": "int",
  "name": "string",
  "normalizedName": "string",
  "idArea": "int",
  "areaName": "string",
  "userCount": "int",
  "permissions": [
    {
      "index": "int",
      "name": "string",
      "normalized": "string",
      "type": "string",
      "status": "string"
    }
  ],
  "modules": [
    {
      "index": "int",
      "name": "string", 
      "normalized": "string"
    }
  ]
}
```

### **Permission** - Permisos del Rol
```json
{
  "index": "int",
  "name": "string",
  "normalized": "string", 
  "type": "string",
  "status": "string"
}
```

### **Module** - Módulos del Rol
```json
{
  "index": "int",
  "name": "string",
  "normalized": "string"
}
```

---

## **Códigos de Estado Comunes**

### **200 - OK**
Operación exitosa (GET, PUT, DELETE)

### **201 - Created**
Rol creado exitosamente (POST)

### **400 - Bad Request**
- Violación de reglas de negocio
- Área no encontrada
- Nombre duplicado
- Rol tiene usuarios asignados

### **404 - Not Found**
- Rol no encontrado por ID
- Rol no encontrado por nombre

### **500 - Internal Server Error**
Error interno del servidor

---

## **Estructura de Respuesta Estándar**

Todas las respuestas siguen el formato `ApiResponse`:

```json
{
  "success": "boolean",
  "message": "string",
  "data": "object | array | null",
  "statusCode": "number",
  "timestamp": "ISO 8601 string"
}
```

---

## **Notas Importantes**

1. **Búsqueda de roles**: Solo se puede usar un parámetro de búsqueda a la vez (`id` O `name`, no ambos).

2. **Nombres únicos**: Los nombres de rol deben ser únicos en todo el sistema.

3. **Dependencias**: Los roles dependen de áreas existentes. No se puede crear un rol sin área válida.

4. **Eliminación**: No se puede eliminar un rol que:
   - Tenga usuarios asignados
   - Sea un rol del sistema predeterminado

5. **Permisos y módulos**: Se almacenan como JSON arrays en la base de datos.

6. **UserCount**: El campo indica cuántos usuarios activos tienen asignado este rol.

7. **Normalized names**: Se usan para búsquedas y referencias internas del sistema.

8. **Campos de fecha**: Se devuelven en formato ISO 8601 (UTC).

9. **Áreas**: Los roles están vinculados a áreas específicas para organización departamental.
