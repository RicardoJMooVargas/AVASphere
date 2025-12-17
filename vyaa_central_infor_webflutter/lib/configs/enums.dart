import 'package:flutter/material.dart';

enum HttpMethod { get, post, put, delete, patch }

enum SystemModule {
  general(0, 'General'),
  common(1, 'Common'),
  system(2, 'System'),
  sales(3, 'Sales'),
  projects(4, 'Projects'),
  inventory(5, 'Inventory'),
  shopping(6, 'Shopping');

  const SystemModule(this.value, this.displayName);
  
  final int value;
  final String displayName;
}

/// Tipos de pantalla para el sistema de rutas
enum ScreenTypeCore {
  system,      // Pantallas del sistema (init, error, etc.)
  auth,        // Pantallas de autenticación (login, setup)
  app,         // Pantallas principales con sidebar
  fullScreen,  // Pantallas de pantalla completa
  modal,       // Pantallas modales o popup
}

/// Tipos de layout para el sistema de rutas
enum LayoutType {
  none,        // Sin layout especial
  sidebar,     // Con sidebar
  mainApp,     // Layout principal de la app
  clean,       // Layout limpio sin elementos adicionales
}

/// Enum para los estados de cotización
enum StatusEnum {
  pending(1, 'Pendiente', Color(0xFFFFA500)), // Naranja
  accepted(2, 'Aceptado', Color(0xFF008000)), // Verde
  rejected(3, 'Rechazado', Color(0xFFFF0000)); // Rojo

  const StatusEnum(this.value, this.displayName, this.color);
  final int value;
  final String displayName;
  final Color color;

  static StatusEnum fromValue(int value) {
    return StatusEnum.values.firstWhere(
          (status) => status.value == value,
      orElse: () => StatusEnum.pending,
    );
  }

  static String getDisplayName(int value) {
    return fromValue(value).displayName;
  }

  static Color getColor(int value) {
    return fromValue(value).color;
  }

  static Color getColorByName(String name) {
    return StatusEnum.values.firstWhere(
          (status) => status.displayName.toLowerCase() == name.toLowerCase(),
      orElse: () => StatusEnum.pending,
    ).color;
  }
}
