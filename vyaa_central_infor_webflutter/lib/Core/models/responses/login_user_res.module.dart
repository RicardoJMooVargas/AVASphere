import 'configsys.module.dart';


class LoginUserRes {
  final String message;
  final String token;
  final User user;
  final ConfigSys configSys;

  LoginUserRes({
    required this.message,
    required this.token,
    required this.user,
    required this.configSys,
  });

  factory LoginUserRes.fromJson(Map<String, dynamic> json) {
    return LoginUserRes(
      message: json['message'] ?? '',
      token: json['token'] ?? '',
      user: User.fromJson(json['user'] as Map<String, dynamic>),
      configSys: ConfigSys.fromJson(json['configSys'] as Map<String, dynamic>),
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'message': message,
      'token': token,
      'user': user.toJson(),
      'configSys': configSys.toJson(),
    };
  }
}

class User {
  final int id;
  final String userName;
  final String name;
  final String lastName;
  final String rol;
  final int idConfigSys;
  final List<UserModule> modules;
  final List<UserPermission> permissions;

  User({
    required this.id,
    required this.userName,
    required this.name,
    required this.lastName,
    required this.rol,
    required this.idConfigSys,
    required this.modules,
    required this.permissions,
  });

  factory User.fromJson(Map<String, dynamic> json) {
    return User(
      id: json['id'] ?? 0,
      userName: json['userName'] ?? '',
      name: json['name'] ?? '',
      lastName: json['lastName'] ?? '',
      rol: json['rol'] ?? '',
      idConfigSys: json['idConfigSys'] ?? 0,
      modules: (json['modules'] as List<dynamic>?)
          ?.map((module) => UserModule.fromJson(module as Map<String, dynamic>))
          .toList() ?? <UserModule>[],
      permissions: (json['permissions'] as List<dynamic>?)
          ?.map((permission) => UserPermission.fromJson(permission as Map<String, dynamic>))
          .toList() ?? <UserPermission>[],
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'id': id,
      'userName': userName,
      'name': name,
      'lastName': lastName,
      'rol': rol,
      'idConfigSys': idConfigSys,
      'modules': modules.map((module) => module.toJson()).toList(),
      'permissions': permissions.map((permission) => permission.toJson()).toList(),
    };
  }
}

/// Modelo para módulos del usuario
class UserModule {
  final int index;
  final String name;
  final String normalized;

  UserModule({
    required this.index,
    required this.name,
    required this.normalized,
  });

  factory UserModule.fromJson(Map<String, dynamic> json) {
    return UserModule(
      index: json['index'] ?? 0,
      name: json['name'] ?? '',
      normalized: json['normalized'] ?? '',
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

/// Modelo para permisos del usuario
class UserPermission {
  final int index;
  final String name;
  final String normalized;
  final String type;
  final String status;

  UserPermission({
    required this.index,
    required this.name,
    required this.normalized,
    required this.type,
    required this.status,
  });

  factory UserPermission.fromJson(Map<String, dynamic> json) {
    return UserPermission(
      index: json['index'] ?? 0,
      name: json['name'] ?? '',
      normalized: json['normalized'] ?? '',
      type: json['type'] ?? '',
      status: json['status'] ?? '',
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
