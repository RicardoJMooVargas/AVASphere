import 'dart:convert';
import 'package:flutter/foundation.dart';
import 'hive.service.dart';
import '../../models/responses/login_user_res.module.dart';

/// Servicio para acceder fácilmente a los datos del usuario guardados en Hive
class UserDataService {
  /// Obtener el ID del usuario actual
  static Future<String?> getUserId() async {
    return await HiveService.getCache('user.id');
  }

  /// Obtener el nombre completo del usuario
  static Future<String?> getUserFullName() async {
    final name = await HiveService.getCache('user.name');
    final lastName = await HiveService.getCache('user.lastName');
    
    if (name != null && lastName != null) {
      return '$name $lastName';
    }
    return name ?? lastName;
  }

  /// Obtener el nombre del usuario
  static Future<String?> getUserName() async {
    return await HiveService.getCache('user.name');
  }

  /// Obtener el apellido del usuario
  static Future<String?> getUserLastName() async {
    return await HiveService.getCache('user.lastName');
  }

  /// Obtener el rol del usuario desde la sesión
  static Future<String?> getUserRole() async {
    final session = await HiveService.getActiveUserSession();
    return session?.roles;
  }

  /// Obtener todos los módulos del usuario
  static Future<List<UserModule>> getUserModules() async {
    try {
      final modulesJson = await HiveService.getCache('user.modules');
      if (modulesJson == null || modulesJson.isEmpty) {
        debugPrint('⚠️ No se encontraron módulos del usuario');
        return [];
      }

      final List<dynamic> modulesList = jsonDecode(modulesJson);
      final modules = modulesList
          .map((json) => UserModule.fromJson(json as Map<String, dynamic>))
          .toList();
      
      debugPrint('✅ Módulos del usuario cargados: ${modules.length}');
      return modules;
    } catch (e) {
      debugPrint('❌ Error al obtener módulos del usuario: $e');
      return [];
    }
  }

  /// Obtener todos los permisos del usuario
  static Future<List<UserPermission>> getUserPermissions() async {
    try {
      final permissionsJson = await HiveService.getCache('user.permissions');
      if (permissionsJson == null || permissionsJson.isEmpty) {
        debugPrint('⚠️ No se encontraron permisos del usuario');
        return [];
      }

      final List<dynamic> permissionsList = jsonDecode(permissionsJson);
      final permissions = permissionsList
          .map((json) => UserPermission.fromJson(json as Map<String, dynamic>))
          .toList();
      
      debugPrint('✅ Permisos del usuario cargados: ${permissions.length}');
      return permissions;
    } catch (e) {
      debugPrint('❌ Error al obtener permisos del usuario: $e');
      return [];
    }
  }

  /// Verificar si el usuario tiene acceso a un módulo específico
  /// Acepta el nombre del módulo (ej: "Sales", "Inventory") o la ruta (ej: "/sales", "/inventory")
  static Future<bool> hasModuleAccess(String moduleName) async {
    final modules = await getUserModules();
    
    // Limpiar el nombre del módulo (remover / y convertir a lowercase)
    final cleanModuleName = moduleName.replaceFirst('/', '').toLowerCase();
    
    return modules.any((module) => 
      module.normalized.toLowerCase().replaceFirst('/', '') == cleanModuleName ||
      module.name.toLowerCase() == cleanModuleName
    );
  }
  
  /// Verificar si el usuario tiene acceso a múltiples módulos
  static Future<Map<String, bool>> hasMultipleModuleAccess(List<String> moduleNames) async {
    final modules = await getUserModules();
    final result = <String, bool>{};
    
    for (var moduleName in moduleNames) {
      final cleanModuleName = moduleName.replaceFirst('/', '').toLowerCase();
      result[moduleName] = modules.any((module) => 
        module.normalized.toLowerCase().replaceFirst('/', '') == cleanModuleName ||
        module.name.toLowerCase() == cleanModuleName
      );
    }
    
    return result;
  }

  /// Verificar si el usuario tiene un permiso específico
  static Future<bool> hasPermission(String permissionName) async {
    final permissions = await getUserPermissions();
    return permissions.any((permission) => 
      permission.normalized.toLowerCase() == permissionName.toLowerCase() ||
      permission.name.toLowerCase() == permissionName.toLowerCase()
    );
  }

  /// Verificar si el usuario es super admin (tiene FULL_ACCESS)
  static Future<bool> isSuperAdmin() async {
    final permissions = await getUserPermissions();
    return permissions.any((permission) => 
      permission.normalized == 'FULL_ACCESS' ||
      permission.type == 'SUPER_ADMIN'
    );
  }

  /// Obtener información completa del usuario
  static Future<User?> getFullUserData() async {
    try {
      final userJson = await HiveService.getCache('user.full');
      if (userJson == null || userJson.isEmpty) {
        debugPrint('⚠️ No se encontró información completa del usuario');
        return null;
      }

      final userData = jsonDecode(userJson) as Map<String, dynamic>;
      return User.fromJson(userData);
    } catch (e) {
      debugPrint('❌ Error al obtener información completa del usuario: $e');
      return null;
    }
  }

  /// Obtener nombres de todos los módulos
  static Future<List<String>> getModuleNames() async {
    final modules = await getUserModules();
    return modules.map((m) => m.name).toList();
  }

  /// Obtener rutas normalizadas de todos los módulos
  static Future<List<String>> getModuleRoutes() async {
    final modules = await getUserModules();
    return modules.map((m) => m.normalized).toList();
  }

  /// Verificar si el usuario tiene permisos activos
  static Future<bool> hasActivePermissions() async {
    final permissions = await getUserPermissions();
    return permissions.any((p) => p.status.toLowerCase() == 'active');
  }

  /// Obtener el ID de configuración del sistema del usuario
  static Future<int?> getUserConfigSysId() async {
    final user = await getFullUserData();
    return user?.idConfigSys;
  }

  /// Log de información del usuario (útil para debugging)
  static Future<void> logUserInfo() async {
    debugPrint('👤 ========== INFORMACIÓN DEL USUARIO ==========');
    
    final userId = await getUserId();
    final fullName = await getUserFullName();
    final role = await getUserRole();
    final modules = await getUserModules();
    final permissions = await getUserPermissions();
    final isSuperAdm = await isSuperAdmin();
    
    debugPrint('   - ID: $userId');
    debugPrint('   - Nombre: $fullName');
    debugPrint('   - Rol: $role');
    debugPrint('   - Es Super Admin: $isSuperAdm');
    debugPrint('   - Módulos (${modules.length}):');
    for (var module in modules) {
      debugPrint('     * ${module.name} → ${module.normalized}');
    }
    debugPrint('   - Permisos (${permissions.length}):');
    for (var permission in permissions) {
      debugPrint('     * ${permission.name} [${permission.type}] (${permission.status})');
    }
    
    debugPrint('👤 ========================================');
  }
}
