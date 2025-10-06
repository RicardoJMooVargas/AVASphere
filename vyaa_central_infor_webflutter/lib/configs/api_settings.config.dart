import 'package:flutter/foundation.dart';

class ApiSettings {
  final String baseUrl;
  final String wsUrl;
  final Map<String, String> headers;

  static const String _productionBaseUrl = 'https://';
  static const String _developmentBaseUrl = 'https://localhost:7100';

  static const String _productionWsUrl = 'wss://';
  static const String _developmentWsUrl = 'wss://localhost:7100';

  ApiSettings({
    this.baseUrl = kDebugMode ? _developmentBaseUrl : _productionBaseUrl,
    this.wsUrl = kDebugMode ? _developmentWsUrl : _productionWsUrl,
    this.headers = const {
      'Content-Type': 'application/json',
    },
  });
}
