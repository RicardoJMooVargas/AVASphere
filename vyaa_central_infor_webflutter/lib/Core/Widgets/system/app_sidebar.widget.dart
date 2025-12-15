import 'package:flutter/material.dart';
import 'package:get/get.dart';
import '../../theme/app_colors.dart';
import '../../Controllers/notification_services.dart';
import '../../Controllers/sidebar_controller.dart';

class AppSidebar extends StatefulWidget {
  final Widget body;
  final List<SidebarItem> sidebarItems;
  final String? userAvatarTooltip;
  final bool showLogout;
  final String? currentRoute; // Ruta actual para detectar selección
  
  const AppSidebar({
    Key? key,
    required this.body,
    required this.sidebarItems,
    this.userAvatarTooltip = 'Usuario',
    this.showLogout = true,
    this.currentRoute,
  }) : super(key: key);

  @override
  State<AppSidebar> createState() => _AppSidebarState();
}

class _AppSidebarState extends State<AppSidebar> {
  late SidebarController _sidebarController;

  @override
  void initState() {
    super.initState();
    // Inicializar o obtener el controller
    _sidebarController = Get.put(SidebarController());
    
    // Actualizar la ruta actual (priorizar la pasada por parámetro)
    final currentRoute = widget.currentRoute ?? Get.currentRoute;
    _sidebarController.updateRoute(currentRoute);
  }

  void _logout() {
    NotificationService.showLogoutConfirmation(
      onConfirm: () => NotificationService.handleLogout(context),
    );
  }

  /// Determina si un item está seleccionado basado en la ruta actual
  bool _isItemSelected(SidebarItem item) {
    if (item.route == null) {
      return false;
    }
    return item.route == _sidebarController.currentRoute.value;
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: Row(
        children: [
          // Barra lateral
          Container(
            width: 60,
            height: double.infinity,
            decoration: const BoxDecoration(
              color: AppColors.tertiaryColor,
              boxShadow: [
                BoxShadow(
                  color: Colors.black12,
                  blurRadius: 10,
                  offset: Offset(2, 0),
                ),
              ],
            ),
            child: Column(
              children: [
                const SizedBox(height: 20),
                
                // Avatar del usuario
                Tooltip(
                  message: widget.userAvatarTooltip ?? 'Usuario',
                  child: CircleAvatar(
                    radius: 20,
                    backgroundColor: AppColors.primaryColor,
                    child: const Icon(
                      Icons.person,
                      color: Colors.white,
                      size: 20,
                    ),
                  ),
                ),
                
                const SizedBox(height: 30),
                
                // Items dinámicos del sidebar (reactivos)
                ...widget.sidebarItems.map((item) {
                  return Padding(
                    padding: const EdgeInsets.only(bottom: 16),
                    child: Obx(() => _buildSidebarIcon(
                      icon: item.icon,
                      tooltip: item.name,
                      isSelected: _isItemSelected(item),
                      onTap: () {
                        // Actualizar la ruta inmediatamente para feedback visual
                        if (item.route != null) {
                          _sidebarController.updateRoute(item.route!);
                        }
                        // Ejecutar la navegación
                        item.onPress();
                      },
                    )),
                  );
                }).toList(),
                
                const Spacer(),
                
                // Icono Logout (condicional)
                if (widget.showLogout)
                  _buildSidebarIcon(
                    icon: Icons.logout,
                    tooltip: 'Cerrar Sesión',
                    isSelected: false,
                    onTap: _logout,
                    isLogout: true,
                  ),
                
                const SizedBox(height: 20),
              ],
            ),
          ),
          
          // Contenido principal
          Expanded(
            child: widget.body,
          ),
        ],
      ),
    );
  }

  Widget _buildSidebarIcon({
    required IconData icon,
    required String tooltip,
    required bool isSelected,
    required VoidCallback onTap,
    bool isLogout = false,
  }) {
    return Tooltip(
      message: tooltip,
      child: Material(
        color: Colors.transparent,
        child: InkWell(
          onTap: onTap,
          borderRadius: BorderRadius.circular(8),
          child: Container(
            width: 44,
            height: 44,
            decoration: BoxDecoration(
              color: isSelected 
                  ? AppColors.primaryColor 
                  : Colors.transparent,
              borderRadius: BorderRadius.circular(8),
            ),
            child: Icon(
              icon,
              color: isLogout 
                  ? Colors.red.shade700
                  : isSelected 
                      ? Colors.white
                      : Colors.black87,
              size: 20,
            ),
          ),
        ),
      ),
    );
  }
}

class SidebarItem {
  final String name;
  final IconData icon;
  final VoidCallback onPress;
  final String? route; // Ruta asociada para detectar selección

  SidebarItem({
    required this.name,
    required this.icon,
    required this.onPress,
    this.route,
  });
}