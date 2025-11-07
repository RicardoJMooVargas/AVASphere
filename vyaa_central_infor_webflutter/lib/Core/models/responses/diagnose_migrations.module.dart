class DiagnoseMigrationsResponse {
  final bool success;
  final String message;
  final DiagnoseMigrationsData data;
  final int statusCode;
  final String timestamp;

  DiagnoseMigrationsResponse({
    required this.success,
    required this.message,
    required this.data,
    required this.statusCode,
    required this.timestamp,
  });

  factory DiagnoseMigrationsResponse.fromJson(Map<String, dynamic> json) {
    return DiagnoseMigrationsResponse(
      success: json['success'] ?? false,
      message: json['message'] ?? '',
      data: DiagnoseMigrationsData.fromJson(json['data'] ?? {}),
      statusCode: json['statusCode'] ?? 0,
      timestamp: json['timestamp'] ?? '',
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'success': success,
      'message': message,
      'data': data.toJson(),
      'statusCode': statusCode,
      'timestamp': timestamp,
    };
  }
}

class DiagnoseMigrationsData {
  final String diagnosis;
  final String info;

  DiagnoseMigrationsData({
    required this.diagnosis,
    required this.info,
  });

  factory DiagnoseMigrationsData.fromJson(Map<String, dynamic> json) {
    return DiagnoseMigrationsData(
      diagnosis: json['diagnosis'] ?? '',
      info: json['info'] ?? '',
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'diagnosis': diagnosis,
      'info': info,
    };
  }
}