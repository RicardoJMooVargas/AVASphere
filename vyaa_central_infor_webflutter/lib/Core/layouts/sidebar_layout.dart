import 'package:flutter/material.dart';
import 'package:get/get.dart';
import '../Widgets/app_sidebar.widget.dart';
import '../controllers/sidebar_controller.dart';

/// Wrapper que proporciona el sidebar a las páginas que lo necesiten
class SidebarLayout extends StatelessWidget {
  final Widget child;
  final String? userAvatarTooltip;
  final bool showLogout;
  final List<SidebarItem>? customSidebarItems;

  const SidebarLayout({
    super.key,
    required this.child,
    this.userAvatarTooltip,
    this.showLogout = true,
    this.customSidebarItems,
  });

  @override
  Widget build(BuildContext context) {
    return AppSidebar(
      sidebarItems: customSidebarItems ?? _getDefaultSidebarItems(),
      userAvatarTooltip: userAvatarTooltip ?? 'Usuario',
      showLogout: showLogout,
      body: child,
      currentRoute: Get.currentRoute, // Pasar la ruta actual
    );
  }

  /// Items por defecto del sidebar para toda la aplicación
  List<SidebarItem> _getDefaultSidebarItems() {
    return [
      SidebarItem(
        name: 'Dashboard',
        icon: Icons.dashboard,
        route: '/home', // Agregar ruta para comparación
        onPress: () => Get.offNamed('/home'),
      ),
      SidebarItem(
        name: 'Ventas',
        icon: Icons.sell,
        route: '/sales', // Agregar ruta para comparación
        onPress: () => Get.offNamed('/sales'),
      ),
      SidebarItem(
        name: 'Inventario',
        icon: Icons.inventory,
        route: '/inventory', // Agregar ruta para comparación
        onPress: () => Get.offNamed('/inventory'),
      ),
      SidebarItem(
        name: 'Suministros',
        icon: Icons.local_shipping,
        route: '/supply', // Agregar ruta para comparación
        onPress: () => Get.offNamed('/supply'),
      ),
    ];
  }
}

/// Widget que no incluye sidebar (para páginas como login)
class NoSidebarLayout extends StatelessWidget {
  final Widget child;

  const NoSidebarLayout({
    super.key,
    required this.child,
  });

  @override
  Widget build(BuildContext context) {
    return child;
  }
}