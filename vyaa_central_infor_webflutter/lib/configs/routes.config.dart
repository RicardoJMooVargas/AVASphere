import 'package:flutter/material.dart';
import 'package:vyaa_central_infor_webflutter/modules/sales/screens/sale_page.dart';

// Core imports
import '../Core/screens/system_init_screen.dart';
import '../Core/screens/server_error_screen.dart';
import '../Core/screens/not_found_screen.dart';

// Module screens
import '../modules/login/screens/login_page.dart';
import '../modules/login/screens/setup_page.dart';
import '../modules/dashboard/screens/home_page.dart';
import '../modules/sales/screens/quotation_page.dart';
import '../modules/inventory/screens/inventory_page.dart';
import '../modules/supply/screens/supply_page.dart';

// Modelo de configuración de rutas
import '../Core/models/base/route_config.module.dart';

/// ========================================
/// AppRoutes - Configuración Centralizada de Rutas
/// ========================================
///
/// PROPÓSITO:
/// Este archivo es el único lugar donde se definen todas las rutas de la aplicación.
/// Cada ruta usa el modelo RouteConfig que permite configurar:
/// - Widgets asociados
/// - Permisos y autenticación
/// - Apariencia (fullscreen vs sidebar)
/// - Jerarquía (rutas principales y subrutas)
///
/// FLUJO DE TRABAJO:
/// 1. Todas las rutas se definen aquí en la lista AppRoutes.routes
/// 2. RouteAppService procesa esta configuración y la convierte a GoRouter
/// 3. main.dart obtiene el router configurado
/// 4. MainAppLayout obtiene las rutas filtradas para el sidebar
///
/// CÓMO AGREGAR UNA NUEVA RUTA:
///
/// 1. Importar el widget de la pantalla:
///    ```dart
///    import '../modules/mi_modulo/screens/mi_pantalla.dart';
///    ```
///
/// 2. Agregar RouteConfig a la lista routes:
///    ```dart
///    RouteConfig(
///      name: 'mi_ruta',
///      path: '/mi-ruta',
///      page: () => const MiPantalla(),
///      title: 'Mi Ruta',
///      icon: Icons.icono,
///      isFullScreen: false,        // false = con sidebar
///      requiresAuth: true,          // true = requiere login
///      showInSidebar: true,         // true = aparece en menú
///      sidebarOrder: 7,             // orden en el menú
///      moduleName: 'MiModulo',      // módulo del backend
///      middlewares: const [],
///    ),
///    ```
///
/// 3. (Opcional) Agregar subrutas:
///    ```dart
///    subRoutes: [
///      RouteConfig(
///        name: 'mi_ruta_detalle',
///        path: '/mi-ruta/:id',
///        page: () => const MiPantallaDetalle(),
///        isSubRoute: true,
///        parentPath: '/mi-ruta',
///        requiresAuth: true,
///        moduleName: 'MiModulo',
///      ),
///    ],
///    ```
///
/// TIPOS DE RUTAS:
///
/// 1. RUTAS DEL SISTEMA (Pantalla completa, sin auth):
///    - / (inicialización)
///    - /server-error
///    - /404
///
/// 2. RUTAS DE AUTENTICACIÓN (Pantalla completa, sin auth):
///    - /login
///    - /setup
///
/// 3. RUTAS DE LA APLICACIÓN (Con sidebar, con auth):
///    - /home (dashboard)
///    - /sales, /inventory, /supply, etc.
///    - Estas rutas se filtran según permisos del usuario
///
/// NOTA IMPORTANTE:
/// - El campo moduleName debe coincidir con los módulos del backend
/// - Solo las rutas con showInSidebar=true aparecen en el menú
/// - El servicio RouteAppService filtra automáticamente según permisos
///
/// ========================================

/// CONFIGURACIÓN CENTRALIZADA DE RUTAS
/// Todas las rutas de la aplicación se definen aquí usando el modelo RouteConfig
/// El servicio RouteAppService procesa esta configuración y la proporciona a:
/// - MaterialApp.router (usando GoRouter)
/// - MainAppLayout (rutas de sidebar filtradas por permisos)
class AppRoutes {
  // ===== CONFIGURACIÓN PRINCIPAL DE RUTAS =====
  /// Lista maestra de todas las rutas de la aplicación
  /// Incluye rutas principales y subrutas de forma jerárquica
  static final List<RouteConfig> routes = [
    // ========================================
    // RUTAS DEL SISTEMA (Pantalla completa, sin auth)
    // ========================================
    RouteConfig(
      name: 'system_init',
      path: '/',
      page: () => const SystemInitScreen(),
      title: 'Inicialización del Sistema',
      isFullScreen: true,
      requiresAuth: false,
      showInSidebar: false,
    ),

    RouteConfig(
      name: 'server_error',
      path: '/server-error',
      page: () => const ServerErrorScreen(),
      title: 'Error del Servidor',
      isFullScreen: true,
      requiresAuth: false,
      showInSidebar: false,
    ),

    RouteConfig(
      name: 'not_found',
      path: '/404',
      page: () => const NotFoundScreen(),
      title: 'Página No Encontrada',
      isFullScreen: true,
      requiresAuth: false,
      showInSidebar: false,
    ),

    // ========================================
    // RUTAS DE AUTENTICACIÓN (Pantalla completa)
    // ========================================
    RouteConfig(
      name: 'login',
      path: '/login',
      page: () => const LoginPage(),
      title: 'Iniciar Sesión',
      isFullScreen: true,
      requiresAuth: false,
      showInSidebar: false,
      middlewares: const [],
    ),

    RouteConfig(
      name: 'setup',
      path: '/setup',
      page: () => const SetupPage(),
      title: 'Configuración Inicial',
      isFullScreen: true,
      requiresAuth: false,
      showInSidebar: false,
    ),

    // ========================================
    // CONFIGURACIÓN DE PÁGINAS INTERNAS
    // ========================================
    // Estas rutas se envuelven automáticamente con MainAppLayout cuando requiresAuth=true y isFullScreen=false

    // Dashboard / Home
    RouteConfig(
      name: 'home',
      path: '/home',
      page: () => const HomePage(),
      title: 'Dashboard',
      icon: Icons.dashboard,
      isFullScreen: false,
      requiresAuth: true,
      showInSidebar: true,
      sidebarOrder: 1,
      moduleName: 'General',
      middlewares: const [],
    ),

    // Ventas
    RouteConfig(
      name: 'sales',
      path: '/sales',
      page: () => const MainSalesPage(),
      title: 'Ventas',
      description: 'Gestión de ventas y clientes',
      icon: Icons.sell_outlined,
      isFullScreen: false,
      requiresAuth: true,
      showInSidebar: true,
      sidebarOrder: 2,
      moduleName: 'Sales',
      middlewares: const [],
      subRoutes: [
        RouteConfig(
          name: 'quotation',
          path: '/sales/quotation',
          page: () => const MainSalesPage(),
          title: 'Cotizaciones',
          isSubRoute: true,
          parentPath: '/sales',
          requiresAuth: true,
          moduleName: 'Sales',
        ),
        RouteConfig(
          name: 'sale_detail',
          path: '/sales/:saleId',
          page: () => SalePage(),
          title: 'Detalle de Venta',
          isSubRoute: true,
          parentPath: '/sales',
          requiresAuth: true,
          moduleName: 'Sales',
        ),
      ],
    ),

    // Inventario
    RouteConfig(
      name: 'inventory',
      path: '/inventory',
      page: () => const InventoryPage(),
      title: 'Inventario',
      description: 'Gestión de productos y stock',
      icon: Icons.inventory_2_outlined,
      isFullScreen: false,
      requiresAuth: true,
      showInSidebar: true,
      sidebarOrder: 3,
      moduleName: 'Inventory',
      middlewares: const [],
    ),

    // Compras
    RouteConfig(
      name: 'supply',
      path: '/supply',
      page: () => const SupplyPage(),
      title: 'Compras',
      description: 'Gestión de compras y proveedores',
      icon: Icons.shopping_cart_outlined,
      isFullScreen: false,
      requiresAuth: true,
      showInSidebar: true,
      sidebarOrder: 4,
      moduleName: 'Shopping',
      middlewares: const [],
    ),

    // Proyectos
    /*
    RouteConfig(
      name: 'projects',
      path: '/projects',
      page: null, // Sin página - solo configuración
      title: 'Proyectos',
      description: 'Gestión de proyectos',
      icon: Icons.folder_outlined,
      isFullScreen: false,
      requiresAuth: true,
      showInSidebar: true,
      sidebarOrder: 5,
      moduleName: 'Projects',
      middlewares: const [],
      // Ejemplo de cómo definir subrutas jerárquicas
      // subRoutes: [
      //   RouteConfig(
      //     name: 'project_detail',
      //     path: '/projects/:projectCode',
      //     page: () => const ProjectDetailPage(),
      //     title: 'Detalle de Proyecto',
      //     isSubRoute: true,
      //     parentPath: '/projects',
      //     requiresAuth: true,
      //     moduleName: 'Projects',
      //     subRoutes: [
      //       RouteConfig(
      //         name: 'project_client',
      //         path: '/projects/:projectCode/:clientCode',
      //         page: () => const ProjectClientPage(),
      //         title: 'Cliente del Proyecto',
      //         isSubRoute: true,
      //         parentPath: '/projects/:projectCode',
      //         requiresAuth: true,
      //         moduleName: 'Projects',
      //       ),
      //     ],
      //   ),
      // ],
    ),
    */

    // Sistema
    RouteConfig(
      name: 'system',
      path: '/system',
      page: null, // Sin página - solo configuración
      title: 'Sistema',
      description: 'Configuración del sistema',
      icon: Icons.settings_outlined,
      isFullScreen: false,
      requiresAuth: true,
      showInSidebar: true,
      sidebarOrder: 6,
      moduleName: 'System',
      middlewares: const [],
    ),
  ];

  // ===== RUTA PARA PÁGINAS NO ENCONTRADAS =====
  /// Configuración de la ruta 404
  static RouteConfig get unknownRoute => RouteConfig(
        name: 'not_found',
        path: '/404',
        page: () => const NotFoundScreen(),
        title: 'Página No Encontrada',
        isFullScreen: true,
        requiresAuth: false,
        showInSidebar: false,
      );

  // ===== MÉTODOS UTILITARIOS =====

  /// Obtiene todas las rutas configuradas (lista plana con subrutas)
  static List<RouteConfig> getAllRoutes() {
    List<RouteConfig> allRoutes = [];
    for (var route in routes) {
      allRoutes.addAll(route.getAllRoutes());
    }
    return allRoutes;
  }

  /// Obtiene solo las rutas principales (sin subrutas)
  static List<RouteConfig> getMainRoutes() {
    return routes;
  }

  /// Obtiene las rutas que se muestran en el sidebar
  static List<RouteConfig> getSidebarRoutes() {
    return routes
        .where((route) => !route.isSubRoute && route.showInSidebar)
        .toList()
      ..sort(
        (a, b) => (a.sidebarOrder ?? 999).compareTo(b.sidebarOrder ?? 999),
      );
  }

  /// Busca una ruta por su path
  static RouteConfig? findRouteByPath(String path) {
    final allRoutes = getAllRoutes();
    for (var route in allRoutes) {
      if (route.matchesPath(path)) {
        return route;
      }
    }
    return null;
  }

  /// Busca una ruta por su name
  static RouteConfig? findRouteByName(String name) {
    final allRoutes = getAllRoutes();
    try {
      return allRoutes.firstWhere((route) => route.name == name);
    } catch (e) {
      return null;
    }
  }

  /// Verifica si una ruta requiere el MainAppLayout
  static bool requiresMainLayout(String path) {
    final route = findRouteByPath(path);
    return route != null && !route.isFullScreen && route.requiresAuth;
  }

  /// Verifica si una ruta es de autenticación
  static bool isAuthRoute(String path) {
    final authPaths = ['/login', '/setup'];
    return authPaths.contains(path);
  }

  /// Verifica si una ruta es del sistema
  static bool isSystemRoute(String path) {
    final systemPaths = ['/', '/server-error', '/404'];
    return systemPaths.contains(path);
  }

  /// Obtiene el título de una ruta
  static String? getRouteTitle(String path) {
    final route = findRouteByPath(path);
    return route?.title;
  }

  /// Debug: imprime información de las rutas
  static void debugRoutes() {
    debugPrint('\n🛣️ === CONFIGURACIÓN DE RUTAS ===');
    debugPrint('📊 Total: ${routes.length} rutas principales');
    debugPrint('📊 Total con subrutas: ${getAllRoutes().length} rutas');

    final sidebarRoutes = getSidebarRoutes();
    debugPrint('📋 Sidebar: ${sidebarRoutes.length} items');

    for (final route in sidebarRoutes) {
      final subs = route.hasSubRoutes
          ? ' (${route.totalSubRoutes} subrutas)'
          : '';
      debugPrint(
        '   ${route.sidebarOrder}. ${route.title} -> ${route.path}$subs',
      );
    }

    debugPrint('🛣️ === FIN CONFIGURACIÓN ===\n');
  }

  // ===== CONSTANTES DE RUTAS (Para compatibilidad con código existente) =====
  static const String systemInit = '/';
  static const String login = '/login';
  static const String setup = '/setup';
  static const String serverError = '/server-error';
  static const String home = '/home';
  static const String sales = '/sales';
  static const String inventory = '/inventory';
  static const String supply = '/supply';
  static const String projects = '/projects';
  static const String system = '/system';
}

/// ===== MODELO SIMPLE PARA SIDEBAR (Para compatibilidad) =====
/// DEPRECATED: Usar RouteConfig directamente en su lugar
@Deprecated('Usar RouteConfig directamente')
class SidebarItemData {
  final String title;
  final IconData icon;
  final String route;
  final int order;
  final bool enabled;
  final String? moduleName;

  const SidebarItemData({
    required this.title,
    required this.icon,
    required this.route,
    required this.order,
    this.enabled = true,
    this.moduleName,
  });

  @override
  String toString() => 'SidebarItem($title -> $route [module: $moduleName])';
}

/// ===== NOTA SOBRE NAVEGACIÓN =====
/// La navegación ahora se maneja con go_router a través de:
/// - RouteAppService.navigateTo(context, route)
/// - context.toAppRoute(route) - extensión de BuildContext
/// - context.go(route) - método nativo de go_router
///
/// Las extensiones están disponibles en route_app.service.dart
