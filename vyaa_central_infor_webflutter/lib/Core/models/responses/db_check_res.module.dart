// Modelo para la respuesta de check de DbTools
class DbCheckResponse {
  final bool isConnected;
  final bool hasData;
  final String message;

  const DbCheckResponse({
    required this.isConnected,
    required this.hasData,
    required this.message,
  });

  factory DbCheckResponse.fromJson(Map<String, dynamic> json) {
    return DbCheckResponse(
      isConnected: json['isConnected'] ?? false,
      hasData: json['hasData'] ?? false,
      message: json['message'] ?? '',
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'isConnected': isConnected,
      'hasData': hasData,
      'message': message,
    };
  }
}
