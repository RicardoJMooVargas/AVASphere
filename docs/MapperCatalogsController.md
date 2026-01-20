# Documentación de Endpoints - Catalogs Controller

## **Descripción General**
Controlador para la gestión de catálogos del sistema. Incluye Areas, Categorías de Proyecto, Propiedades y Valores de Propiedades.

---

## **AREAS - Gestión de Áreas**

### **POST /api/common/Catalogs/new-area**
Crea una nueva área en el sistema.

#### **Request Body:**
```json
{
  "name": "Área Comercial",
  "description": "Descripción del área comercial"
}
```

#### **Response 201 - Created:**
```json
{
  "success": true,
  "message": "Area created successfully",
  "data": {
    "idArea": 1,
    "name": "Área Comercial",
    "description": "Descripción del área comercial",
    "createdAt": "2026-01-02T10:30:00Z"
  },
  "statusCode": 201,
  "timestamp": "2026-01-02T10:30:00Z"
}
```

#### **Response 400 - Bad Request:**
```json
{
  "success": false,
  "message": "El nombre del área ya existe",
  "data": null,
  "statusCode": 400,
  "timestamp": "2026-01-02T10:30:00Z"
}
```

---

### **GET /api/common/Catalogs/get-areas**
Obtiene áreas del sistema con filtros opcionales.

#### **Query Parameters:**
- `id` (int, opcional): ID específico del área
- `name` (string, opcional): Nombre específico del área

#### **Ejemplos de uso:**
- `GET /api/common/Catalogs/get-areas` - Obtener todas las áreas
- `GET /api/common/Catalogs/get-areas?id=1` - Obtener área por ID
- `GET /api/common/Catalogs/get-areas?name=Comercial` - Obtener área por nombre

#### **Response 200 - Todas las áreas:**
```json
{
  "success": true,
  "message": "Areas retrieved successfully",
  "data": [
    {
      "idArea": 1,
      "name": "Área Comercial",
      "description": "Descripción del área comercial",
      "createdAt": "2026-01-02T10:30:00Z"
    },
    {
      "idArea": 2,
      "name": "Área Técnica",
      "description": "Descripción del área técnica", 
      "createdAt": "2026-01-02T11:00:00Z"
    }
  ],
  "statusCode": 200,
  "timestamp": "2026-01-02T12:00:00Z"
}
```

#### **Response 200 - Área específica:**
```json
{
  "success": true,
  "message": "Area retrieved successfully",
  "data": {
    "idArea": 1,
    "name": "Área Comercial",
    "description": "Descripción del área comercial",
    "createdAt": "2026-01-02T10:30:00Z"
  },
  "statusCode": 200,
  "timestamp": "2026-01-02T12:00:00Z"
}
```

#### **Response 404 - Not Found:**
```json
{
  "success": false,
  "message": "Area with ID 999 not found",
  "data": null,
  "statusCode": 404,
  "timestamp": "2026-01-02T12:00:00Z"
}
```

---

### **PUT /api/common/Catalogs/edit-areas/{id}**
Actualiza un área existente.

#### **Request Body:**
```json
{
  "name": "Área Comercial Actualizada",
  "description": "Nueva descripción del área comercial"
}
```

#### **Response 200 - OK:**
```json
{
  "success": true,
  "message": "Area updated successfully",
  "data": {
    "idArea": 1,
    "name": "Área Comercial Actualizada",
    "description": "Nueva descripción del área comercial",
    "createdAt": "2026-01-02T10:30:00Z",
    "updatedAt": "2026-01-02T14:30:00Z"
  },
  "statusCode": 200,
  "timestamp": "2026-01-02T14:30:00Z"
}
```

---

### **DELETE /api/common/Catalogs/delete-areas/{id}**
Elimina un área por ID.

#### **Response 200 - OK:**
```json
{
  "success": true,
  "message": "Area deleted successfully",
  "data": null,
  "statusCode": 200,
  "timestamp": "2026-01-02T15:00:00Z"
}
```

#### **Response 404 - Not Found:**
```json
{
  "success": false,
  "message": "Area with ID 999 not found",
  "data": null,
  "statusCode": 404,
  "timestamp": "2026-01-02T15:00:00Z"
}
```

---

## **PROJECT CATEGORIES - Gestión de Categorías de Proyecto**

### **POST /api/common/Catalogs/new-projectCategory**
Crea una nueva categoría de proyecto.

#### **Request Body:**
```json
{
  "name": "Cocinas",
  "description": "Proyectos de diseño de cocinas",
  "idArea": 1
}
```

#### **Response 201 - Created:**
```json
{
  "success": true,
  "message": "Project Category created successfully",
  "data": {
    "idProjectCategory": 1,
    "name": "Cocinas",
    "description": "Proyectos de diseño de cocinas",
    "idArea": 1,
    "areaName": "Área Comercial",
    "createdAt": "2026-01-02T10:30:00Z"
  },
  "statusCode": 201,
  "timestamp": "2026-01-02T10:30:00Z"
}
```

---

### **GET /api/common/Catalogs/get-projectCategory**
Obtiene categorías de proyecto con filtros opcionales.

#### **Query Parameters:**
- `id` (int, opcional): ID específico de la categoría
- `name` (string, opcional): Nombre específico de la categoría

#### **Ejemplos de uso:**
- `GET /api/common/Catalogs/get-projectCategory` - Obtener todas las categorías
- `GET /api/common/Catalogs/get-projectCategory?id=1` - Obtener categoría por ID
- `GET /api/common/Catalogs/get-projectCategory?name=Cocinas` - Obtener categoría por nombre

#### **Response 200 - Todas las categorías:**
```json
{
  "success": true,
  "message": "Project categories retrieved successfully",
  "data": [
    {
      "idProjectCategory": 1,
      "name": "Cocinas",
      "description": "Proyectos de diseño de cocinas",
      "idArea": 1,
      "areaName": "Área Comercial",
      "createdAt": "2026-01-02T10:30:00Z"
    },
    {
      "idProjectCategory": 2,
      "name": "Baños",
      "description": "Proyectos de diseño de baños",
      "idArea": 1,
      "areaName": "Área Comercial",
      "createdAt": "2026-01-02T11:00:00Z"
    }
  ],
  "statusCode": 200,
  "timestamp": "2026-01-02T12:00:00Z"
}
```

---

### **PUT /api/common/Catalogs/edit-projectCategory/{id}**
Actualiza una categoría de proyecto existente.

#### **Request Body:**
```json
{
  "name": "Cocinas Integrales",
  "description": "Proyectos de diseño de cocinas integrales",
  "idArea": 1
}
```

#### **Response 200 - OK:**
```json
{
  "success": true,
  "message": "Project Category updated successfully",
  "data": {
    "idProjectCategory": 1,
    "name": "Cocinas Integrales",
    "description": "Proyectos de diseño de cocinas integrales",
    "idArea": 1,
    "areaName": "Área Comercial",
    "createdAt": "2026-01-02T10:30:00Z",
    "updatedAt": "2026-01-02T14:30:00Z"
  },
  "statusCode": 200,
  "timestamp": "2026-01-02T14:30:00Z"
}
```

---

## **PROPERTIES - Gestión de Propiedades**

### **POST /api/common/Catalogs/new-property**
Crea una nueva propiedad.

#### **Request Body:**
```json
{
  "name": "Material",
  "description": "Tipo de material utilizado",
  "dataType": "string"
}
```

#### **Response 201 - Created:**
```json
{
  "success": true,
  "message": "Property created successfully",
  "data": {
    "idProperty": 1,
    "name": "Material",
    "description": "Tipo de material utilizado",
    "dataType": "string",
    "createdAt": "2026-01-02T10:30:00Z"
  },
  "statusCode": 201,
  "timestamp": "2026-01-02T10:30:00Z"
}
```

---

### **GET /api/common/Catalogs/get-property**
Obtiene propiedades con filtros opcionales.

#### **Query Parameters:**
- `id` (int, opcional): ID específico de la propiedad
- `name` (string, opcional): Nombre específico de la propiedad

#### **Ejemplos de uso:**
- `GET /api/common/Catalogs/get-property` - Obtener todas las propiedades
- `GET /api/common/Catalogs/get-property?id=1` - Obtener propiedad por ID
- `GET /api/common/Catalogs/get-property?name=Material` - Obtener propiedad por nombre

#### **Response 200 - Todas las propiedades:**
```json
{
  "success": true,
  "message": "Property retrieved successfully",
  "data": [
    {
      "idProperty": 1,
      "name": "Material",
      "description": "Tipo de material utilizado",
      "dataType": "string",
      "createdAt": "2026-01-02T10:30:00Z"
    },
    {
      "idProperty": 2,
      "name": "Color",
      "description": "Color del material",
      "dataType": "string",
      "createdAt": "2026-01-02T11:00:00Z"
    }
  ],
  "statusCode": 200,
  "timestamp": "2026-01-02T12:00:00Z"
}
```

---

### **PUT /api/common/Catalogs/edit-property/{id}**
Actualiza una propiedad existente.

#### **Request Body:**
```json
{
  "name": "Material Principal",
  "description": "Tipo de material principal utilizado",
  "dataType": "string"
}
```

#### **Response 200 - OK:**
```json
{
  "success": true,
  "message": "Property updated successfully",
  "data": {
    "idProperty": 1,
    "name": "Material Principal",
    "description": "Tipo de material principal utilizado",
    "dataType": "string",
    "createdAt": "2026-01-02T10:30:00Z",
    "updatedAt": "2026-01-02T14:30:00Z"
  },
  "statusCode": 200,
  "timestamp": "2026-01-02T14:30:00Z"
}
```

---

### **DELETE /api/common/Catalogs/delete-property/{id}**
Elimina una propiedad por ID.

#### **Response 200 - OK:**
```json
{
  "success": true,
  "message": "Property deleted successfully",
  "data": null,
  "statusCode": 200,
  "timestamp": "2026-01-02T15:00:00Z"
}
```

---

## **PROPERTY VALUES - Gestión de Valores de Propiedades**

### **GET /api/common/Catalogs/get-propertyValue**
Obtiene valores de propiedades con filtros opcionales.

#### **Query Parameters:**
- `idPropertyValue` (int, opcional): ID específico del valor de propiedad
- `value` (string, opcional): Valor específico a buscar
- `idPropertyOrName` (string, opcional): ID o nombre de la propiedad

#### **Ejemplos de uso:**
- `GET /api/common/Catalogs/get-propertyValue` - Obtener todos los valores
- `GET /api/common/Catalogs/get-propertyValue?idPropertyValue=1` - Obtener valor por ID
- `GET /api/common/Catalogs/get-propertyValue?value=Madera` - Buscar por valor
- `GET /api/common/Catalogs/get-propertyValue?idPropertyOrName=1` - Valores de propiedad con ID 1
- `GET /api/common/Catalogs/get-propertyValue?idPropertyOrName=Material` - Valores de propiedad "Material"

#### **Response 200 - OK:**
```json
{
  "success": true,
  "message": "Property values retrieved successfully",
  "data": [
    {
      "idPropertyValue": 1,
      "value": "Madera",
      "idProperty": 1,
      "propertyName": "Material",
      "createdAt": "2026-01-02T10:30:00Z"
    },
    {
      "idPropertyValue": 2,
      "value": "Metal",
      "idProperty": 1,
      "propertyName": "Material",
      "createdAt": "2026-01-02T11:00:00Z"
    }
  ],
  "statusCode": 200,
  "timestamp": "2026-01-02T12:00:00Z"
}
```

---

### **POST /api/common/Catalogs/new-propertyValue**
Crea un nuevo valor de propiedad.

#### **Request Body:**
```json
{
  "value": "Madera",
  "idProperty": 1
}
```

#### **Response 201 - Created:**
```json
{
  "success": true,
  "message": "Property value created successfully",
  "data": {
    "idPropertyValue": 1,
    "value": "Madera",
    "idProperty": 1,
    "propertyName": "Material",
    "createdAt": "2026-01-02T10:30:00Z"
  },
  "statusCode": 201,
  "timestamp": "2026-01-02T10:30:00Z"
}
```

#### **Response 404 - Property Not Found:**
```json
{
  "success": false,
  "message": "Property with ID 999 not found",
  "data": null,
  "statusCode": 404,
  "timestamp": "2026-01-02T10:30:00Z"
}
```

---

### **PUT /api/common/Catalogs/edit-propertyValue/{id}**
Actualiza un valor de propiedad existente.

#### **Request Body:**
```json
{
  "value": "Madera Roble",
  "idProperty": 1
}
```

#### **Response 200 - OK:**
```json
{
  "success": true,
  "message": "Property value updated successfully",
  "data": {
    "idPropertyValue": 1,
    "value": "Madera Roble",
    "idProperty": 1,
    "propertyName": "Material",
    "createdAt": "2026-01-02T10:30:00Z",
    "updatedAt": "2026-01-02T14:30:00Z"
  },
  "statusCode": 200,
  "timestamp": "2026-01-02T14:30:00Z"
}
```

---

### **DELETE /api/common/Catalogs/delete-propertyValue/{id}**
Elimina un valor de propiedad por ID.

#### **Response 200 - OK:**
```json
{
  "success": true,
  "message": "Property value deleted successfully",
  "data": null,
  "statusCode": 200,
  "timestamp": "2026-01-02T15:00:00Z"
}
```

#### **Response 400 - Business Rule Violation:**
```json
{
  "success": false,
  "message": "No se puede eliminar el valor porque está siendo utilizado en registros existentes",
  "data": null,
  "statusCode": 400,
  "timestamp": "2026-01-02T15:00:00Z"
}
```

---

### **DELETE /api/common/Catalogs/force-delete-propertyValue/{id}**
Fuerza la eliminación de un valor de propiedad, reemplazando referencias con un valor genérico.

#### **Response 200 - OK:**
```json
{
  "success": true,
  "message": "Property value force deleted successfully. Related records updated with generic value",
  "data": null,
  "statusCode": 200,
  "timestamp": "2026-01-02T15:00:00Z"
}
```

---

## **Códigos de Estado Comunes**

### **200 - OK**
Operación exitosa (GET, PUT, DELETE)

### **201 - Created** 
Recurso creado exitosamente (POST)

### **400 - Bad Request**
- Violación de reglas de negocio
- Datos de entrada inválidos
- Nombres duplicados

### **404 - Not Found**
- Recurso no encontrado por ID
- Recurso no encontrado por nombre
- Propiedad no encontrada al crear valor

### **500 - Internal Server Error**
Error interno del servidor

---

## **Estructura de Respuesta Estándar**

Todas las respuestas siguen el formato `ApiResponse`:

```json
{
  "success": boolean,
  "message": "string",
  "data": object | array | null,
  "statusCode": number,
  "timestamp": "ISO 8601 string"
}
```

---

## **Notas Importantes**

1. **Parámetros de Query**: Todos los endpoints GET admiten filtros opcionales. Si no se especifica ningún filtro, se devuelven todos los registros.

2. **Nombres Únicos**: Los nombres en Areas, Project Categories y Properties deben ser únicos.

3. **Eliminación Cascada**: 
   - Areas: No se puede eliminar si tiene categorías de proyecto asociadas
   - Properties: No se puede eliminar si tiene valores asociados
   - Property Values: Eliminación normal fallará si está en uso, usar force-delete para forzar

4. **Campos de Fecha**: Se devuelven en formato ISO 8601 (UTC)

5. **Validaciones**:
   - Names no pueden estar vacíos
   - IDs deben ser números positivos
   - Referencias foráneas deben existir

6. **Force Delete**: Solo disponible para Property Values, reemplaza referencias existentes con un valor genérico del sistema.
