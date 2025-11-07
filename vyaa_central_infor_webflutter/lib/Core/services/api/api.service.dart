// api_client.dart
import 'dart:convert';
import 'package:http/http.dart' as http;
import 'package:flutter/foundation.dart';
import '../../../configs/api_settings.config.dart';
import '../../services/data/cache.service.dart';
import '../../../configs/api_response.config.dart';
import '../../models/base/api_endpoints.module.dart';
import 'package:vyaa_central_infor_webflutter/configs/enums.dart';

class ApiService {
  static final _settings = ApiSettings();

  // Método genérico con mapeo automático
  static Future<ApiResponse<T>> requestWithModel<T, R>(
    ApiEndpoint<R, T> endpoint, {
    R? model,
    Map<String, String>? urlParams,
  }) async {
    try {
      // Mapear el modelo de entrada si existe
      Map<String, dynamic>? data;
      if (model != null && endpoint.requestMapper != null) {
        data = endpoint.requestMapper!(model);
      }

      // Realizar la petición base
      final response = await _makeRequest(endpoint, data: data, urlParams: urlParams);
      
      if (!response.success) {
        return ApiResponse.error(response.message ?? 'Error en la petición');
      }

      // Extraer datos del formato estándar { "success": true, "message": "...", "data": {...} }
      final extractedData = _extractDataFromStandardResponse(response.data);
      
      // Mapear la respuesta si existe un mapper
      if (endpoint.responseMapper != null && extractedData != null) {
        try {
          final mappedData = endpoint.responseMapper!(extractedData);
          return ApiResponse.success(mappedData, statusCode: response.statusCode);
        } catch (e) {
          return ApiResponse.error('Error al mapear la respuesta: ${e.toString()}');
        }
      }

      // Si no hay mapper, devolver los datos extraídos tal como vienen
      return ApiResponse.success(extractedData as T, statusCode: response.statusCode);
    } catch (e) {
      return ApiResponse.error('Error inesperado: ${e.toString()}');
    }
  }

  // Método original para compatibilidad hacia atrás
  static Future<ApiResponse<dynamic>> request(
    ApiEndpoint endpoint, {
    Map<String, dynamic>? data,
    Map<String, String>? urlParams,
  }) async {
    final response = await _makeRequest(endpoint, data: data, urlParams: urlParams);
    
    if (!response.success) {
      return response;
    }
    
    // Extraer datos del formato estándar para mantener consistencia
    final extractedData = _extractDataFromStandardResponse(response.data);
    return ApiResponse.success(extractedData, statusCode: response.statusCode);
  }

  // Método interno para realizar la petición HTTP
  static Future<ApiResponse<dynamic>> _makeRequest(
    ApiEndpoint endpoint, {
    Map<String, dynamic>? data,
    Map<String, String>? urlParams,
  }) async {
    try {
      // Construir la URL base
      String url = '${_settings.baseUrl}${endpoint.path}';

      // Reemplazar parámetros dinámicos {id}, {name}, etc.
      if (urlParams != null) {
        urlParams.forEach((key, value) {
          url = url.replaceAll('{$key}', value);
        });
      }

      // Headers base
      final headers = Map<String, String>.from(_settings.headers);

      // Token si requiere autenticación
      if (endpoint.requiresAuth) {
        final token = await CacheService.getToken();
        if (token != null && token.isNotEmpty) {
          headers['Authorization'] = 'Bearer $token';
        }
      }

      final uri = Uri.parse(url);
      http.Response response;

      debugPrint('➡️ [${endpoint.method.name.toUpperCase()}] $url');

      switch (endpoint.method) {
        case HttpMethod.get:
          final uriWithQuery = endpoint.useQuery && data != null
              ? uri.replace(queryParameters: data.map((k, v) => MapEntry(k, v.toString())))
              : uri;
          response = await http.get(uriWithQuery, headers: headers);
          break;

        case HttpMethod.post:
          response = await http.post(uri, headers: headers, body: endpoint.useBody ? jsonEncode(data) : null);
          break;

        case HttpMethod.put:
          response = await http.put(uri, headers: headers, body: endpoint.useBody ? jsonEncode(data) : null);
          break;

        case HttpMethod.delete:
          response = await http.delete(uri, headers: headers);
          break;

        case HttpMethod.patch:
          response = await http.patch(uri, headers: headers, body: endpoint.useBody ? jsonEncode(data) : null);
          break;
      }

      // Manejo de respuesta
      if (response.statusCode >= 200 && response.statusCode < 300) {
        final decoded = jsonDecode(response.body);
        
        // Verificar si la respuesta tiene el formato estándar y success = false
        if (decoded is Map<String, dynamic> && 
            decoded.containsKey('success') && 
            decoded['success'] == false) {
          
          // El servidor respondió con éxito HTTP pero indica error en el negocio
          final errorMessage = decoded['message']?.toString() ?? 'Error en la operación';
          return ApiResponse.error(errorMessage, statusCode: response.statusCode);
        }
        
        return ApiResponse.success(decoded, statusCode: response.statusCode);
      }

      // Token expirado
      if (response.statusCode == 401) {
        await CacheService.clearAll();
        return ApiResponse.error('Token expirado o no autorizado', statusCode: response.statusCode);
      }

      final msg = ApiResponse.getErrorMessage(response.statusCode, response.body);
      return ApiResponse.error(msg, statusCode: response.statusCode);
    } catch (e) {
      return ApiResponse.error('Error inesperado: ${e.toString()}');
    }
  }

  /// Extrae los datos del formato estándar de respuesta de la API
  /// Formato esperado: { "success": true, "message": "...", "data": {...} }
  static dynamic _extractDataFromStandardResponse(dynamic responseBody) {
    if (responseBody == null) return null;
    
    // Si la respuesta es un Map y tiene el formato estándar
    if (responseBody is Map<String, dynamic>) {
      // Verificar si tiene el formato estándar con "success", "message" y "data"
      if (responseBody.containsKey('success') && 
          responseBody.containsKey('data')) {
        
        // Si success es false, significa que hay un error en el backend
        if (responseBody['success'] == false) {
          // Esto debería manejarse en _makeRequest, pero por seguridad lo validamos aquí también
          return null;
        }
        
        // Retornar el contenido de "data"
        return responseBody['data'];
      }
    }
    
    // Si no tiene el formato estándar, devolver la respuesta tal como viene
    // Esto mantiene compatibilidad con APIs que no usen el formato estándar
    return responseBody;
  }
}