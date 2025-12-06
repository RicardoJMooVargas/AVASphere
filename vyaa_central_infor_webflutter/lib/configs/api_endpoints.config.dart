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
}

class _QuotationController {
  const _QuotationController();

  /// POST /api/QuotationManager/Register/Quotation
  /// Endpoint para crear una nueva cotización
  ApiEndpoint<QuotationReq, QuotationRes> get create =>
      ApiEndpoint<QuotationReq, QuotationRes>(
        path: '${ApiEndpoints.root}/QuotationManager/Register/Quotation',
        method: HttpMethod.post,
        requiresAuth: true,
        useBody: true,
        requestMapper: (QuotationReq model) => model.toJson(),
        responseMapper: (dynamic data) {
          debugPrint('🔍 Create quotation response: $data (Type: ${data.runtimeType})');

          // Los datos ya vienen extraídos del formato estándar por ApiService
          // data contiene directamente el contenido del campo "data" de la respuesta
          if (data is Map<String, dynamic>) {
            try {
              final quotation = QuotationRes.fromJson(data);
              debugPrint('✅ Cotización mapeada exitosamente: ${quotation.folio}');
              return quotation;
            } catch (e, stackTrace) {
              debugPrint('❌ Error mapeando cotización: $e');
              debugPrint('📊 Stack trace: $stackTrace');
              debugPrint('📋 Data recibida: $data');
              rethrow;
            }
          }

          debugPrint('❌ Data no es un Map válido. Tipo: ${data.runtimeType}');
          throw Exception('Respuesta de servidor en formato inesperado');
        },
      );

  /// GET /api/QuotationManager/GetAll/Quotations
  /// Endpoint para obtener todas las cotizaciones con filtros opcionales
  /// Query params: startDate, endDate, customerName, folio
  ApiEndpoint<Map<String, dynamic>, List<QuotationRes>> get getAll =>
      ApiEndpoint<Map<String, dynamic>, List<QuotationRes>>(
        path: '${ApiEndpoints.root}/QuotationManager/GetAll/Quotations',
        method: HttpMethod.get,
        requiresAuth: true,
        useQuery: true,
        responseMapper: (dynamic data) {
          debugPrint('🔍 Get all quotations response: $data (Type: ${data.runtimeType})');
          if (data is List<dynamic>) {
            try {
              final List<QuotationRes> mappedList = data
                  .where((item) => item is Map<String, dynamic>)
                  .map((item) {
                    debugPrint('📄 Mapping quotation: ${item['folio']}');
                    return QuotationRes.fromJson(item as Map<String, dynamic>);
                  })
                  .toList()
                  .cast<QuotationRes>();

              debugPrint('✅ Quotations mapped successfully: ${mappedList.length} elements');
              return mappedList;
            } catch (e, stackTrace) {
              debugPrint('❌ Error mapping quotations: $e');
              debugPrint('📊 Stack trace: $stackTrace');
              return <QuotationRes>[];
            }
          }
          debugPrint('❌ Data is not a valid list. Type: ${data.runtimeType}');
          return <QuotationRes>[];
        },
      );

  /// GET /api/QuotationManager/GetById{id}
  /// Endpoint para obtener una cotización por ID
  ApiEndpoint<Map<String, dynamic>, QuotationRes> get getById =>
      ApiEndpoint<Map<String, dynamic>, QuotationRes>(
        path: '${ApiEndpoints.root}/QuotationManager/GetById{id}',
        method: HttpMethod.get,
        requiresAuth: true,
        responseMapper: (dynamic data) {
          debugPrint('🔍 Get quotation by ID response: $data (Type: ${data.runtimeType})');
          return QuotationRes.fromJson(data as Map<String, dynamic>);
        },
      );

  /// GET /api/QuotationManager/Get/Folio
  /// Endpoint para obtener cotización por folio
  ApiEndpoint<Map<String, dynamic>, QuotationRes> get getByFolio =>
      ApiEndpoint<Map<String, dynamic>, QuotationRes>(
        path: '${ApiEndpoints.root}/QuotationManager/Get/Folio',
        method: HttpMethod.get,
        requiresAuth: true,
        useQuery: true,
        responseMapper: (dynamic data) {
          debugPrint('🔍 Get quotation by folio response: $data (Type: ${data.runtimeType})');
          return QuotationRes.fromJson(data as Map<String, dynamic>);
        },
      );

  /// GET /api/QuotationManager/Customer/IdCustomer
  /// Endpoint para obtener cotizaciones por cliente
  ApiEndpoint<Map<String, dynamic>, List<QuotationRes>> get getByCustomer =>
      ApiEndpoint<Map<String, dynamic>, List<QuotationRes>>(
        path: '${ApiEndpoints.root}/QuotationManager/Customer/IdCustomer',
        method: HttpMethod.get,
        requiresAuth: true,
        useQuery: true,
        responseMapper: (dynamic data) {
          debugPrint('🔍 Get quotations by customer response: $data (Type: ${data.runtimeType})');
          if (data is List<dynamic>) {
            try {
              final List<QuotationRes> mappedList = data
                  .where((item) => item is Map<String, dynamic>)
                  .map((item) {
                    debugPrint('📄 Mapping customer quotation: ${item['folio']}');
                    return QuotationRes.fromJson(item as Map<String, dynamic>);
                  })
                  .toList()
                  .cast<QuotationRes>();

              debugPrint('✅ Customer quotations mapped successfully: ${mappedList.length} elements');
              return mappedList;
            } catch (e, stackTrace) {
              debugPrint('❌ Error mapping customer quotations: $e');
              debugPrint('📊 Stack trace: $stackTrace');
              return <QuotationRes>[];
            }
          }
          debugPrint('❌ Data is not a valid list. Type: ${data.runtimeType}');
          return <QuotationRes>[];
        },
      );

  /// PUT /api/QuotationManager/Update/{IdQuotation}
  /// Endpoint para actualizar una cotización
  ApiEndpoint<QuotationUpdateReq, QuotationRes> get update =>
      ApiEndpoint<QuotationUpdateReq, QuotationRes>(
        path: '${ApiEndpoints.root}/QuotationManager/Update/{IdQuotation}',
        method: HttpMethod.put,
        requiresAuth: true,
        useBody: true,
        requestMapper: (QuotationUpdateReq model) => model.toJson(),
        responseMapper: (dynamic data) {
          debugPrint('🔍 Update quotation response: $data (Type: ${data.runtimeType})');
          return QuotationRes.fromJson(data as Map<String, dynamic>);
        },
      );

  /// DELETE /api/QuotationManager/Delete/IdQuotation
  /// Endpoint para eliminar una cotización
  /// Query Parameters: IdQuotation (int, requerido) - ID de la cotización a eliminar
  /// URL esperada: /api/QuotationManager/Delete/IdQuotation?IdQuotation=1
  ApiEndpoint<Map<String, dynamic>, Map<String, dynamic>> get delete =>
      ApiEndpoint<Map<String, dynamic>, Map<String, dynamic>>(
        path: '${ApiEndpoints.root}/QuotationManager/Delete/IdQuotation',
        method: HttpMethod.delete,
        requiresAuth: true,
        useQuery: true,
        requestMapper: (Map<String, dynamic> model) {
          debugPrint('🔧 Delete endpoint requestMapper - Input: $model');
          final result = {
            'IdQuotation': model['IdQuotation'].toString(),
          };
          debugPrint('🔧 Delete endpoint requestMapper - Output: $result');
          return result;
        },
        responseMapper: (dynamic data) {
          debugPrint('🔍 Delete quotation response: $data (Type: ${data.runtimeType})');
          // Si el servidor no devuelve datos, crear una respuesta de éxito genérica
          if (data == null || (data is Map && data.isEmpty)) {
            return {'success': true, 'message': 'Cotización eliminada exitosamente'};
          }
          // Si devuelve datos, retornarlos como Map
          if (data is Map<String, dynamic>) {
            return data;
          }
          return {'success': true, 'message': 'Operación completada', 'data': data};
        },
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
        requestMapper: (CreateFollowupReq model) => model.toJson(),
        responseMapper: (dynamic data) {
          debugPrint('🔍 Add followup response: $data (Type: ${data.runtimeType})');
          return data;
        },
      );

  /// DELETE /api/QuotationManager/Delete/IdFollowupsJson
  /// Endpoint para eliminar un seguimiento de una cotización
  ApiEndpoint<Map<String, dynamic>, dynamic> get deleteFollowup =>
      ApiEndpoint<Map<String, dynamic>, dynamic>(
        path: '${ApiEndpoints.root}/QuotationManager/Delete/IdFollowupsJson',
        method: HttpMethod.delete,
        requiresAuth: true,
        useQuery: true,
        responseMapper: (dynamic data) {
          debugPrint('🔍 Delete followup response: $data (Type: ${data.runtimeType})');
          return data;
        },
      );
}



// MODULO DE SISTEMA
class _SystemModule {
  const _SystemModule();

  _ConfigController get config => const _ConfigController();
  _ToolsController get tools => const _ToolsController();
  _DbToolsController get dbTools => const _DbToolsController();
  _CustomerController get customer => const _CustomerController();
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

class _CustomerController {
  const _CustomerController();

  /// GET /api/common/Customer/search?searchText=alfr
  /// Endpoint para buscar clientes por texto
  /// Respuesta esperada: { "success": true, "message": "Customer search completed successfully", "data": [...], "statusCode": 200, "timestamp": "..." }
  ApiEndpoint<Map<String, dynamic>, List<CustomerRes>> get search =>
      ApiEndpoint<Map<String, dynamic>, List<CustomerRes>>(
        path: '${ApiEndpoints.root}/common/Customer/search',
        method: HttpMethod.get,
        requiresAuth: true,
        useQuery: true,
        responseMapper: (dynamic data) {
          debugPrint('🔍 Datos recibidos para mapeo: $data (Tipo: ${data.runtimeType})');

          if (data is List<dynamic>) {
            try {
              final List<CustomerRes> mappedList = data
                  .where((item) => item is Map<String, dynamic>)
                  .map((item) {
                    debugPrint('📄 Mapeando item: ${item['name']} ${item['lastName']}');
                    return CustomerRes.fromJson(item as Map<String, dynamic>);
                  })
                  .toList()
                  .cast<CustomerRes>(); // Cast explícito para asegurar el tipo

              debugPrint('✅ Lista mapeada exitosamente: ${mappedList.length} elementos');
              return mappedList;
            } catch (e, stackTrace) {
              debugPrint('❌ Error al mapear lista de clientes: $e');
              debugPrint('📊 Stack trace: $stackTrace');
              return <CustomerRes>[];
            }
          }

          debugPrint('❌ Los datos no son una lista válida. Tipo: ${data.runtimeType}');
          return <CustomerRes>[];
        },
      );
}
