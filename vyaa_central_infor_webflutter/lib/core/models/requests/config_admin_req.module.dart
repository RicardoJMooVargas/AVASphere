import 'package:flutter/widgets.dart';

class ConfigAdminReq {
  final TextEditingController userNameController;
  final TextEditingController passwordController;

  ConfigAdminReq({
    String? userName,
    String? password,
  })  : userNameController = TextEditingController(text: userName ?? ''),
        passwordController = TextEditingController(text: password ?? '');

  /// Create from existing controllers
  ConfigAdminReq.fromControllers({
    required this.userNameController,
    required this.passwordController,
  });

  /// Produce a Map ready for JSON encoding
  Map<String, dynamic> toJson() {
    return {
      'userName': userNameController.text.trim(),
      'password': passwordController.text.trim(),
    };
  }

  /// Convenience factory from raw data
  factory ConfigAdminReq.fromRaw({
    String? userName,
    String? password,
  }) =>
      ConfigAdminReq(
        userName: userName,
        password: password,
      );

  /// Factory from JSON
  factory ConfigAdminReq.fromJson(Map<String, dynamic> json) {
    return ConfigAdminReq(
      userName: json['userName'] as String?,
      password: json['password'] as String?,
    );
  }

  /// Validate if both fields are not empty
  bool isValid() {
    return userNameController.text.trim().isNotEmpty && 
           passwordController.text.trim().isNotEmpty;
  }

  /// Check if userName is valid (not empty and basic validation)
  bool isUserNameValid() {
    final userName = userNameController.text.trim();
    return userName.isNotEmpty && userName.length >= 3;
  }

  /// Check if password is valid (not empty and meets minimum requirements)
  bool isPasswordValid() {
    final password = passwordController.text.trim();
    return password.isNotEmpty && password.length >= 6;
  }

  /// Clear all fields
  void clear() {
    userNameController.clear();
    passwordController.clear();
  }

  /// Dispose all controllers
  void dispose() {
    userNameController.dispose();
    passwordController.dispose();
  }
}
