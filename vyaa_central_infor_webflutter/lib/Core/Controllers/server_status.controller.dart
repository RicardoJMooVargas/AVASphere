import 'package:get/get.dart';
import 'package:flutter/material.dart';

/// Controlador global que maneja el estado de disponibilidad del servidor
class ServerStatusController extends GetxController {
  // Estado del servidor (true = disponible, false = error)
  final RxBool _isServerAvailable = true.obs;
  
  bool get isServerAvailable => _isServerAvailable.value;
  
  /// Marca el servidor como no disponible
  void markServerUnavailable() {
    debugPrint('🔴 Servidor marcado como no disponible');
    _isServerAvailable.value = false;
  }
  
  /// Marca el servidor como disponible
  void markServerAvailable() {
    debugPrint('✅ Servidor marcado como disponible');
    _isServerAvailable.value = true;
  }
  
  /// Verifica si se puede navegar a una ruta
  bool canNavigateTo(String route) {
    // Si el servidor no está disponible, solo permitir /server-error
    if (!_isServerAvailable.value && route != '/server-error') {
      debugPrint('🚫 Navegación bloqueada a $route - servidor no disponible');
      return false;
    }
    return true;
  }
}
