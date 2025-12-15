import 'package:flutter/material.dart';
import 'package:get/get.dart';
import 'package:go_router/go_router.dart';
import 'package:flutter_spinkit/flutter_spinkit.dart';

import '../controllers/server_status.controller.dart';
import '../controllers/system_setup.controller.dart';
import '../middlewares/global_init.middleware.dart';


/// Pantalla de inicialización del sistema que muestra un loader mientras
/// se cargan los datos necesarios y se determina la ruta inicial
class SystemInitScreen extends StatefulWidget {
  const SystemInitScreen({super.key});

  @override
  State<SystemInitScreen> createState() => _SystemInitScreenState();
}

class _SystemInitScreenState extends State<SystemInitScreen> {
  String _currentStatus = 'Iniciando aplicación...';
  bool _hasError = false;
  String? _errorMessage;

  @override
  void initState() {
    super.initState();
    _initializeSystem();
  }

  /// Inicializa el sistema y ejecuta la verificación de configuración
  Future<void> _initializeSystem() async {
    try {
      setState(() {
        _hasError = false;
        _errorMessage = null;
      });
      // Inicializar controladores básicos
      _updateStatus('Inicializando aplicación...');
      await Future.delayed(const Duration(milliseconds: 500));
      Get.put(ServerStatusController(), permanent: true);
      Get.put(SystemSetupController(), permanent: true);
      _updateStatus('Verificando configuración del servidor...');
      await Future.delayed(const Duration(milliseconds: 500));
      // Ejecutar verificación de configuración inicial
      String targetRoute;
      try {
        targetRoute = await GlobalInitMiddleware.checkAndDetermineRoute();
      } catch (e) {
        // SERIVDOR ANDA FALLANDO
        debugPrint('❌ Error en verificación de configuración: $e');
        _updateStatus('Error de conexión detectado...');
        await Future.delayed(const Duration(milliseconds: 500));
        targetRoute = '/server-error';
        final serverStatus = Get.find<ServerStatusController>();
        serverStatus.markServerUnavailable();
      }

      // Configurar estado del sistema
      _updateStatus('Configurando estado del sistema...');
      await Future.delayed(const Duration(milliseconds: 300));

      final serverStatus = Get.find<ServerStatusController>();
      if (targetRoute == '/server-error') {
        debugPrint('🔴 Servidor marcado como no disponible');
        serverStatus.markServerUnavailable();
      } else {
        debugPrint('✅ Servidor marcado como disponible');
        serverStatus.markServerAvailable();
      }

      // Sistema determinó la ruta apropiada directamente

      // Finalizar y redirigir
      _updateStatus('¡Listo! Redirigiendo...');
      await Future.delayed(const Duration(milliseconds: 500));

      debugPrint('🎯 Navegando a ruta final: $targetRoute');
      if (mounted) {
        context.go(targetRoute);
      }

    } catch (e) {
      debugPrint('❌ Error durante la inicialización: $e');
      setState(() {
        _hasError = true;
        _errorMessage = e.toString();
        _currentStatus = 'Error durante la inicialización';
      });
    }
  }

  /// Actualiza el estado mostrado al usuario
  void _updateStatus(String status) {
    if (mounted) {
      setState(() {
        _currentStatus = status;
      });
      debugPrint('📱 Estado: $status');
    }
  }

  /// Intenta reiniciar el proceso de inicialización
  void _retry() {
    setState(() {
      _currentStatus = 'Reintentando inicialización...';
      _hasError = false;
      _errorMessage = null;
    });
    _initializeSystem();
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: Theme.of(context).colorScheme.surface,
      body: Center(
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            // Logo/Título de la aplicación
            Container(
              margin: const EdgeInsets.only(bottom: 48),
              child: Column(
                children: [
                  Icon(
                    Icons.settings_applications,
                    size: 64,
                    color: Theme.of(context).colorScheme.primary,
                  ),
                  const SizedBox(height: 16),
                  Text(
                    'AVASphere',
                    style: Theme.of(context).textTheme.headlineMedium?.copyWith(
                      fontWeight: FontWeight.bold,
                      color: Theme.of(context).colorScheme.primary,
                    ),
                  ),
                ],
              ),
            ),

            // Loader o mensaje de error
            if (_hasError) ...[
              // Mostrar error
              Icon(
                Icons.error_outline,
                size: 48,
                color: Theme.of(context).colorScheme.error,
              ),
              const SizedBox(height: 16),
              Text(
                'Error de inicialización',
                style: Theme.of(context).textTheme.titleLarge?.copyWith(
                  color: Theme.of(context).colorScheme.error,
                ),
              ),
              const SizedBox(height: 8),
              Container(
                padding: const EdgeInsets.symmetric(horizontal: 32),
                child: Text(
                  _errorMessage ?? 'Error desconocido',
                  textAlign: TextAlign.center,
                  style: Theme.of(context).textTheme.bodyMedium?.copyWith(
                    color: Theme.of(context).colorScheme.onSurface.withOpacity(0.7),
                  ),
                ),
              ),
              const SizedBox(height: 24),
              ElevatedButton.icon(
                onPressed: _retry,
                icon: const Icon(Icons.refresh),
                label: const Text('Reintentar'),
              ),
            ] else ...[
              // Mostrar loader
              SpinKitFadingCube(
                color: Theme.of(context).colorScheme.primary,
                size: 50.0,
              ),
              const SizedBox(height: 32),
              Container(
                padding: const EdgeInsets.symmetric(horizontal: 32),
                child: Text(
                  _currentStatus,
                  textAlign: TextAlign.center,
                  style: Theme.of(context).textTheme.bodyLarge?.copyWith(
                    color: Theme.of(context).colorScheme.onSurface.withOpacity(0.8),
                  ),
                ),
              ),
            ],
          ],
        ),
      ),
    );
  }
}