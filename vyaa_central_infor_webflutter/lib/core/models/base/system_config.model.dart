import 'package:hive/hive.dart';

part 'system_config.model.g.dart';

/// Modelo para almacenar la configuración inicial del sistema
@HiveType(typeId: 0)
class SystemConfig extends HiveObject {
  @HiveField(0)
  late bool hasConfiguration;

  @HiveField(1)
  late bool requiresMigration;

  @HiveField(2)
  late String message;

  @HiveField(3)
  String? details;
  
  @HiveField(4)
  late DateTime lastUpdated;

  @HiveField(5)
  late bool isActive;

  @HiveField(6)
  String? serverVersion;

  @HiveField(7)
  String? environment;
  
  @HiveField(8)
  String? rawJsonData;



  SystemConfig();

  /// Constructor con parámetros
  SystemConfig.create({
    required this.hasConfiguration,
    required this.requiresMigration,
    required this.message,
    this.details,
    this.serverVersion,
    this.environment,
    this.rawJsonData,
  }) {
    lastUpdated = DateTime.now();
    isActive = true;
  }

  /// Factory para crear desde CheckInitConfigData
  factory SystemConfig.fromCheckInitConfig({
    required bool hasConfiguration,
    required bool requiresMigration,
    required String message,
    String? details,
    String? serverVersion,
    String? rawJson,
  }) {
    return SystemConfig.create(
      hasConfiguration: hasConfiguration,
      requiresMigration: requiresMigration,
      message: message,
      details: details,
      serverVersion: serverVersion,
      rawJsonData: rawJson,
    );
  }

  /// Convertir a Map para compatibilidad con API
  Map<String, dynamic> toJson() {
    return {
      'hasConfiguration': hasConfiguration,
      'requiresMigration': requiresMigration,
      'message': message,
      'details': details,
      'lastUpdated': lastUpdated.toIso8601String(),
      'isActive': isActive,
      'serverVersion': serverVersion,
      'environment': environment,
      'rawJsonData': rawJsonData,
    };
  }

  /// Crear desde Map
  factory SystemConfig.fromJson(Map<String, dynamic> json) {
    return SystemConfig()
      ..hasConfiguration = json['hasConfiguration'] ?? false
      ..requiresMigration = json['requiresMigration'] ?? false
      ..message = json['message'] ?? ''
      ..details = json['details']
      ..lastUpdated = DateTime.tryParse(json['lastUpdated'] ?? '') ?? DateTime.now()
      ..isActive = json['isActive'] ?? true
      ..serverVersion = json['serverVersion']
      ..environment = json['environment']
      ..rawJsonData = json['rawJsonData'];
  }

  /// Obtener estado del sistema como string
  String getStatusString() {
    if (requiresMigration) return 'needsMigration';
    if (!hasConfiguration) return 'needsSetup';
    return 'ready';
  }

  /// Obtener estado del sistema como enum
  SystemStatus getStatus() {
    if (requiresMigration) return SystemStatus.needsMigration;
    if (!hasConfiguration) return SystemStatus.needsSetup;
    return SystemStatus.ready;
  }

  /// Verificar si el sistema está listo
  bool get isSystemReady => hasConfiguration && !requiresMigration;
}

/// Estados posibles del sistema
enum SystemStatus {
  needsSetup,    // Necesita configuración inicial
  needsMigration, // Necesita migración de BD
  ready          // Listo para usar
}