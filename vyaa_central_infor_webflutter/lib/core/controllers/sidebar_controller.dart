import 'package:get/get.dart';

/// Controller reactivo para manejar la ruta actual del sidebar
class SidebarController extends GetxController {
  final RxString currentRoute = ''.obs;

  @override
  void onInit() {
    super.onInit();
    // Inicializar con la ruta actual
    updateRoute(Get.currentRoute);
  }

  /// Actualizar la ruta actual
  void updateRoute(String route) {
    if (currentRoute.value != route) {
      currentRoute.value = route;
      print('🔄 Sidebar route updated to: $route'); // Debug
    }
  }

  /// Verificar si una ruta específica está activa
  bool isRouteActive(String route) {
    return currentRoute.value == route;
  }
}