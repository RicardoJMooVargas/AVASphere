import 'package:flutter/foundation.dart';
import 'package:get/get.dart';
import '../services/data/cache.service.dart';

/// Controlador para manejar el estado de configuración del sistema
class SystemSetupController extends GetxController {
  static SystemSetupController get to => Get.find();

  final RxBool _isSystemConfigured = false.obs;
  final RxBool _isChecking = false.obs;

  bool get isSystemConfigured => _isSystemConfigured.value;
  bool get isChecking => _isChecking.value;

  @override
  void onInit() {
    super.onInit();
    _loadSystemStatus();
  }

  /// Carga el estado de configuración del sistema desde el caché
  Future<void> _loadSystemStatus() async {
    try {
      _isChecking.value = true;
      final configured = await CacheService.isSystemInitialized();
      _isSystemConfigured.value = configured;
      debugPrint('🔧 Estado del sistema cargado: ${configured ? "Configurado" : "Requiere configuración"}');
    } catch (e) {
      debugPrint('⚠️ Error al cargar estado del sistema: $e');
      _isSystemConfigured.value = false; // Asumir no configurado por seguridad
    } finally {
      _isChecking.value = false;
    }
  }

  /// Actualiza el estado de configuración del sistema
  Future<void> updateSystemStatus(bool configured) async {
    try {
      await CacheService.saveSystemInitialized(configured);
      _isSystemConfigured.value = configured;
      debugPrint('🔧 Estado del sistema actualizado: ${configured ? "Configurado" : "Requiere configuración"}');
    } catch (e) {
      debugPrint('⚠️ Error al actualizar estado del sistema: $e');
    }
  }

  /// Fuerza una recarga del estado desde el servidor
  Future<void> refreshSystemStatus() async {
    await _loadSystemStatus();
  }

  /// Verifica si se puede navegar a una ruta específica
  bool canNavigateTo(String route) {
    // Rutas que no requieren configuración del sistema
    final excludedRoutes = ['/setup', '/server-error', '/notfound'];
    if (excludedRoutes.contains(route)) {
      return true;
    }

    // Para otras rutas, verificar si el sistema está configurado
    return _isSystemConfigured.value;
  }
}