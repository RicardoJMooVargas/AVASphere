// auth_service.dart
import 'dart:convert';
import 'package:vyaa_central_infor_webflutter/Core/core.dart';

import '../../models/auth_req.module.dart';
import 'package:vyaa_central_infor_webflutter/configs/config.dart';
import '../../../../Core/middlewares/global_init.middleware.dart';

class AuthService {
  /// Login con usuario y contraseña
  Future<ApiResponse<LoginUserRes>> login(AuthReq request) async {
    final response = await ApiService.requestWithModel<LoginUserRes, AuthReq>(
      ApiEndpoints.common.auth.loginWithModel,
      model: request,
    );

    if (response.success && response.data != null) {
      // Guardar token y datos del usuario
      await CacheService.saveToken(response.data!.token);
      await CacheService.saveUserId(response.data!.user.id.toString());
      
      // Resetear el middleware para que pueda redirigir correctamente después del login
      GlobalInitMiddleware.reset();
      
      return response;
    }

    return ApiResponse.error(response.message ?? 'Error al iniciar sesión');
  }

  /// Validar token actual
  /* ACTUALMENTE SIN USO
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
  */
}
