import 'dart:convert';

import 'package:http/http.dart' as http;
import '../local/cache_service.service.dart';
import '../../models/requests/auth_req.module.dart';
import '../../configs/api_settings.config.dart';
import '../../configs/api_response.config.dart';
import '../../core/internalControllers/notification_services.dart';

class AuthService {
  final ApiSettings _settings;

  AuthService([ApiSettings? settings]) : _settings = settings ?? ApiSettings();

  Uri _url(String path) => Uri.parse('${_settings.baseUrl}$path');

  /// Login with username and password. Returns ApiResponse with token string on success.
  Future<ApiResponse<String>> login(AuthReq request) async {
    try {
      final uri = _url('/api/system/Auth/login');
      final body = jsonEncode(request.toJson());

      final response = await http.post(
        uri,
        headers: _settings.headers,
        body: body,
      );

      if (response.statusCode == 200 || response.statusCode == 201) {
        final Map<String, dynamic> decoded = jsonDecode(response.body);
        // Assuming the API returns a token field (jwt) - adapt if different
        final token = decoded['token'] as String? ?? decoded['access_token'] as String? ?? '';
        if (token.isNotEmpty) {
          await CacheService.saveToken(token);
          return ApiResponse.success(token);
        }
        return ApiResponse.error('Token no encontrado en la respuesta del servidor');
      }

      final errorMessage = ApiResponse.getErrorMessage(response.statusCode, response.body);
      return ApiResponse.error(errorMessage);
      
    } catch (e) {
      // Handle network errors, JSON parsing errors, etc.
      if (e.toString().contains('SocketException') || e.toString().contains('TimeoutException')) {
        return ApiResponse.error('Error de conexión: Verifique su conexión a internet');
      } else if (e.toString().contains('FormatException')) {
        return ApiResponse.error('Error en el formato de respuesta del servidor');
      } else {
        return ApiResponse.error('Error inesperado: ${e.toString()}');
      }
    }
  }

  /// Validate token. Returns ApiResponse with user data on success.
  Future<ApiResponse<Map<String, dynamic>>> validateToken() async {
    try {
      final token = await CacheService.getToken();
      if (token == null) {
        return ApiResponse.error('No hay token guardado: Debe iniciar sesión');
      }

      final uri = _url('/api/system/Auth/validate-token');

      final headers = Map<String, String>.from(_settings.headers);
      headers['Authorization'] = 'Bearer $token';

      final response = await http.get(uri, headers: headers);

      if (response.statusCode == 200) {
        final Map<String, dynamic> decoded = jsonDecode(response.body);
        return ApiResponse.success(decoded);
      }

      // If token is invalid, handle token expiration
      if (response.statusCode == 401) {
        NotificationService.handleTokenExpired();
        return ApiResponse.error('Token expirado');
      }

      final errorMessage = ApiResponse.getErrorMessage(response.statusCode, response.body);
      return ApiResponse.error(errorMessage);
      
    } catch (e) {
      // Handle network errors, JSON parsing errors, etc.
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
