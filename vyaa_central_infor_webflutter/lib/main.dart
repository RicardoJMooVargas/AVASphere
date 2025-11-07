import 'package:flutter/material.dart';
import 'package:get/get.dart';

import 'modules/login/screens/login_page.dart';
import 'modules/login/screens/setup_page.dart';

import 'modules/dashboard/screens/home_page.dart';
import 'modules/sales/screens/sales_page.dart';
import 'modules/inventory/screens/inventory_page.dart';
import 'modules/supply/screens/supply_page.dart';
import 'core/theme/app_theme.dart';
import 'Core/services/data/cache.service.dart';
import 'Core/layouts/sidebar_layout.dart';

void main() async {
  WidgetsFlutterBinding.ensureInitialized();
  final token = await CacheService.getToken();
  runApp(Principal(initialRoute: token == null ? '/login' : '/home'));
}

class Principal extends StatelessWidget {
  final String initialRoute;

  const Principal({super.key, required this.initialRoute});

  @override
  Widget build(BuildContext context) {
    return GetMaterialApp(
      title: 'VYAACentral',
      theme: AppTheme.light(),
      darkTheme: AppTheme.dark(),
      initialRoute: initialRoute,
      getPages: [
        // Rutas de configuración inicial del sistema
        GetPage(
          name: '/setup',
          page: () => const NoSidebarLayout(
            child: SetupPage(),
          ),
        ),

        // Rutas sin sidebar
        GetPage(
          name: '/login',
          page: () => const NoSidebarLayout(
            child: LoginPage(),
          ),
        ),
        
        // Rutas con sidebar
        GetPage(
          name: '/home',
          page: () => const SidebarLayout(
            userAvatarTooltip: 'Usuario Admin',
            child: HomePage(),
          ),
        ),  
        GetPage(
          name: '/sales',
          page: () => const SidebarLayout(
            userAvatarTooltip: 'Usuario Admin',
            child: SalesPage(),
          ),
        ),
        GetPage(
          name: '/inventory',
          page: () => const SidebarLayout(
            userAvatarTooltip: 'Usuario Admin',
            child: InventoryPage(),
          ),
        ),
        GetPage(
          name: '/supply',
          page: () => const SidebarLayout(
            userAvatarTooltip: 'Usuario Admin',
            child: SupplyPage(),
          ),
        ),
      ],
    );
  }
}
