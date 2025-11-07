# Sistema de Respuestas Estándar - AVASphere API

## Resumen

Este documento describe el sistema de respuestas estándar implementado en AVASphere API para garantizar consistencia en todas las respuestas de los endpoints.

## Formato de Respuesta Estándar

Todas las respuestas de la API siguen el siguiente formato JSON:

```json
{
  "success": true,
  "message": "Operation completed successfully",
  "data": {
    // Datos específicos del endpoint (puede ser null)
  },
  "statusCode": 200,
  "timestamp": "2024-11-01T10:30:00.000Z"
}
```

### Campos de la Respuesta

| Campo | Tipo | Descripción |
|-------|------|-------------|
| `success` | boolean | Indica si la operación fue exitosa |
| `message` | string | Mensaje descriptivo de la respuesta |
| `data` | T \| null | Datos de la respuesta (tipado según el endpoint) |
| `statusCode` | number | Código de estado HTTP |
| `timestamp` | string | Marca de tiempo ISO 8601 de la respuesta |

## Implementación para Desarrolladores

### 1. Usando las Extensiones de Controlador

Todas las extensiones están disponibles importando:
```csharp
using AVASphere.WebApi.Common.Extensions;
```

#### Respuestas Exitosas (200)
```csharp
// Con datos
return this.OkResponse(userData, "Usuario obtenido exitosamente");

// Sin datos específicos
return this.OkResponse(null, "Operación completada");
```

#### Recursos Creados (201)
```csharp
return this.CreatedResponse(
    createdUser, 
    nameof(GetUser), 
    new { id = createdUser.Id }, 
    "Usuario creado exitosamente"
);
```

#### Errores de Validación (400)
```csharp
return this.BadRequestResponse("Datos de entrada inválidos", validationErrors);
```

#### Recursos No Encontrados (404)
```csharp
return this.NotFoundResponse($"Usuario con ID {id} no encontrado");
```

#### Conflictos (409)
```csharp
return this.ConflictResponse("El nombre de usuario ya existe");
```

#### Errores del Servidor (500)
```csharp
return this.InternalServerErrorResponse("Error interno del servidor");
```

### 2. Usando el Controlador Base

Para funcionalidad adicional, herede de `BaseController`:

```csharp
public class MyController : BaseController
{
    public MyController(ILogger<MyController> logger) : base(logger)
    {
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<UserDto>>> GetUser(int id)
    {
        try
        {
            var user = await _userService.GetAsync(id);
            return Success(user, "Usuario encontrado exitosamente");
        }
        catch (Exception ex)
        {
            return HandleException(ex, "GetUser");
        }
    }
}
```

### 3. Validación Automática de ModelState

El filtro `ValidateModelStateFilter` se aplica globalmente y valida automáticamente el ModelState:

```csharp
// Si ModelState.IsValid == false, automáticamente devuelve:
{
  "success": false,
  "message": "Validation failed",
  "data": {
    "propertyName": ["Error message 1", "Error message 2"]
  },
  "statusCode": 400,
  "timestamp": "2024-11-01T10:30:00.000Z"
}
```

## Ejemplos de Respuestas

### Respuesta Exitosa con Datos
```json
{
  "success": true,
  "message": "Users retrieved successfully",
  "data": [
    {
      "id": 1,
      "userName": "admin",
      "name": "Administrator",
      "email": "admin@example.com"
    }
  ],
  "statusCode": 200,
  "timestamp": "2024-11-01T10:30:00.000Z"
}
```

### Respuesta de Error de Validación
```json
{
  "success": false,
  "message": "Validation failed",
  "data": {
    "userName": ["Username is required"],
    "email": ["Invalid email format"]
  },
  "statusCode": 400,
  "timestamp": "2024-11-01T10:30:00.000Z"
}
```

### Respuesta de Recurso No Encontrado
```json
{
  "success": false,
  "message": "User with ID 999 not found",
  "data": null,
  "statusCode": 404,
  "timestamp": "2024-11-01T10:30:00.000Z"
}
```

### Respuesta de Error del Servidor
```json
{
  "success": false,
  "message": "Internal server error",
  "data": null,
  "statusCode": 500,
  "timestamp": "2024-11-01T10:30:00.000Z"
}
```

## Middleware y Filtros

### GlobalExceptionMiddleware
Captura todas las excepciones no manejadas y las convierte en respuestas estándar:

- `KeyNotFoundException` → 404 Not Found
- `ArgumentException` → 400 Bad Request
- `InvalidOperationException` (duplicados) → 409 Conflict
- `UnauthorizedAccessException` → 401 Unauthorized
- Otras excepciones → 500 Internal Server Error

### ValidateModelStateFilter
Valida automáticamente el ModelState en todos los endpoints y devuelve respuestas estándar para errores de validación.

### StandardizeResponseFilter
Envuelve respuestas que no usan el formato estándar en objetos `ApiResponse`.

## Migración de Controladores Existentes

### Antes
```csharp
[HttpGet("{id}")]
public async Task<IActionResult> GetUser(int id)
{
    var user = await _service.GetAsync(id);
    if (user == null)
        return NotFound("User not found");
    
    return Ok(user);
}
```

### Después
```csharp
[HttpGet("{id}")]
public async Task<ActionResult<ApiResponse<UserDto>>> GetUser(int id)
{
    var user = await _service.GetAsync(id);
    if (user == null)
        return this.NotFoundResponse("User not found");
    
    return this.OkResponse(user, "User retrieved successfully");
}
```

## Beneficios

1. **Consistencia**: Todas las respuestas tienen el mismo formato
2. **Manejo de Errores**: Gestión centralizada de excepciones
3. **Validación Automática**: ModelState se valida automáticamente
4. **Documentación**: Swagger genera documentación consistente
5. **Facilidad de Uso**: Extensiones simples para usar
6. **Escalabilidad**: Fácil agregar nuevos tipos de respuesta

## Próximos Pasos Sugeridos

1. **Códigos de Error Personalizados**: Agregar códigos específicos para tipos de error
2. **Localización**: Soporte para mensajes en múltiples idiomas
3. **Paginación**: Estándar para respuestas paginadas
4. **Rate Limiting**: Información sobre límites de velocidad en las respuestas
5. **Correlación**: IDs de correlación para rastrear requests
