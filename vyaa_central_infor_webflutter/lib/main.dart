import 'package:flutter/material.dart';
import 'package:get/get.dart';

import 'modules/login/screens/login_page.dart';
import 'modules/login/screens/setup_page.dart';

import 'modules/dashboard/screens/home_page.dart';
import 'modules/sales/screens/sales_page.dart';
import 'modules/inventory/screens/inventory_page.dart';
import 'modules/supply/screens/supply_page.dart';
import 'core/theme/app_theme.dart';
import 'Core/screens/server_error_screen.dart';
import 'Core/screens/not_found_screen.dart';
import 'Core/screens/system_init_screen.dart';
import 'Core/layouts/sidebar_layout.dart';
import 'Core/middlewares/global_init.middleware.dart';
// Hive Database
import 'Core/services/data/hive.service.dart';

void main() async {
  WidgetsFlutterBinding.ensureInitialized();
  
  // Inicializar Hive Database antes de ejecutar la app
  try {
    debugPrint('🗃️ Inicializando Hive Database...');
    await HiveService.initialize();
    debugPrint('✅ Hive Database inicializada correctamente');
  } catch (e) {
    debugPrint('❌ Error inicializando Hive Database: $e');
    // La app puede continuar, pero mostrará errores al usar la DB
  }
  
  runApp(const Principal());
}

class Principal extends StatelessWidget {
  const Principal({super.key});

  @override
  Widget build(BuildContext context) {
    return GetMaterialApp(
      title: 'VYAACentral',
      theme: AppTheme.light(),
      darkTheme: AppTheme.dark(),
      initialRoute: '/',
      unknownRoute: GetPage(
        name: '/notfound',
        page: () => const NotFoundScreen(),
      ),
      getPages: [
        // Ruta de inicialización del sistema (debe ser la primera)
        GetPage(
          name: '/',
          page: () {
            debugPrint('📍 Cargando pantalla de inicialización del sistema');
            return const SystemInitScreen();
          },
        ),
        // Rutas sin sidebar
        GetPage(
          name: '/login',
          page: () {
            debugPrint('📍 Cargando página: /login');
            return const NoSidebarLayout(child: LoginPage());
          },
          middlewares: [GlobalInitMiddleware()],
        ),
        
        // Rutas de configuración inicial del sistema
        GetPage(
          name: '/setup',
          page: () {
            debugPrint('📍 Cargando página: /setup');
            return const NoSidebarLayout(child: SetupPage());
          },
          middlewares: [GlobalInitMiddleware()],
        ),

        // Ruta de error de servidor
        GetPage(
          name: '/server-error',
          page: () {
            debugPrint('📍 Cargando página: /server-error');
            return const ServerErrorScreen();
          },
          middlewares: [GlobalInitMiddleware()],
        ),
        
        // Rutas con sidebar - todas requieren verificación inicial
        GetPage(
          name: '/home',
          page: () => const SidebarLayout(
            userAvatarTooltip: 'Usuario Admin',
            child: HomePage(),
          ),
          middlewares: [GlobalInitMiddleware()],
        ),  
        GetPage(
          name: '/sales',
          page: () => const SidebarLayout(
            userAvatarTooltip: 'Usuario Admin',
            child: SalesPage(),
          ),
          middlewares: [GlobalInitMiddleware()],
        ),
        GetPage(
          name: '/inventory',
          page: () => const SidebarLayout(
            userAvatarTooltip: 'Usuario Admin',
            child: InventoryPage(),
          ),
          middlewares: [GlobalInitMiddleware()],
        ),
        GetPage(
          name: '/supply',
          page: () => const SidebarLayout(
            userAvatarTooltip: 'Usuario Admin',
            child: SupplyPage(),
          ),
          middlewares: [GlobalInitMiddleware()],
        ),
      ],
    );
  }
}