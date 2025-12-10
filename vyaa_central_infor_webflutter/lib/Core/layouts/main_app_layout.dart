import 'package:flutter/material.dart';
import 'package:get/get.dart';
import '../controllers/sidebar_controller.dart';
import '../controllers/notification_services.dart';
import '../controllers/app_init.controller.dart';
import '../../modules/login/services/api/auth.service.dart';

// Servicios
import '../services/data/route_app.service.dart';

// Modelos
import '../models/base/route_config.module.dart';

// Theme
import '../../core/theme/app_colors.dart';

/// Layout principal de la aplicación que maneja la navegación interna
/// sin recargar el sidebar. Usa RouteAppService para obtener rutas filtradas
class MainAppLayout extends StatefulWidget {
  const MainAppLayout({super.key});

  @override
  State<MainAppLayout> createState() => _MainAppLayoutState();
}

class _MainAppLayoutState extends State<MainAppLayout> {
  late SidebarController _sidebarController;
  late RouteAppService _routeService;
  late AppInitController _appInitController;

  // Mapa de rutas a widgets para navegación interna
  Map<String, Widget> _pages = {};
  
  // Lista de rutas del sidebar filtradas según permisos del usuario
  List<RouteConfig> _allowedSidebarRoutes = [];
  bool _isLoadingPermissions = true;

  @override
  void initState() {
    super.initState();
    
    // Obtener o crear controladores (solo una vez)
    _sidebarController = Get.find<SidebarController>(tag: 'sidebar');
    _routeService = RouteAppService();
    
    // Obtener el controlador de inicialización (ya existe en Get)
    _appInitController = Get.find<AppInitController>();
    
    // Cargar rutas del usuario y configurar el sidebar (solo una vez)
    _loadUserRoutesAndSetup();
    
    // Inicializar con la ruta original desde argumentos o la ruta actual
    String initialRoute = '/home';
    
    // Verificar si hay argumentos con la ruta original
    final arguments = Get.arguments;
    if (arguments != null && arguments is Map && arguments['originalRoute'] != null) {
      initialRoute = arguments['originalRoute'] as String;
      debugPrint('🎯 Ruta original detectada desde argumentos: $initialRoute');
    } else {
      // Usar el controlador para resolver la ruta
      final currentRoute = Get.currentRoute;
      initialRoute = _appInitController.resolveAppRoute(currentRoute);
      debugPrint('🌐 MainAppLayout inicializando con ruta: $initialRoute');
    }
    
    _sidebarController.updateRoute(initialRoute);
    debugPrint('✅ Ruta inicial configurada: $initialRoute');
  }
  
  /// Cargar rutas del usuario desde el servicio y configurar el layout
  Future<void> _loadUserRoutesAndSetup() async {
    // Evitar cargar múltiples veces
    if (!_isLoadingPermissions) {
      return;
    }
    
    try {
      debugPrint('🔐 ========== CARGANDO RUTAS DEL USUARIO ==========');
      
      // Obtener rutas del sidebar desde el servicio (ya filtradas por permisos)
      _allowedSidebarRoutes = await _routeService.getSidebarRoutes();
      
      // Construir mapa de páginas desde las route configs
      _pages = {};
      for (var route in _allowedSidebarRoutes) {
        if (route.page != null) {
          _pages[route.path] = route.page!();
        }
      }
      
      debugPrint('📊 Rutas cargadas: ${_allowedSidebarRoutes.length}');
      debugPrint('📄 Páginas internas: ${_pages.length}');
      
      for (var route in _allowedSidebarRoutes) {
        debugPrint('   ✅ ${route.title} → ${route.path}');
      }
      
      debugPrint('🔐 ========== FIN CARGA DE RUTAS ==========');
      
      if (mounted) {
        setState(() {
          _isLoadingPermissions = false;
        });
      }
    } catch (e) {
      debugPrint('❌ Error al cargar rutas del usuario: $e');
      
      if (mounted) {
        setState(() {
          _isLoadingPermissions = false;
        });
      }
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
          // Renderizar items del sidebar desde las rutas configuradas
          ..._allowedSidebarRoutes.map((route) => Padding(
            padding: const EdgeInsets.only(bottom: 16),
            child: _buildSidebarItem(
              icon: route.icon ?? Icons.circle_outlined,
              tooltip: route.title ?? route.name,
              route: route.path,
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