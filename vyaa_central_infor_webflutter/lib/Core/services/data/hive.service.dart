import 'dart:convert';
import 'package:flutter/foundation.dart';
import 'package:flutter/material.dart';
import 'package:hive_flutter/hive_flutter.dart';

// Modelos Hive
import '../../models/base/system_config.model.dart';
import '../../models/base/user_session.model.dart';
import '../../models/base/app_cache.model.dart';
import '../../models/responses/check_init_config.module.dart';

/// Servicio principal para manejo de datos con Hive
/// Reemplaza IsarService y es completamente compatible con web
class HiveService {
  static bool _isInitialized = false;

  // Nombres de las cajas Hive
  static const String _systemConfigBox = 'systemConfig';
  static const String _userSessionBox = 'userSession';
  static const String _appCacheBox = 'appCache';
  static const String _appSettingsBox = 'appSettings';

  /// Inicializar Hive Database
  static Future<void> initialize() async {
    if (_isInitialized) return;

    try {
      debugPrint('🗃️ Inicializando Hive Database...');
      // Inicializar Hive para Flutter
      await Hive.initFlutter();
      
      // Registrar adaptadores
      if (!Hive.isAdapterRegistered(0)) {
        Hive.registerAdapter(SystemConfigAdapter());
      }
      if (!Hive.isAdapterRegistered(1)) {
        Hive.registerAdapter(UserSessionAdapter());
      }
      if (!Hive.isAdapterRegistered(2)) {
        Hive.registerAdapter(AppCacheAdapter());
      }
      if (!Hive.isAdapterRegistered(3)) {
        Hive.registerAdapter(AppSettingsAdapter());
      }
      // Abrir cajas
      await Future.wait([
        Hive.openBox<SystemConfig>(_systemConfigBox),
        Hive.openBox<UserSession>(_userSessionBox),
        Hive.openBox<AppCache>(_appCacheBox),
        Hive.openBox<AppSettings>(_appSettingsBox),
      ]);
      _isInitialized = true;
      // Crear configuración por defecto si no existe
      await _ensureDefaultSettings();
      
    } catch (e) {
      debugPrint('❌ Error inicializando Hive: $e');
      rethrow;
    }
  }

  /// Asegurar que existan configuraciones por defecto
  static Future<void> _ensureDefaultSettings() async {
    final settingsBox = Hive.box<AppSettings>(_appSettingsBox);
    
    if (settingsBox.isEmpty) {
      final defaultSettings = AppSettings.createDefault();
      await settingsBox.add(defaultSettings);
      debugPrint('📝 Configuraciones por defecto creadas');
    }
  }

  /// Cerrar conexiones (llamar al cerrar la app)
  static Future<void> close() async {
    if (_isInitialized) {
      await Hive.close();
      _isInitialized = false;
      debugPrint('📤 Hive Database cerrada');
    }
  }

  // ========================================
  // CONFIGURACIÓN DEL SISTEMA
  // ========================================

  /// Guardar configuración inicial del sistema
  static Future<void> saveSystemConfig(CheckInitConfigData response) async {
    final box = Hive.box<SystemConfig>(_systemConfigBox);
    
    final config = SystemConfig.fromCheckInitConfig(
      hasConfiguration: response.hasConfiguration,
      requiresMigration: response.requiresMigration,
      message: response.requiresMigration ? 'Sistema requiere migración' : 'DB configurada correctamentew',
      details: response.hasConfiguration ? null : 'Tabla existe: ${response.tableExists}',
      rawJson: jsonEncode(response.toJson()),
    );
    // Limpiar configuraciones anteriores
    await box.clear();
    // Guardar nueva configuración
    await box.add(config);
    
    debugPrint('💾 Configuración del sistema guardada');
  }

  /// Obtener configuración como CheckInitConfigResponse (compatibilidad)
  /// Devuelve null si no existe configuración
  static Future<CheckInitConfigData?> getSystemConfigAsCheckInitConfig() async {
    final config = await getActiveSystemConfig();
    if (config == null) return null;
    
    return CheckInitConfigData(
      hasConfiguration: config.hasConfiguration,
      requiresMigration: config.requiresMigration,
      tableExists: config.hasConfiguration ? true : (config.details?.contains('Tabla existe: true') ?? false),
    );
  }

  /// Obtener configuración activa del sistema
  static Future<SystemConfig?> getActiveSystemConfig() async {
    final box = Hive.box<SystemConfig>(_systemConfigBox);
    if (box.isEmpty) return null;
    
    // Buscar la configuración más reciente y activa
    SystemConfig? latestConfig;
    for (var config in box.values) {
      if (config.isActive && 
          (latestConfig == null || config.lastUpdated.isAfter(latestConfig.lastUpdated))) {
        latestConfig = config;
      }
    }
    return latestConfig ?? box.values.first;
  }

  /// Stream de configuración del sistema (tiempo real con ValueListenable)
  static ValueListenable<Box<SystemConfig>> watchSystemConfig() {
    return Hive.box<SystemConfig>(_systemConfigBox).listenable();
  }
  /// Limpiar configuración del sistema
  static Future<void> clearSystemConfig() async {
    final box = Hive.box<SystemConfig>(_systemConfigBox);
    await box.clear();
    debugPrint('🧹 Configuración del sistema limpiada');
  }

  // ========================================
  // SESIONES DE USUARIO
  // ========================================

  /// Guardar sesión de usuario
  static Future<void> saveUserSession(UserSession session) async {
    final box = Hive.box<UserSession>(_userSessionBox);
    
    // Desactivar sesiones anteriores
    for (var existingSession in box.values) {
      if (existingSession.isActive) {
        existingSession.logout();
        await existingSession.save();
      }
    }
    
    // Guardar nueva sesión
    await box.add(session);
    
    debugPrint('👤 Sesión de usuario guardada');
  }

  /// Obtener sesión activa
  static Future<UserSession?> getActiveUserSession() async {
    final box = Hive.box<UserSession>(_userSessionBox);
    
    for (var session in box.values) {
      if (session.isActive && session.isValid) {
        return session;
      }
    }
    return null;
  }

  /// Stream de sesión activa (tiempo real)
  static ValueListenable<Box<UserSession>> watchUserSession() {
    return Hive.box<UserSession>(_userSessionBox).listenable();
  }

  /// Cerrar sesión activa
  static Future<void> logout() async {
    final activeSession = await getActiveUserSession();
    
    if (activeSession != null) {
      activeSession.logout();
      await activeSession.save();
      debugPrint('👋 Sesión cerrada');
    }
  }

  /// Verificar si hay sesión válida
  static Future<bool> hasValidSession() async {
    final session = await getActiveUserSession();
    return session?.isValid ?? false;
  }

  // ========================================
  // CONFIGURACIONES DE LA APP
  // ========================================

  /// Obtener configuraciones de la app
  static Future<AppSettings> getAppSettings() async {
    final box = Hive.box<AppSettings>(_appSettingsBox);
    
    AppSettings? settings;
    for (var s in box.values) {
      if (s.isActive) {
        settings = s;
        break;
      }
    }
    
    if (settings == null) {
      settings = AppSettings.createDefault();
      await box.add(settings);
    }
    
    return settings;
  }

  /// Actualizar configuraciones de la app
  static Future<void> updateAppSettings(AppSettings settings) async {
    settings.touch();
    await settings.save();
    debugPrint('⚙️ Configuraciones de app actualizadas');
  }

  /// Stream de configuraciones (tiempo real)
  static ValueListenable<Box<AppSettings>> watchAppSettings() {
    return Hive.box<AppSettings>(_appSettingsBox).listenable();
  }

  // Métodos de compatibilidad con el servicio anterior

  /// Verificar si es primera vez
  static Future<bool> isFirstTimeCheck() async {
    final settings = await getAppSettings();
    return settings.isFirstTimeCheck;
  }

  /// Marcar primera verificación como completada
  static Future<void> markFirstTimeCheckDone() async {
    final settings = await getAppSettings();
    settings.isFirstTimeCheck = false;
    await updateAppSettings(settings);
  }

  /// Verificar si el sistema está inicializado
  static Future<bool> isSystemInitialized() async {
    final settings = await getAppSettings();
    return settings.isSystemInitialized;
  }

  /// Guardar estado de inicialización
  static Future<void> saveSystemInitialized(bool initialized) async {
    final settings = await getAppSettings();
    settings.isSystemInitialized = initialized;
    await updateAppSettings(settings);
  }

  /// Obtener última ruta
  static Future<String?> getLastRoute() async {
    final settings = await getAppSettings();
    return settings.lastRoute;
  }

  /// Guardar última ruta
  static Future<void> saveLastRoute(String route) async {
    final settings = await getAppSettings();
    settings.lastRoute = route;
    await updateAppSettings(settings);
  }

  /// Limpiar flags de inicialización
  static Future<void> clearSystemInitFlags() async {
    final settings = await getAppSettings();
    settings.isFirstTimeCheck = true;
    settings.isSystemInitialized = false;
    settings.lastRoute = null;
    await updateAppSettings(settings);
  }

  // ========================================
  // CACHE GENERAL
  // ========================================

  /// Guardar en cache general
  static Future<void> setCache({
    required String key,
    required String value,
    String? category,
    Duration? expiry,
    int priority = 3,
    String? description,
  }) async {
    final box = Hive.box<AppCache>(_appCacheBox);
    
    // Eliminar cache anterior con la misma key
    final existingKeys = <int>[];
    for (var i = 0; i < box.length; i++) {
      if (box.getAt(i)?.key == key) {
        existingKeys.add(i);
      }
    }
    
    for (var i in existingKeys.reversed) {
      await box.deleteAt(i);
    }
    
    final cache = AppCache.create(
      key: key,
      value: value,
      category: category,
      priority: priority,
      expiresAt: expiry != null ? DateTime.now().add(expiry) : null,
      description: description,
    );

    await box.add(cache);
  }

  /// Obtener del cache general
  static Future<String?> getCache(String key) async {
    final box = Hive.box<AppCache>(_appCacheBox);
    
    AppCache? foundCache;
    for (var cache in box.values) {
      if (cache.key == key && cache.isActive && !cache.isExpired) {
        foundCache = cache;
        break;
      }
    }
    
    if (foundCache == null) return null;
    
    // Actualizar último acceso
    foundCache.touch();
    await foundCache.save();
    
    return foundCache.value;
  }

  /// Eliminar del cache
  static Future<void> removeCache(String key) async {
    final box = Hive.box<AppCache>(_appCacheBox);
    
    final toDelete = <int>[];
    for (var i = 0; i < box.length; i++) {
      if (box.getAt(i)?.key == key) {
        toDelete.add(i);
      }
    }
    
    for (var i in toDelete.reversed) {
      await box.deleteAt(i);
    }
  }

  /// Limpiar cache expirado
  static Future<void> cleanExpiredCache() async {
    final box = Hive.box<AppCache>(_appCacheBox);
    final now = DateTime.now();
    
    final toDelete = <int>[];
    for (var i = 0; i < box.length; i++) {
      final cache = box.getAt(i);
      if (cache != null && cache.expiresAt != null && now.isAfter(cache.expiresAt!)) {
        toDelete.add(i);
      }
    }
    
    for (var i in toDelete.reversed) {
      await box.deleteAt(i);
    }
    
    debugPrint('🧹 Cache expirado limpiado (${toDelete.length} entradas)');
  }

  // ========================================
  // MÉTODOS DE DIAGNÓSTICO
  // ========================================

  /// Obtener estadísticas de la base de datos
  static Future<Map<String, int>> getDatabaseStats() async {
    return {
      'systemConfigs': Hive.box<SystemConfig>(_systemConfigBox).length,
      'userSessions': Hive.box<UserSession>(_userSessionBox).length,
      'appSettings': Hive.box<AppSettings>(_appSettingsBox).length,
      'cacheEntries': Hive.box<AppCache>(_appCacheBox).length,
    };
  }

  /// Limpiar toda la base de datos (usar con cuidado)
  static Future<void> clearAllData() async {
    await Future.wait([
      Hive.box<SystemConfig>(_systemConfigBox).clear(),
      Hive.box<UserSession>(_userSessionBox).clear(),
      Hive.box<AppSettings>(_appSettingsBox).clear(),
      Hive.box<AppCache>(_appCacheBox).clear(),
    ]);
    
    // Recrear configuración por defecto
    await _ensureDefaultSettings();
    
    debugPrint('🔥 Toda la base de datos limpiada');
  }
}