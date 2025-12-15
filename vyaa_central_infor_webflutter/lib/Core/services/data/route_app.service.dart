import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';

// Core imports
import '../../layouts/main_app_layout.dart';

// Configuración de rutas
import '../../../configs/routes.config.dart';

// Modelo de configuración de rutas
import '../../models/base/route_config.module.dart';

/// ========================================
/// RouteAppService - Servicio de Rutas
/// ========================================
///
/// PROPÓSITO:
/// Traduce la configuración de rutas (RouteConfig) a GoRouter.
/// Convierte la estructura jerárquica y configuración de rutas
/// en un GoRouter funcional que Flutter puede usar.
///
/// CARACTERÍSTICAS:
/// - Convierte RouteConfig a GoRoute
/// - Maneja rutas con y sin sidebar (MainAppLayout)
/// - Gestiona subrutas y rutas anidadas
/// - Redirige rutas basándose en autenticación y configuración
/// - Provee redirección global y manejo de errores
///
/// USO:
/// ```dart
/// final router = RouteAppService.createRouter();
/// 
/// MaterialApp.router(
///   routerConfig: router,
/// )
/// ```
///
/// ========================================

class RouteAppService {
  /// Crea el GoRouter a partir de la configuración de rutas
  static GoRouter createRouter({String? initialLocation}) {
    return GoRouter(
      initialLocation: initialLocation ?? '/',
      debugLogDiagnostics: true,
      routes: _buildRoutes(),
      errorBuilder: (context, state) {
        final page = AppRoutes.unknownRoute.page;
        return page != null ? page() : const SizedBox();
      },
      redirect: (context, state) => _handleRedirect(context, state),
    );
  }

  /// Construye todas las rutas para GoRouter
  static List<RouteBase> _buildRoutes() {
    final List<RouteBase> routes = [];

    // Convertir cada RouteConfig a GoRoute
    for (final routeConfig in AppRoutes.routes) {
      final goRoute = _convertToGoRoute(routeConfig);
      if (goRoute != null) {
        routes.add(goRoute);
      }
    }

    // Agregar ruta especial para app (con MainAppLayout)
    routes.add(_createAppRoute());

    return routes;
  }

  /// Convierte un RouteConfig a GoRoute
  static GoRoute? _convertToGoRoute(RouteConfig routeConfig) {
    // Si no hay página definida, no crear ruta
    if (routeConfig.page == null) return null;
    
    // No crear rutas para páginas que requieren MainAppLayout
    // Esas se manejan internamente en /app
    if (routeConfig.requiresAuth && !routeConfig.isFullScreen) {
      return null;
    }

    // Construir subrutas si existen
    final List<RouteBase> subRoutes = [];
    if (routeConfig.hasSubRoutes) {
      for (final subRoute in routeConfig.subRoutes!) {
        final convertedSubRoute = _convertToGoRoute(subRoute);
        if (convertedSubRoute != null) {
          subRoutes.add(convertedSubRoute);
        }
      }
    }

    return GoRoute(
      path: routeConfig.path,
      name: routeConfig.name,
      builder: (context, state) => routeConfig.page!(),
      routes: subRoutes,
    );
  }

  /// Crea la ruta especial /app que envuelve las rutas internas con MainAppLayout
  static ShellRoute _createAppRoute() {
    return ShellRoute(
      builder: (context, state, child) {
        // MainAppLayout envuelve todas las rutas internas
        // child contiene el widget de la ruta actual (home, sales, etc.)
        return MainAppLayout(child: child);
      },
      routes: _buildInternalRoutes(),
    );
  }

  /// Construye las rutas internas que se manejan dentro de MainAppLayout
  static List<RouteBase> _buildInternalRoutes() {
    final List<RouteBase> routes = [];

    // Filtrar solo rutas que van con sidebar (requiresAuth y !isFullScreen)
    final appRoutes = AppRoutes.routes.where(
      (route) => route.requiresAuth && !route.isFullScreen,
    );

    for (final routeConfig in appRoutes) {
      // Para ShellRoute, las rutas hijas deben incluir /app en el path
      final internalPath = '/app${routeConfig.path}';

      final List<RouteBase> subRoutes = [];
      if (routeConfig.hasSubRoutes) {
        for (final subRoute in routeConfig.subRoutes!) {
          if (subRoute.page != null) {
            // Las subrutas son relativas a su padre
            final subPath = subRoute.path.replaceFirst(routeConfig.path, '');
            final cleanSubPath = subPath.startsWith('/') ? subPath.substring(1) : subPath;
            
            // Usar un nombre único que incluya el prefijo del padre
            subRoutes.add(
              GoRoute(
                path: cleanSubPath,
                name: '${routeConfig.name}_${subRoute.name}',
                builder: (context, state) => subRoute.page!(),
              ),
            );
          }
        }
      }

      if (routeConfig.page != null) {
        routes.add(
          GoRoute(
            path: internalPath,
            name: 'app_${routeConfig.name}',
            builder: (context, state) => routeConfig.page!(),
            routes: subRoutes,
          ),
        );
      }
    }

    return routes;
  }

  /// Maneja redirecciones globales
  static String? _handleRedirect(BuildContext context, GoRouterState state) {
    final location = state.uri.toString();
    final path = state.uri.path;
    
    debugPrint('🧭 GoRouter: Navegando a $location');

    // Manejar /app?route=/sales -> /app/sales
    if (path == '/app' && state.uri.queryParameters.containsKey('route')) {
      final targetRoute = state.uri.queryParameters['route']!;
      // Convertir /home -> /app/home, /sales -> /app/sales
      final cleanRoute = targetRoute.startsWith('/') ? targetRoute.substring(1) : targetRoute;
      debugPrint('🔄 Redirigiendo de $location a /app/$cleanRoute');
      return '/app/$cleanRoute';
    }

    // Permitir navegación a rutas del sistema y autenticación
    if (AppRoutes.isSystemRoute(location) || AppRoutes.isAuthRoute(location)) {
      return null; // No redirigir
    }

    // Si intenta acceder a una ruta directa que requiere MainLayout, redirigir a /app/ruta
    if (AppRoutes.requiresMainLayout(location) && !path.startsWith('/app/')) {
      final cleanPath = path.startsWith('/') ? path.substring(1) : path;
      debugPrint('🔄 Redirigiendo de $location a /app/$cleanPath');
      return '/app/$cleanPath';
    }

    return null; // No redirigir
  }

  /// Obtiene las rutas filtradas para el sidebar según permisos del usuario
  /// Este método será usado por MainAppLayout para mostrar el menú
  static List<RouteConfig> getSidebarRoutes({List<String>? userModules}) {
    var routes = AppRoutes.getSidebarRoutes();

    // Si hay módulos de usuario, filtrar por ellos
    if (userModules != null && userModules.isNotEmpty) {
      routes = routes.where((route) {
        // Si la ruta no tiene moduleName, mostrarla siempre
        if (route.moduleName == null) return true;

        // Verificar si el usuario tiene acceso al módulo
        return userModules.contains(route.moduleName);
      }).toList();
    }

    return routes;
  }

  /// Navega a una ruta usando GoRouter
  static void navigateTo(BuildContext context, String path) {
    debugPrint('🧭 Navegando a: $path');
    context.go(path);
  }

  /// Navega reemplazando la ruta actual
  static void navigateReplace(BuildContext context, String path) {
    debugPrint('🔄 Reemplazando con: $path');
    context.go(path);
  }

  /// Navega y limpia el stack
  static void navigateAndClear(BuildContext context, String path) {
    debugPrint('🧹 Limpiando y navegando a: $path');
    context.go(path);
  }

  /// Navega al home
  static void goToHome(BuildContext context) {
    debugPrint('🏠 Navegando al home');
    context.go('/app/home');
  }

  /// Navega al login
  static void goToLogin(BuildContext context) {
    debugPrint('🔑 Navegando al login');
    context.go('/login');
  }

  /// Navega al setup
  static void goToSetup(BuildContext context) {
    debugPrint('⚙️ Navegando al setup');
    context.go('/setup');
  }

  /// Navega al error del servidor
  static void goToServerError(BuildContext context) {
    debugPrint('❌ Navegando a error del servidor');
    context.go('/server-error');
  }

  /// Debug: imprime información de las rutas
  static void debugRoutes() {
    debugPrint('\n🛣️ === CONFIGURACIÓN DE GO_ROUTER ===');
    AppRoutes.debugRoutes();
    debugPrint('🛣️ === FIN CONFIGURACIÓN ===\n');
  }
}

/// ===== EXTENSIONES PARA NAVEGACIÓN SIMPLIFICADA =====
extension GoRouterExtension on BuildContext {
  /// Navegación simple a cualquier ruta de la app
  void toAppRoute(String route) {
    RouteAppService.navigateTo(this, route);
  }

  /// Reemplazar ruta actual
  void offAppRoute(String route) {
    RouteAppService.navigateReplace(this, route);
  }

  /// Ir al inicio
  void offAllToHome() {
    RouteAppService.goToHome(this);
  }

  /// Ir al login
  void offAllToLogin() {
    RouteAppService.goToLogin(this);
  }

  /// Ir a setup
  void offAllToSetup() {
    RouteAppService.goToSetup(this);
  }

  /// Ir al error del servidor
  void toServerError() {
    RouteAppService.goToServerError(this);
  }
}
