import 'package:flutter/widgets.dart';

class AuthReq {
  // Controllers intended to be used directly by the UI
  final TextEditingController userNameController;
  final TextEditingController passwordController;

  AuthReq({String? userName, String? password})
      : userNameController = TextEditingController(text: userName ?? ''),
        passwordController = TextEditingController(text: password ?? '');

  /// Create from existing controllers (optional)
  AuthReq.fromControllers(this.userNameController, this.passwordController);

  /// Produce a trimmed Map ready for JSON encoding
  Map<String, dynamic> toJson() {
    final user = userNameController.text.trim();
    final pass = passwordController.text.trim();
    return {
      'userName': user,
      'password': pass,
    };
  }

  /// Convenience factory from raw strings
  factory AuthReq.fromRaw(String? userName, String? password) =>
      AuthReq(userName: userName?.trim(), password: password?.trim());

  /// Dispose controllers when no longer needed
  void dispose() {
    userNameController.dispose();
    passwordController.dispose();
  }
}
