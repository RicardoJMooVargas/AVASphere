import 'package:flutter/material.dart';
import 'package:get/get.dart';

// Configuraciones
import 'core/theme/app_theme.dart';

// Hive Database
import 'core/services/data/hive.service.dart';

// Servicio de rutas
import 'core/services/data/route_app.service.dart';

// Servicio de inicialización del sistema
import 'core/services/system_init.service.dart';

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

class Principal extends StatefulWidget {
  const Principal({super.key});

  @override
  State<Principal> createState() => _PrincipalState();
}

class _PrincipalState extends State<Principal> {
  final _systemInitService = SystemInitService();
  late Future<String> _initialRouteFuture;

  @override
  void initState() {
    super.initState();
    // Determinar la ruta inicial
    _initialRouteFuture = _systemInitService.determineInitialRoute();
  }

  @override
  Widget build(BuildContext context) {
    return FutureBuilder<String>(
      future: _initialRouteFuture,
      builder: (context, snapshot) {
        // Mostrar pantalla de carga mientras se determina la ruta inicial
        if (snapshot.connectionState == ConnectionState.waiting) {
          return MaterialApp(
            title: 'AVASphere',
            theme: AppTheme.light(),
            home: const Scaffold(
              body: Center(child: CircularProgressIndicator()),
            ),
          );
        }

        // Si hay error, ir a la ruta de error
        if (snapshot.hasError) {
          debugPrint('❌ Error determinando ruta inicial: ${snapshot.error}');
          return _buildApp('/server-error');
        }

        // Obtener la ruta inicial determinada
        final initialRoute = snapshot.data ?? '/';
        debugPrint('✅ Ruta inicial determinada: $initialRoute');

        // Ya no necesitamos conversión, las rutas vienen correctas
        return _buildApp(initialRoute);
      },
    );
  }

  /// Construye la app con go_router
  Widget _buildApp(String initialLocation) {
    // Crear el router con la ruta inicial
    final router = RouteAppService.createRouter(
      initialLocation: initialLocation,
    );

    // Debug de rutas (solo en desarrollo)
    RouteAppService.debugRoutes();

    // Usar GetMaterialApp.router para mantener GetX disponible para snackbars
    return GetMaterialApp.router(
      title: 'AVASphere',
      theme: AppTheme.light(),
      routeInformationParser: router.routeInformationParser,
      routeInformationProvider: router.routeInformationProvider,
      routerDelegate: router.routerDelegate,
      debugShowCheckedModeBanner: false,
    );
  }
}
