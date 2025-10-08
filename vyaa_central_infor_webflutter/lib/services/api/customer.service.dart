import 'dart:convert';
import 'package:http/http.dart' as http;
import '../local/cache_service.service.dart';
import '../../configs/api_settings.config.dart';
import '../../configs/api_response.config.dart';
import '../../models/responses/customer_res.module.dart';
import '../../core/internalControllers/notification_services.dart';

class CustomerService {
  final ApiSettings _settings;

  CustomerService([ApiSettings? settings]) : _settings = settings ?? ApiSettings();

  Uri _url(String path, [Map<String, String>? queryParameters]) {
    return Uri.parse('${_settings.baseUrl}$path').replace(queryParameters: queryParameters);
  }

  Future<ApiResponse<List<CustomerRes>>> searchCustomers({
    required String name,
  }) async {
    try {
      // Validar que el nombre tenga al menos 3 caracteres
      if (name.isEmpty) {
        return ApiResponse.error('El término de búsqueda no puede estar vacío');
      }

      if (name.length < 2) {
        return ApiResponse.error('Ingrese al menos 3 caracteres para realizar la búsqueda');
      }
  
      final token = await CacheService.getToken();
      if (token == null) {
        return ApiResponse.error('No hay token guardado: Debe iniciar sesión');
      }

      final Map<String, String> queryParameters = {
        'name': name,
      };

      final uri = _url('/api/customer/search', queryParameters);

      final headers = Map<String, String>.from(_settings.headers);
      headers['Authorization'] = 'Bearer $token';

      final response = await http.get(
        uri,
        headers: headers,
      );

      if (response.statusCode == 200) {
        final List<dynamic> jsonResponse = json.decode(response.body);
        final customers = jsonResponse.map((json) => CustomerRes.fromJson(json)).toList();
        return ApiResponse.success(customers);
      }

      // Si el servidor devuelve 400 (Bad Request) por nombre vacío o muy corto
      if (response.statusCode == 400) {
        return ApiResponse.error('Término de búsqueda inválido: Ingrese al menos 3 caracteres');
      }

      // If token is invalid, handle token expiration
      if (response.statusCode == 401) {
        NotificationService.handleTokenExpired();
        return ApiResponse.error('Token expirado');
      }

      final errorMessage = ApiResponse.getErrorMessage(response.statusCode, response.body);
      return ApiResponse.error(errorMessage);
    } catch (e) {
      if (e.toString().contains('SocketException') || e.toString().contains('TimeoutException')) {
        return ApiResponse.error('Error de conexión: Verifique su conexión a internet');
      } else if (e.toString().contains('FormatException')) {
        return ApiResponse.error('Error en el formato de respuesta del servidor');
      } else {
        return ApiResponse.error('Error inesperado: ${e.toString()}');
      }
    }
  }
}