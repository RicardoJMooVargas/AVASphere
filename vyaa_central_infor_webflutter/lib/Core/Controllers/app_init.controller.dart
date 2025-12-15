import 'package:flutter/material.dart';
import 'package:get/get.dart';

// Servicios
import '../services/system_init.service.dart';
import '../services/data/hive.service.dart';

// Controladores
import './sidebar_controller.dart';

/// Controlador de inicialización de la aplicación
/// Maneja la lógica que antes estaba en GlobalInitMiddleware
/// Determina la ruta inicial basándose en el estado del sistema y la sesión del usuario
class AppInitController extends GetxController {
  final _systemInitService = SystemInitService();
  
  /// Ruta inicial determinada
  final initialRoute = Rx<String>('/');
  
  /// Estado de carga
  final isLoading = true.obs;
  
  /// Error si existe
  final error = Rx<String?>(null);

  @override
  void onInit() {
    super.onInit();
    // Inicializar SidebarController una sola vez
    Get.put(SidebarController(), tag: 'sidebar', permanent: true);
    _determineInitialRoute();
  }

  /// Determina la ruta inicial de la aplicación
  /// Esta lógica reemplaza al GlobalInitMiddleware
  Future<void> _determineInitialRoute() async {
    try {
      debugPrint('🔍 Determinando ruta inicial de la aplicación...');
      
      // Usar SystemInitService para determinar la ruta
      final route = await _systemInitService.determineInitialRoute();
      
      // Si la ruta es /home o cualquier ruta de aplicación, redirigir a /app
      // MainAppLayout manejará la navegación interna
      if (route == '/home' || route.startsWith('/sales') || 
          route.startsWith('/inventory') || route.startsWith('/supply') ||
          route.startsWith('/projects') || route.startsWith('/system')) {
        initialRoute.value = '/app';
        // Guardar la ruta interna deseada en el SidebarController
        final sidebarController = Get.find<SidebarController>(tag: 'sidebar');
        sidebarController.updateRoute(route);
      } else {
        initialRoute.value = route;
      }
      
      isLoading.value = false;
      
      debugPrint('✅ Ruta inicial determinada: ${initialRoute.value}');
    } catch (e) {
      debugPrint('❌ Error al determinar ruta inicial: $e');
      error.value = e.toString();
      initialRoute.value = '/server-error';
      isLoading.value = false;
    }
  }

  /// Verifica si hay una sesión activa
  Future<bool> hasActiveSession() async {
    try {
      return await HiveService.hasValidSession();
    } catch (e) {
      debugPrint('❌ Error al verificar sesión: $e');
      return false;
    }
  }

  /// Verifica si el sistema está inicializado
  Future<bool> isSystemInitialized() async {
    try {
      return await HiveService.isSystemInitialized();
    } catch (e) {
      debugPrint('❌ Error al verificar sistema: $e');
      return false;
    }
  }

  /// Resuelve la ruta para MainAppLayout
  /// Las rutas de aplicación se devuelven sin cambios para que MainAppLayout las maneje
  String resolveAppRoute(String requestedRoute) {
    // Si es /app, usar /home como default
    if (requestedRoute == '/app') {
      return '/home';
    }
    
    // Para las demás rutas de aplicación, devolverlas tal cual
    final appRoutes = ['/home', '/sales', '/inventory', '/supply', '/projects', '/system'];
    
    if (appRoutes.contains(requestedRoute)) {
      debugPrint('🔀 Ruta de app detectada: $requestedRoute');
      return requestedRoute;
    }
    
    // Si no es ruta de app, devolver /home como fallback
    debugPrint('⚠️ Ruta desconocida: $requestedRoute → usando /home');
    return '/home';
  }

  /// Obtiene argumentos para pasar la ruta original al MainAppLayout
  Map<String, dynamic>? getRouteArguments(String requestedRoute) {
    final appRoutes = ['/sales', '/inventory', '/supply', '/projects', '/system'];
    
    if (appRoutes.contains(requestedRoute)) {
      return {'originalRoute': requestedRoute};
    }
    
    return null;
  }
}
