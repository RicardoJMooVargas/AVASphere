import 'package:flutter/material.dart';
import 'package:get/get.dart';

// Configuraciones
import 'configs/routes.config.dart';
import 'core/theme/app_theme.dart';

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
      title: 'AVASphere',
      theme: AppTheme.light(),
      darkTheme: AppTheme.dark(),
      initialRoute: '/',
      unknownRoute: AppRoutes.unknownRoute, 
      getPages: AppRoutes.getPages,
    );
  }
}