// auth_service.dart
import 'dart:convert';
import 'package:flutter/material.dart';
import 'package:vyaa_central_infor_webflutter/core/core.dart';

import '../../models/auth_req.module.dart';
import 'package:vyaa_central_infor_webflutter/configs/config.dart';
import '../../../../core/middlewares/global_init.middleware.dart';
import '../../../../core/services/data/hive.service.dart';
import '../../../../core/models/data/user_session.model.dart';

class AuthService {
  /// Login con usuario y contraseña
  Future<ApiResponse<LoginUserRes>> login(AuthReq request) async {
    final response = await ApiService.requestWithModel<LoginUserRes, AuthReq>(
      ApiEndpoints.common.auth.loginWithModel,
      model: request,
    );

    if (response.success && response.data != null) {
      debugPrint('🔐 ========== INICIO DE GUARDADO EN HIVE ==========');
      
      try {
        // 1. Guardar sesión de usuario en Hive
        debugPrint('👤 Guardando sesión de usuario...');
        final userSession = UserSession.create(
          userId: response.data!.user.id.toString(),
          username: response.data!.user.userName,
          email: '${response.data!.user.userName}@system.local', // Email derivado si no viene
          token: response.data!.token,
          rememberMe: true,
          roles: response.data!.user.rol,
          expiresAt: DateTime.now().add(const Duration(hours: 2)), // Token expira en 2 horas
        );
        
        await HiveService.saveUserSession(userSession);
        debugPrint('✅ Sesión de usuario guardada:');
        debugPrint('   - User ID: ${userSession.userId}');
        debugPrint('   - Username: ${userSession.username}');
        debugPrint('   - Rol: ${userSession.roles}');
        debugPrint('   - Token: ${userSession.token?.substring(0, 20)}...');
        debugPrint('   - Expira: ${userSession.expiresAt}');

        // 2. Guardar configuración del sistema en cache general
        debugPrint('⚙️ Guardando configuración del sistema en cache...');
        
        // Guardar cada campo importante de configSys
        await HiveService.setCache(
          key: 'configSys.idConfigSys',
          value: response.data!.configSys.idConfigSys.toString(),
          category: 'system_config',
          description: 'ID de configuración del sistema',
        );
        
        await HiveService.setCache(
          key: 'configSys.companyName',
          value: response.data!.configSys.companyName,
          category: 'system_config',
          description: 'Nombre de la empresa',
        );
        
        await HiveService.setCache(
          key: 'configSys.branchName',
          value: response.data!.configSys.branchName,
          category: 'system_config',
          description: 'Nombre de la sucursal',
        );
        
        await HiveService.setCache(
          key: 'configSys.logoUrl',
          value: response.data!.configSys.logoUrl,
          category: 'system_config',
          description: 'URL del logo',
        );

        // Guardar toda la configuración como JSON
        await HiveService.setCache(
          key: 'configSys.full',
          value: jsonEncode(response.data!.configSys.toJson()),
          category: 'system_config',
          description: 'Configuración completa del sistema',
        );

        debugPrint('✅ Configuración del sistema guardada:');
        debugPrint('   - ID Config: ${response.data!.configSys.idConfigSys}');
        debugPrint('   - Empresa: ${response.data!.configSys.companyName}');
        debugPrint('   - Sucursal: ${response.data!.configSys.branchName}');
        debugPrint('   - Logo URL: ${response.data!.configSys.logoUrl}');
        debugPrint('   - Colores: ${response.data!.configSys.colors.length}');
        debugPrint('   - Módulos no usados: ${response.data!.configSys.notUseModules.length}');

        // 3. Guardar información del usuario en cache
        debugPrint('👨‍💼 Guardando información del usuario en cache...');
        await HiveService.setCache(
          key: 'user.id',
          value: response.data!.user.id.toString(),
          category: 'user_data',
          description: 'ID del usuario',
        );
        
        await HiveService.setCache(
          key: 'user.name',
          value: response.data!.user.name,
          category: 'user_data',
          description: 'Nombre del usuario',
        );
        
        await HiveService.setCache(
          key: 'user.lastName',
          value: response.data!.user.lastName,
          category: 'user_data',
          description: 'Apellido del usuario',
        );
        
        await HiveService.setCache(
          key: 'user.full',
          value: jsonEncode(response.data!.user.toJson()),
          category: 'user_data',
          description: 'Información completa del usuario',
        );

        // 4. Guardar módulos del usuario
        debugPrint('📦 Guardando módulos del usuario...');
        await HiveService.setCache(
          key: 'user.modules',
          value: jsonEncode(response.data!.user.modules.map((m) => m.toJson()).toList()),
          category: 'user_data',
          description: 'Módulos asignados al usuario',
        );

        // 5. Guardar permisos del usuario
        debugPrint('🔐 Guardando permisos del usuario...');
        await HiveService.setCache(
          key: 'user.permissions',
          value: jsonEncode(response.data!.user.permissions.map((p) => p.toJson()).toList()),
          category: 'user_data',
          description: 'Permisos asignados al usuario',
        );

        debugPrint('✅ Información del usuario guardada:');
        debugPrint('   - ID: ${response.data!.user.id}');
        debugPrint('   - Nombre: ${response.data!.user.name} ${response.data!.user.lastName}');
        debugPrint('   - Username: ${response.data!.user.userName}');
        debugPrint('   - ID Config Sys: ${response.data!.user.idConfigSys}');
        debugPrint('   - Módulos: ${response.data!.user.modules.length}');
        debugPrint('   - Permisos: ${response.data!.user.permissions.length}');
        
        // Log detallado de módulos
        if (response.data!.user.modules.isNotEmpty) {
          debugPrint('📦 Módulos del usuario:');
          for (var module in response.data!.user.modules) {
            debugPrint('   - ${module.name} (${module.normalized})');
          }
        }
        
        // Log detallado de permisos
        if (response.data!.user.permissions.isNotEmpty) {
          debugPrint('🔐 Permisos del usuario:');
          for (var permission in response.data!.user.permissions) {
            debugPrint('   - ${permission.name} [${permission.type}] (${permission.normalized})');
          }
        }

        // 6. Verificar que los datos se guardaron correctamente
        debugPrint('🔍 Verificando datos guardados...');
        
        final savedSession = await HiveService.getActiveUserSession();
        if (savedSession != null) {
          debugPrint('✅ Sesión verificada en Hive:');
          debugPrint('   - Es válida: ${savedSession.isValid}');
          debugPrint('   - User ID: ${savedSession.userId}');
          debugPrint('   - Username: ${savedSession.username}');
        } else {
          debugPrint('⚠️ No se encontró sesión activa después de guardar');
        }

        final savedConfigId = await HiveService.getCache('configSys.idConfigSys');
        final savedCompanyName = await HiveService.getCache('configSys.companyName');
        debugPrint('✅ Config verificada en cache:');
        debugPrint('   - Config ID: $savedConfigId');
        debugPrint('   - Empresa: $savedCompanyName');

        final savedModules = await HiveService.getCache('user.modules');
        final savedPermissions = await HiveService.getCache('user.permissions');
        if (savedModules != null) {
          final modulesList = jsonDecode(savedModules) as List;
          debugPrint('✅ Módulos verificados en cache: ${modulesList.length} módulos');
        }
        if (savedPermissions != null) {
          final permissionsList = jsonDecode(savedPermissions) as List;
          debugPrint('✅ Permisos verificados en cache: ${permissionsList.length} permisos');
        }

        // 7. Obtener estadísticas de la base de datos
        final stats = await HiveService.getDatabaseStats();
        debugPrint('📊 Estadísticas de Hive:');
        debugPrint('   - User Sessions: ${stats['userSession']}');
        debugPrint('   - App Cache: ${stats['appCache']}');
        debugPrint('   - System Config: ${stats['systemConfig']}');
        debugPrint('   - App Settings: ${stats['appSettings']}');

        debugPrint('🔐 ========== FIN DE GUARDADO EN HIVE ==========');
        
        // Resetear el middleware para que pueda redirigir correctamente después del login
        GlobalInitMiddleware.reset();
        
      } catch (e, stackTrace) {
        debugPrint('❌ ERROR al guardar en Hive: $e');
        debugPrint('Stack trace: $stackTrace');
        // No retornar error, el login fue exitoso, solo el guardado falló
      }
      
      return response;
    }

    return ApiResponse.error(response.message ?? 'Error al iniciar sesión');
  }

  /// Logout - Cierra la sesión y limpia todos los datos
  Future<void> logout() async {
    debugPrint('🚪 ========== INICIANDO LOGOUT (AuthService) ==========');
    
    try {
      // 1. Cerrar sesión activa en Hive
      debugPrint('👤 Cerrando sesión activa en Hive...');
      await HiveService.logout();
      debugPrint('✅ Sesión cerrada en Hive');
      
      // 2. Limpiar configuración del sistema
      debugPrint('⚙️ Limpiando configuración del sistema...');
      await HiveService.removeCache('configSys.idConfigSys');
      await HiveService.removeCache('configSys.companyName');
      await HiveService.removeCache('configSys.branchName');
      await HiveService.removeCache('configSys.logoUrl');
      await HiveService.removeCache('configSys.full');
      debugPrint('✅ Configuración del sistema limpiada');
      
      // 3. Limpiar información del usuario
      debugPrint('👨‍💼 Limpiando información del usuario...');
      await HiveService.removeCache('user.id');
      await HiveService.removeCache('user.name');
      await HiveService.removeCache('user.lastName');
      await HiveService.removeCache('user.full');
      await HiveService.removeCache('user.modules');
      await HiveService.removeCache('user.permissions');
      debugPrint('✅ Información del usuario limpiada (incluyendo módulos y permisos)');
      
      // 4. Limpiar flags de inicialización
      debugPrint('🔧 Limpiando flags de inicialización...');
      await HiveService.clearSystemInitFlags();
      debugPrint('✅ Flags de inicialización limpiados');
      
      // 5. Limpiar cache antiguo (por compatibilidad)
      debugPrint('🧹 Limpiando cache antiguo...');
      await CacheService.clearAll();
      debugPrint('✅ Cache antiguo limpiado');
      
      // 6. Verificar limpieza
      debugPrint('🔍 Verificando limpieza...');
      final session = await HiveService.getActiveUserSession();
      final hasSession = session != null;
      debugPrint('   - Sesión activa encontrada: $hasSession');
      
      final stats = await HiveService.getDatabaseStats();
      debugPrint('📊 Estadísticas después del logout:');
      debugPrint('   - User Sessions: ${stats['userSession']}');
      debugPrint('   - App Cache: ${stats['appCache']}');
      
      // 7. Resetear el middleware
      debugPrint('🔄 Reseteando middleware...');
      GlobalInitMiddleware.reset();
      debugPrint('✅ Middleware reseteado');
      
      debugPrint('🚪 ========== LOGOUT COMPLETADO (AuthService) ==========');
      
    } catch (e, stackTrace) {
      debugPrint('❌ ERROR durante el logout: $e');
      debugPrint('Stack trace: $stackTrace');
      rethrow;
    }
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
  */
}
