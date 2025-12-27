import 'package:hive/hive.dart';

part 'app_cache.model.g.dart';

/// Modelo para cache general de la aplicación
@HiveType(typeId: 2)
class AppCache extends HiveObject {
  @HiveField(0)
  late String key;
  
  @HiveField(1)
  late String value;
  
  @HiveField(2)
  late DateTime createdAt;

  @HiveField(3)
  late DateTime lastAccessed;

  @HiveField(4)
  DateTime? expiresAt;
  
  @HiveField(5)
  String? category;

  @HiveField(6)
  int? priority;
  
  @HiveField(7)
  late bool isActive;

  @HiveField(8)
  String? description;

  AppCache();

  /// Constructor con parámetros
  AppCache.create({
    required this.key,
    required this.value,
    this.category,
    this.priority = 3,
    this.expiresAt,
    this.description,
  }) {
    createdAt = DateTime.now();
    lastAccessed = DateTime.now();
    isActive = true;
  }

  /// Actualizar acceso
  void touch() {
    lastAccessed = DateTime.now();
  }

  /// Verificar si ha expirado
  bool get isExpired {
    if (expiresAt == null) return false;
    return DateTime.now().isAfter(expiresAt!);
  }

  /// Verificar si es válido
  bool get isValid => isActive && !isExpired;

  /// Convertir a Map
  Map<String, dynamic> toJson() {
    return {
      'key': key,
      'value': value,
      'createdAt': createdAt.toIso8601String(),
      'lastAccessed': lastAccessed.toIso8601String(),
      'expiresAt': expiresAt?.toIso8601String(),
      'category': category,
      'priority': priority,
      'isActive': isActive,
      'description': description,
    };
  }

  /// Crear desde Map
  factory AppCache.fromJson(Map<String, dynamic> json) {
    return AppCache()
      ..key = json['key'] ?? ''
      ..value = json['value'] ?? ''
      ..createdAt = DateTime.tryParse(json['createdAt'] ?? '') ?? DateTime.now()
      ..lastAccessed = DateTime.tryParse(json['lastAccessed'] ?? '') ?? DateTime.now()
      ..expiresAt = DateTime.tryParse(json['expiresAt'] ?? '')
      ..category = json['category']
      ..priority = json['priority'] ?? 3
      ..isActive = json['isActive'] ?? true
      ..description = json['description'];
  }
}

/// Modelo para configuraciones específicas de la app
@HiveType(typeId: 3)
class AppSettings extends HiveObject {
  @HiveField(0)
  late bool isFirstTimeCheck;

  @HiveField(1)
  late bool isSystemInitialized;

  @HiveField(2)
  String? lastRoute;
  
  @HiveField(3)
  String? theme;

  @HiveField(4)
  String? language;

  @HiveField(5)
  double? fontSize;
  
  @HiveField(6)
  int? apiTimeout;

  @HiveField(7)
  String? baseUrl;

  @HiveField(8)
  bool? enableLogging;
  
  @HiveField(9)
  late DateTime createdAt;

  @HiveField(10)
  late DateTime updatedAt;
  
  @HiveField(11)
  late bool isActive;

  AppSettings();

  /// Constructor por defecto
  AppSettings.createDefault() {
    isFirstTimeCheck = true;
    isSystemInitialized = false;
    theme = 'system';
    language = 'es';
    fontSize = 14.0;
    apiTimeout = 30;
    enableLogging = true;
    createdAt = DateTime.now();
    updatedAt = DateTime.now();
    isActive = true;
  }

  /// Actualizar timestamp
  void touch() {
    updatedAt = DateTime.now();
  }

  /// Convertir a Map
  Map<String, dynamic> toJson() {
    return {
      'isFirstTimeCheck': isFirstTimeCheck,
      'isSystemInitialized': isSystemInitialized,
      'lastRoute': lastRoute,
      'theme': theme,
      'language': language,
      'fontSize': fontSize,
      'apiTimeout': apiTimeout,
      'baseUrl': baseUrl,
      'enableLogging': enableLogging,
      'createdAt': createdAt.toIso8601String(),
      'updatedAt': updatedAt.toIso8601String(),
      'isActive': isActive,
    };
  }

  /// Crear desde Map
  factory AppSettings.fromJson(Map<String, dynamic> json) {
    return AppSettings()
      ..isFirstTimeCheck = json['isFirstTimeCheck'] ?? true
      ..isSystemInitialized = json['isSystemInitialized'] ?? false
      ..lastRoute = json['lastRoute']
      ..theme = json['theme'] ?? 'system'
      ..language = json['language'] ?? 'es'
      ..fontSize = json['fontSize']?.toDouble() ?? 14.0
      ..apiTimeout = json['apiTimeout'] ?? 30
      ..baseUrl = json['baseUrl']
      ..enableLogging = json['enableLogging'] ?? true
      ..createdAt = DateTime.tryParse(json['createdAt'] ?? '') ?? DateTime.now()
      ..updatedAt = DateTime.tryParse(json['updatedAt'] ?? '') ?? DateTime.now()
      ..isActive = json['isActive'] ?? true;
  }
}