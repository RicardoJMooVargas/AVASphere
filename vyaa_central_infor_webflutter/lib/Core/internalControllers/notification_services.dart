import 'package:flutter/material.dart';
import 'package:get/get.dart';
import '../../services/local/cache_service.service.dart';

class NotificationService {
  static const Duration _snackbarDuration = Duration(seconds: 4);

  /// Muestra un snackbar de error general
  static void showError(String message) {
    Get.snackbar(
      'Error',
      message,
      snackPosition: SnackPosition.BOTTOM,
      maxWidth: 100,
      backgroundColor: Colors.red.withOpacity(0.1),
      colorText: Colors.red.shade700,
      borderColor: Colors.red.shade300,
      borderWidth: 1,
      padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 8),
      duration: _snackbarDuration,
      titleText: Text(
        'Error',
        style: TextStyle(
          color: Colors.red.shade700,
          fontSize: 12,
          fontWeight: FontWeight.bold,
        ),
      ),
      messageText: Text(
        message,
        style: TextStyle(
          color: Colors.red.shade700,
          fontSize: 11,
        ),
      ),
      icon: Icon(
        Icons.error_outline,
        color: Colors.red.shade700,
        size: 16,
      ),
    );
  }

  /// Muestra un snackbar de éxito
  static void showSuccess(String message) {
    Get.snackbar(
      'Éxito',
      message,
      snackPosition: SnackPosition.BOTTOM,
      maxWidth: 300,
      backgroundColor: Colors.green.withOpacity(0.1),
      colorText: Colors.green.shade700,
      borderColor: Colors.green.shade300,
      borderWidth: 1,
      padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 8),
      duration: _snackbarDuration,
      titleText: Text(
        'Éxito',
        style: TextStyle(
          color: Colors.green.shade700,
          fontSize: 12,
          fontWeight: FontWeight.bold,
        ),
      ),
      messageText: Text(
        message,
        style: TextStyle(
          color: Colors.green.shade700,
          fontSize: 11,
        ),
      ),
      icon: Icon(
        Icons.check_circle_outline,
        color: Colors.green.shade700,
        size: 16,
      ),
    );
  }

  /// Muestra un snackbar de advertencia
  static void showWarning(String message) {
    Get.snackbar(
      'Advertencia',
      message,
      snackPosition: SnackPosition.BOTTOM,
      backgroundColor: Colors.orange.withOpacity(0.1),
      colorText: Colors.orange.shade700,
      borderColor: Colors.orange.shade300,
      borderWidth: 1,
      margin: const EdgeInsets.only(bottom: 16, right: 16, left: 400),
      padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 8),
      duration: _snackbarDuration,
      titleText: Text(
        'Advertencia',
        style: TextStyle(
          color: Colors.orange.shade700,
          fontSize: 12,
          fontWeight: FontWeight.bold,
        ),
      ),
      messageText: Text(
        message,
        style: TextStyle(
          color: Colors.orange.shade700,
          fontSize: 11,
        ),
      ),
      icon: Icon(
        Icons.warning_amber_outlined,
        color: Colors.orange.shade700,
        size: 16,
      ),
    );
  }

  /// Maneja la expiración del token y redirecciona al login
  static void handleTokenExpired() async {
    // Eliminar el token del cache
    await CacheService.removeToken();
    
    // Mostrar snackbar de token expirado
    Get.snackbar(
      'Sesión Expirada',
      'Tu sesión ha expirado. Serás redirigido al login.',
      snackPosition: SnackPosition.BOTTOM,
      backgroundColor: Colors.amber.withOpacity(0.1),
      colorText: Colors.amber.shade800,
      borderColor: Colors.amber.shade300,
      borderWidth: 1,
      margin: const EdgeInsets.only(bottom: 16, right: 16, left: 400),
      padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 8),
      duration: const Duration(seconds: 3),
      titleText: Text(
        'Sesión Expirada',
        style: TextStyle(
          color: Colors.amber.shade800,
          fontSize: 12,
          fontWeight: FontWeight.bold,
        ),
      ),
      messageText: Text(
        'Tu sesión ha expirado. Serás redirigido al login.',
        style: TextStyle(
          color: Colors.amber.shade800,
          fontSize: 11,
        ),
      ),
      icon: Icon(
        Icons.access_time_outlined,
        color: Colors.amber.shade800,
        size: 16,
      ),
    );

    // Esperar un momento para que el usuario vea el mensaje
    await Future.delayed(const Duration(milliseconds: 1500));
    
    // Redirigir al login y limpiar el stack de navegación
    Get.offAllNamed('/login');
  }

  /// Muestra un diálogo de confirmación para logout
  static void showLogoutConfirmation({
    required VoidCallback onConfirm,
  }) {
    Get.dialog(
      AlertDialog(
        title: const Row(
          children: [
            Icon(Icons.logout, color: Colors.orange),
            SizedBox(width: 8),
            Text('Cerrar Sesión'),
          ],
        ),
        content: const Text('¿Estás seguro de que deseas cerrar sesión?'),
        actions: [
          TextButton(
            onPressed: () => Get.back(),
            child: const Text('Cancelar'),
          ),
          ElevatedButton(
            onPressed: () {
              Get.back();
              onConfirm();
            },
            style: ElevatedButton.styleFrom(
              backgroundColor: Colors.orange,
              foregroundColor: Colors.white,
            ),
            child: const Text('Cerrar Sesión'),
          ),
        ],
      ),
      barrierDismissible: false,
    );
  }

  /// Maneja el logout manual del usuario
  static void handleLogout() async {
    await CacheService.removeToken();
    showSuccess('Sesión cerrada correctamente');
    
    // Esperar un momento para que el usuario vea el mensaje
    await Future.delayed(const Duration(milliseconds: 500));
    
    // Redirigir al login y limpiar el stack de navegación
    Get.offAllNamed('/login');
  }

  /// Muestra información general
  static void showInfo(String message) {
    Get.snackbar(
      'Información',
      message,
      snackPosition: SnackPosition.BOTTOM,
      backgroundColor: Colors.blue.withOpacity(0.1),
      colorText: Colors.blue.shade700,
      borderColor: Colors.blue.shade300,
      borderWidth: 1,
      margin: const EdgeInsets.only(bottom: 16, right: 16, left: 400),
      padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 8),
      duration: _snackbarDuration,
      titleText: Text(
        'Información',
        style: TextStyle(
          color: Colors.blue.shade700,
          fontSize: 12,
          fontWeight: FontWeight.bold,
        ),
      ),
      messageText: Text(
        message,
        style: TextStyle(
          color: Colors.blue.shade700,
          fontSize: 11,
        ),
      ),
      icon: Icon(
        Icons.info_outline,
        color: Colors.blue.shade700,
        size: 16,
      ),
    );
  }
}