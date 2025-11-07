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

  User({
    required this.id,
    required this.userName,
    required this.name,
    required this.lastName,
    required this.rol,
    required this.idConfigSys,
  });

  factory User.fromJson(Map<String, dynamic> json) {
    return User(
      id: json['id'] ?? 0,
      userName: json['userName'] ?? '',
      name: json['name'] ?? '',
      lastName: json['lastName'] ?? '',
      rol: json['rol'] ?? '',
      idConfigSys: json['idConfigSys'] ?? 0,
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
    };
  }
}
