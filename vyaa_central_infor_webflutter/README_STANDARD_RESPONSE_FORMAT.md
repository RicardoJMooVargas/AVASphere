# Manejo del Formato Estándar de Respuestas API

## Formato de Respuesta Estándar

Todas las respuestas de la API siguen este formato:

```json
{
  "success": true,
  "message": "Operation completed successfully", 
  "data": {
    // Datos específicos del endpoint (puede ser null)
  }
}
```

### Casos de Respuesta:

#### ✅ **Éxito con Datos**
```json
{
  "success": true,
  "message": "User retrieved successfully",
  "data": {
    "id": 1,
    "name": "John Doe",
    "email": "john@example.com"
  }
}
```

#### ✅ **Éxito sin Datos**
```json
{
  "success": true,
  "message": "User deleted successfully",
  "data": null
}
```

#### ❌ **Error de Negocio (HTTP 200 pero success: false)**
```json
{
  "success": false,
  "message": "User not found",
  "data": null
}
```

#### ❌ **Error HTTP (401, 404, 500, etc.)**
```json
{
  "success": false,
  "message": "Unauthorized access",
  "data": null
}
```

## Funcionamiento del ApiService

### 1. **Detección Automática del Formato**
El `ApiService` detecta automáticamente si la respuesta tiene el formato estándar:
- Si tiene `success`, `message` y `data` → Procesa como formato estándar
- Si no → Mantiene compatibilidad con APIs legacy

### 2. **Extracción de Datos**
```dart
// Método interno que extrae el campo "data"
static dynamic _extractDataFromStandardResponse(dynamic responseBody) {
  if (responseBody is Map<String, dynamic>) {
    if (responseBody.containsKey('success') && responseBody.containsKey('data')) {
      return responseBody['data']; // ← Solo retorna el contenido de "data"
    }
  }
  return responseBody; // Compatibilidad con formato legacy
}
```

### 3. **Manejo de Errores de Negocio**
```dart
// Si success = false, se trata como error aunque HTTP sea 200
if (decoded['success'] == false) {
  final errorMessage = decoded['message'] ?? 'Error en la operación';
  return ApiResponse.error(errorMessage);
}
```

## Ejemplos de Uso

### **Con Mapeo de Modelos**
```dart
// Endpoint configurado con mapper
ApiEndpoint<AuthReq, LoginUserRes> get loginWithModel => ApiEndpoint<AuthReq, LoginUserRes>(
  path: '/api/auth/login',
  method: HttpMethod.post,
  requestMapper: (AuthReq model) => model.toJson(),
  responseMapper: (Map<String, dynamic> json) => LoginUserRes.fromJson(json), // ← Recibe solo "data"
);

// Uso en el servicio
final response = await ApiService.requestWithModel<LoginUserRes, AuthReq>(
  ApiEndpoints.common.auth.loginWithModel,
  model: authRequest,
);

if (response.success) {
  LoginUserRes user = response.data!; // ← Objeto ya mapeado desde "data"
}
```

### **Sin Mapeo (JSON Dinámico)**
```dart
// Endpoint sin mapper
final response = await ApiService.request(
  ApiEndpoints.common.auth.login,
  data: {'username': 'user', 'password': 'pass'},
);

if (response.success) {
  // response.data contiene solo el contenido del campo "data" de la respuesta
  final userData = response.data; // ← JSON extraído automáticamente
  final userId = userData['id'];
  final userName = userData['name'];
}
```

## Comparación: Antes vs Ahora

### **Antes (Sin Formato Estándar)**
```dart
// Respuesta cruda del servidor
final response = await ApiService.request(endpoint);
if (response.success) {
  final fullResponse = response.data; // Toda la respuesta
  final actualData = fullResponse['data']; // Extracción manual
  final user = User.fromJson(actualData); // Mapeo manual
}
```

### **Ahora (Con Formato Estándar)**
```dart
// Extracción automática del campo "data"
final response = await ApiService.requestWithModel<User, LoginReq>(endpoint, model: loginReq);
if (response.success) {
  final user = response.data!; // ← Directamente el objeto User mapeado
}
```

## Ventajas del Sistema

### ✅ **Simplicidad**
- El desarrollador siempre recibe directamente los datos útiles
- No necesita extraer manualmente el campo `data`
- Mapeo automático si se proporciona mapper

### ✅ **Consistencia**
- Todas las respuestas siguen el mismo patrón
- Manejo uniforme de errores de negocio vs HTTP
- Mensajes de error claros desde el backend

### ✅ **Compatibilidad**
- Funciona con APIs que usen el formato estándar
- Mantiene compatibilidad con APIs legacy
- Migración gradual sin romper código existente

### ✅ **Robustez**
- Detecta errores de negocio (success: false) aunque HTTP sea 200
- Manejo automático de casos edge
- Logging y debug integrados

## Casos de Uso Reales

### **Login Exitoso**
```json
// Respuesta del servidor
{
  "success": true,
  "message": "Login successful",
  "data": {
    "token": "jwt_token_here",
    "user": { "id": 1, "name": "John" },
    "configSys": { "theme": "dark" }
  }
}

// Lo que recibe el desarrollador en response.data
{
  "token": "jwt_token_here", 
  "user": { "id": 1, "name": "John" },
  "configSys": { "theme": "dark" }
}
```

### **Error de Credenciales**
```json
// Respuesta del servidor (HTTP 200)
{
  "success": false,
  "message": "Invalid credentials",
  "data": null
}

// Lo que recibe el desarrollador
ApiResponse.error("Invalid credentials")
```

### **Lista de Usuarios**
```json
// Respuesta del servidor
{
  "success": true,
  "message": "Users retrieved successfully",
  "data": [
    { "id": 1, "name": "John" },
    { "id": 2, "name": "Jane" }
  ]
}

// Con mapper: List<User>
// Sin mapper: List<Map<String, dynamic>>
```

## Migración

### **Código Existente (No Cambios)**
```dart
// Este código sigue funcionando igual
final response = await ApiService.request(endpoint, data: requestData);
// Ahora response.data ya contiene solo el contenido útil (campo "data")
```

### **Código Nuevo (Recomendado)**
```dart
// Usar mappers para tipado fuerte
final response = await ApiService.requestWithModel<ResponseModel, RequestModel>(
  endpoint,
  model: requestModel,
);
```