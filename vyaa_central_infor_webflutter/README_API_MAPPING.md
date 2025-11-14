# API Service con Mapeo de Modelos

## Descripción
El `ApiService` ha sido mejorado para permitir el mapeo automático de modelos tanto en las peticiones como en las respuestas. Esto proporciona flexibilidad para trabajar con objetos fuertemente tipados o con JSON dinámico según las necesidades del proyecto.

## Configuración de Endpoints

### Endpoint sin Mapeo (Comportamiento Original)
```dart
ApiEndpoint get login => const ApiEndpoint(
  path: '/api/common/Auth/login',
  method: HttpMethod.post,
  requiresAuth: false,
  useBody: true,
);
```

### Endpoint con Mapeo de Modelos
```dart
ApiEndpoint<AuthReq, LoginUserRes> get loginWithModel => ApiEndpoint<AuthReq, LoginUserRes>(
  path: '/api/common/Auth/login',
  method: HttpMethod.post,
  requiresAuth: false,
  useBody: true,
  requestMapper: (AuthReq model) => model.toJson(),
  responseMapper: (Map<String, dynamic> json) => LoginUserRes.fromJson(json),
);
```

## Uso del API Service

### 1. Con Mapeo Automático de Modelos
```dart
// Petición con modelo tipado
final authRequest = AuthReq(userName: 'usuario', password: 'password');

final response = await ApiService.requestWithModel<LoginUserRes, AuthReq>(
  ApiEndpoints.common.auth.loginWithModel,
  model: authRequest,
);

if (response.success) {
  LoginUserRes loginData = response.data!; // Objeto ya mapeado
  print('Usuario: ${loginData.user.name}');
  print('Token: ${loginData.token}');
}
```

### 2. Sin Mapeo (Método Original)
```dart
// Petición con JSON directo
final response = await ApiService.request(
  ApiEndpoints.common.auth.login,
  data: {
    'userName': 'usuario',
    'password': 'password',
  },
);

if (response.success) {
  Map<String, dynamic> data = response.data; // JSON dinámico
  final token = data['token'];
  final userName = data['user']['name'];
}
```

### 3. Con Modelo de Entrada pero sin Mapeo de Respuesta
```dart
final authRequest = AuthReq(userName: 'usuario', password: 'password');

final response = await ApiService.request(
  ApiEndpoints.common.auth.login,
  data: authRequest.toJson(), // Mapeo manual del modelo
);

if (response.success) {
  // Manejo manual del JSON de respuesta
  final loginData = LoginUserRes.fromJson(response.data);
}
```

## Ventajas del Nuevo Sistema

### Con Mapeo de Modelos:
- ✅ **Tipado fuerte**: Los objetos de respuesta están fuertemente tipados
- ✅ **Autocompletado**: Mejor experiencia de desarrollo con IntelliSense
- ✅ **Validación en tiempo de compilación**: Los errores se detectan antes
- ✅ **Mantenibilidad**: Cambios en la API se reflejan en los modelos
- ✅ **Reutilización**: Los mappers se definen una vez y se reusan

### Sin Mapeo (JSON Dinámico):
- ✅ **Flexibilidad**: Útil para APIs que cambian frecuentemente
- ✅ **Prototipado rápido**: No requiere crear modelos para pruebas
- ✅ **Retrocompatibilidad**: Mantiene el comportamiento original
- ✅ **APIs externas**: Ideal para APIs de terceros sin modelos definidos

## Migración

### Código Existente (No requiere cambios)
```dart
// Este código sigue funcionando igual
final response = await ApiService.request(endpoint, data: data);
```

### Código Nuevo (Opcional)
```dart
// Puedes migrar gradualmente a modelos tipados
final response = await ApiService.requestWithModel<ResponseModel, RequestModel>(
  endpoint,
  model: requestModel,
);
```

## Ejemplo Práctico: Controller de Login

```dart
class LoginController extends GetxController {
  final AuthService _authService = AuthService();
  
  // Método con mapeo automático
  Future<void> loginWithModel(String userName, String password) async {
    final request = AuthReq(userName: userName, password: password);
    
    final response = await _authService.loginWithModel(request);
    
    if (response.success) {
      // response.data ya es de tipo LoginUserRes
      final user = response.data!.user;
      Get.snackbar('Éxito', 'Bienvenido ${user.name}');
      Get.offNamed('/home');
    } else {
      Get.snackbar('Error', response.message ?? 'Error desconocido');
    }
  }
  
  // Método tradicional (para comparar)
  Future<void> loginTraditional(String userName, String password) async {
    final request = AuthReq(userName: userName, password: password);
    
    final response = await _authService.loginTraditional(request);
    
    if (response.success) {
      // response.data es String (token)
      Get.snackbar('Éxito', 'Login exitoso');
      Get.offNamed('/home');
    } else {
      Get.snackbar('Error', response.message ?? 'Error desconocido');
    }
  }
}
```

## Notas Importantes

1. **Compatibilidad**: El método `request()` original se mantiene sin cambios
2. **Flexibilidad**: Puedes usar ambos enfoques en el mismo proyecto
3. **Migración gradual**: No es necesario migrar todo el código de una vez
4. **Tipado opcional**: Los generics son opcionales, puedes usar `dynamic` si lo prefieres
5. **Manejo de errores**: Ambos métodos manejan errores de la misma manera

## Configuración de Nuevos Endpoints

Para crear un nuevo endpoint con mapeo de modelos:

```dart
class _UserController {
  const _UserController();
  
  // Endpoint con mapeo completo
  ApiEndpoint<CreateUserReq, UserRes> get createUser => ApiEndpoint<CreateUserReq, UserRes>(
    path: '/api/users',
    method: HttpMethod.post,
    requiresAuth: true,
    useBody: true,
    requestMapper: (CreateUserReq model) => model.toJson(),
    responseMapper: (Map<String, dynamic> json) => UserRes.fromJson(json),
  );
  
  // Endpoint solo con mapeo de respuesta
  ApiEndpoint<dynamic, List<UserRes>> get getUsers => ApiEndpoint<dynamic, List<UserRes>>(
    path: '/api/users',
    method: HttpMethod.get,
    requiresAuth: true,
    responseMapper: (Map<String, dynamic> json) => 
      (json['users'] as List).map((user) => UserRes.fromJson(user)).toList(),
  );
}
```