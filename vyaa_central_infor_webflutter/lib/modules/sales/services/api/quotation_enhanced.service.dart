// quotation_enhanced.service.dart
import 'package:vyaa_central_infor_webflutter/Core/core.dart';
import 'package:vyaa_central_infor_webflutter/configs/config.dart';
import '../../models/requests/quotation_req.module.dart';
import '../../models/response/quotation_res.module.dart';

class QuotationEnhancedService {
  
  /// Obtener cotizaciones con mapeo automático
  /// Devuelve List<QuotationRes> directamente
  Future<ApiResponse<List<QuotationRes>>> getQuotationsWithModel({
    int? page,
    int? limit,
    String? status,
  }) async {
    final queryParams = <String, dynamic>{};
    if (page != null) queryParams['page'] = page.toString();
    if (limit != null) queryParams['limit'] = limit.toString();
    if (status != null) queryParams['status'] = status;

    final response = await ApiService.requestWithModel<List<QuotationRes>, dynamic>(
      ApiEndpoints.sales.quotations.getQuotationsWithModel,
    );

    return response;
  }

  /// Crear cotización con mapeo automático
  /// Devuelve QuotationRes directamente
  Future<ApiResponse<QuotationRes>> createQuotationWithModel(QuotationReq quotation) async {
    final response = await ApiService.requestWithModel<QuotationRes, QuotationReq>(
      ApiEndpoints.sales.quotations.createQuotationWithModel,
      model: quotation,
    );

    return response;
  }

  /// Obtener cotización por ID con mapeo automático
  /// Devuelve QuotationRes directamente
  Future<ApiResponse<QuotationRes>> getQuotationByIdWithModel(String id) async {
    final response = await ApiService.requestWithModel<QuotationRes, dynamic>(
      ApiEndpoints.sales.quotations.getQuotationByIdWithModel,
      urlParams: {'id': id},
    );

    return response;
  }

  // ========== Métodos tradicionales (sin mapeo) ==========

  /// Obtener cotizaciones sin mapeo (método tradicional)
  /// Devuelve Map<String, dynamic>
  Future<ApiResponse<List<Map<String, dynamic>>>> getQuotationsTraditional({
    int? page,
    int? limit,
    String? status,
  }) async {
    final queryParams = <String, dynamic>{};
    if (page != null) queryParams['page'] = page.toString();
    if (limit != null) queryParams['limit'] = limit.toString();
    if (status != null) queryParams['status'] = status;

    final response = await ApiService.request(
      ApiEndpoints.sales.quotations.getQuotations,
      data: queryParams,
    );

    if (response.success) {
      // Manejo manual del JSON
      final data = response.data;
      List<Map<String, dynamic>> quotations = [];
      
      if (data is Map) {
        // Si viene en un wrapper como { "quotations": [...] }
        if (data.containsKey('quotations')) {
          quotations = List<Map<String, dynamic>>.from(data['quotations']);
        } else if (data.containsKey('data')) {
          quotations = List<Map<String, dynamic>>.from(data['data']);
        }
      } else if (data is List) {
        // Si viene directamente como array
        quotations = List<Map<String, dynamic>>.from(data);
      }

      return ApiResponse.success(quotations, statusCode: response.statusCode);
    }

    return ApiResponse.error(response.message ?? 'Error al obtener cotizaciones');
  }

  /// Crear cotización sin mapeo (método tradicional)
  /// Devuelve Map<String, dynamic>
  Future<ApiResponse<Map<String, dynamic>>> createQuotationTraditional(QuotationReq quotation) async {
    final response = await ApiService.request(
      ApiEndpoints.sales.quotations.createQuotation,
      data: quotation.toJson(),
    );

    if (response.success) {
      return ApiResponse.success(response.data, statusCode: response.statusCode);
    }

    return ApiResponse.error(response.message ?? 'Error al crear cotización');
  }

  /// Obtener cotización por ID sin mapeo (método tradicional)
  /// Devuelve Map<String, dynamic>
  Future<ApiResponse<Map<String, dynamic>>> getQuotationByIdTraditional(String id) async {
    final response = await ApiService.request(
      ApiEndpoints.sales.quotations.getQuotationById,
      urlParams: {'id': id},
    );

    if (response.success) {
      return ApiResponse.success(response.data, statusCode: response.statusCode);
    }

    return ApiResponse.error(response.message ?? 'Error al obtener cotización');
  }
}