import 'package:flutter/foundation.dart';
import 'package:vyaa_central_infor_webflutter/configs/api_endpoints.config.dart';
import 'package:vyaa_central_infor_webflutter/configs/api_response.config.dart';
import 'package:vyaa_central_infor_webflutter/Core/models/responses/customer_res.module.dart';
import 'api.service.dart';

class CustomerService {

  /// Busca clientes por nombre usando el endpoint tipado
  /// Retorna una lista de clientes que coincidan con el término de búsqueda
  /// Utiliza requestWithModel para manejo correcto de tipos genéricos
  ///
  /// Ejemplo de uso:
  /// ```dart
  /// final customerService = CustomerService();
  /// try {
  ///   final response = await customerService.searchCustomers(name: 'Juan');
  ///   if (response.success) {
  ///     final customers = response.data!;
  ///     print('Clientes encontrados: ${customers.length}');
  ///   }
  /// } catch (e) {
  ///   print('Error: $e');
  /// }
  /// ```
  Future<ApiResponse<List<CustomerRes>>> searchCustomers({
    required String name,
  }) async {
    try {
      // Validar que el nombre tenga al menos 2 caracteres
      if (name.isEmpty) {
        return ApiResponse.error('El término de búsqueda no puede estar vacío');
      }

      if (name.length < 2) {
        return ApiResponse.error('Ingrese al menos 2 caracteres para realizar la búsqueda');
      }

      debugPrint('🔍 Buscando clientes con texto: "$name"');

      // Usar el endpoint correcto para búsqueda de clientes
      final endpoint = ApiEndpoints.system.customer.search;

      // Usar requestWithModel para un manejo correcto de tipos genéricos
      final apiResponse = await ApiService.requestWithModel<List<CustomerRes>, Map<String, dynamic>>(
        endpoint,
        model: {'searchText': name}, // Para endpoints con useQuery: true, se pasa como model
      );

      if (!apiResponse.success) {
        debugPrint('❌ Error en búsqueda de clientes: ${apiResponse.message}');
        return ApiResponse.error(apiResponse.message ?? 'Error al buscar clientes');
      }

      // La respuesta ya viene correctamente tipada gracias a requestWithModel
      final customers = apiResponse.data ?? <CustomerRes>[];

      debugPrint('✅ Clientes encontrados: ${customers.length}');
      return ApiResponse.success(customers);
      
    } catch (e) {
      debugPrint('❌ Error inesperado en búsqueda de clientes: $e');
      return ApiResponse.error('Error inesperado: ${e.toString()}');
    }
  }
}