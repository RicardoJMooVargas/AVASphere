import 'package:flutter/material.dart';
import 'package:get/get.dart';
import '../../core/controllers/system_setup.controller.dart';
import '../../core/controllers/server_status.controller.dart';
import '../services/system_init.service.dart';
import 'global_init.middleware.dart';

/// Middleware que verifica si el sistema requiere configuración inicial
/// y redirige automáticamente a la pantalla de setup si es necesario
/// También verifica la conectividad del servidor
class SystemSetupMiddleware extends GetMiddleware {
  @override
  int? get priority => 2; // Ejecutar después del GlobalInitMiddleware

  @override
  RouteSettings? redirect(String? route) {
    try {
      debugPrint('🔧 SystemSetupMiddleware verificando ruta: $route');

      // Rutas que no requieren verificación de configuración del sistema
      final excludedRoutes = ['/setup', '/server-error', '/notfound', '/'];
      if (excludedRoutes.contains(route)) {
        debugPrint('✅ Ruta excluida de verificación de configuración: $route');
        return null;
      }

      // 1. Verificar estado del servidor primero
      final serverStatus = Get.find<ServerStatusController>();
      if (!serverStatus.isServerAvailable) {
        debugPrint('🚫 Servidor no disponible - Redirigiendo a /server-error');
        return const RouteSettings(name: '/server-error');
      }

      // 2. Verificar si el sistema está configurado
      final systemSetup = Get.find<SystemSetupController>();
      debugPrint('🔧 Estado del sistema: ${systemSetup.isSystemConfigured ? "Configurado" : "Requiere configuración"}');

      // Si el sistema no está configurado, redirigir a setup
      if (!systemSetup.canNavigateTo(route ?? '')) {
        debugPrint('🚫 Sistema requiere configuración - Redirigiendo a /setup');
        return const RouteSettings(name: '/setup');
      }

      debugPrint('✅ Sistema configurado y servidor disponible - Permitiendo navegación');
      return null; // Todo OK, permitir navegación

    } catch (e) {
      debugPrint('⚠️ Error en SystemSetupMiddleware: $e');
      
      // Si hay error, verificar si es por servidor no disponible
      _checkServerAndRedirect();
      
      // Fallback a setup por seguridad
      debugPrint('🚫 Error en middleware - Redirigiendo a /setup por seguridad');
      return const RouteSettings(name: '/setup');
    }
  }

  /// Verifica el servidor de forma asíncrona y redirige si es necesario
  Future<void> _checkServerAndRedirect() async {
    try {
      debugPrint('🔍 Verificando servidor por error en middleware...');
      
      final systemInitService = SystemInitService();
      await systemInitService.determineInitialRoute();
      
      // Si llegamos aquí, el servidor está OK
      final serverStatus = Get.find<ServerStatusController>();
      serverStatus.markServerAvailable();
      
    } catch (e) {
      debugPrint('🔴 Error de servidor detectado en middleware: $e');
      
      // Verificar si es un error de servidor
      String errorString = e.toString().toLowerCase();
      if (errorString.contains('failed to fetch') ||
          errorString.contains('clientexception') ||
          errorString.contains('socketexception') ||
          errorString.contains('connection') ||
          errorString.contains('500') ||
          errorString.contains('server error') ||
          errorString.contains('timeout')) {
        
        // Marcar servidor como no disponible
        final serverStatus = Get.find<ServerStatusController>();
        serverStatus.markServerUnavailable();
        
        // Resetear inicialización para forzar nueva verificación
        GlobalInitMiddleware.reset();
        
        // Redirigir a error de servidor
        Get.offAllNamed('/server-error');
      }
    }
  }
}