import 'package:flutter/material.dart';
import 'package:get/get.dart';

import 'screens/login_page.dart';
import 'screens/home_page.dart';
import 'core/theme/app_theme.dart';
import 'services/local/cache_service.service.dart';

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
        GetPage(
          name: '/login',
          page: () => const LoginPage(),
        ),
        GetPage(
          name: '/home',
          page: () => HomePage(),
        ),
      ],
    );
  }
}
