# Documentación de Endpoints - Users Controller

## **Descripción General**
Controlador para la gestión de usuarios del sistema. Permite obtener, crear y actualizar usuarios, así como configurar el usuario administrador inicial.

---

## **GET /api/common/Users**

### **Descripción**
Obtiene una lista de usuarios del sistema. Permite obtener todos los usuarios o buscar por criterios específicos.

### **Parámetros de consulta**
| Parámetro | Tipo | Requerido | Descripción |
|-----------|------|-----------|-------------|
| `idUsers` | `int?` | No | ID específico del usuario a buscar |
| `userName` | `string?` | No | Nombre de usuario específico a buscar |

**Nota:** Solo se puede usar un parámetro a la vez. Si no se especifica ninguno, devuelve todos los usuarios.

### **Ejemplos de uso**

#### 1. Obtener todos los usuarios
```
GET /api/common/Users
```

#### 2. Buscar por ID de usuario
```
GET /api/common/Users?idUsers=1
```

#### 3. Buscar por nombre de usuario
```
GET /api/common/Users?userName=Ricardo
```

### **Respuestas**

#### **200 OK - Todos los usuarios**
```json
{
  "success": true,
  "message": "Usuarios obtenidos exitosamente",
  "data": [
    {
      "idUsers": 4,
      "userName": "VentasLocales",
      "name": "Ventas",
      "lastName": "Locales",
      "status": "Active",
      "aux": "014 CANDY",
      "createAt": "2025-12-26",
      "verified": "False",
      "idRols": 2,
      "rolName": "Ventas",
      "idConfigSys": 1,
      "configSysName": "AGUA",
      "companyName": "AGUA",
      "branchName": "DOS",
      "logoUrl": "",
      "modules": [
        {
          "index": 0,
          "name": "Ventas",
          "normalized": "/main-sales"
        }
      ],
      "permissions": [
        {
          "index": 0,
          "name": "Gestión de Ventas",
          "normalized": "SALES_MANAGEMENT",
          "type": "SALES",
          "status": "active"
        }
      ]
    },
    {
      "idUsers": 3,
      "userName": "Ricardo",
      "name": "Ricardo J",
      "lastName": "Moo Vargas",
      "status": "Active",
      "aux": "string",
      "createAt": "2025-12-03",
      "verified": "False",
      "idRols": 1,
      "rolName": "Administrador",
      "idConfigSys": 1,
      "configSysName": "AGUA",
      "companyName": "AGUA",
      "branchName": "DOS",
      "logoUrl": "",
      "modules": [
        {
          "index": 999,
          "name": "Admin",
          "normalized": "/"
        }
      ],
      "permissions": [
        {
          "index": 0,
          "name": "Acceso Total",
          "normalized": "FULL_ACCESS",
          "type": "SUPER_ADMIN",
          "status": "active"
        }
      ]
    }
  ],
  "statusCode": 200,
  "timestamp": "2026-01-02T17:26:48.006925Z"
}
```

#### **200 OK - Usuario específico encontrado**
```json
{
  "success": true,
  "message": "Usuario encontrado exitosamente",
  "data": {
    "idUsers": 3,
    "userName": "Ricardo",
    "name": "Ricardo J",
    "lastName": "Moo Vargas",
    "status": "Active",
    "aux": "string",
    "createAt": "2025-12-03",
    "verified": "False",
    "idRols": 1,
    "rolName": "Administrador",
    "idConfigSys": 1,
    "configSysName": "AGUA",
    "companyName": "AGUA",
    "branchName": "DOS",
    "logoUrl": "",
    "modules": [
      {
        "index": 999,
        "name": "Admin",
        "normalized": "/"
      }
    ],
    "permissions": [
      {
        "index": 0,
        "name": "Acceso Total",
        "normalized": "FULL_ACCESS",
        "type": "SUPER_ADMIN",
        "status": "active"
      }
    ]
  },
  "statusCode": 200,
  "timestamp": "2026-01-02T17:26:48.006925Z"
}
```

#### **400 Bad Request - Parámetros inválidos**
```json
{
  "success": false,
  "message": "Solo se puede buscar por ID o por UserName, no ambos",
  "data": null,
  "statusCode": 400,
  "timestamp": "2026-01-02T17:26:48.006925Z"
}
```

#### **404 Not Found - Usuario no encontrado**
```json
{
  "success": false,
  "message": "Usuario con ID 999 no encontrado",
  "data": null,
  "statusCode": 404,
  "timestamp": "2026-01-02T17:26:48.006925Z"
}
```

#### **500 Internal Server Error**
```json
{
  "success": false,
  "message": "Error interno del servidor",
  "data": null,
  "statusCode": 500,
  "timestamp": "2026-01-02T17:26:48.006925Z"
}
```

---

## **GET /api/common/Users/{idUsers}**

### **Descripción**
Obtiene un usuario específico por su ID (endpoint de compatibilidad).

### **Parámetros de ruta**
| Parámetro | Tipo | Requerido | Descripción |
|-----------|------|-----------|-------------|
| `idUsers` | `int` | Sí | ID del usuario a obtener |

### **Ejemplo de uso**
```
GET /api/common/Users/1
```

### **Respuestas**
Mismas respuestas que el endpoint de búsqueda, pero solo devuelve un usuario específico.

---

## **POST /api/common/Users**

### **Descripción**
Crea un nuevo usuario en el sistema.

### **Cuerpo de la petición**
```json
{
  "userName": "nuevoUsuario",
  "name": "Nuevo", 
  "lastName": "Usuario",
  "password": "contraseña123",
  "status": "Active",
  "aux": "",
  "idConfigSys": 1,
  "idRol": 2
}
```

### **Validaciones**
- `userName`: Requerido, debe ser único
- `password`: Requerido
- `name`: Opcional
- `lastName`: Opcional
- `status`: Opcional (por defecto "Active")
- `aux`: Opcional
- `idConfigSys`: Opcional (se usa configuración por defecto si no se especifica)
- `idRol`: Requerido

### **Respuesta exitosa - 201 Created**
```json
{
  "success": true,
  "message": "Usuario creado exitosamente",
  "data": {
    "idUsers": 5,
    "userName": "nuevoUsuario",
    "name": "Nuevo",
    "lastName": "Usuario",
    "status": "Active",
    "aux": "",
    "createAt": "2026-01-02",
    "verified": "False",
    "idRols": 2,
    "rolName": "Ventas",
    "idConfigSys": 1,
    "configSysName": "AGUA",
    "companyName": "AGUA",
    "branchName": "DOS",
    "logoUrl": "",
    "modules": [
      {
        "index": 0,
        "name": "Ventas",
        "normalized": "/main-sales"
      }
    ],
    "permissions": [
      {
        "index": 0,
        "name": "Gestión de Ventas",
        "normalized": "SALES_MANAGEMENT",
        "type": "SALES",
        "status": "active"
      }
    ]
  },
  "statusCode": 201,
  "timestamp": "2026-01-02T17:26:48.006925Z"
}
```

#### **400 Bad Request - Datos inválidos**
```json
{
  "success": false,
  "message": "El nombre de usuario es requerido",
  "data": null,
  "statusCode": 400,
  "timestamp": "2026-01-02T17:26:48.006925Z"
}
```

#### **409 Conflict - Usuario duplicado**
```json
{
  "success": false,
  "message": "El nombre de usuario 'Ricardo' ya está en uso",
  "data": null,
  "statusCode": 409,
  "timestamp": "2026-01-02T17:26:48.006925Z"
}
```

---

## **PUT /api/common/Users/{idUsers}**

### **Descripción**
Actualiza un usuario existente.

### **Parámetros de ruta**
| Parámetro | Tipo | Requerido | Descripción |
|-----------|------|-----------|-------------|
| `idUsers` | `int` | Sí | ID del usuario a actualizar |

### **Cuerpo de la petición**
```json
{
  "idUsers": 1,
  "userName": "usuarioActualizado",
  "name": "Usuario",
  "lastName": "Actualizado", 
  "password": "nuevaContraseña123",
  "status": "Active",
  "aux": "updated",
  "idConfigSys": 1,
  "idRol": 1
}
```

### **Validaciones**
- El ID de la ruta debe coincidir con el ID del cuerpo
- `userName`: Opcional, debe ser único si se cambia
- `password`: Opcional (si no se proporciona, se mantiene la anterior)
- Otros campos opcionales

### **Respuesta exitosa - 200 OK**
```json
{
  "success": true,
  "message": "Usuario actualizado exitosamente",
  "data": {
    "idUsers": 1,
    "userName": "usuarioActualizado",
    "name": "Usuario",
    "lastName": "Actualizado", 
    "status": "Active",
    "aux": "updated",
    "createAt": "2025-11-25",
    "verified": "True",
    "idRols": 1,
    "rolName": "Administrador",
    "idConfigSys": 1,
    "configSysName": "AGUA",
    "companyName": "AGUA",
    "branchName": "DOS",
    "logoUrl": "",
    "modules": [
      {
        "index": 999,
        "name": "Admin",
        "normalized": "/"
      }
    ],
    "permissions": [
      {
        "index": 0,
        "name": "Acceso Total",
        "normalized": "FULL_ACCESS",
        "type": "SUPER_ADMIN",
        "status": "active"
      }
    ]
  },
  "statusCode": 200,
  "timestamp": "2026-01-02T17:26:48.006925Z"
}
```

#### **400 Bad Request - ID no coincide**
```json
{
  "success": false,
  "message": "El ID de la ruta no coincide con el ID del usuario",
  "data": null,
  "statusCode": 400,
  "timestamp": "2026-01-02T17:26:48.006925Z"
}
```

#### **404 Not Found - Usuario no existe**
```json
{
  "success": false,
  "message": "Usuario con ID 999 no encontrado",
  "data": null,
  "statusCode": 404,
  "timestamp": "2026-01-02T17:26:48.006925Z"
}
```

#### **409 Conflict - Nombre duplicado**
```json
{
  "success": false,
  "message": "El nombre de usuario 'admin' ya está en uso",
  "data": null,
  "statusCode": 409,
  "timestamp": "2026-01-02T17:26:48.006925Z"
}
```

---

## **POST /api/common/Users/setup-admin**

### **Descripción**
Configura el usuario administrador inicial del sistema. Este endpoint está diseñado para la configuración inicial del sistema.

### **Cuerpo de la petición**
```json
{
  "userName": "admin",
  "password": "admin123"
}
```

### **Validaciones**
- `userName`: Requerido, no debe existir previamente
- `password`: Requerido
- Debe existir una configuración del sistema previamente

### **Respuesta exitosa - 201 Created**
```json
{
  "success": true,
  "message": "Usuario administrador configurado exitosamente",
  "data": {
    "idUsers": 1,
    "userName": "admin",
    "name": "admin",
    "lastName": "",
    "status": "Active",
    "aux": "",
    "createAt": "2026-01-02",
    "verified": "True",
    "idRols": 1,
    "rolName": "Administrador",
    "idConfigSys": 1,
    "configSysName": "Sistema",
    "companyName": "Mi Empresa",
    "branchName": "Sucursal Principal",
    "logoUrl": "",
    "modules": [
      {
        "index": 999,
        "name": "Admin",
        "normalized": "/"
      }
    ],
    "permissions": [
      {
        "index": 0,
        "name": "Acceso Total",
        "normalized": "FULL_ACCESS",
        "type": "SUPER_ADMIN",
        "status": "active"
      }
    ]
  },
  "statusCode": 201,
  "timestamp": "2026-01-02T17:26:48.006925Z"
}
```

#### **400 Bad Request - Usuario ya existe**
```json
{
  "success": false,
  "message": "El usuario 'admin' ya existe",
  "data": null,
  "statusCode": 400,
  "timestamp": "2026-01-02T17:26:48.006925Z"
}
```

---

## **OPTIONS /api/common/Users**

### **Descripción**
Maneja las solicitudes preflight de CORS.

### **Respuesta - 200 OK**
```json
{
  "success": true,
  "message": "Options request successful",
  "data": null,
  "statusCode": 200,
  "timestamp": "2026-01-02T17:26:48.006925Z"
}
```

**Headers adicionales:**
- `Allow`: `GET,POST,PUT,OPTIONS`

---

## **Modelos de Datos para Flutter/Dart**

### **user_res.dart** - Respuesta de Usuario
```dart
class UserRes {
  final int idUsers;
  final String userName;
  final String name;
  final String lastName;
  final String status;
  final String aux;
  final String createAt;  // formato: YYYY-MM-DD
  final String verified;  // "True" o "False"
  final int idRols;
  final String rolName;
  final int idConfigSys;
  final String configSysName;
  final String companyName;
  final String branchName;
  final String logoUrl;
  final List<ModuleRes> modules;
  final List<PermissionRes> permissions;

  UserRes({
    required this.idUsers,
    required this.userName,
    required this.name,
    required this.lastName,
    required this.status,
    required this.aux,
    required this.createAt,
    required this.verified,
    required this.idRols,
    required this.rolName,
    required this.idConfigSys,
    required this.configSysName,
    required this.companyName,
    required this.branchName,
    required this.logoUrl,
    required this.modules,
    required this.permissions,
  });

  factory UserRes.fromJson(Map<String, dynamic> json) {
    return UserRes(
      idUsers: json['idUsers'] as int,
      userName: json['userName'] as String,
      name: json['name'] as String,
      lastName: json['lastName'] as String,
      status: json['status'] as String,
      aux: json['aux'] as String,
      createAt: json['createAt'] as String,
      verified: json['verified'] as String,
      idRols: json['idRols'] as int,
      rolName: json['rolName'] as String,
      idConfigSys: json['idConfigSys'] as int,
      configSysName: json['configSysName'] as String,
      companyName: json['companyName'] as String,
      branchName: json['branchName'] as String,
      logoUrl: json['logoUrl'] as String,
      modules: (json['modules'] as List<dynamic>)
          .map((module) => ModuleRes.fromJson(module))
          .toList(),
      permissions: (json['permissions'] as List<dynamic>)
          .map((permission) => PermissionRes.fromJson(permission))
          .toList(),
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'idUsers': idUsers,
      'userName': userName,
      'name': name,
      'lastName': lastName,
      'status': status,
      'aux': aux,
      'createAt': createAt,
      'verified': verified,
      'idRols': idRols,
      'rolName': rolName,
      'idConfigSys': idConfigSys,
      'configSysName': configSysName,
      'companyName': companyName,
      'branchName': branchName,
      'logoUrl': logoUrl,
      'modules': modules.map((module) => module.toJson()).toList(),
      'permissions': permissions.map((permission) => permission.toJson()).toList(),
    };
  }
}
```

### **module_res.dart** - Módulo del Sistema
```dart
class ModuleRes {
  final int index;
  final String name;
  final String normalized;  // ruta de navegación

  ModuleRes({
    required this.index,
    required this.name,
    required this.normalized,
  });

  factory ModuleRes.fromJson(Map<String, dynamic> json) {
    return ModuleRes(
      index: json['index'] as int,
      name: json['name'] as String,
      normalized: json['normalized'] as String,
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'index': index,
      'name': name,
      'normalized': normalized,
    };
  }
}
```

### **permission_res.dart** - Permisos del Usuario
```dart
class PermissionRes {
  final int index;
  final String name;
  final String normalized;
  final String type;
  final String status;

  PermissionRes({
    required this.index,
    required this.name,
    required this.normalized,
    required this.type,
    required this.status,
  });

  factory PermissionRes.fromJson(Map<String, dynamic> json) {
    return PermissionRes(
      index: json['index'] as int,
      name: json['name'] as String,
      normalized: json['normalized'] as String,
      type: json['type'] as String,
      status: json['status'] as String,
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'index': index,
      'name': name,
      'normalized': normalized,
      'type': type,
      'status': status,
    };
  }
}
```

### **create_user_request.dart** - Crear Usuario
```dart
class CreateUserRequest {
  final String userName;
  final String? name;
  final String? lastName;
  final String password;
  final String? status;
  final String? aux;
  final int? idConfigSys;
  final int idRol;

  CreateUserRequest({
    required this.userName,
    this.name,
    this.lastName,
    required this.password,
    this.status,
    this.aux,
    this.idConfigSys,
    required this.idRol,
  });

  Map<String, dynamic> toJson() {
    final Map<String, dynamic> data = {
      'userName': userName,
      'password': password,
      'idRol': idRol,
    };
    
    if (name != null) data['name'] = name;
    if (lastName != null) data['lastName'] = lastName;
    if (status != null) data['status'] = status;
    if (aux != null) data['aux'] = aux;
    if (idConfigSys != null) data['idConfigSys'] = idConfigSys;
    
    return data;
  }
}
```

### **update_user_request.dart** - Actualizar Usuario
```dart
class UpdateUserRequest {
  final int idUsers;
  final String? userName;
  final String? name;
  final String? lastName;
  final String? password;
  final String? status;
  final String? aux;
  final int? idConfigSys;
  final int? idRol;

  UpdateUserRequest({
    required this.idUsers,
    this.userName,
    this.name,
    this.lastName,
    this.password,
    this.status,
    this.aux,
    this.idConfigSys,
    this.idRol,
  });

  Map<String, dynamic> toJson() {
    final Map<String, dynamic> data = {
      'idUsers': idUsers,
    };
    
    if (userName != null) data['userName'] = userName;
    if (name != null) data['name'] = name;
    if (lastName != null) data['lastName'] = lastName;
    if (password != null) data['password'] = password;
    if (status != null) data['status'] = status;
    if (aux != null) data['aux'] = aux;
    if (idConfigSys != null) data['idConfigSys'] = idConfigSys;
    if (idRol != null) data['idRol'] = idRol;
    
    return data;
  }
}
```

### **admin_setup_request.dart** - Configurar Administrador
```dart
class AdminSetupRequest {
  final String userName;
  final String password;

  AdminSetupRequest({
    required this.userName,
    required this.password,
  });

  Map<String, dynamic> toJson() {
    return {
      'userName': userName,
      'password': password,
    };
  }
}
```

### **filter_users_get.dart** - Filtros para búsqueda de usuarios
```dart
class FilterUsersGet {
  final int? idUsers;
  final String? userName;

  FilterUsersGet({
    this.idUsers,
    this.userName,
  });

  Map<String, String> toQueryParams() {
    final Map<String, String> params = {};
    
    if (idUsers != null) params['idUsers'] = idUsers.toString();
    if (userName != null && userName!.isNotEmpty) params['userName'] = userName!;
    
    return params;
  }
}
```

---

## **Servicios Flutter/Dart**

### **user_service.dart** - Servicio para consumir API de usuarios
```dart
import 'dart:convert';
import 'package:http/http.dart' as http;

class UserService {
  final String baseUrl;
  final http.Client httpClient;

  UserService({
    required this.baseUrl,
    http.Client? httpClient,
  }) : httpClient = httpClient ?? http.Client();

  /// GET /api/common/Users - Obtener todos los usuarios o buscar por filtros
  Future<List<UserRes>> getUsers({FilterUsersGet? filter}) async {
    final uri = Uri.parse('$baseUrl/api/common/Users');
    
    if (filter != null) {
      final params = filter.toQueryParams();
      if (params.isNotEmpty) {
        uri = uri.replace(queryParameters: params);
      }
    }

    final response = await httpClient.get(
      uri,
      headers: {'Content-Type': 'application/json'},
    );

    if (response.statusCode == 200) {
      final Map<String, dynamic> body = json.decode(response.body);
      
      if (body['success'] == true) {
        // Si es búsqueda específica, data será un objeto
        if (filter?.idUsers != null || (filter?.userName != null && filter!.userName!.isNotEmpty)) {
          final userData = body['data'] as Map<String, dynamic>;
          return [UserRes.fromJson(userData)];
        }
        // Si es obtener todos, data será una lista
        else {
          final List<dynamic> usersData = body['data'] as List<dynamic>;
          return usersData.map((userData) => UserRes.fromJson(userData)).toList();
        }
      } else {
        throw Exception(body['message'] ?? 'Error desconocido');
      }
    } else if (response.statusCode == 404) {
      return []; // Usuario no encontrado
    } else {
      final Map<String, dynamic> body = json.decode(response.body);
      throw Exception(body['message'] ?? 'Error HTTP ${response.statusCode}');
    }
  }

  /// GET /api/common/Users/{id} - Obtener usuario por ID específico
  Future<UserRes?> getUserById(int idUsers) async {
    final uri = Uri.parse('$baseUrl/api/common/Users/$idUsers');

    final response = await httpClient.get(
      uri,
      headers: {'Content-Type': 'application/json'},
    );

    if (response.statusCode == 200) {
      final Map<String, dynamic> body = json.decode(response.body);
      
      if (body['success'] == true) {
        return UserRes.fromJson(body['data']);
      } else {
        throw Exception(body['message'] ?? 'Error desconocido');
      }
    } else if (response.statusCode == 404) {
      return null; // Usuario no encontrado
    } else {
      final Map<String, dynamic> body = json.decode(response.body);
      throw Exception(body['message'] ?? 'Error HTTP ${response.statusCode}');
    }
  }

  /// POST /api/common/Users - Crear nuevo usuario
  Future<UserRes> createUser(CreateUserRequest request) async {
    final uri = Uri.parse('$baseUrl/api/common/Users');

    final response = await httpClient.post(
      uri,
      headers: {'Content-Type': 'application/json'},
      body: json.encode(request.toJson()),
    );

    final Map<String, dynamic> body = json.decode(response.body);

    if (response.statusCode == 201) {
      if (body['success'] == true) {
        return UserRes.fromJson(body['data']);
      } else {
        throw Exception(body['message'] ?? 'Error desconocido');
      }
    } else if (response.statusCode == 409) {
      throw UserAlreadyExistsException(body['message'] ?? 'Usuario ya existe');
    } else if (response.statusCode == 400) {
      throw InvalidUserDataException(body['message'] ?? 'Datos inválidos');
    } else {
      throw Exception(body['message'] ?? 'Error HTTP ${response.statusCode}');
    }
  }

  /// PUT /api/common/Users/{id} - Actualizar usuario existente
  Future<UserRes> updateUser(int idUsers, UpdateUserRequest request) async {
    final uri = Uri.parse('$baseUrl/api/common/Users/$idUsers');

    final response = await httpClient.put(
      uri,
      headers: {'Content-Type': 'application/json'},
      body: json.encode(request.toJson()),
    );

    final Map<String, dynamic> body = json.decode(response.body);

    if (response.statusCode == 200) {
      if (body['success'] == true) {
        return UserRes.fromJson(body['data']);
      } else {
        throw Exception(body['message'] ?? 'Error desconocido');
      }
    } else if (response.statusCode == 404) {
      throw UserNotFoundException(body['message'] ?? 'Usuario no encontrado');
    } else if (response.statusCode == 409) {
      throw UserAlreadyExistsException(body['message'] ?? 'Nombre de usuario ya existe');
    } else if (response.statusCode == 400) {
      throw InvalidUserDataException(body['message'] ?? 'Datos inválidos');
    } else {
      throw Exception(body['message'] ?? 'Error HTTP ${response.statusCode}');
    }
  }

  /// POST /api/common/Users/setup-admin - Configurar administrador inicial
  Future<UserRes> setupAdmin(AdminSetupRequest request) async {
    final uri = Uri.parse('$baseUrl/api/common/Users/setup-admin');

    final response = await httpClient.post(
      uri,
      headers: {'Content-Type': 'application/json'},
      body: json.encode(request.toJson()),
    );

    final Map<String, dynamic> body = json.decode(response.body);

    if (response.statusCode == 201) {
      if (body['success'] == true) {
        return UserRes.fromJson(body['data']);
      } else {
        throw Exception(body['message'] ?? 'Error desconocido');
      }
    } else if (response.statusCode == 400) {
      throw AdminSetupException(body['message'] ?? 'Error al configurar administrador');
    } else {
      throw Exception(body['message'] ?? 'Error HTTP ${response.statusCode}');
    }
  }

  void dispose() {
    httpClient.close();
  }
}
```

### **user_exceptions.dart** - Excepciones específicas
```dart
class UserException implements Exception {
  final String message;
  UserException(this.message);

  @override
  String toString() => 'UserException: $message';
}

class UserNotFoundException extends UserException {
  UserNotFoundException(String message) : super(message);
}

class UserAlreadyExistsException extends UserException {
  UserAlreadyExistsException(String message) : super(message);
}

class InvalidUserDataException extends UserException {
  InvalidUserDataException(String message) : super(message);
}

class AdminSetupException extends UserException {
  AdminSetupException(String message) : super(message);
}
```

### **Ejemplos de uso en Flutter**

#### **Listar todos los usuarios**
```dart
// En tu widget o provider
final userService = UserService(baseUrl: 'https://tu-api.com');

try {
  final users = await userService.getUsers();
  print('Total usuarios: ${users.length}');
  for (final user in users) {
    print('${user.userName} - ${user.name} ${user.lastName}');
  }
} catch (e) {
  print('Error al obtener usuarios: $e');
}
```

#### **Buscar usuario por ID**
```dart
try {
  final filter = FilterUsersGet(idUsers: 1);
  final users = await userService.getUsers(filter: filter);
  
  if (users.isNotEmpty) {
    final user = users.first;
    print('Usuario encontrado: ${user.userName}');
  } else {
    print('Usuario no encontrado');
  }
} catch (e) {
  print('Error al buscar usuario: $e');
}
```

#### **Buscar usuario por nombre**
```dart
try {
  final filter = FilterUsersGet(userName: 'Ricardo');
  final users = await userService.getUsers(filter: filter);
  
  if (users.isNotEmpty) {
    final user = users.first;
    print('Usuario encontrado: ${user.name} ${user.lastName}');
  }
} catch (e) {
  print('Error al buscar usuario: $e');
}
```

#### **Crear nuevo usuario**
```dart
try {
  final newUser = CreateUserRequest(
    userName: 'nuevoUsuario',
    name: 'Nuevo',
    lastName: 'Usuario',
    password: 'password123',
    idRol: 2,
  );
  
  final createdUser = await userService.createUser(newUser);
  print('Usuario creado: ID ${createdUser.idUsers}');
} catch (UserAlreadyExistsException e) {
  print('El usuario ya existe: $e');
} catch (InvalidUserDataException e) {
  print('Datos inválidos: $e');
} catch (e) {
  print('Error al crear usuario: $e');
}
```

#### **Actualizar usuario**
```dart
try {
  final updateUser = UpdateUserRequest(
    idUsers: 1,
    name: 'Nombre Actualizado',
    status: 'Active',
  );
  
  final updatedUser = await userService.updateUser(1, updateUser);
  print('Usuario actualizado: ${updatedUser.name}');
} catch (UserNotFoundException e) {
  print('Usuario no encontrado: $e');
} catch (UserAlreadyExistsException e) {
  print('Nombre de usuario ya existe: $e');
} catch (e) {
  print('Error al actualizar usuario: $e');
}
```

#### **Configurar administrador inicial**
```dart
try {
  final adminSetup = AdminSetupRequest(
    userName: 'admin',
    password: 'admin123',
  );
  
  final admin = await userService.setupAdmin(adminSetup);
  print('Administrador configurado: ${admin.userName}');
} catch (AdminSetupException e) {
  print('Error al configurar admin: $e');
} catch (e) {
  print('Error: $e');
}
```

1. **Búsqueda de usuarios**: Solo se puede usar un parámetro de búsqueda a la vez (`idUsers` O `userName`, no ambos).

2. **Creación de usuarios**: 
   - El `userName` debe ser único en el sistema
   - Si no se especifica `idConfigSys`, se usa la configuración por defecto
   - La contraseña se encripta automáticamente antes de guardarla

3. **Actualización de usuarios**:
   - Solo se actualizan los campos proporcionados
   - Si no se proporciona nueva contraseña, se mantiene la anterior
   - El `userName` debe seguir siendo único si se cambia

4. **Setup de administrador**:
   - Solo para configuración inicial del sistema
   - Crea automáticamente el rol de administrador si no existe
   - El usuario se crea como verificado (`verified: true`)

5. **Respuestas**: Todas las respuestas siguen el formato estándar `ApiResponse` con `success`, `message`, `data`, `statusCode` y `timestamp`.

6. **Campos de fecha**: Los campos de fecha se devuelven en formato `YYYY-MM-DD`.

7. **Campo verified**: Se devuelve como string (`"True"` o `"False"`) en lugar de boolean.

8. **Dependencias Flutter requeridas**:
   ```yaml
   dependencies:
     http: ^1.1.0  # Para peticiones HTTP
   ```

9. **Manejo de errores**: Se incluyen excepciones específicas para diferentes tipos de errores de la API.

10. **Filtros de búsqueda**: 
    - El `FilterUsersGet` maneja automáticamente la construcción de query parameters
    - Solo se incluyen en la URL los parámetros que no son nulos

11. **Respuestas de la API**: 
    - El servicio maneja automáticamente si la respuesta es un objeto único o una lista
    - Para búsquedas específicas devuelve una lista con un elemento
    - Para obtener todos devuelve la lista completa

12. **Null Safety**: Todos los modelos están diseñados con null safety de Dart 2.12+

13. **JSON Serialization**: 
    - Los métodos `fromJson` y `toJson` están implementados para facilitar la serialización
    - Los campos opcionales se manejan correctamente en `toJson`
