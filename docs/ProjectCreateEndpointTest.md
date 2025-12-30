# Prueba del Endpoint POST - Crear Proyecto

## Requisitos Previos
- Base de datos con al menos 1 ConfigSys
- Base de datos con al menos 1 Customer
- Base de datos con al menos 1 ProjectCategory

---

## Ejemplo 1: Crear Proyecto con 3 Categorías

### Request:
```http
POST https://localhost:5001/api/projects/Project/create
Content-Type: application/json

{
  "idConfigSys": 1,
  "idCustomer": 5,
  "idProjectCategories": [1, 2, 3]
}
```

### Expected Response (201 Created):
```json
{
  "success": true,
  "message": "Project created successfully",
  "data": {
    "idProject": 123,
    "idProjectQuote": 0,
    "idConfigSys": 1,
    "idCustomer": 5,
    "currentHito": 0,
    "writtenAddress": null,
    "exactAddress": null,
    "appointmentJson": null,
    "visitsJson": []
  },
  "statusCode": 201,
  "timestamp": "2025-12-23T10:30:00.000Z"
}
```

---

## Ejemplo 2: Error - Sin Categorías

### Request:
```http
POST https://localhost:5001/api/projects/Project/create
Content-Type: application/json

{
  "idConfigSys": 1,
  "idCustomer": 5,
  "idProjectCategories": []
}
```

### Expected Response (400 Bad Request):
```json
{
  "success": false,
  "message": "At least one project category is required",
  "data": null,
  "statusCode": 400,
  "timestamp": "2025-12-23T10:30:00.000Z"
}
```

---

## Ejemplo 3: Error - IdCustomer Inválido

### Request:
```http
POST https://localhost:5001/api/projects/Project/create
Content-Type: application/json

{
  "idConfigSys": 1,
  "idCustomer": 0,
  "idProjectCategories": [1, 2]
}
```

### Expected Response (400 Bad Request):
```json
{
  "success": false,
  "message": "IdCustomer must be greater than 0",
  "data": null,
  "statusCode": 400,
  "timestamp": "2025-12-23T10:30:00.000Z"
}
```

---

## Ejemplo 4: Error - Categorías Duplicadas

### Request:
```http
POST https://localhost:5001/api/projects/Project/create
Content-Type: application/json

{
  "idConfigSys": 1,
  "idCustomer": 5,
  "idProjectCategories": [1, 2, 1]
}
```

### Expected Response (400 Bad Request):
```json
{
  "success": false,
  "message": "Duplicate project category IDs are not allowed",
  "data": null,
  "statusCode": 400,
  "timestamp": "2025-12-23T10:30:00.000Z"
}
```

---

## Ejemplo 5: Error - Categoría No Existe

### Request:
```http
POST https://localhost:5001/api/projects/Project/create
Content-Type: application/json

{
  "idConfigSys": 1,
  "idCustomer": 5,
  "idProjectCategories": [1, 999]
}
```

### Expected Response (404 Not Found):
```json
{
  "success": false,
  "message": "Project category with Id 999 not found.",
  "data": null,
  "statusCode": 404,
  "timestamp": "2025-12-23T10:30:00.000Z"
}
```

---

## Prueba con cURL (Windows PowerShell)

### Crear Proyecto:
```powershell
$body = @{
    idConfigSys = 1
    idCustomer = 5
    idProjectCategories = @(1, 2, 3)
} | ConvertTo-Json

Invoke-RestMethod -Uri "https://localhost:5001/api/projects/Project/create" `
    -Method Post `
    -Body $body `
    -ContentType "application/json"
```

---

## Prueba con cURL (Linux/Mac)

```bash
curl -X POST https://localhost:5001/api/projects/Project/create \
  -H "Content-Type: application/json" \
  -d '{
    "idConfigSys": 1,
    "idCustomer": 5,
    "idProjectCategories": [1, 2, 3]
  }'
```

---

## Verificación en Base de Datos

Después de crear un proyecto, verificar:

### 1. Tabla Project:
```sql
SELECT * FROM Projects 
WHERE IdProject = [IdProyectoCreado]
ORDER BY IdProject DESC;
```

### 2. Tabla ListOfCategories:
```sql
SELECT * FROM ListOfCategories 
WHERE IdProject = [IdProyectoCreado];
```

### 3. Verificar Relaciones:
```sql
SELECT 
    p.IdProject,
    p.IdCustomer,
    c.Name as CustomerName,
    p.CurrentHito,
    lc.IdProjectCategory,
    pc.Name as CategoryName
FROM Projects p
INNER JOIN Customers c ON p.IdCustomer = c.IdCustomer
LEFT JOIN ListOfCategories lc ON p.IdProject = lc.IdProject
LEFT JOIN ProjectCategories pc ON lc.IdProjectCategory = pc.IdProjectCategory
WHERE p.IdProject = [IdProyectoCreado];
```

---

## Swagger UI

También puedes probar el endpoint desde Swagger UI:

1. Ejecutar el proyecto: `dotnet run`
2. Abrir navegador: `https://localhost:5001/swagger`
3. Buscar: **Projects - Project** → **POST /api/projects/Project/create**
4. Click en "Try it out"
5. Ingresar el JSON de prueba
6. Click en "Execute"

---

## Postman Collection

### Configuración:
- **Method**: POST
- **URL**: `{{baseUrl}}/api/projects/Project/create`
- **Headers**: `Content-Type: application/json`
- **Body** (raw JSON):
```json
{
  "idConfigSys": 1,
  "idCustomer": 5,
  "idProjectCategories": [1, 2, 3]
}
```

### Variables de Entorno:
```
baseUrl = https://localhost:5001
```

---

## Notas de Prueba

1. **Valores por Defecto**:
   - `IdProjectQuote` siempre será 0 al crear
   - `CurrentHito` siempre será 0 (Appointment)
   - `VisitsJson` siempre será array vacío

2. **Categorías**:
   - Se valida que todas existan antes de crear el proyecto
   - Si alguna no existe, el proyecto NO se crea
   - Se crea la relación en `ListOfCategories` automáticamente

3. **Transaccionalidad**:
   - Si falla la creación de categorías, el proyecto se revierte
   - Entity Framework Core maneja las transacciones

4. **Performance**:
   - Se hacen N consultas para validar N categorías
   - Se puede optimizar con un solo query si es necesario

