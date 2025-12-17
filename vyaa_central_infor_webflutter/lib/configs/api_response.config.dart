import 'dart:convert';

class ApiResponse<T> {
  final bool success;
  final T? data;
  final String? message;
  final int? statusCode;
  final bool isWarning; // Indica si es una advertencia en lugar de error crítico

  ApiResponse._({
    required this.success,
    this.data,
    this.message,
    this.statusCode,
    this.isWarning = false,
  });

  factory ApiResponse.success(T data, {int? statusCode}) {
    return ApiResponse._(success: true, data: data, statusCode: statusCode);
  }

  factory ApiResponse.error(String message, {int? statusCode}) {
    return ApiResponse._(success: false, message: message, statusCode: statusCode, isWarning: false);
  }

  factory ApiResponse.warning(String message, {int? statusCode}) {
    return ApiResponse._(success: false, message: message, statusCode: statusCode, isWarning: true);
  }

  static String getErrorMessage(int statusCode, String body) {
    try {
      final decoded = jsonDecode(body);
      if (decoded is Map && decoded['message'] != null) return decoded['message'].toString();
      if (decoded is Map && decoded['error'] != null) return decoded['error'].toString();
    } catch (_) {}
    return 'HTTP $statusCode: $body';
  }
}
