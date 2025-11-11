import 'package:flutter/material.dart';
import 'package:get/get.dart';

// Core imports
import '../Core/screens/system_init_screen.dart';
import '../Core/screens/server_error_screen.dart';
import '../Core/screens/not_found_screen.dart';
import '../Core/layouts/main_app_layout.dart';
import '../Core/middlewares/global_init.middleware.dart';

// Module screens
import '../modules/login/screens/login_page.dart';
import '../modules/login/screens/setup_page.dart';

/// CONFIGURACIÓN SIMPLE Y FUNCIONAL DE RUTAS
/// Esta configuración evita problemas de GlobalKey y middlewares complejos
class AppRoutes {
  // ===== CONSTANTES DE RUTAS =====
  static const String systemInit = '/';
  static const String login = '/login';
  static const String setup = '/setup';
  static const String serverError = '/server-error';
  static const String home = '/home';
  static const String sales = '/sales';
  static const String inventory = '/inventory';
  static const String supply = '/supply';

  /// ===== CONFIGURACIÓN PRINCIPAL =====
  /// Lista simple y funcional de rutas para GetMaterialApp
  static List<GetPage> get getPages => [
    // SISTEMA - Sin middleware para evitar conflictos
    GetPage(
      name: systemInit,
      page: () => const SystemInitScreen(),
      transition: Transition.noTransition,
    ),
    
    GetPage(
      name: serverError,
      page: () => const ServerErrorScreen(),
      transition: Transition.noTransition,
    ),

    // AUTENTICACIÓN - Middleware simple y confiable
    GetPage(
      name: login,
      page: () => const LoginPage(),
      middlewares: [GlobalInitMiddleware()],
      transition: Transition.fadeIn,
    ),
    
    GetPage(
      name: setup,
      page: () => const SetupPage(),
      transition: Transition.fadeIn,
    ),

    // APLICACIÓN PRINCIPAL - UNA SOLA INSTANCIA DE MainAppLayout
    GetPage(
      name: home,
      page: () => const MainAppLayout(),
      middlewares: [GlobalInitMiddleware()],
      transition: Transition.noTransition,
    ),
    
    // ALIAS PARA NAVEGACIÓN INTERNA - Todas redirigen a /home
    GetPage(
      name: sales,
      page: () => const MainAppLayout(),
      middlewares: [GlobalInitMiddleware()],
      transition: Transition.noTransition,
    ),
    
    GetPage(
      name: inventory,
      page: () => const MainAppLayout(),
      middlewares: [GlobalInitMiddleware()],
      transition: Transition.noTransition,
    ),
    
    GetPage(
      name: supply,
      page: () => const MainAppLayout(),
      middlewares: [GlobalInitMiddleware()],
      transition: Transition.noTransition,
    ),
  ];
  /// ===== CONFIGURACIÓN DEL SIDEBAR =====
  /// Items del sidebar para MainAppLayout (navegación interna)
  static const List<SidebarItemData> sidebarItems = [
    SidebarItemData(
      title: 'Dashboard',
      icon: Icons.dashboard,
      route: '/home',
      order: 1,
    ),
    SidebarItemData(
      title: 'Ventas',
      icon: Icons.sell_outlined,
      route: '/sales',
      order: 2,
    ),
    SidebarItemData(
      title: 'Inventario',
      icon: Icons.inventory_2_outlined,
      route: '/inventory',
      order: 3,
    ),
    SidebarItemData(
      title: 'Suministros',
      icon: Icons.local_shipping_outlined,
      route: '/supply',
      order: 4,
    ),
  ];
  /// ===== RUTA PARA PÁGINAS NO ENCONTRADAS =====
  /// Página que se muestra cuando no se encuentra una ruta
  static GetPage get unknownRoute => GetPage(
    name: '/404',
    page: () => const NotFoundScreen(),
    transition: Transition.noTransition,
  );

  /// ===== MÉTODOS UTILITARIOS =====
  
  /// Obtiene los items del sidebar ordenados
  static List<SidebarItemData> getSidebarItemsOrdered() {
    List<SidebarItemData> items = List.from(sidebarItems);
    items.sort((a, b) => a.order.compareTo(b.order));
    return items;
  }

  /// Verifica si una ruta requiere el MainAppLayout
  static bool requiresMainLayout(String route) {
    const appRoutes = [home, sales, inventory, supply];
    return appRoutes.contains(route);
  }

  /// Verifica si una ruta es de autenticación
  static bool isAuthRoute(String route) {
    const authRoutes = [login, setup];
    return authRoutes.contains(route);
  }

  /// Verifica si una ruta es del sistema
  static bool isSystemRoute(String route) {
    const systemRoutes = [systemInit, serverError];
    return systemRoutes.contains(route);
  }

  /// Obtiene el título de una ruta
  static String? getRouteTitle(String route) {
    try {
      return sidebarItems.firstWhere((item) => item.route == route).title;
    } catch (e) {
      return null;
    }
  }

  /// Debug: imprime información de las rutas
  static void debugRoutes() {
    debugPrint('\n🛣️ === CONFIGURACIÓN DE RUTAS ===');
    debugPrint('📊 Total: ${getPages.length} rutas principales');
    debugPrint('🏠 Principal: $home');
    debugPrint('🔐 Auth: $login, $setup');
    debugPrint('⚙️ Sistema: $systemInit, $serverError');
    debugPrint('📋 Sidebar: ${sidebarItems.length} items');
    
    for (final item in getSidebarItemsOrdered()) {
      debugPrint('   ${item.order}. ${item.title} -> ${item.route}');
    }
    
    debugPrint('🛣️ === FIN CONFIGURACIÓN ===\n');
  }
}

/// ===== MODELO SIMPLE PARA SIDEBAR =====
class SidebarItemData {
  final String title;
  final IconData icon;
  final String route;
  final int order;
  final bool enabled;

  const SidebarItemData({
    required this.title,
    required this.icon,
    required this.route,
    required this.order,
    this.enabled = true,
  });

  @override
  String toString() => 'SidebarItem($title -> $route)';
}

/// ===== EXTENSIONES PARA NAVEGACIÓN SIMPLIFICADA =====
extension AppNavigation on GetInterface {
  /// Navegación simple a cualquier ruta de la app
  void toAppRoute(String route) {
    final title = AppRoutes.getRouteTitle(route);
    debugPrint('🧭 Navegando a: ${title ?? route}');
    Get.toNamed(route);
  }

  /// Reemplazar ruta actual
  void offAppRoute(String route) {
    final title = AppRoutes.getRouteTitle(route);
    debugPrint('🔄 Reemplazando con: ${title ?? route}');
    Get.offNamed(route);
  }

  /// Ir al inicio limpiando todo
  void offAllToHome() {
    debugPrint('🏠 Navegando al inicio');
    Get.offAllNamed(AppRoutes.home);
  }

  /// Ir al login limpiando todo
  void offAllToLogin() {
    debugPrint('🔑 Navegando al login');
    Get.offAllNamed(AppRoutes.login);
  }

  /// Ir a setup limpiando todo
  void offAllToSetup() {
    debugPrint('⚙️ Navegando al setup');
    Get.offAllNamed(AppRoutes.setup);
  }

  /// Ir al error del servidor
  void toServerError() {
    debugPrint('❌ Navegando a error del servidor');
    Get.toNamed(AppRoutes.serverError);
  }
}