import 'package:hive/hive.dart';

part 'user_session.model.g.dart';

/// Modelo para manejar sesiones de usuario
@HiveType(typeId: 1)
class UserSession extends HiveObject {
  @HiveField(0)
  late String userId;

  @HiveField(1)
  String? username;

  @HiveField(2)
  String? email;

  @HiveField(3)
  String? token;

  @HiveField(4)
  String? refreshToken;
  
  @HiveField(5)
  late DateTime loginTime;

  @HiveField(6)
  DateTime? lastActivity;

  @HiveField(7)
  DateTime? expiresAt;
  
  @HiveField(8)
  late bool isActive;

  @HiveField(9)
  late bool rememberMe;
  
  @HiveField(10)
  String? permissions;

  @HiveField(11)
  String? roles;
  
  @HiveField(12)
  String? deviceInfo;

  @HiveField(13)
  String? ipAddress;

  UserSession();

  /// Constructor con parámetros
  UserSession.create({
    required this.userId,
    this.username,
    this.email,
    this.token,
    this.refreshToken,
    this.expiresAt,
    this.rememberMe = false,
    this.permissions,
    this.roles,
    this.deviceInfo,
    this.ipAddress,
  }) {
    loginTime = DateTime.now();
    lastActivity = DateTime.now();
    isActive = true;
  }

  /// Actualizar actividad
  void updateActivity() {
    lastActivity = DateTime.now();
  }

  /// Verificar si la sesión sigue válida
  bool get isValid {
    if (!isActive) return false;
    if (expiresAt != null && DateTime.now().isAfter(expiresAt!)) {
      return false;
    }
    return token != null && token!.isNotEmpty;
  }

  /// Cerrar sesión
  void logout() {
    isActive = false;
    token = null;
    refreshToken = null;
  }

  /// Convertir a Map
  Map<String, dynamic> toJson() {
    return {
      'userId': userId,
      'username': username,
      'email': email,
      'token': token,
      'refreshToken': refreshToken,
      'loginTime': loginTime.toIso8601String(),
      'lastActivity': lastActivity?.toIso8601String(),
      'expiresAt': expiresAt?.toIso8601String(),
      'isActive': isActive,
      'rememberMe': rememberMe,
      'permissions': permissions,
      'roles': roles,
      'deviceInfo': deviceInfo,
      'ipAddress': ipAddress,
    };
  }

  /// Crear desde Map
  factory UserSession.fromJson(Map<String, dynamic> json) {
    return UserSession()
      ..userId = json['userId'] ?? ''
      ..username = json['username']
      ..email = json['email']
      ..token = json['token']
      ..refreshToken = json['refreshToken']
      ..loginTime = DateTime.tryParse(json['loginTime'] ?? '') ?? DateTime.now()
      ..lastActivity = DateTime.tryParse(json['lastActivity'] ?? '')
      ..expiresAt = DateTime.tryParse(json['expiresAt'] ?? '')
      ..isActive = json['isActive'] ?? false
      ..rememberMe = json['rememberMe'] ?? false
      ..permissions = json['permissions']
      ..roles = json['roles']
      ..deviceInfo = json['deviceInfo']
      ..ipAddress = json['ipAddress'];
  }
}