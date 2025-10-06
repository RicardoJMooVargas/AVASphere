import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';

import 'screens/login_page.dart';
import 'screens/home_page.dart';
import 'Core/theme/app_theme.dart';
import 'services/local/cache_service.dart';

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
    final GoRouter router = GoRouter(
      initialLocation: initialRoute,
      routes: <RouteBase>[
        GoRoute(
          path: '/login',
          builder: (context, state) => const LoginPage(),
        ),
        GoRoute(
          path: '/home',
          builder: (context, state) => const HomePage(),
        ),
      ],
    );

    return MaterialApp.router(
      title: 'VYAACentral',
      theme: AppTheme.light(),
      darkTheme: AppTheme.dark(),
      routerConfig: router,
    );
  }
}
