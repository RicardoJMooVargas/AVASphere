import 'package:flutter/material.dart';
import 'package:get/get.dart';

/// ========================================
/// RouteConfig - Modelo de Configuración de Rutas
/// ========================================
/// 
/// PROPÓSITO:
/// Define la estructura de configuración para cada ruta de la aplicación.
/// Permite crear jerarquías de rutas (principal → subrutas → subrutas anidadas)
/// y controlar el comportamiento de cada ruta (pantalla completa, sidebar, permisos).
/// 
/// CARACTERÍSTICAS PRINCIPALES:
/// 
/// 1. JERARQUÍA DE RUTAS:
///    - Rutas principales (isSubRoute = false)
///    - Subrutas de primer nivel (parentPath apunta a la ruta principal)
///    - Subrutas anidadas (múltiples niveles de profundidad)
///    
///    Ejemplo:
///    /projects (principal)
///      ├─ /projects/:projectCode (subruta)
///      │   └─ /projects/:projectCode/:clientCode (subruta anidada)
///      └─ /projects/new (subruta)
/// 
/// 2. CONTROL DE LAYOUT:
///    - isFullScreen: true → Sin sidebar (login, setup, errores)
///    - isFullScreen: false → Con sidebar (app principal)
/// 
/// 3. GESTIÓN DE PERMISOS:
///    - requiresAuth: true/false (requiere autenticación)
///    - moduleName: nombre del módulo del backend para verificar acceso
///    - El servicio RouteAppService filtra rutas basándose en estos campos
/// 
/// 4. INTEGRACIÓN CON SIDEBAR:
///    - showInSidebar: true/false (aparece en el menú lateral)
///    - sidebarOrder: orden de aparición
///    - icon, title: para renderizar en la UI
/// 
/// USO:
/// ```dart
/// RouteConfig(
///   name: 'sales',
///   path: '/sales',
///   page: () => const SalesPage(),
///   title: 'Ventas',
///   icon: Icons.sell_outlined,
///   isFullScreen: false,           // Muestra con sidebar
///   requiresAuth: true,             // Requiere autenticación
///   showInSidebar: true,            // Aparece en el menú
///   sidebarOrder: 2,                // Segunda posición
///   moduleName: 'Sales',            // Módulo del backend
///   middlewares: [AuthMiddleware()],
///   transition: Transition.fadeIn,
///   
///   // Subrutas opcionales
///   subRoutes: [
///     RouteConfig(
///       name: 'sale_detail',
///       path: '/sales/:id',
///       page: () => const SaleDetailPage(),
///       isSubRoute: true,
///       parentPath: '/sales',
///       moduleName: 'Sales',
///     ),
///   ],
/// )
/// ```
/// 
/// MÉTODOS ÚTILES:
/// - getAllRoutes(): retorna ruta + todas sus subrutas recursivamente
/// - toGetPage(): convierte la configuración a GetPage de GetX
/// - getAllGetPages(): convierte ruta + subrutas a lista de GetPage
/// - matchesPath(): verifica si un path coincide con esta ruta
/// 
/// ========================================

/// Modelo de configuración centralizada para rutas de la aplicación
/// Permite definir rutas principales y subrutas de forma jerárquica
/// Controla si una ruta es pantalla completa o con sidebar
class RouteConfig {
  /// Identificador único de la ruta (ej: 'home', 'sales', 'projects')
  final String name;
  
  /// Ruta completa (ej: '/home', '/sales', '/projects/:projectCode/:clientCode')
  final String path;
  
  /// Widget que se renderiza en esta ruta
  /// Null si la ruta es solo configuración (no se convierte a GetPage)
  final Widget Function()? page;
  
  /// Título de la ruta para mostrar en UI
  final String? title;
  
  /// Descripción opcional de la ruta
  final String? description;
  
  /// Icono para mostrar en sidebar o navegación
  final IconData? icon;
  
  /// Define si la ruta se muestra en pantalla completa (sin sidebar)
  /// true = pantalla completa, false = con sidebar
  final bool isFullScreen;
  
  /// Indica si la ruta requiere autenticación
  final bool requiresAuth;
  
  /// Middlewares que se ejecutan antes de acceder a la ruta
  final List<GetMiddleware> middlewares;
  
  /// Transición de navegación
  final Transition? transition;
  
  /// Duración de la transición
  final Duration? transitionDuration;
  
  /// Define si la ruta se muestra en el sidebar
  final bool showInSidebar;
  
  /// Orden de aparición en el sidebar (menor número = más arriba)
  final int? sidebarOrder;
  
  /// Nombre del módulo del backend para verificar permisos
  /// Debe coincidir con los módulos que retorna el backend
  final String? moduleName;
  
  /// Color de fondo de la ruta (opcional)
  final Color? backgroundColor;
  
  /// Subrutas anidadas bajo esta ruta principal
  /// Ej: /projects puede tener /projects/:id, /projects/:id/:clientCode
  final List<RouteConfig>? subRoutes;
  
  /// Indica si es una subruta (true) o ruta principal (false)
  final bool isSubRoute;
  
  /// Ruta padre (solo si es subruta)
  final String? parentPath;

  const RouteConfig({
    required this.name,
    required this.path,
    this.page,
    this.title,
    this.description,
    this.icon,
    this.isFullScreen = false,
    this.requiresAuth = true,
    this.middlewares = const [],
    this.transition,
    this.transitionDuration,
    this.showInSidebar = false,
    this.sidebarOrder,
    this.moduleName,
    this.backgroundColor,
    this.subRoutes,
    this.isSubRoute = false,
    this.parentPath,
  });

  /// Obtiene todas las rutas (principal + subrutas) como lista plana
  List<RouteConfig> getAllRoutes() {
    List<RouteConfig> routes = [this];
    
    if (subRoutes != null && subRoutes!.isNotEmpty) {
      for (var subRoute in subRoutes!) {
        routes.addAll(subRoute.getAllRoutes());
      }
    }
    
    return routes;
  }

  /// Verifica si tiene subrutas
  bool get hasSubRoutes => subRoutes != null && subRoutes!.isNotEmpty;

  /// Obtiene el número total de subrutas (incluyendo anidadas)
  int get totalSubRoutes {
    if (subRoutes == null || subRoutes!.isEmpty) return 0;
    
    int count = subRoutes!.length;
    for (var subRoute in subRoutes!) {
      count += subRoute.totalSubRoutes;
    }
    return count;
  }

  /// Convierte la ruta a GetPage para GetMaterialApp
  /// Retorna null si la ruta no tiene página (es solo configuración)
  GetPage? toGetPage() {
    // Si no hay página, esta ruta es solo configuración
    if (page == null) return null;
    
    return GetPage(
      name: path,
      page: page!,
      // Crear una lista mutable si hay middlewares, vacía si no
      middlewares: middlewares.isEmpty ? [] : List.from(middlewares),
      transition: transition ?? Transition.noTransition,
      transitionDuration: transitionDuration,
    );
  }

  /// Obtiene todas las GetPages (principal + subrutas)
  /// Solo incluye rutas que tienen página definida
  List<GetPage> getAllGetPages() {
    return getAllRoutes()
        .map((route) => route.toGetPage())
        .where((page) => page != null)
        .cast<GetPage>()
        .toList();
  }

  /// Verifica si la ruta coincide con un path dado
  bool matchesPath(String testPath) {
    // Comparación exacta
    if (path == testPath) return true;
    
    // Verificar en subrutas
    if (hasSubRoutes) {
      for (var subRoute in subRoutes!) {
        if (subRoute.matchesPath(testPath)) return true;
      }
    }
    
    return false;
  }

  @override
  String toString() {
    String routeType = isFullScreen ? 'Fullscreen' : 'With Sidebar';
    String sidebar = showInSidebar ? 'Visible in Sidebar' : 'Hidden';
    String auth = requiresAuth ? 'Auth Required' : 'Public';
    String subs = hasSubRoutes ? '${totalSubRoutes} subroutes' : 'No subroutes';
    
    return 'RouteConfig($name -> $path [$routeType, $sidebar, $auth, $subs])';
  }

  /// Copia la configuración con cambios
  RouteConfig copyWith({
    String? name,
    String? path,
    Widget Function()? page,
    String? title,
    String? description,
    IconData? icon,
    bool? isFullScreen,
    bool? requiresAuth,
    List<GetMiddleware>? middlewares,
    Transition? transition,
    Duration? transitionDuration,
    bool? showInSidebar,
    int? sidebarOrder,
    String? moduleName,
    Color? backgroundColor,
    List<RouteConfig>? subRoutes,
    bool? isSubRoute,
    String? parentPath,
  }) {
    return RouteConfig(
      name: name ?? this.name,
      path: path ?? this.path,
      page: page ?? this.page,
      title: title ?? this.title,
      description: description ?? this.description,
      icon: icon ?? this.icon,
      isFullScreen: isFullScreen ?? this.isFullScreen,
      requiresAuth: requiresAuth ?? this.requiresAuth,
      middlewares: middlewares ?? this.middlewares,
      transition: transition ?? this.transition,
      transitionDuration: transitionDuration ?? this.transitionDuration,
      showInSidebar: showInSidebar ?? this.showInSidebar,
      sidebarOrder: sidebarOrder ?? this.sidebarOrder,
      moduleName: moduleName ?? this.moduleName,
      backgroundColor: backgroundColor ?? this.backgroundColor,
      subRoutes: subRoutes ?? this.subRoutes,
      isSubRoute: isSubRoute ?? this.isSubRoute,
      parentPath: parentPath ?? this.parentPath,
    );
  }
}