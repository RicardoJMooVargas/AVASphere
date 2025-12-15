import 'package:flutter/foundation.dart';
import 'package:vyaa_central_infor_webflutter/Core/models/responses/check_init_config.module.dart';
import 'package:vyaa_central_infor_webflutter/Core/models/responses/diagnose_migrations.module.dart';
import 'package:vyaa_central_infor_webflutter/configs/api_endpoints.config.dart';
import 'api.service.dart';

class SystemService {
  /// Verifica la configuración inicial del sistema
  /// Retorna información sobre si el sistema tiene configuración,
  /// si las tablas existen y si requiere migración
  ///
  /// Ejemplo de uso:
  /// ```dart
  /// final systemService = SystemService();
  /// try {
  ///   final configResponse = await systemService.checkInitialConfig();
  ///   if (configResponse.success) {
  ///     if (!configResponse.data.hasConfiguration) {
  ///       // Mostrar pantalla de configuración inicial
  ///       print('Sistema requiere configuración inicial');
  ///     } else if (configResponse.data.requiresMigration) {
  ///       // Mostrar pantalla de migración
  ///       print('Sistema requiere migración');
  ///     } else {
  ///       // Sistema configurado correctamente
  ///       print('Sistema configurado correctamente');
  ///     }
  ///   }
  /// } catch (e) {
  ///   print('Error: $e');
  /// }
  /// ```
  Future<CheckInitConfigData> checkInitialConfig() async {
    try {
      debugPrint('🔍 Verificando configuración inicial del sistema...');

      final endpoint = ApiEndpoints.system.config.checkInitialConfig;
      final apiResponse = await ApiService.requestWithModel(endpoint);

      if (!apiResponse.success) {
        throw Exception(apiResponse.message ?? 'Error al verificar configuración inicial');
      }

      final response = apiResponse.data as CheckInitConfigData;
      return response;
    } catch (e) {
      debugPrint('❌ Error al verificar configuración inicial: $e');
      rethrow;
    }
  }

  /// Diagnóstica el estado de las migraciones del sistema
  /// Retorna información detallada sobre las migraciones aplicadas,
  /// pendientes y el estado de la conexión a la base de datos
  ///
  /// Ejemplo de uso:
  /// ```dart
  /// final systemService = SystemService();
  /// try {
  ///   final diagnosisResponse = await systemService.diagnoseMigrations();
  ///   if (diagnosisResponse.success) {
  ///     print('Diagnóstico: ${diagnosisResponse.data.diagnosis}');
  ///     print('Info: ${diagnosisResponse.data.info}');
  ///   }
  /// } catch (e) {
  ///   print('Error: $e');
  /// }
  /// ```
  Future<DiagnoseMigrationsResponse> diagnoseMigrations() async {
    try {
      debugPrint('🔍 Diagnosticando estado de migraciones del sistema...');

      final endpoint = ApiEndpoints.system.config.diagnoseMigrations;
      final apiResponse = await ApiService.request(endpoint);

      if (!apiResponse.success) {
        throw Exception(apiResponse.message ?? 'Error al diagnosticar migraciones');
      }

      // Mapear la respuesta completa (no usar el campo 'data' extraído)
      final responseData = apiResponse.data as Map<String, dynamic>;
      final mappedResponse = DiagnoseMigrationsResponse.fromJson(responseData);

      return mappedResponse;
    } catch (e) {
      debugPrint('❌ Error al diagnosticar migraciones: $e');
      rethrow;
    }
  }
}