// auth_enhanced_service.dart
import 'dart:convert';
import 'package:vyaa_central_infor_webflutter/core/core.dart';
import 'package:vyaa_central_infor_webflutter/core/models/responses/login_user_res.module.dart';

import '../../models/auth_req.module.dart';
import 'package:vyaa_central_infor_webflutter/configs/config.dart';

class AuthEnhancedService {
  
  /// Login con mapeo automático de modelos
  /// Devuelve el objeto LoginUserRes directamente mapeado
  Future<ApiResponse<LoginUserRes>> loginWithModel(AuthReq request) async {
    final response = await ApiService.requestWithModel<LoginUserRes, AuthReq>(
      ApiEndpoints.common.auth.loginWithModel,
      model: request,
    );

    if (response.success && response.data != null) {
      // Guardar token y datos del usuario
      await CacheService.saveToken(response.data!.token);
      await CacheService.saveUserId(response.data!.user.id.toString());
      
      return response;
    }

    return ApiResponse.error(response.message ?? 'Error al iniciar sesión');
  }

  /// Login tradicional sin mapeo (mantiene compatibilidad)
  /// Devuelve Map<String, dynamic> y maneja el mapeo manualmente
  Future<ApiResponse<String>> loginTraditional(AuthReq request) async {
    final response = await ApiService.request(
      ApiEndpoints.common.auth.login,
      data: request.toJson(),
    );

    if (response.success && response.data != null) {
      final decoded = response.data is String ? jsonDecode(response.data) : response.data;

      final token = decoded['token'] ?? decoded['access_token'];
      if (token != null && token.isNotEmpty) {
        await CacheService.saveToken(token);

        final user = decoded['user'];
        if (user != null && user['id'] != null) {
          await CacheService.saveUserId(user['id'].toString());
        }

        return ApiResponse.success(token);
      }

      return ApiResponse.error('Token no encontrado en la respuesta');
    }

    return ApiResponse.error(response.message ?? 'Error al iniciar sesión');
  }

  /// Ejemplo de uso con datos JSON directos (sin modelo de entrada)
  Future<ApiResponse<Map<String, dynamic>>> loginWithJsonData(String userName, String password) async {
    final response = await ApiService.request(
      ApiEndpoints.common.auth.login,
      data: {
        'userName': userName,
        'password': password,
      },
    );

    if (response.success) {
      return ApiResponse.success(response.data);
    }

    return ApiResponse.error(response.message ?? 'Error al iniciar sesión');
  }

  /// Validar token actual
  Future<ApiResponse<Map<String, dynamic>>> validateToken() async {
    final response = await ApiService.request(ApiEndpoints.common.auth.validateToken);

    if (response.success && response.data != null) {
      return ApiResponse.success(response.data);
    }

    return ApiResponse.error(response.message ?? 'Error al validar token');
  }

  /// Logout
  Future<void> logout() async {
    await CacheService.clearAll();
  }
}