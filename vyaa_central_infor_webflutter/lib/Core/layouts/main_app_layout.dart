import 'package:flutter/material.dart';
import 'package:get/get.dart';
import '../controllers/sidebar_controller.dart';
import '../controllers/notification_services.dart';
// Configuraciones
import '../../configs/routes.config.dart';
import '../../modules/dashboard/screens/home_page.dart';
import '../../modules/sales/screens/sales_page.dart';
import '../../modules/inventory/screens/inventory_page.dart';
import '../../modules/supply/screens/supply_page.dart';
import '../../core/theme/app_colors.dart';

/// Layout principal de la aplicación que maneja la navegación interna
/// sin recargar el sidebar
class MainAppLayout extends StatefulWidget {
  const MainAppLayout({super.key});

  @override
  State<MainAppLayout> createState() => _MainAppLayoutState();
}



class _MainAppLayoutState extends State<MainAppLayout> {
  late SidebarController _sidebarController;

  // Mapa de rutas a widgets para navegación interna
  late Map<String, Widget> _pages;

  @override
  void initState() {
    super.initState();
    _sidebarController = Get.put(SidebarController());
    
    // Inicializar mapa de páginas
    _pages = {
      '/home': const HomePage(),
      '/sales': const SalesPage(),
      '/inventory': const InventoryPage(),
      '/supply': const SupplyPage(),
    };
    
    // Inicializar con la ruta original desde argumentos o la ruta actual
    String initialRoute = '/home';
    
    // Verificar si hay argumentos con la ruta original
    final arguments = Get.arguments;
    if (arguments != null && arguments is Map && arguments['originalRoute'] != null) {
      initialRoute = arguments['originalRoute'] as String;
      debugPrint('🎯 Ruta original detectada desde argumentos: $initialRoute');
    } else {
      final currentRoute = Get.currentRoute;
      if (_pages.containsKey(currentRoute)) {
        initialRoute = currentRoute;
      }
      debugPrint('🌐 MainAppLayout inicializando con ruta: $initialRoute');
    }
    
    if (_pages.containsKey(initialRoute)) {
      _sidebarController.updateRoute(initialRoute);
      debugPrint('✅ Ruta válida detectada: $initialRoute');
    } else {
      _sidebarController.updateRoute('/home');
      debugPrint('🏠 Usando ruta por defecto: /home');
    }
  }

  /// Navegar internamente sin afectar el sidebar
  void _navigateToPage(String route) {
    debugPrint('🧭 Navegación interna a: $route');
    _sidebarController.updateRoute(route);
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: Row(
        children: [
          // Sidebar fijo
          Container(
            width: 60,
            height: double.infinity,
            decoration: const BoxDecoration(
              color: AppColors.primaryColor,
              boxShadow: [
                BoxShadow(
                  color: Colors.black12,
                  blurRadius: 10,
                  offset: Offset(2, 0),
                ),
              ],
            ),
            child: _buildSidebar(),
          ),
          
          // Contenido principal que cambia
          Expanded(
            child: Obx(() {
              final currentRoute = _sidebarController.currentRoute.value;
              return AnimatedSwitcher(
                duration: const Duration(milliseconds: 300),
                transitionBuilder: (Widget child, Animation<double> animation) {
                  return FadeTransition(
                    opacity: animation,
                    child: SlideTransition(
                      position: Tween<Offset>(
                        begin: const Offset(0.05, 0.0),
                        end: Offset.zero,
                      ).animate(animation),
                      child: child,
                    ),
                  );
                },
                child: Container(
                  key: ValueKey(currentRoute),
                  child: _pages[currentRoute] ?? _pages['/home'] ?? const Center(
                    child: Text('Página no encontrada'),
                  ),
                ),
              );
            }),
          ),
        ],
      ),
    );
  }

  Widget _buildSidebar() {
    return Column(
      children: [
        const SizedBox(height: 20),
        
        // Avatar del usuario
        const Tooltip(
          message: 'Usuario Admin',
          child: CircleAvatar(
            radius: 20,
            backgroundColor: AppColors.tertiaryColor,
            child: Icon(
              Icons.person,
              color: Colors.white,
              size: 20,
            ),
          ),
        ),
        
        const SizedBox(height: 30),
        
        // Items del sidebar usando configuración
        ...AppRoutes.getSidebarItemsOrdered().map((item) => Padding(
          padding: const EdgeInsets.only(bottom: 16),
          child: _buildSidebarItem(
            icon: item.icon,
            tooltip: item.title,
            route: item.route,
          ),
        )).toList(),
        
        const Spacer(),
        
        // Botón de logout
        _buildLogoutButton(),
        
        const SizedBox(height: 20),
      ],
    );
  }

  Widget _buildSidebarItem({
    required IconData icon,
    required String tooltip,
    required String route,
  }) {
    return Obx(() {
      final isSelected = _sidebarController.currentRoute.value == route;
      
      return Tooltip(
        message: tooltip,
        child: Material(
          color: Colors.transparent,
          child: InkWell(
            onTap: () => _navigateToPage(route),
            borderRadius: BorderRadius.circular(8),
            child: Container(
              width: 44,
              height: 44,
              decoration: BoxDecoration(
                color: isSelected 
                    ? AppColors.tertiaryColor
                    : Colors.transparent,
                borderRadius: BorderRadius.circular(8),
              ),
              child: Icon(
                icon,
                color: isSelected 
                    ? Colors.white
                    : Colors.grey,
                size: 20,
              ),
            ),
          ),
        ),
      );
    });
  }

  Widget _buildLogoutButton() {
    return Tooltip(
      message: 'Cerrar Sesión',
      child: Material(
        color: Colors.transparent,
        child: InkWell(
          onTap: () {
            // Usar el servicio de notificaciones para logout
            NotificationService.showLogoutConfirmation(
              onConfirm: () => NotificationService.handleLogout(),
            );
          },
          borderRadius: BorderRadius.circular(8),
          child: Container(
            width: 44,
            height: 44,
            decoration: BoxDecoration(
              borderRadius: BorderRadius.circular(8),
            ),
            child: Icon(
              Icons.logout,
              color: Colors.red.shade700,
              size: 20,
            ),
          ),
        ),
      ),
    );
  }
}