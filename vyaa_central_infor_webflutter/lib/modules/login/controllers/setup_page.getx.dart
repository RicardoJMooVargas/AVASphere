import 'dart:convert';
import 'package:flutter/foundation.dart';
import 'package:flutter/material.dart';
import 'package:get/get.dart';
import '../../../Core/services/data/hive.service.dart';
import '../../../Core/services/api/system.service.dart';
import '../../../Core/services/api/api.service.dart';
import '../../../Core/models/requests/config_sys_req.module.dart';
import '../../../Core/models/requests/config_admin_req.module.dart';
import '../../../Core/models/responses/check_init_config.module.dart';
import '../../../configs/api_endpoints.config.dart';


class SetupController extends GetxController {
  // Observable variables
  final RxBool isLoading = true.obs;
  final RxString statusMessage = 'Cargando...'.obs;
  final RxString rawData = ''.obs;
  
  // Datos específicos de Hive
  final RxBool isFirstTimeCheck = true.obs;
  final RxBool isSystemInitialized = false.obs;
  final RxString lastRoute = 'No definida'.obs;
  final RxString systemConfigData = ''.obs;
  final RxString userSessionData = ''.obs;
  final RxString appCacheData = ''.obs;
  final RxMap<String, int> databaseStats = <String, int>{}.obs;

  // Estado del sistema desde el servidor
  final Rx<CheckInitConfigData?> systemConfig = Rx<CheckInitConfigData?>(null);
  
  // Estados del formulario
  final RxBool isFormLoading = false.obs;
  final RxString formStatus = ''.obs;
  final RxInt currentStep = 0.obs; // 0: migrations, 1: system config, 2: admin config
  
  // Validación reactiva
  final RxBool isSystemFormValid = false.obs;
  final RxBool isAdminFormValid = false.obs;
  
  // Formularios
  late ConfigSysReq configSysReq;
  late ConfigAdminReq configAdminReq;
  
  // Servicios
  final SystemService _systemService = SystemService();

  @override
  void onInit() {
    super.onInit();
    // Inicializar formularios
    configSysReq = ConfigSysReq();
    configAdminReq = ConfigAdminReq();
    
    // Agregar listeners para validación reactiva
    _setupFormValidation();
    
    loadHiveData();
    checkSystemStatus();
  }

  @override
  void onClose() {
    // Limpiar controladores
    configSysReq.dispose();
    configAdminReq.dispose();
    super.onClose();
  }

  /// Cargar todos los datos de Hive
  Future<void> loadHiveData() async {
    try {
      isLoading.value = true;
      statusMessage.value = 'Cargando datos de Hive...';

      // Cargar configuraciones básicas
      await _loadBasicSettings();
      
      // Cargar configuración del sistema
      await _loadSystemConfig();
      
      // Cargar sesión de usuario
      await _loadUserSession();
      
      // Cargar estadísticas de BD
      await _loadDatabaseStats();

      statusMessage.value = 'Datos cargados correctamente';
      isLoading.value = false;

    } catch (e) {
      statusMessage.value = 'Error al cargar datos: $e';
      isLoading.value = false;
      debugPrint('❌ Error cargando datos de Hive: $e');
    }
  }

  /// Cargar configuraciones básicas de la app
  Future<void> _loadBasicSettings() async {
    try {
      isFirstTimeCheck.value = await HiveService.isFirstTimeCheck();
      isSystemInitialized.value = await HiveService.isSystemInitialized();
      lastRoute.value = await HiveService.getLastRoute() ?? 'No definida';
    } catch (e) {
      debugPrint('Error cargando configuraciones básicas: $e');
    }
  }

  /// Cargar configuración del sistema
  Future<void> _loadSystemConfig() async {
    try {
      final systemConfig = await HiveService.getActiveSystemConfig();
      if (systemConfig != null) {
        systemConfigData.value = jsonEncode(systemConfig.toJson());
      } else {
        systemConfigData.value = 'Sin configuración del sistema';
      }
    } catch (e) {
      systemConfigData.value = 'Error: $e';
      debugPrint('Error cargando configuración del sistema: $e');
    }
  }

  /// Cargar datos de sesión de usuario
  Future<void> _loadUserSession() async {
    try {
      final userSession = await HiveService.getActiveUserSession();
      if (userSession != null) {
        userSessionData.value = jsonEncode(userSession.toJson());
      } else {
        userSessionData.value = 'Sin sesión activa';
      }
    } catch (e) {
      userSessionData.value = 'Error: $e';
      debugPrint('Error cargando sesión de usuario: $e');
    }
  }

  /// Cargar estadísticas de la base de datos
  Future<void> _loadDatabaseStats() async {
    try {
      final stats = await HiveService.getDatabaseStats();
      databaseStats.value = stats;
    } catch (e) {
      debugPrint('Error cargando estadísticas de BD: $e');
    }
  }

  /// Refrescar todos los datos
  Future<void> refreshData() async {
    await loadHiveData();
  }

  /// Obtener icono basado en el estado
  IconData getStatusIcon() {
    if (isLoading.value) return Icons.hourglass_empty;
    if (!isSystemInitialized.value) return Icons.warning;
    return Icons.check_circle;
  }

  /// Configurar validación reactiva de los formularios
  void _setupFormValidation() {
    // Listeners para el formulario del sistema
    configSysReq.companyNameController.addListener(() {
      isSystemFormValid.value = validateSystemForm();
    });
    
    configSysReq.branchNameController.addListener(() {
      isSystemFormValid.value = validateSystemForm();
    });
    
    // Listeners para el formulario del admin
    configAdminReq.userNameController.addListener(() {
      isAdminFormValid.value = validateAdminForm();
    });
    
    configAdminReq.passwordController.addListener(() {
      isAdminFormValid.value = validateAdminForm();
    });
    
    // Validar inicialmente
    isSystemFormValid.value = validateSystemForm();
    isAdminFormValid.value = validateAdminForm();
  }

  /// Obtener datos crudos combinados para mostrar
  String getRawDataForDisplay() {
    Map<String, dynamic> allData = {
      'configuraciones_basicas': {
        'primera_vez': isFirstTimeCheck.value,
        'sistema_inicializado': isSystemInitialized.value,
        'ultima_ruta': lastRoute.value,
      },
      'estadisticas_bd': Map<String, int>.from(databaseStats),
      'configuracion_sistema': systemConfigData.value.isNotEmpty ? 
        (systemConfigData.value.startsWith('{') ? 
          jsonDecode(systemConfigData.value) : 
          systemConfigData.value) : 
        'No disponible',
      'sesion_usuario': userSessionData.value.isNotEmpty ? 
        (userSessionData.value.startsWith('{') ? 
          jsonDecode(userSessionData.value) : 
          userSessionData.value) : 
        'No disponible',
      'estado_sistema': systemConfig.value?.toJson() ?? 'No consultado',
    };

    return const JsonEncoder.withIndent('  ').convert(allData);
  }

  // ========================================
  // MÉTODOS DEL FORMULARIO DE CONFIGURACIÓN
  // ========================================

  /// Verificar el estado actual del sistema
  Future<void> checkSystemStatus() async {
    try {
      isFormLoading.value = true;
      formStatus.value = 'Verificando estado del sistema...';
      
      final config = await _systemService.checkInitialConfig();
      systemConfig.value = config;
      
      // Log detallado para debug
      debugPrint('🔍 Estado del sistema recibido:');
      debugPrint('   - hasConfiguration: ${config.hasConfiguration}');
      debugPrint('   - tableExists: ${config.tableExists}');
      debugPrint('   - requiresMigration: ${config.requiresMigration}');
      
      // Determinar el paso actual basado en la configuración
      if (config.requiresMigration) {
        // Necesita aplicar migraciones primero
        debugPrint('📝 Determinando paso: 0 (Migraciones) - requiresMigration es true');
        currentStep.value = 0;
        formStatus.value = 'Sistema requiere aplicar migraciones';
      } else if (!config.hasConfiguration) {
        // No requiere migraciones pero necesita configuración del sistema
        debugPrint('📝 Determinando paso: 1 (Configuración) - requiresMigration es false pero hasConfiguration es false');
        currentStep.value = 1;
        formStatus.value = 'Sistema listo para configuración';
      } else {
        // Sistema ya configurado completamente
        debugPrint('📝 Determinando paso: 3 (Completado) - hasConfiguration es true y requiresMigration es false');
        currentStep.value = 3;
        formStatus.value = 'Sistema ya configurado';
      }
      
      debugPrint('🎯 Paso actual establecido: ${currentStep.value}');
      
      isFormLoading.value = false;
    } catch (e) {
      isFormLoading.value = false;
      
      // Manejar el caso específico donde las tablas existen pero no hay configuración
      final errorMessage = e.toString();
      if (errorMessage.contains('La tabla ConfigSys existe pero no hay configuración inicial del sistema')) {
        debugPrint('🔧 Error específico detectado: Tablas existen pero sin configuración');
        debugPrint('🔧 Interpretando como: requiresMigration=false, hasConfiguration=false, tableExists=true');
        
        // Crear un objeto de configuración simulado basado en el error
        systemConfig.value = CheckInitConfigData(
          hasConfiguration: false,
          tableExists: true,
          requiresMigration: false,
        );
        
        // Ir directamente a configuración
        currentStep.value = 1;
        formStatus.value = 'Sistema listo para configuración (tablas ya existen)';
        debugPrint('🎯 Paso establecido a configuración: ${currentStep.value}');
      } else {
        // Otros errores
        formStatus.value = 'Error al verificar sistema: $e';
        debugPrint('❌ Error verificando estado del sistema: $e');
      }
    }
  }

  /// Aplicar migraciones del sistema
  Future<void> applyMigrations() async {
    try {
      isFormLoading.value = true;
      formStatus.value = 'Aplicando migraciones...';
      
      final endpoint = ApiEndpoints.system.tools.applyMigrations;
      final response = await ApiService.request(endpoint);
      
      if (response.success) {
        formStatus.value = 'Migraciones aplicadas correctamente';
        currentStep.value = 1; // Pasar a configuración del sistema
        
        // Verificar nuevamente el estado
        await checkSystemStatus();
      } else {
        throw Exception(response.message ?? 'Error al aplicar migraciones');
      }
      
      isFormLoading.value = false;
    } catch (e) {
      isFormLoading.value = false;
      formStatus.value = 'Error al aplicar migraciones: $e';
      debugPrint('❌ Error aplicando migraciones: $e');
    }
  }

  /// Configurar el sistema
  Future<void> configureSystem() async {
    try {
      isFormLoading.value = true;
      formStatus.value = 'Configurando sistema...';
      
      final endpoint = ApiEndpoints.system.config.configureSystem;
      final response = await ApiService.requestWithModel(endpoint, model: configSysReq);
      
      if (response.success) {
        formStatus.value = 'Sistema configurado correctamente';
        currentStep.value = 2; // Pasar a configuración del admin
      } else {
        throw Exception(response.message ?? 'Error al configurar sistema');
      }
      
      isFormLoading.value = false;
    } catch (e) {
      isFormLoading.value = false;
      formStatus.value = 'Error al configurar sistema: $e';
      debugPrint('❌ Error configurando sistema: $e');
    }
  }

  /// Configurar administrador
  Future<void> configureAdmin() async {
    try {
      isFormLoading.value = true;
      formStatus.value = 'Configurando administrador...';
      
      final endpoint = ApiEndpoints.system.config.configureAdmin;
      final response = await ApiService.requestWithModel(endpoint, model: configAdminReq);
      
      if (response.success) {
        formStatus.value = 'Configuración completada exitosamente';
        currentStep.value = 3; // Configuración completa
        
        // Redirigir al login después de un breve delay
        Future.delayed(const Duration(seconds: 2), () {
          Get.offAllNamed('/login');
        });
      } else {
        throw Exception(response.message ?? 'Error al configurar administrador');
      }
      
      isFormLoading.value = false;
    } catch (e) {
      isFormLoading.value = false;
      formStatus.value = 'Error al configurar administrador: $e';
      debugPrint('❌ Error configurando administrador: $e');
    }
  }

  /// Validar formulario del sistema
  bool validateSystemForm() {
    final companyName = configSysReq.companyNameController.text.trim();
    final branchName = configSysReq.branchNameController.text.trim();
    
    debugPrint('🔍 Validando formulario del sistema:');
    debugPrint('   - Nombre empresa: "$companyName" (${companyName.isNotEmpty})');
    debugPrint('   - Nombre sucursal: "$branchName" (${branchName.isNotEmpty})');
    
    final isValid = companyName.isNotEmpty && branchName.isNotEmpty;
    debugPrint('   - Formulario válido: $isValid');
    
    return isValid;
  }

  /// Validar formulario del administrador
  bool validateAdminForm() {
    return configAdminReq.isValid();
  }

  /// Obtener el título del paso actual
  String getCurrentStepTitle() {
    switch (currentStep.value) {
      case 0:
        return 'Aplicar Migraciones';
      case 1:
        return 'Configurar Sistema';
      case 2:
        return 'Configurar Administrador';
      case 3:
        return 'Configuración Completada';
      default:
        return 'Configuración del Sistema';
    }
  }

  /// Obtener descripción del paso actual
  String getCurrentStepDescription() {
    switch (currentStep.value) {
      case 0:
        return 'El sistema necesita aplicar migraciones a la base de datos antes de continuar.';
      case 1:
        return 'Configure los datos básicos del sistema como nombre de empresa y sucursal.';
      case 2:
        return 'Configure las credenciales del usuario administrador del sistema.';
      case 3:
        return 'El sistema ha sido configurado exitosamente y está listo para usar.';
      default:
        return 'Complete los siguientes pasos para configurar el sistema.';
    }
  }

  /// Forzar paso a configuración (para casos donde la lógica automática falla)
  void forceGoToConfiguration() {
    debugPrint('🔧 Forzando paso a configuración del sistema');
    currentStep.value = 1;
    formStatus.value = 'Paso forzado a configuración del sistema';
  }
}