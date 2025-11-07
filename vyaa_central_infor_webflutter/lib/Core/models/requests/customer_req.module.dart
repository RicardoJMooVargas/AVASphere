import 'package:flutter/widgets.dart';

class CustomerReq {
  final TextEditingController customerIdController;
  final TextEditingController codeController;
  final TextEditingController fullNameController;
  final TextEditingController emailController;
  final TextEditingController phoneController;

  CustomerReq({
    String? customerId,
    String? code,
    String? fullName,
    String? email,
    String? phone,
  })  : customerIdController = TextEditingController(text: customerId ?? ''),
        codeController = TextEditingController(text: code ?? ''),
        fullNameController = TextEditingController(text: fullName ?? ''),
        emailController = TextEditingController(text: email ?? ''),
        phoneController = TextEditingController(text: phone ?? '');

  /// Create from existing controllers
  CustomerReq.fromControllers({
    required this.customerIdController,
    required this.codeController,
    required this.fullNameController,
    required this.emailController,
    required this.phoneController,
  });

  /// Produce a Map ready for JSON encoding (solo campos necesarios)
  Map<String, dynamic> toJson() {
    // Convertir el teléfono en un arreglo
    final phoneText = phoneController.text.trim();
    final phoneArray = phoneText.isNotEmpty ? [phoneText] : <String>[];
    
    return {
      'customerId': customerIdController.text.trim(),
      'code': codeController.text.trim(),
      'fullName': fullNameController.text.trim(),
      'email': emailController.text.trim(),
      'phones': phoneArray,
    };
  }

  /// Convenience factory from raw data
  factory CustomerReq.fromRaw({
    String? customerId,
    String? code,
    String? fullName,
    String? email,
    String? phone,
  }) =>
      CustomerReq(
        customerId: customerId,
        code: code,
        fullName: fullName,
        email: email,
        phone: phone,
      );

  /// Dispose all controllers
  void dispose() {
    customerIdController.dispose();
    codeController.dispose();
    fullNameController.dispose();
    emailController.dispose();
    phoneController.dispose();
  }
}
