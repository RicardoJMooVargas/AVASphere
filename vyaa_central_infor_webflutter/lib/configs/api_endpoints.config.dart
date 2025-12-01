// api_endpoints.dart
import 'package:flutter/foundation.dart';
import '../Core/core.dart';
import '../modules/login/models/auth_req.module.dart';
import '../modules/sales/models/requests/quotation_req.module.dart';
import '../modules/sales/models/requests/quotation_update_req.module.dart';
import '../modules/sales/models/requests/create_followup_req.module.dart';
import '../modules/sales/models/response/quotation_res.module.dart';
import '../Core/models/responses/customer_res.module.dart';
import 'package:vyaa_central_infor_webflutter/configs/config.dart';

class ApiEndpoints {
  static const String root = '/api';

  static const common = _CommonModule();
  static const sales = _SalesModule();
  static const system = _SystemModule();
  static const customer = _CustomerModule();
}

// MODULO DE CLIENTE
class _CustomerModule {
  const _CustomerModule();

  /// GET /api/Customer/Search
  /// Endpoint para buscar clientes por nombre
  ApiEndpoint<dynamic, List<CustomerRes>> get searchCustomers =>
      ApiEndpoint<dynamic, List<CustomerRes>>(
        path: '${ApiEndpoints.root}/Customer/Search',
        method: HttpMethod.get,
        requiresAuth: true,
        useQuery: true,
        responseMapper: (dynamic data) {
          if (data is List) {
            return data
                .map(
                  (item) => CustomerRes.fromJson(item as Map<String, dynamic>),
                )
                .toList();
          }
          if (data is Map<String, dynamic>) {
            final customers = data['customers'] as List? ?? [];
            return customers
                .map(
                  (item) => CustomerRes.fromJson(item as Map<String, dynamic>),
                )
                .toList();
          }
          return <CustomerRes>[];
        },
      );
}

// MODULO COMUN
class _CommonModule {
  const _CommonModule();
  _AuthController get auth => const _AuthController();
}

class _AuthController {
  const _AuthController();

  /// POST /api/common/Auth/login
  /// Respuesta real del servidor: { "success": true, "message": "...", "token": "...", "user": {...}, "configSys": {...}, "statusCode": 200, "timestamp": "..." }
  ApiEndpoint<AuthReq, LoginUserRes> get loginWithModel =>
      ApiEndpoint<AuthReq, LoginUserRes>(
        path: '${ApiEndpoints.root}/common/Auth/login',
        method: HttpMethod.post,
        requiresAuth: false,
        useBody: true,
        requestMapper: (AuthReq model) => model.toJson(),
        responseMapper: (dynamic data) {
          try {
            // El servidor devuelve los datos directamente, no dentro de "data"
            // Necesitamos extraer solo los campos que necesita LoginUserRes
            final responseData = data as Map<String, dynamic>;

            // Debug: Imprimir la respuesta para ver qué contiene
            debugPrint('🔍 Login response data: $responseData');

            return LoginUserRes.fromJson({
              'message': responseData['message'],
              'token': responseData['token'],
              'user': responseData['user'],
              'configSys': responseData['configSys'],
            });
          } catch (e) {
            debugPrint('❌ Error in login response mapper: $e');
            debugPrint('📄 Response data type: ${data.runtimeType}');
            debugPrint('📄 Response data content: $data');
            rethrow;
          }
        },
      );

  /// POST /api/common/Auth/login (sin mapeo de modelos)
  ApiEndpoint get login => const ApiEndpoint(
    path: '${ApiEndpoints.root}/common/Auth/login',
    method: HttpMethod.post,
    requiresAuth: false,
    useBody: true,
  );

  /// GET /api/common/Auth/validate-token
  ApiEndpoint get validateToken => const ApiEndpoint(
    path: '${ApiEndpoints.root}/common/Auth/validate-token',
    method: HttpMethod.get,
    requiresAuth: true,
  );
}

// MODULO DE VENTAS
class _SalesModule {
  const _SalesModule();

  _QuotationController get quotations => const _QuotationController();
  _FollowUpController get followUps => const _FollowUpController();
}

class _QuotationController {
  const _QuotationController();

  ApiEndpoint<QuotationReq, QuotationRes> get createQuotation =>
      ApiEndpoint<QuotationReq, QuotationRes>(
        path: '${ApiEndpoints.root}/api/QuotationManager/Register/Quotation',
        method: HttpMethod.post,
        requiresAuth: true,
        useBody: true,
        requestMapper: (QuotationReq dto) => dto.toJson(),
        responseMapper: (dynamic data) =>
            QuotationRes.fromJson(data as Map<String, dynamic>),
      );

  /// PUT /api/QuotationManager/Update/{IdQuotation}
  /// Endpoint para actualizar una cotización
  ApiEndpoint<QuotationUpdateReq, QuotationRes> get updateQuotation =>
      ApiEndpoint<QuotationUpdateReq, QuotationRes>(
        path: '${ApiEndpoints.root}/QuotationManager/Update/{IdQuotation}',
        method: HttpMethod.put,
        requiresAuth: true,
        useBody: true,
        urlParams: ['IdQuotation'],
        requestMapper: (QuotationUpdateReq dto) => dto.toJson(),
        responseMapper: (dynamic data) =>
            QuotationRes.fromJson(data as Map<String, dynamic>),
      );

  /// DELETE /api/QuotationManager/Delete/IdQuotation
  /// Endpoint para eliminar una cotización
  ApiEndpoint get deleteQuotation => const ApiEndpoint(
    path: '${ApiEndpoints.root}/QuotationManager/Delete/IdQuotation',
    method: HttpMethod.delete,
    requiresAuth: true,
    useQuery: true,
  );

  /// PUT /api/QuotationManager/Register/FollowupsJson
  /// Endpoint para agregar un seguimiento a una cotización
  ApiEndpoint<CreateFollowupReq, dynamic> get addFollowup =>
      ApiEndpoint<CreateFollowupReq, dynamic>(
        path: '${ApiEndpoints.root}/QuotationManager/Register/FollowupsJson',
        method: HttpMethod.put,
        requiresAuth: true,
        useBody: true,
        useQuery: true,
        requestMapper: (CreateFollowupReq dto) => dto.toJson(),
        responseMapper: (dynamic data) => data,
      );

  /// DELETE /api/QuotationManager/Delete/IdFollowupsJson
  /// Endpoint para eliminar un seguimiento de una cotización
  ApiEndpoint get deleteFollowup => const ApiEndpoint(
    path: '${ApiEndpoints.root}/QuotationManager/Delete/IdFollowupsJson',
    method: HttpMethod.delete,
    requiresAuth: true,
    useQuery: true,
  );

  /// GET /api/Quotation/GetById/{IdQuotation}
  /// Endpoint para obtener una cotización por ID
  ApiEndpoint<dynamic, QuotationRes> get getQuotationById =>
      ApiEndpoint<dynamic, QuotationRes>(
        path: '${ApiEndpoints.root}/Quotation/GetById/{IdQuotation}',
        method: HttpMethod.get,
        requiresAuth: true,
        urlParams: ['IdQuotation'],
        responseMapper: (dynamic data) =>
            QuotationRes.fromJson(data as Map<String, dynamic>),
      );

  /// GET /api/QuotationManager/Get/Folio
  /// Endpoint para obtener una cotización por folio
  ApiEndpoint<dynamic, QuotationRes> get getQuotationByFolio =>
      ApiEndpoint<dynamic, QuotationRes>(
        path: '${ApiEndpoints.root}/QuotationManager/Get/Folio',
        method: HttpMethod.get,
        requiresAuth: true,
        useQuery: true,
        responseMapper: (dynamic data) =>
            QuotationRes.fromJson(data as Map<String, dynamic>),
      );

  /// GET /api/QuotationManager/Customer/IdCustomer
  /// Endpoint para obtener cotizaciones por ID de cliente
  ApiEndpoint<dynamic, List<QuotationRes>> get getQuotationsByCustomer =>
      ApiEndpoint<dynamic, List<QuotationRes>>(
        path: '${ApiEndpoints.root}/QuotationManager/Customer/IdCustomer',
        method: HttpMethod.get,
        requiresAuth: true,
        useQuery: true,
        responseMapper: (dynamic data) {
          if (data is List) {
            return data
                .map(
                  (item) => QuotationRes.fromJson(item as Map<String, dynamic>),
                )
                .toList();
          }
          if (data is Map<String, dynamic>) {
            final quotations = data['quotations'] as List? ?? [];
            return quotations
                .map(
                  (item) => QuotationRes.fromJson(item as Map<String, dynamic>),
                )
                .toList();
          }
          return <QuotationRes>[];
        },
      );

  /// GET /api/QuotationManager/GetAll/Quotations (con mapeo de respuesta)
  /// Parámetros query: startDate, endDate, customerName, folio
  /// Respuesta esperada: { "success": true, "data": [QuotationRes, ...] }
  ApiEndpoint<dynamic, List<QuotationRes>>
  get getAllQuotations => ApiEndpoint<dynamic, List<QuotationRes>>(
    path: '${ApiEndpoints.root}/QuotationManager/GetAll/Quotations',
    method: HttpMethod.get,
    requiresAuth: true,
    useQuery: true,
    responseMapper: (dynamic data) {
      // data ya contiene solo el contenido de "data" extraído por ApiService
      if (data is List) {
        return data
            .map((item) => QuotationRes.fromJson(item as Map<String, dynamic>))
            .toList();
      }
      // Si viene como objeto con array interno
      if (data is Map<String, dynamic>) {
        final quotations = data['quotations'] as List? ?? [];
        return quotations
            .map((item) => QuotationRes.fromJson(item as Map<String, dynamic>))
            .toList();
      }
      return <QuotationRes>[];
    },
  );

  /// GET /api/sales/quotations/{id} (con mapeo de respuesta)
  /// Respuesta esperada: { "success": true, "data": QuotationRes }
  ApiEndpoint<dynamic, QuotationRes> get getQuotationByIdWithModel =>
      ApiEndpoint<dynamic, QuotationRes>(
        path: '${ApiEndpoints.root}/sales/quotations/{id}',
        method: HttpMethod.get,
        requiresAuth: true,
        urlParams: ['id'],
        responseMapper: (dynamic data) =>
            QuotationRes.fromJson(data as Map<String, dynamic>),
      );

  /// Endpoints sin mapeo (para compatibilidad)
  /// GET /api/sales/quotations
  ApiEndpoint get getQuotations => const ApiEndpoint(
    path: '${ApiEndpoints.root}/sales/quotations',
    method: HttpMethod.get,
    requiresAuth: true,
    useQuery: true,
  );
}

class _FollowUpController {
  const _FollowUpController();

  /// GET /api/sales/followups
  ApiEndpoint get getFollowUps => const ApiEndpoint(
    path: '${ApiEndpoints.root}/sales/followups',
    method: HttpMethod.get,
    requiresAuth: true,
    useQuery: true,
  );

  /// POST /api/sales/followups
  ApiEndpoint get createFollowUp => const ApiEndpoint(
    path: '${ApiEndpoints.root}/sales/followups',
    method: HttpMethod.post,
    requiresAuth: true,
    useBody: true,
  );
}

// MODULO DE SISTEMA
class _SystemModule {
  const _SystemModule();

  _ConfigController get config => const _ConfigController();
  _ToolsController get tools => const _ToolsController();
  _DbToolsController get dbTools => const _DbToolsController();
}

class _ConfigController {
  const _ConfigController();

  /// GET /api/system/Config/check-initial-config
  /// Respuesta esperada: { "success": true, "message": "Success", "data": {...}, "statusCode": 200, "timestamp": "..." }
  ApiEndpoint<dynamic, CheckInitConfigData> get checkInitialConfig =>
      ApiEndpoint<dynamic, CheckInitConfigData>(
        path: '${ApiEndpoints.root}/system/Config/check-initial-config',
        method: HttpMethod.get,
        requiresAuth: false,
        responseMapper: (dynamic data) =>
            CheckInitConfigData.fromJson(data as Map<String, dynamic>),
      );

  /// GET /api/system/Config/diagnose-migrations
  /// Respuesta esperada: { "success": true, "message": "Success", "data": {...}, "statusCode": 200, "timestamp": "..." }
  ApiEndpoint<dynamic, DiagnoseMigrationsResponse> get diagnoseMigrations =>
      ApiEndpoint<dynamic, DiagnoseMigrationsResponse>(
        path: '${ApiEndpoints.root}/system/Config/diagnose-migrations',
        method: HttpMethod.get,
        requiresAuth: false,
        responseMapper: (dynamic data) =>
            DiagnoseMigrationsResponse.fromJson(data as Map<String, dynamic>),
      );

  /// POST /api/system/Config/configure-system
  /// Endpoint para configurar el sistema
  ApiEndpoint<ConfigSysReq, dynamic> get configureSystem =>
      ApiEndpoint<ConfigSysReq, dynamic>(
        path: '${ApiEndpoints.root}/system/Config/configure-system',
        method: HttpMethod.post,
        requiresAuth: false,
        useBody: true,
        requestMapper: (ConfigSysReq model) => model.toJson(),
        responseMapper: (dynamic data) => data,
      );

  /// POST /api/system/Config/configure-admin
  /// Endpoint para configurar el administrador
  ApiEndpoint<ConfigAdminReq, dynamic> get configureAdmin =>
      ApiEndpoint<ConfigAdminReq, dynamic>(
        path: '${ApiEndpoints.root}/system/Config/configure-admin',
        method: HttpMethod.post,
        requiresAuth: false,
        useBody: true,
        requestMapper: (ConfigAdminReq model) => model.toJson(),
        responseMapper: (dynamic data) => data,
      );
}

class _ToolsController {
  const _ToolsController();

  /// POST /api/system/Tools/apply-migrations
  /// Endpoint para aplicar migraciones
  ApiEndpoint get applyMigrations => const ApiEndpoint(
    path: '${ApiEndpoints.root}/system/Tools/apply-migrations',
    method: HttpMethod.post,
    requiresAuth: false,
  );

  /// POST /api/system/Tools/apply-migrations-force
  /// Endpoint para forzar aplicar migraciones
  ApiEndpoint get applyMigrationsForce => const ApiEndpoint(
    path: '${ApiEndpoints.root}/system/Tools/apply-migrations-force',
    method: HttpMethod.post,
    requiresAuth: false,
  );
}

class _DbToolsController {
  const _DbToolsController();

  /// DELETE /api/system/DbTools/drop-tables
  /// Endpoint para eliminar tablas
  ApiEndpoint get dropTables => const ApiEndpoint(
    path: '${ApiEndpoints.root}/system/DbTools/drop-tables',
    method: HttpMethod.delete,
    requiresAuth: false,
  );

  /// GET /api/system/DbTools/check
  /// Endpoint para verificar estado de la base de datos
  /// Respuesta esperada: { "success": true, "message": "Success", "data": { "isConnected": true, "hasData": true, "message": "..." }, "statusCode": 200, "timestamp": "..." }
  ApiEndpoint<dynamic, DbCheckResponse> get check =>
      ApiEndpoint<dynamic, DbCheckResponse>(
        path: '${ApiEndpoints.root}/system/DbTools/check',
        method: HttpMethod.get,
        requiresAuth: false,
        responseMapper: (dynamic data) =>
            DbCheckResponse.fromJson(data as Map<String, dynamic>),
      );
}
