import 'package:flutter/material.dart';
import 'package:get/get.dart';

// Configuraciones
import 'core/theme/app_theme.dart';

// Hive Database
import 'Core/services/data/hive.service.dart';

// Servicio de rutas
import 'Core/services/data/route_app.service.dart';

// Controlador de inicialización
import 'Core/controllers/app_init.controller.dart';

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
    // Inicializar controlador de app
    final appInitController = Get.put(AppInitController());
    
    // Obtener servicio de rutas
    final routeService = RouteAppService();
    
    // Usar Obx para esperar a que se determine la ruta inicial
    return Obx(() {
      // Mostrar pantalla de carga mientras se determina la ruta
      if (appInitController.isLoading.value) {
        return MaterialApp(
          title: 'AVASphere',
          theme: AppTheme.light(),
          home: const Scaffold(
            body: Center(
              child: CircularProgressIndicator(),
            ),
          ),
        );
      }
      
      // Mostrar la app con la ruta inicial determinada
      return GetMaterialApp(
        title: 'AVASphere',
        theme: AppTheme.light(),
        darkTheme: AppTheme.dark(),
        initialRoute: appInitController.initialRoute.value,
        // Usar el servicio de rutas para obtener todas las páginas
        getPages: routeService.getAllGetPages(),
        // Ruta desconocida desde el servicio
        unknownRoute: routeService.getUnknownRoute(),
      );
    });
  }
}