import 'dart:convert';

import 'package:http/http.dart' as http;
import '../local/cache_service.dart';
import '../../models/requests/auth_request.model.dart';
import '../../configs/api_settings.config.dart';

class AuthService {
  final ApiSettings _settings;

  AuthService([ApiSettings? settings]) : _settings = settings ?? ApiSettings();

  Uri _url(String path) => Uri.parse('${_settings.baseUrl}$path');

  /// Login with username and password. Returns the token string on success.
  Future<String> login(AuthRequest request) async {
    final uri = _url('/api/system/Auth/login');
    final body = jsonEncode(request.toJson());

    final response = await http.post(
      uri,
      headers: _settings.headers,
      body: body,
    );

    if (response.statusCode == 200 || response.statusCode == 201) {
      final Map<String, dynamic> decoded = jsonDecode(response.body);
      // Assuming the API returns a token field (jwt) - adapt if different
      final token = decoded['token'] as String? ?? decoded['access_token'] as String? ?? '';
      if (token.isNotEmpty) {
        await CacheService.saveToken(token);
        return token;
      }
      throw Exception('Token not found in response');
    }

    throw Exception('Login failed (${response.statusCode}): ${response.body}');
  }

  /// Validate token. Returns the message and user (may be null) as Map.
  Future<Map<String, dynamic>> validateToken() async {
    final token = await CacheService.getToken();
    if (token == null) throw Exception('No token saved');

    final uri = _url('/api/system/Auth/validate-token');

    final headers = Map<String, String>.from(_settings.headers);
    headers['Authorization'] = 'Bearer $token';

    final response = await http.get(uri, headers: headers);

    if (response.statusCode == 200) {
      final Map<String, dynamic> decoded = jsonDecode(response.body);
      return decoded;
    }

    throw Exception('Validate failed (${response.statusCode}): ${response.body}');
  }
}
