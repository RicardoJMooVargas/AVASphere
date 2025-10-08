import 'package:shared_preferences/shared_preferences.dart';

class CacheService {
  static const String _keyToken = 'auth_token';
  static const String _keyUserId = 'user_id';

  // Save token
  static Future<bool> saveToken(String token) async {
    final prefs = await SharedPreferences.getInstance();
    return prefs.setString(_keyToken, token);
  }

  // Get token
  static Future<String?> getToken() async {
    final prefs = await SharedPreferences.getInstance();
    return prefs.getString(_keyToken);
  }

  // Remove token
  static Future<bool> removeToken() async {
    final prefs = await SharedPreferences.getInstance();
    return prefs.remove(_keyToken);
  }

  // Save user ID
  static Future<bool> saveUserId(String userId) async {
    final prefs = await SharedPreferences.getInstance();
    return prefs.setString(_keyUserId, userId);
  }

  // Get user ID
  static Future<String?> getUserId() async {
    final prefs = await SharedPreferences.getInstance();
    return prefs.getString(_keyUserId);
  }

  // Remove user ID
  static Future<bool> removeUserId() async {
    final prefs = await SharedPreferences.getInstance();
    return prefs.remove(_keyUserId);
  }

  // Clear all cached data
  static Future<bool> clearAll() async {
    await removeToken();
    await removeUserId();
    return true;
  }
}
