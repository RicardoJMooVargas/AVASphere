import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import '../widgets/system/app_sidebar.widget.dart';
import '../services/data/route_app.service.dart';

/// MainAppLayout
/// Layout principal de la aplicación con sidebar
/// 
/// Envuelve todas las rutas internas de la aplicación (/app/home, /app/sales, etc.)
/// y muestra el sidebar con navegación y el contenido dinámico.
class MainAppLayout extends StatelessWidget {
  final Widget child;
  
  const MainAppLayout({super.key, required this.child});

  @override
  Widget build(BuildContext context) {
    // Obtener la ruta actual
    final location = GoRouterState.of(context).uri.path;
    
    // Obtener las rutas del sidebar filtradas por permisos
    // TODO: Pasar los módulos del usuario desde el estado de autenticación
    final sidebarRoutes = RouteAppService.getSidebarRoutes();
    
    // Convertir RouteConfig a SidebarItem
    final sidebarItems = sidebarRoutes.map((route) {
      return SidebarItem(
        name: route.title ?? 'Sin título',
        icon: route.icon ?? Icons.circle,
        route: '/app${route.path}', // Prefijo /app para rutas internas
        onPress: () {
          // Navegar a la ruta interna
          final targetPath = '/app${route.path}';
          debugPrint('📍 Navegando desde sidebar a: $targetPath');
          context.go(targetPath);
        },
      );
    }).toList();
    
    return AppSidebar(
      body: child, // El contenido dinámico de la ruta actual
      sidebarItems: sidebarItems,
      currentRoute: location,
      userAvatarTooltip: 'Usuario', // TODO: Obtener del estado
      showLogout: true,
    );
  }
}
