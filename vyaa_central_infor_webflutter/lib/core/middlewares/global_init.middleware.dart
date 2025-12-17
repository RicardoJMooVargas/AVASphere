import 'package:flutter/material.dart';
import 'package:get/get.dart';
import '../services/system_init.service.dart';

/// Middleware global simplificado que usa SystemInitService para determinar rutas
class GlobalInitMiddleware extends GetMiddleware {
  @override
  int? get priority => 1; // Prioridad normal
  static bool _hasCheckedInitialConfig = false;
  static String? _determinedRoute;
  /// Verifica si ya se revisó la configuración inicial
  static bool get hasCheckedInitialConfig => _hasCheckedInitialConfig;
  /// Obtiene la ruta determinada
  static String? get determinedRoute => _determinedRoute;


  /// Marca que ya se verificó la configuración inicial
  static void markAsChecked(String route) {
    _hasCheckedInitialConfig = true;
    _determinedRoute = route;
    debugPrint('✅ Configuración inicial verificada - Ruta determinada: $route');
  }

  /// Resetea el estado de verificación
  static void reset() {
    _hasCheckedInitialConfig = false;
    _determinedRoute = null;
    debugPrint('🔄 Estado de middleware reseteado');
  }

  @override
  RouteSettings? redirect(String? route) {
    debugPrint('🌐 GlobalInitMiddleware interceptando ruta: $route');

    // Permitir acceso directo a rutas del sistema sin verificación
    final systemRoutes = [
      '/', // System init screen - debe procesar siempre
      '/server-error',
      '/setup',
    ];
    
    // Rutas de la aplicación principal que deben redirigir a /home
    final appRoutes = [
      '/sales',
      '/inventory',
      '/supply',
    ];
    
    if (systemRoutes.contains(route)) {
      debugPrint('✅ Permitiendo acceso directo a ruta del sistema: $route');
      return null;
    }
    
    // Redirigir rutas de app internas a /home una vez verificada la configuración inicial
    if (appRoutes.contains(route) && _hasCheckedInitialConfig) {
      debugPrint('🔀 Redirigiendo ruta de app $route a /home con argumentos');
      return RouteSettings(
        name: '/home',
        arguments: {'originalRoute': route},
      );
    }

    // Para otras rutas, verificar si ya se determinó la ruta correcta
    if (!_hasCheckedInitialConfig) {
      debugPrint('🚫 Configuración no verificada - Redirigiendo a SystemInitScreen');
      return const RouteSettings(name: '/');
    }

    // Si ya se verificó y la ruta actual no es la determinada, redirigir
    if (_determinedRoute != null && route != _determinedRoute) {
      debugPrint('🔀 Redirigiendo de $route a $_determinedRoute (ruta determinada)');
      return RouteSettings(name: _determinedRoute);
    }
    


    // Permitir navegación normal
    debugPrint('✅ Permitiendo navegación a: $route');
    return null;
  }

  /// Método para usar SystemInitService y determinar ruta
  static Future<String> checkAndDetermineRoute() async {
    try {
      debugPrint('🔍 Usando SystemInitService para determinar ruta...');
      final systemInitService = SystemInitService();
      final route = await systemInitService.determineInitialRoute();
      
      markAsChecked(route);
      return route;
    } catch (e) {
      debugPrint('❌ Error al determinar ruta: $e');
      
      // En caso de error, marcar como verificado con ruta de error
      markAsChecked('/server-error');
      return '/server-error';
    }
  }
}
