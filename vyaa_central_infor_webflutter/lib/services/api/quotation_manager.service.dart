import 'dart:convert';
import 'package:http/http.dart' as http;
import 'package:vyaa_central_infor_webflutter/models/requests/quotation_req.module.dart';
import '../local/cache_service.service.dart';
import '../../configs/api_settings.config.dart';
import '../../configs/api_response.config.dart';
import '../../models/responses/quotation_res.module.dart';
import '../../core/internalControllers/notification_services.dart';

class QuotationManagerService {
  final ApiSettings _settings;

  QuotationManagerService([ApiSettings? settings]) : _settings = settings ?? ApiSettings();

  Uri _url(String path, [Map<String, String>? queryParameters]) {
    return Uri.parse('${_settings.baseUrl}$path').replace(queryParameters: queryParameters);
  }

  Future<ApiResponse<List<QuotationRes>>> getQuotations({
    DateTime? startDate,
    DateTime? endDate,
    String? customerName,
    int? folio,
  }) async {
    try {
      final token = await CacheService.getToken();
      if (token == null) {
        return ApiResponse.error('No hay token guardado: Debe iniciar sesión');
      }

      final Map<String, String> queryParameters = {};

      if (startDate != null) {
        queryParameters['StartDate'] = startDate.toIso8601String();
      }

      if (endDate != null) {
        queryParameters['EndDate'] = endDate.toIso8601String();
      }

      if (customerName != null && customerName.isNotEmpty) {
        queryParameters['CustomerName'] = customerName;
      }

      if (folio != null) {
        queryParameters['Folio'] = folio.toString();
      }

      final uri = _url('/api/QuotationManager', queryParameters);

      final headers = Map<String, String>.from(_settings.headers);
      headers['Authorization'] = 'Bearer $token';

      final response = await http.get(
        uri,
        headers: headers,
      );

      if (response.statusCode == 200) {
        final List<dynamic> jsonResponse = json.decode(response.body);
        final quotations = jsonResponse.map((json) => QuotationRes.fromJson(json)).toList();
        return ApiResponse.success(quotations);
      }

      // If token is invalid, handle token expiration
      if (response.statusCode == 401) {
        NotificationService.handleTokenExpired();
        return ApiResponse.error('Token expirado');
      }

      final errorMessage = ApiResponse.getErrorMessage(response.statusCode, response.body);
      return ApiResponse.error(errorMessage);
    } catch (e) {
      if (e.toString().contains('SocketException') || e.toString().contains('TimeoutException')) {
        return ApiResponse.error('Error de conexión: Verifique su conexión a internet');
      } else if (e.toString().contains('FormatException')) {
        return ApiResponse.error('Error en el formato de respuesta del servidor');
      } else {
        return ApiResponse.error('Error inesperado: ${e.toString()}');
      }
    }
  }

  // En tu QuotationManagerService existente
Future<ApiResponse<QuotationRes>> createQuotation({
  required QuotationReq quotationReq,
}) async {
  try {
    final token = await CacheService.getToken();
    final userId = await CacheService.getUserId();
    if (token == null) {
      return ApiResponse.error('No hay token guardado: Debe iniciar sesión');
    }
    if (userId == null) {
      return ApiResponse.error('No hay UserId guardado: Debe iniciar sesión');
    }

    final uri = _url('/api/QuotationManager');

    final headers = Map<String, String>.from(_settings.headers);
    headers['Authorization'] = 'Bearer $token';
    headers['UserId'] = userId; // Header parameter
    headers['Content-Type'] = 'application/json';

    // Aquí quotationReq.toJson() generará exactamente el JSON que necesitas
    final body = json.encode(quotationReq.toJson());

    final response = await http.post(
      uri,
      headers: headers,
      body: body,
    );

    if (response.statusCode == 200 || response.statusCode == 201) {
      final dynamic jsonResponse = json.decode(response.body);
      final quotation = QuotationRes.fromJson(jsonResponse);
      return ApiResponse.success(quotation);
    }

    // If token is invalid, handle token expiration
    if (response.statusCode == 401) {
      NotificationService.handleTokenExpired();
      return ApiResponse.error('Token expirado');
    }

    final errorMessage = ApiResponse.getErrorMessage(response.statusCode, response.body);
    return ApiResponse.error(errorMessage);
  } catch (e) {
    if (e.toString().contains('SocketException') || e.toString().contains('TimeoutException')) {
      return ApiResponse.error('Error de conexión: Verifique su conexión a internet');
    } else if (e.toString().contains('FormatException')) {
      return ApiResponse.error('Error en el formato de respuesta del servidor');
    } else {
      return ApiResponse.error('Error inesperado: ${e.toString()}');
    }
  }
}
}