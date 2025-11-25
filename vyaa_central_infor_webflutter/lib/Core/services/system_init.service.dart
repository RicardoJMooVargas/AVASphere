import 'package:flutter/foundation.dart';
import 'package:get/get.dart';
import 'api/system.service.dart';
import '../controllers/system_setup.controller.dart';
import 'data/hive.service.dart';

/// Servicio para manejar la inicialización y verificación del sistema
class SystemInitService {
  final SystemService _systemService = SystemService();

  /// Verifica el estado del sistema y determina la ruta inicial apropiada
  /// Retorna:
  /// - '/setup' si el sistema no está inicializado correctamente
  /// - '/login' si el sistema está OK pero no hay token
  /// - '/home' si el sistema está OK y hay token válido
  Future<String> determineInitialRoute() async {
    try {
      debugPrint('🚀 Iniciando verificación del sistema...');
      
      // Verificar directamente con el servidor la configuración inicial
      debugPrint('🔍 Consultando configuración del sistema...');
      final configResponse = await _systemService.checkInitialConfig();
      
      debugPrint('📊 Respuesta del servidor:');
      debugPrint('   - hasConfiguration: ${configResponse.hasConfiguration}');
      debugPrint('   - tableExists: ${configResponse.tableExists}');
      debugPrint('   - requiresMigration: ${configResponse.requiresMigration}');
      
      // Actualizar el controlador de estado del sistema si existe
      if (Get.isRegistered<SystemSetupController>()) {
        final systemSetupController = Get.find<SystemSetupController>();
        final isSystemOk = configResponse.hasConfiguration && !configResponse.requiresMigration;
        await systemSetupController.updateSystemStatus(isSystemOk);
      }
      
      // Determinar ruta basada en la configuración del sistema
      if (configResponse.hasConfiguration && !configResponse.requiresMigration) {
        // Sistema configurado correctamente -> verificar token/sesión
        debugPrint('✅ Sistema configurado correctamente - Verificando sesión...');
        
        // Verificar si existe sesión válida en Hive
        final hasValidSession = await HiveService.hasValidSession();
        if (hasValidSession) {
          final session = await HiveService.getActiveUserSession();
          debugPrint('🔑 Sesión válida encontrada en Hive:');
          debugPrint('   - User ID: ${session?.userId}');
          debugPrint('   - Username: ${session?.username}');
          debugPrint('   - Token: ${session?.token?.substring(0, 20)}...');
          debugPrint('   - Es válida: ${session?.isValid}');
          debugPrint('✅ Redirigiendo a aplicación principal');
          return '/home'; // Siempre /home, MainAppLayout maneja el contenido específico
        } else {
          debugPrint('🚫 No hay sesión válida en Hive - Redirigiendo a login');
          return '/login';
        }
      } else {
        // Sistema necesita configuración o migración -> ir a setup
        debugPrint('🔧 Sistema requiere configuración/migración - Redirigiendo a setup');
        return '/setup';
      }
      
    } catch (e) {
      debugPrint('❌ Error al verificar estado del sistema: $e');
      
      // Verificar si es un error de conexión al servidor
      final errorString = e.toString().toLowerCase();
      final isConnectionError = errorString.contains('clientexception') ||
                                errorString.contains('failed to fetch') ||
                                errorString.contains('connection') ||
                                errorString.contains('network') ||
                                errorString.contains('socketexception') ||
                                errorString.contains('timeout') ||
                                errorString.contains('refused') ||
                                errorString.contains('500');
      
      if (isConnectionError) {
        debugPrint('🔴 Error de conexión detectado - Redirigiendo a pantalla de error de servidor');
        return '/server-error';
      }
      
      // En caso de otro tipo de error, asumir que necesita configuración
      debugPrint('⚠️ Error desconocido - Redirigiendo a setup por seguridad');
      return '/setup';
    }
  }

  /// Fuerza una nueva verificación del sistema (útil después de completar setup)
  Future<void> forceSystemCheck() async {
    try {
      debugPrint('🔄 Forzando nueva verificación del sistema...');
      
      final configResponse = await _systemService.checkInitialConfig();
      final isSystemOk = configResponse.hasConfiguration && !configResponse.requiresMigration;
      
      // Actualizar el controlador de estado del sistema si existe
      if (Get.isRegistered<SystemSetupController>()) {
        final systemSetupController = Get.find<SystemSetupController>();
        await systemSetupController.updateSystemStatus(isSystemOk);
      }
      
      debugPrint(isSystemOk 
        ? '✅ Sistema verificado y actualizado como OK' 
        : '⚠️ Sistema aún requiere configuración');
        
    } catch (e) {
      debugPrint('❌ Error al forzar verificación del sistema: $e');
      rethrow;
    }
  }

  /// Resetea el estado de inicialización (útil para testing o reinstalación)
  Future<void> resetSystemState() async {
    debugPrint('🔄 Estado de inicialización del sistema reseteado');
    // Por ahora no hace nada ya que no guardamos estado en Hive
  }

  /// Obtiene la ruta determinada por verificación directa del sistema
  Future<String> getSystemRoute() async {
    return await determineInitialRoute();
  }
}
