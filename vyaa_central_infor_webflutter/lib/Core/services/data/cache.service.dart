import 'package:shared_preferences/shared_preferences.dart';

class CacheService {
  static const String _keyToken = 'auth_token';
  static const String _keyUserId = 'user_id';
  static const String _keySystemInitialized = 'system_initialized';
  static const String _keyFirstTimeCheck = 'first_time_check_done';
  static const String _keyLastRoute = 'last_initial_route';
  static const String _keyCheckInitConfig = 'check_init_config_data';

  // ///////////////////////////////////////////////
  // Token DATA MANAGEMENT
  // ///////////////////////////////////////////////
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
  // ///////////////////////////////////////////////
  // User DATA MANAGEMENT
  // ///////////////////////////////////////////////
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
  // ///////////////////////////////////////////////
  // System Initialization DATA MANAGEMENT
  // ///////////////////////////////////////////////
  // Check if this is the first time checking system status
  static Future<bool> isFirstTimeCheck() async {
    final prefs = await SharedPreferences.getInstance();
    return !(prefs.getBool(_keyFirstTimeCheck) ?? false);
  }
  
  // Mark first time check as done
  static Future<bool> markFirstTimeCheckDone() async {
    final prefs = await SharedPreferences.getInstance();
    return prefs.setBool(_keyFirstTimeCheck, true);
  }
  
  // Save system initialization status
  static Future<bool> saveSystemInitialized(bool isInitialized) async {
    final prefs = await SharedPreferences.getInstance();
    return prefs.setBool(_keySystemInitialized, isInitialized);
  }
  
  // Get system initialization status
  static Future<bool> isSystemInitialized() async {
    final prefs = await SharedPreferences.getInstance();
    return prefs.getBool(_keySystemInitialized) ?? false;
  }
  
  // ///////////////////////////////////////////////
  // Last Route DATA MANAGEMENT
  // ///////////////////////////////////////////////
  // Save last initial route
  static Future<bool> saveLastRoute(String route) async {
    final prefs = await SharedPreferences.getInstance();
    return prefs.setString(_keyLastRoute, route);
  }
  
  // Get last initial route
  static Future<String?> getLastRoute() async {
    final prefs = await SharedPreferences.getInstance();
    return prefs.getString(_keyLastRoute);
  }
  
  // Clear last route
  static Future<bool> clearLastRoute() async {
    final prefs = await SharedPreferences.getInstance();
    return prefs.remove(_keyLastRoute);
  }
  
  // Clear system initialization flags (for testing/reset purposes)
  static Future<bool> clearSystemInitFlags() async {
    final prefs = await SharedPreferences.getInstance();
    await prefs.remove(_keySystemInitialized);
    await prefs.remove(_keyFirstTimeCheck);
    await prefs.remove(_keyLastRoute);
    return true;
  }
  
  // Clear session data (token, userId and last route) - for logout
  static Future<bool> clearSession() async {
    await removeToken();
    await removeUserId();
    await clearLastRoute();
    return true;
  }
  
  // ///////////////////////////////////////////////
  // Check Initial Config DATA MANAGEMENT
  // ///////////////////////////////////////////////
  // Save check initial config data as JSON string
  static Future<bool> saveCheckInitConfig(String configJsonString) async {
    final prefs = await SharedPreferences.getInstance();
    return prefs.setString(_keyCheckInitConfig, configJsonString);
  }
  
  // Get check initial config data as JSON string
  static Future<String?> getCheckInitConfig() async {
    final prefs = await SharedPreferences.getInstance();
    return prefs.getString(_keyCheckInitConfig);
  }
  
  // Remove check initial config data
  static Future<bool> removeCheckInitConfig() async {
    final prefs = await SharedPreferences.getInstance();
    return prefs.remove(_keyCheckInitConfig);
  }

  // Clear all cached data
  static Future<bool> clearAll() async {
    await removeToken();
    await removeUserId();
    await clearSystemInitFlags();
    await removeCheckInitConfig();
    return true;
  }
}