
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

