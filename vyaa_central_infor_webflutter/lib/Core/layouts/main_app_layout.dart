import 'package:flutter/material.dart';
import 'package:get/get.dart';
import '../controllers/sidebar_controller.dart';
import '../controllers/notification_services.dart';
import '../../modules/login/services/api/auth.service.dart';
import '../services/data/user_data.service.dart';
// Configuraciones
import '../../configs/routes.config.dart';
import '../../modules/dashboard/screens/home_page.dart';
import '../../modules/sales/screens/main_sales_page.dart';
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
  
  // Lista de items del sidebar filtrados según permisos del usuario
  List<SidebarItemData> _allowedSidebarItems = [];
  bool _isLoadingPermissions = true;

  @override
  void initState() {
    super.initState();
    _sidebarController = Get.put(SidebarController());
    
    // Inicializar mapa de páginas
    _pages = {
      '/home': const HomePage(),
      '/sales': const MainSalesPage(),
      '/inventory': const InventoryPage(),
      '/supply': const SupplyPage(),
    };
    
    // Cargar permisos del usuario y filtrar sidebar
    _loadUserPermissionsAndFilterSidebar();
    
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
  
  /// Cargar permisos del usuario y filtrar items del sidebar
  Future<void> _loadUserPermissionsAndFilterSidebar() async {
    try {
      debugPrint('🔐 ========== CARGANDO PERMISOS DEL USUARIO ==========');
      
      // Obtener módulos del usuario
      final userModules = await UserDataService.getUserModules();
      
      debugPrint('📦 Módulos del usuario: ${userModules.length}');
      for (var module in userModules) {
        debugPrint('   - ${module.name} (${module.normalized})');
      }
      
      // Obtener todos los items del sidebar
      final allSidebarItems = AppRoutes.getSidebarItemsOrdered();
      
      // Filtrar items basándose en los módulos del usuario
      _allowedSidebarItems = allSidebarItems.where((item) {
        // Siempre permitir /home o /dashboard
        if (item.route == '/home' || item.route == '/dashboard') {
          debugPrint('✅ Permitiendo: ${item.title} (ruta base)');
          return true;
        }
        
        // Verificar si el usuario tiene acceso al módulo
        final hasAccess = _checkModuleAccess(item, userModules);
        
        if (hasAccess) {
          debugPrint('✅ Permitiendo: ${item.title} → ${item.route} [${item.moduleName}]');
        } else {
          debugPrint('🚫 Bloqueando: ${item.title} → ${item.route} [${item.moduleName}]');
        }
        
        return hasAccess;
      }).toList();
      
      debugPrint('📊 Sidebar filtrado: ${_allowedSidebarItems.length} de ${allSidebarItems.length} items');
      debugPrint('🔐 ========== FIN CARGA DE PERMISOS ==========');
      
      if (mounted) {
        setState(() {
          _isLoadingPermissions = false;
        });
      }
    } catch (e) {
      debugPrint('❌ Error al cargar permisos del usuario: $e');
      
      // En caso de error, mostrar todos los items (fallback)
      _allowedSidebarItems = AppRoutes.getSidebarItemsOrdered();
      
      if (mounted) {
        setState(() {
          _isLoadingPermissions = false;
        });
      }
    }
  }
  
  /// Verificar si el usuario tiene acceso a un módulo basándose en el item del sidebar
  bool _checkModuleAccess(SidebarItemData item, List<dynamic> userModules) {
    // Si el item tiene un moduleName definido, usarlo para la comparación
    if (item.moduleName != null && item.moduleName!.isNotEmpty) {
      // Buscar coincidencia exacta con el nombre del módulo
      for (var module in userModules) {
        final moduleName = module.name.toString();
        
        // Comparación case-insensitive
        if (moduleName.toLowerCase() == item.moduleName!.toLowerCase()) {
          return true;
        }
      }
      return false;
    }
    
    // Fallback: extraer el nombre del módulo de la ruta
    // Ej: /sales → sales, /inventory → inventory
    final routeName = item.route.replaceFirst('/', '').toLowerCase();
    
    // Buscar en los módulos del usuario
    for (var module in userModules) {
      final moduleName = module.name.toLowerCase();
      final moduleNormalized = module.normalized.toLowerCase().replaceFirst('/', '');
      
      // Comparar por nombre o ruta normalizada
      if (moduleName == routeName || moduleNormalized == routeName) {
        return true;
      }
    }
    
    return false;
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
        
        // Items del sidebar filtrados según permisos del usuario
        if (_isLoadingPermissions)
          const Padding(
            padding: EdgeInsets.all(8.0),
            child: SizedBox(
              width: 20,
              height: 20,
              child: CircularProgressIndicator(
                strokeWidth: 2,
                valueColor: AlwaysStoppedAnimation<Color>(Colors.white),
              ),
            ),
          )
        else
          ..._allowedSidebarItems.map((item) => Padding(
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
            // Usar el servicio de notificaciones para mostrar confirmación
            NotificationService.showLogoutConfirmation(
              onConfirm: () async {
                try {
                  // Ejecutar logout desde AuthService
                  final authService = AuthService();
                  await authService.logout();
                  
                  NotificationService.showSuccess('Sesión cerrada correctamente');
                  
                  // Esperar un momento para que el usuario vea el mensaje
                  await Future.delayed(const Duration(milliseconds: 500));
                  
                  // Redirigir a la ruta raíz
                  debugPrint('🏠 Redirigiendo a ruta raíz "/"');
                  Get.offAllNamed('/');
                } catch (e) {
                  debugPrint('❌ ERROR durante el logout: $e');
                  NotificationService.showError('Error al cerrar sesión');
                }
              },
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