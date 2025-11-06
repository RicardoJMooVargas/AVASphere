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
import 'Core/middlewares/system_setup.middleware.dart';

void main() {
  WidgetsFlutterBinding.ensureInitialized();
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
          middlewares: [GlobalInitMiddleware(), SystemSetupMiddleware()],
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

        // Ruta de error de servidor (solo GlobalInitMiddleware)
        GetPage(
          name: '/server-error',
          page: () {
            debugPrint('📍 Cargando página: /server-error');
            return const ServerErrorScreen();
          },
          middlewares: [GlobalInitMiddleware()],
        ),
        
        // Rutas con sidebar
        GetPage(
          name: '/home',
          page: () => const SidebarLayout(
            userAvatarTooltip: 'Usuario Admin',
            child: HomePage(),
          ),
          middlewares: [GlobalInitMiddleware(), SystemSetupMiddleware()],
        ),  
        GetPage(
          name: '/sales',
          page: () => const SidebarLayout(
            userAvatarTooltip: 'Usuario Admin',
            child: SalesPage(),
          ),
          middlewares: [GlobalInitMiddleware(), SystemSetupMiddleware()],
        ),
        GetPage(
          name: '/inventory',
          page: () => const SidebarLayout(
            userAvatarTooltip: 'Usuario Admin',
            child: InventoryPage(),
          ),
          middlewares: [GlobalInitMiddleware(), SystemSetupMiddleware()],
        ),
        GetPage(
          name: '/supply',
          page: () => const SidebarLayout(
            userAvatarTooltip: 'Usuario Admin',
            child: SupplyPage(),
          ),
          middlewares: [GlobalInitMiddleware(), SystemSetupMiddleware()],
        ),
      ],
    );
  }
}