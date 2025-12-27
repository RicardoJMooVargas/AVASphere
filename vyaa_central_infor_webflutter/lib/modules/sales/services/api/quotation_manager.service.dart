import 'package:flutter/foundation.dart';
import '../../../../core/services/api/api.service.dart';
import '../../../../core/services/data/hive.service.dart';
import '../../../../configs/api_endpoints.config.dart';
import '../../../../configs/api_response.config.dart';
import '../../models/requests/quotation_req.module.dart';
import '../../models/requests/quotation_update_req.module.dart';
import '../../models/requests/create_followup_req.module.dart';
import '../../models/response/quotation_res.module.dart';

/// Servicio para gestión de cotizaciones usando ApiService normalizado
/// Sigue el patrón establecido en CustomerService
///
/// Este servicio proporciona métodos para:
/// - Obtener cotizaciones con filtros
/// - Crear nuevas cotizaciones
/// - Actualizar cotizaciones existentes
/// - Eliminar cotizaciones
/// - Gestionar seguimientos de cotizaciones
///
/// Todas las operaciones incluyen logging detallado para debugging
/// y manejo robusto de errores.
class QuotationManagerService {
  const QuotationManagerService();

  /// Obtiene todas las cotizaciones con filtros opcionales
  ///
  /// Permite filtrar cotizaciones por fechas, nombre de cliente y folio.
  /// Los filtros son opcionales y se pueden combinar según sea necesario.
  ///
  /// Parámetros:
  /// - [startDate]: Fecha de inicio para filtrar cotizaciones (opcional)
  /// - [endDate]: Fecha de fin para filtrar cotizaciones (opcional)
  /// - [customerName]: Nombre del cliente para filtrar (opcional)
  /// - [folio]: Número de folio específico para filtrar (opcional)
  ///
  /// Retorna:
  /// - [ApiResponse<List<QuotationRes>>]: Lista de cotizaciones que coinciden con los filtros
  ///
  /// Ejemplo de uso:
  /// ```dart
  /// final service = QuotationManagerService();
  /// final response = await service.getQuotations(
  ///   startDate: DateTime(2024, 1, 1),
  ///   endDate: DateTime(2024, 12, 31),
  ///   customerName: 'Juan Pérez',
  ///   folio: 123,
  /// );
  ///
  /// if (response.success) {
  ///   final quotations = response.data!;
  ///   print('Encontradas ${quotations.length} cotizaciones');
  /// } else {
  ///   print('Error: ${response.message}');
  /// }
  /// ```
  ///
  /// Consideraciones:
  /// - Si no se especifica ningún filtro, se obtienen todas las cotizaciones
  /// - Las fechas se convierten automáticamente a formato ISO 8601
  /// - Los filtros se aplican en el backend para optimizar la consulta
  Future<ApiResponse<List<QuotationRes>>> getQuotations({
    DateTime? startDate,
    DateTime? endDate,
    String? customerName,
    int? folio,
  }) async {
    try {
      debugPrint('🔍 Obteniendo cotizaciones con filtros...');

      final Map<String, dynamic> queryParams = {};

      if (startDate != null) {
        queryParams['startDate'] = startDate.toIso8601String();
        debugPrint('📅 Fecha inicio: ${startDate.toIso8601String()}');
      }

      if (endDate != null) {
        queryParams['endDate'] = endDate.toIso8601String();
        debugPrint('📅 Fecha fin: ${endDate.toIso8601String()}');
      }

      if (customerName != null && customerName.isNotEmpty) {
        queryParams['customerName'] = customerName;
        debugPrint('👤 Nombre cliente: $customerName');
      }

      if (folio != null) {
        queryParams['folio'] = folio;
        debugPrint('📋 Folio: $folio');
      }

      final endpoint = ApiEndpoints.sales.quotations.getAll;
      final apiResponse = await ApiService.requestWithModel<List<QuotationRes>, Map<String, dynamic>>(
        endpoint,
        model: queryParams.isNotEmpty ? queryParams : null,
      );

      if (!apiResponse.success) {
        debugPrint('❌ Error obteniendo cotizaciones: ${apiResponse.message}');
        return ApiResponse.error(apiResponse.message ?? 'Error al obtener cotizaciones');
      }

      final quotations = apiResponse.data ?? <QuotationRes>[];
      debugPrint('✅ Cotizaciones obtenidas: ${quotations.length}');
      return ApiResponse.success(quotations);

    } catch (e) {
      debugPrint('❌ Error inesperado obteniendo cotizaciones: $e');
      return ApiResponse.error('Error inesperado: ${e.toString()}');
    }
  }

  /// Crea una nueva cotización
  ///
  /// Crea una cotización completa con todos sus datos asociados.
  /// Automáticamente obtiene y asigna el idConfigSys desde el cache del usuario
  /// y asigna el userId del usuario actual a todos los followups que no tengan uno.
  ///
  /// Parámetros:
  /// - [quotationData]: Objeto [QuotationReq] con todos los datos de la cotización
  ///   - Debe incluir: folio, saleDate, generalComment, customer, followups, etc.
  ///   - Los campos salesExecutives y followups se obtienen automáticamente si están vacíos
  ///
  /// Retorna:
  /// - [ApiResponse<QuotationRes>]: La cotización creada con todos sus datos
  ///
  /// Ejemplo de uso:
  /// ```dart
  /// final quotationData = QuotationReq(
  ///   folio: 123,
  ///   saleDate: DateTime.now(),
  ///   generalComment: 'Cotización de prueba',
  ///   customer: customerData,
  ///   // ... otros campos
  /// );
  ///
  /// final service = QuotationManagerService();
  /// final response = await service.createQuotation(
  ///   quotationData: quotationData,
  /// );
  ///
  /// if (response.success) {
  ///   final quotation = response.data!;
  ///   print('Cotización creada con ID: ${quotation.quotationId}');
  /// } else {
  ///   print('Error: ${response.message}');
  /// }
  /// ```
  ///
  /// Consideraciones:
  /// - El idConfigSys se obtiene automáticamente del cache del usuario
  /// - Los followups sin userId se asignan automáticamente con el ID del usuario actual
  /// - Los salesExecutives se obtienen del modelo si están definidos
  /// - La respuesta incluye todos los datos de la cotización creada
  Future<ApiResponse<QuotationRes>> createQuotation({
    required QuotationReq quotationData,
  }) async {
    try {
      debugPrint('🔍 Creando nueva cotización...');

      // Obtener datos del usuario actual desde cache de Hive
      final idConfigSysStr = await HiveService.getCache('configSys.idConfigSys');
      final idConfigSys = int.tryParse(idConfigSysStr ?? '0') ?? 0;
      final currentUserId = await HiveService.getCache('user.id') ?? '';

      debugPrint('🔧 idConfigSys obtenido: $idConfigSys');
      debugPrint('👤 userId obtenido: $currentUserId');

      // Asignar userId a followups que no tengan uno asignado
      for (final followup in quotationData.followups) {
        if (followup.userIdController.text.trim().isEmpty && currentUserId.isNotEmpty) {
          followup.userIdController.text = currentUserId;
          debugPrint('✅ UserId asignado automáticamente a followup');
        }
      }

      // Generar JSON base desde el modelo (ahora con userIds actualizados)
      final jsonData = quotationData.toJson();

      // Agregar idConfigSys al JSON
      jsonData['idConfigSys'] = idConfigSys;


      debugPrint('📋 JSON final a enviar: $jsonData');

      final endpoint = ApiEndpoints.sales.quotations.create;

      // Usar ApiService.request que manejará automáticamente el responseMapper
      final apiResponse = await ApiService.request(
        endpoint,
        data: jsonData,
      );

      if (!apiResponse.success) {
        // Si es una advertencia (código 400), retornar como warning
        if (apiResponse.isWarning) {
          debugPrint('⚠️ Advertencia creando cotización: ${apiResponse.message}');
          return ApiResponse.warning(apiResponse.message ?? 'Advertencia al crear cotización', statusCode: apiResponse.statusCode);
        }

        // Si es un error crítico, retornar como error
        debugPrint('❌ Error creando cotización: ${apiResponse.message}');
        return ApiResponse.error(apiResponse.message ?? 'Error al crear cotización', statusCode: apiResponse.statusCode);
      }

      final quotation = apiResponse.data;
      if (quotation == null) {
        return ApiResponse.error('La cotización se creó pero no se recibieron datos');
      }

      debugPrint('✅ Cotización creada exitosamente: ${quotation.folio}');
      return ApiResponse.success(quotation);

    } catch (e) {
      debugPrint('❌ Error inesperado creando cotización: $e');
      return ApiResponse.error('Error inesperado: ${e.toString()}');
    }
  }

  /// Obtiene una cotización por ID
  ///
  /// Busca y retorna una cotización específica utilizando su identificador único.
  ///
  /// Parámetros:
  /// - [id]: Identificador único de la cotización (requerido)
  ///   - Debe ser un número entero positivo
  ///   - Corresponde al campo 'idQuotation' en la base de datos
  ///
  /// Retorna:
  /// - [ApiResponse<QuotationRes>]: La cotización encontrada con todos sus datos
  ///
  /// Ejemplo de uso:
  /// ```dart
  /// final service = QuotationManagerService();
  /// final response = await service.getQuotationById(id: 123);
  ///
  /// if (response.success) {
  ///   final quotation = response.data!;
  ///   print('Cotización encontrada: ${quotation.folio}');
  ///   print('Cliente: ${quotation.customer.name}');
  /// } else {
  ///   print('Error: ${response.message}');
  /// }
  /// ```
  ///
  /// Consideraciones:
  /// - Si la cotización no existe, retorna error con mensaje descriptivo
  /// - Incluye todos los datos relacionados: cliente, followups, productos, etc.
  /// - El ID se pasa como parámetro de URL en la petición HTTP
  Future<ApiResponse<QuotationRes>> getQuotationById({
    required int id,
  }) async {
    try {
      debugPrint('🔍 Obteniendo cotización por ID: $id');

      final endpoint = ApiEndpoints.sales.quotations.getById;
      final apiResponse = await ApiService.requestWithModel<QuotationRes, Map<String, dynamic>>(
        endpoint,
        urlParams: {'id': id.toString()},
      );

      if (!apiResponse.success) {
        debugPrint('❌ Error obteniendo cotización por ID: ${apiResponse.message}');
        return ApiResponse.error(apiResponse.message ?? 'Error al obtener cotización');
      }

      final quotation = apiResponse.data;
      if (quotation == null) {
        return ApiResponse.error('Cotización no encontrada');
      }
      debugPrint('✅ Cotización obtenida: ${quotation.folio}');
      return ApiResponse.success(quotation);

    } catch (e) {
      debugPrint('❌ Error inesperado obteniendo cotización por ID: $e');
      return ApiResponse.error('Error inesperado: ${e.toString()}');
    }
  }

  /// Obtiene una cotización por folio
  ///
  /// Busca y retorna una cotización específica utilizando su número de folio.
  ///
  /// Parámetros:
  /// - [folio]: Número de folio de la cotización (requerido)
  ///   - Debe ser un número entero positivo
  ///   - Es único por cotización y se usa como identificador alternativo
  ///
  /// Retorna:
  /// - [ApiResponse<QuotationRes>]: La cotización encontrada con todos sus datos
  ///
  /// Ejemplo de uso:
  /// ```dart
  /// final service = QuotationManagerService();
  /// final response = await service.getQuotationByFolio(folio: 12345);
  ///
  /// if (response.success) {
  ///   final quotation = response.data!;
  ///   print('Cotización encontrada: ${quotation.folio}');
  ///   print('Cliente: ${quotation.customer.name}');
  /// } else {
  ///   print('Error: ${response.message}');
  /// }
  /// ```
  ///
  /// Consideraciones:
  /// - Si no existe cotización con ese folio, retorna error con mensaje descriptivo
  /// - Incluye todos los datos relacionados: cliente, followups, productos, etc.
  /// - El folio se pasa como parámetro de query en la petición HTTP
  Future<ApiResponse<QuotationRes>> getQuotationByFolio({
    required int folio,
  }) async {
    try {
      debugPrint('🔍 Obteniendo cotización por folio: $folio');

      final endpoint = ApiEndpoints.sales.quotations.getByFolio;
      final apiResponse = await ApiService.requestWithModel<QuotationRes, Map<String, dynamic>>(
        endpoint,
        model: {'folio': folio},
      );

      if (!apiResponse.success) {
        debugPrint('❌ Error obteniendo cotización por folio: ${apiResponse.message}');
        return ApiResponse.error(apiResponse.message ?? 'Error al obtener cotización');
      }

      final quotation = apiResponse.data;
      if (quotation == null) {
        return ApiResponse.error('Cotización no encontrada con ese folio');
      }
      debugPrint('✅ Cotización obtenida por folio: ${quotation.folio}');
      return ApiResponse.success(quotation);

    } catch (e) {
      debugPrint('❌ Error inesperado obteniendo cotización por folio: $e');
      return ApiResponse.error('Error inesperado: ${e.toString()}');
    }
  }

  /// Obtiene cotizaciones por cliente
  ///
  /// Retorna todas las cotizaciones asociadas a un cliente específico.
  ///
  /// Parámetros:
  /// - [idCustomer]: ID del cliente (requerido)
  ///   - Debe ser un número entero positivo
  ///   - Corresponde al campo 'idCustomer' en la base de datos
  ///
  /// Retorna:
  /// - [ApiResponse<List<QuotationRes>>]: Lista de cotizaciones del cliente
  ///
  /// Ejemplo de uso:
  /// ```dart
  /// final service = QuotationManagerService();
  /// final response = await service.getQuotationsByCustomer(idCustomer: 123);
  ///
  /// if (response.success) {
  ///   final quotations = response.data!;
  ///   print('Cliente tiene ${quotations.length} cotizaciones');
  ///   for (var quotation in quotations) {
  ///     print('Folio: ${quotation.folio}, Fecha: ${quotation.saleDate}');
  ///   }
  /// } else {
  ///   print('Error: ${response.message}');
  /// }
  /// ```
  ///
  /// Consideraciones:
  /// - Si el cliente no tiene cotizaciones, retorna lista vacía
  /// - Incluye todas las cotizaciones del cliente sin importar su estado
  /// - El ID del cliente se pasa como parámetro de query en la petición HTTP
  Future<ApiResponse<List<QuotationRes>>> getQuotationsByCustomer({
    required int idCustomer,
  }) async {
    try {
      debugPrint('🔍 Obteniendo cotizaciones por cliente: $idCustomer');

      final endpoint = ApiEndpoints.sales.quotations.getByCustomer;
      final apiResponse = await ApiService.requestWithModel<List<QuotationRes>, Map<String, dynamic>>(
        endpoint,
        model: {'IdCustomer': idCustomer},
      );

      if (!apiResponse.success) {
        debugPrint('❌ Error obteniendo cotizaciones por cliente: ${apiResponse.message}');
        return ApiResponse.error(apiResponse.message ?? 'Error al obtener cotizaciones del cliente');
      }

      final quotations = apiResponse.data ?? <QuotationRes>[];
      debugPrint('✅ Cotizaciones del cliente obtenidas: ${quotations.length}');
      return ApiResponse.success(quotations);

    } catch (e) {
      debugPrint('❌ Error inesperado obteniendo cotizaciones por cliente: $e');
      return ApiResponse.error('Error inesperado: ${e.toString()}');
    }
  }

  /// Actualiza una cotización
  ///
  /// Modifica los datos de una cotización existente utilizando su ID.
  ///
  /// Parámetros:
  /// - [idQuotation]: ID de la cotización a actualizar (requerido)
  ///   - Debe ser un número entero positivo
  ///   - La cotización debe existir en la base de datos
  /// - [quotationData]: Objeto [QuotationUpdateReq] con los datos a actualizar
  ///   - Solo incluye los campos que se van a modificar
  ///
  /// Retorna:
  /// - [ApiResponse<QuotationRes>]: La cotización actualizada con todos sus datos
  ///
  /// Ejemplo de uso:
  /// ```dart
  /// final updateData = QuotationUpdateReq(
  ///   generalComment: 'Comentario actualizado',
  ///   status: 2, // Cambiar estado
  /// );
  ///
  /// final service = QuotationManagerService();
  /// final response = await service.updateQuotation(
  ///   idQuotation: 123,
  ///   quotationData: updateData,
  /// );
  ///
  /// if (response.success) {
  ///   final quotation = response.data!;
  ///   print('Cotización actualizada: ${quotation.folio}');
  /// } else {
  ///   print('Error: ${response.message}');
  /// }
  /// ```
  ///
  /// Consideraciones:
  /// - Si la cotización no existe, retorna error con mensaje descriptivo
  /// - Solo se actualizan los campos proporcionados en quotationData
  /// - La respuesta incluye todos los datos de la cotización actualizada
  /// - El ID se pasa como parámetro de URL en la petición HTTP
  Future<ApiResponse<QuotationRes>> updateQuotation({
    required int idQuotation,
    required QuotationUpdateReq quotationData,
  }) async {
    try {
      debugPrint('🔍 Actualizando cotización: $idQuotation');

      final endpoint = ApiEndpoints.sales.quotations.update;
      final apiResponse = await ApiService.requestWithModel<QuotationRes, QuotationUpdateReq>(
        endpoint,
        model: quotationData,
        urlParams: {'IdQuotation': idQuotation.toString()},
      );

      // Manejar warnings (errores de negocio como código 400)
      if (apiResponse.isWarning) {
        debugPrint('⚠️ Advertencia al actualizar cotización: ${apiResponse.message}');
        return ApiResponse.warning(apiResponse.message ?? 'No se pudo actualizar la cotización', statusCode: apiResponse.statusCode);
      }

      if (!apiResponse.success) {
        debugPrint('❌ Error actualizando cotización: ${apiResponse.message}');
        return ApiResponse.error(apiResponse.message ?? 'Error al actualizar cotización', statusCode: apiResponse.statusCode);
      }

      final quotation = apiResponse.data;
      if (quotation == null) {
        return ApiResponse.error('La cotización se actualizó pero no se recibieron datos');
      }
      debugPrint('✅ Cotización actualizada: ${quotation.folio}');
      return ApiResponse.success(quotation, statusCode: apiResponse.statusCode);

    } catch (e) {
      debugPrint('❌ Error inesperado actualizando cotización: $e');
      return ApiResponse.error('Error inesperado: ${e.toString()}');
    }
  }

  /// Elimina una cotización
  ///
  /// Elimina permanentemente una cotización de la base de datos utilizando su ID.
  /// Esta operación no se puede deshacer.
  ///
  /// Parámetros:
  /// - [idQuotation]: ID de la cotización a eliminar (requerido)
  ///   - Debe ser un número entero positivo
  ///   - La cotización debe existir en la base de datos
  ///
  /// Retorna:
  /// - [ApiResponse<dynamic>]: Confirmación de eliminación exitosa
  ///
  /// Ejemplo de uso:
  /// ```dart
  /// final service = QuotationManagerService();
  /// final response = await service.deleteQuotation(idQuotation: 123);
  ///
  /// if (response.success) {
  ///   print('Cotización eliminada exitosamente');
  ///   // Actualizar lista de cotizaciones
  /// } else {
  ///   print('Error al eliminar: ${response.message}');
  /// }
  /// ```
  ///
  /// Consideraciones:
  /// - Si la cotización no existe, retorna error con mensaje descriptivo
  /// - Esta operación elimina también todos los seguimientos asociados
  /// - No se puede recuperar una cotización eliminada
  /// - El ID se pasa como parámetro de query en la petición HTTP
  Future<ApiResponse<Map<String, dynamic>>> deleteQuotation({
    required int idQuotation,
  }) async {
    try {
      debugPrint('🔍 Eliminando cotización: $idQuotation');

      final endpoint = ApiEndpoints.sales.quotations.delete;
      final apiResponse = await ApiService.requestWithModel<Map<String, dynamic>, Map<String, dynamic>>(
        endpoint,
        model: {'IdQuotation': idQuotation},
      );

      // Manejar warnings (errores de negocio como código 400)
      if (apiResponse.isWarning) {
        debugPrint('⚠️ Advertencia al eliminar cotización: ${apiResponse.message}');
        return ApiResponse.warning(apiResponse.message ?? 'No se pudo eliminar la cotización', statusCode: apiResponse.statusCode);
      }

      if (!apiResponse.success) {
        debugPrint('❌ Error eliminando cotización: ${apiResponse.message}');
        return ApiResponse.error(apiResponse.message ?? 'Error al eliminar cotización', statusCode: apiResponse.statusCode);
      }

      debugPrint('✅ Cotización eliminada exitosamente');
      final result = apiResponse.data ?? {'success': true, 'message': 'Cotización eliminada exitosamente'};
      return ApiResponse.success(result, statusCode: apiResponse.statusCode);

    } catch (e) {
      debugPrint('❌ Error inesperado eliminando cotización: $e');
      return ApiResponse.error('Error inesperado: ${e.toString()}');
    }
  }

  /// Agrega un seguimiento a una cotización
  ///
  /// Crea y agrega un nuevo seguimiento a una cotización existente.
  /// El seguimiento incluye comentario, fecha y automáticamente el userId del usuario actual.
  ///
  /// Parámetros:
  /// - [idQuotation]: ID de la cotización donde agregar el seguimiento (requerido)
  ///   - Debe ser un número entero positivo
  ///   - La cotización debe existir en la base de datos
  /// - [followupData]: Objeto [CreateFollowupReq] con los datos del seguimiento
  ///   - Debe incluir: comment, date (opcional, por defecto fecha actual)
  ///   - El userId se asigna automáticamente desde el usuario actual
  ///
  /// Retorna:
  /// - [ApiResponse<dynamic>]: Confirmación de que el seguimiento fue agregado
  ///
  /// Ejemplo de uso:
  /// ```dart
  /// final followupData = CreateFollowupReq(
  ///   comment: 'Cliente interesado, esperando respuesta',
  ///   date: DateTime.now(),
  /// );
  ///
  /// final service = QuotationManagerService();
  /// final response = await service.addFollowup(
  ///   idQuotation: 123,
  ///   followupData: followupData,
  /// );
  ///
  /// if (response.success) {
  ///   print('Seguimiento agregado exitosamente');
  ///   // Actualizar vista de la cotización
  /// } else {
  ///   print('Error al agregar seguimiento: ${response.message}');
  /// }
  /// ```
  ///
  /// Consideraciones:
  /// - Si la cotización no existe, retorna error con mensaje descriptivo
  /// - El userId del seguimiento se asigna automáticamente desde el usuario actual
  /// - Si no se especifica fecha, se usa la fecha y hora actual
  /// - El seguimiento se agrega al final de la lista de followups de la cotización
  /// - Los parámetros se pasan como URL params y body según corresponda
  Future<ApiResponse<dynamic>> addFollowup({
    required int idQuotation,
    required CreateFollowupReq followupData,
  }) async {
    try {
      debugPrint('🔍 Agregando seguimiento a cotización: $idQuotation');

      final endpoint = ApiEndpoints.sales.quotations.addFollowup;
      final apiResponse = await ApiService.requestWithModel<dynamic, CreateFollowupReq>(
        endpoint,
        model: followupData,
        urlParams: {'IdQuotation': idQuotation.toString()},
      );

      if (!apiResponse.success) {
        debugPrint('❌ Error agregando seguimiento: ${apiResponse.message}');
        return ApiResponse.error(apiResponse.message ?? 'Error al agregar seguimiento');
      }

      debugPrint('✅ Seguimiento agregado exitosamente');
      return ApiResponse.success(apiResponse.data);

    } catch (e) {
      debugPrint('❌ Error inesperado agregando seguimiento: $e');
      return ApiResponse.error('Error inesperado: ${e.toString()}');
    }
  }

  /// Elimina un seguimiento de una cotización
  ///
  /// Elimina un seguimiento específico de una cotización utilizando sus IDs.
  /// Esta operación no se puede deshacer.
  ///
  /// Parámetros:
  /// - [idQuotation]: ID de la cotización que contiene el seguimiento (requerido)
  ///   - Debe ser un número entero positivo
  ///   - La cotización debe existir en la base de datos
  /// - [idFollowupsJson]: ID del seguimiento a eliminar (requerido)
  ///   - Debe ser un número entero positivo
  ///   - El seguimiento debe existir en la cotización especificada
  ///
  /// Retorna:
  /// - [ApiResponse<dynamic>]: Confirmación de eliminación exitosa
  ///
  /// Ejemplo de uso:
  /// ```dart
  /// final service = QuotationManagerService();
  /// final response = await service.deleteFollowup(
  ///   idQuotation: 123,
  ///   idFollowupsJson: 456,
  /// );
  ///
  /// if (response.success) {
  ///   print('Seguimiento eliminado exitosamente');
  ///   // Actualizar vista de la cotización
  /// } else {
  ///   print('Error al eliminar seguimiento: ${response.message}');
  /// }
  /// ```
  ///
  /// Consideraciones:
  /// - Si la cotización o el seguimiento no existen, retorna error con mensaje descriptivo
  /// - Esta operación elimina permanentemente el seguimiento
  /// - No se puede recuperar un seguimiento eliminado
  /// - Los parámetros se pasan como query parameters en la petición HTTP
  /// - Solo el usuario que creó el seguimiento o un administrador pueden eliminarlo
  Future<ApiResponse<dynamic>> deleteFollowup({
    required int idQuotation,
    required int idFollowupsJson,
  }) async {
    try {
      debugPrint('🔍 Eliminando seguimiento: $idFollowupsJson de cotización: $idQuotation');

      final endpoint = ApiEndpoints.sales.quotations.deleteFollowup;
      final apiResponse = await ApiService.requestWithModel<dynamic, Map<String, dynamic>>(
        endpoint,
        model: {
          'IdQuotation': idQuotation,
          'IdFollowupsJson': idFollowupsJson,
        },
      );

      if (!apiResponse.success) {
        debugPrint('❌ Error eliminando seguimiento: ${apiResponse.message}');
        return ApiResponse.error(apiResponse.message ?? 'Error al eliminar seguimiento');
      }

      debugPrint('✅ Seguimiento eliminado exitosamente');
      final result = apiResponse.data ?? {'success': true, 'message': 'Seguimiento eliminado exitosamente'};
      return ApiResponse.success(result);

    } catch (e) {
      debugPrint('❌ Error inesperado eliminando seguimiento: $e');
      return ApiResponse.error('Error inesperado: ${e.toString()}');
    }
  }
}