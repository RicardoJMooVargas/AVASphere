import 'package:flutter/widgets.dart';

class CustomerReq {
  final TextEditingController customerIdController;
  final TextEditingController codeController;
  final TextEditingController fullNameController;
  final TextEditingController emailController;
  final List<TextEditingController> phoneControllers;

  CustomerReq({
    String? customerId,
    String? code,
    String? fullName,
    String? email,
    List<String>? phones,
  })  : customerIdController = TextEditingController(text: customerId ?? ''),
        codeController = TextEditingController(text: code ?? ''),
        fullNameController = TextEditingController(text: fullName ?? ''),
        emailController = TextEditingController(text: email ?? ''),
        phoneControllers = (phones ?? ['']).map((phone) => TextEditingController(text: phone)).toList();

  /// Create from existing controllers
  CustomerReq.fromControllers({
    required this.customerIdController,
    required this.codeController,
    required this.fullNameController,
    required this.emailController,
    required this.phoneControllers,
  });

  /// Produce a Map ready for JSON encoding (solo campos necesarios)
  Map<String, dynamic> toJson() {
    return {
      'customerId': customerIdController.text.trim(),
      'code': codeController.text.trim(),
      'fullName': fullNameController.text.trim(),
      'email': emailController.text.trim(),
      'phones': phoneControllers
          .map((controller) => controller.text.trim())
          .where((phone) => phone.isNotEmpty)
          .toList(),
    };
  }

  /// Convenience factory from raw data
  factory CustomerReq.fromRaw({
    String? customerId,
    String? code,
    String? fullName,
    String? email,
    List<String>? phones,
  }) =>
      CustomerReq(
        customerId: customerId,
        code: code,
        fullName: fullName,
        email: email,
        phones: phones,
      );

  /// Add a new phone controller
  void addPhone() {
    phoneControllers.add(TextEditingController());
  }

  /// Remove a phone controller
  void removePhone(int index) {
    if (index >= 0 && index < phoneControllers.length) {
      phoneControllers[index].dispose();
      phoneControllers.removeAt(index);
    }
  }

  /// Dispose all controllers
  void dispose() {
    customerIdController.dispose();
    codeController.dispose();
    fullNameController.dispose();
    emailController.dispose();
    for (final controller in phoneControllers) {
      controller.dispose();
    }
  }
}