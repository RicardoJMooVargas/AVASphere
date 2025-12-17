import 'package:flutter/widgets.dart';
import '../../../../Core/models/requests/customer_req.module.dart';
import 'followups_req.module.dart';

class QuotationReq {
  final TextEditingController generalCommentController;
  final TextEditingController folioController;
  final List<TextEditingController> salesExecutiveControllers;
  final List<FollowupReq> followups;
  final CustomerReq customer;
  final DateTime saleDate;

  QuotationReq({
    String? generalComment,
    int? folio,
    List<String>? salesExecutives,
    List<FollowupReq>? followups,
    CustomerReq? customer,
    DateTime? saleDate,
  })  : generalCommentController = TextEditingController(text: generalComment ?? ''),
        folioController = TextEditingController(text: (folio ?? 0).toString()),
        salesExecutiveControllers = (salesExecutives ?? ['']).map((exec) => TextEditingController(text: exec)).toList(),
        followups = followups ?? [],
        customer = customer ?? CustomerReq(),
        saleDate = saleDate ?? DateTime.now();

  /// Create from existing controllers and objects
  QuotationReq.fromComponents({
    required this.generalCommentController,
    required this.folioController,
    required this.salesExecutiveControllers,
    required this.followups,
    required this.customer,
    required this.saleDate,
  });

  /// Produce a Map ready for JSON encoding según la estructura del backend
  Map<String, dynamic> toJson() {
    final folioValue = int.tryParse(folioController.text.trim()) ?? 0;
    final customerIdValue = int.tryParse(customer.customerIdController.text.trim()) ?? 0;
    final isNewCustomer = customerIdValue == 0;

    return {
      'folio': folioValue,
      'saleDate': saleDate.toIso8601String().split('T')[0], // Solo la fecha YYYY-MM-DD
      'status': 1, // Por defecto 1
      'generalComment': generalCommentController.text.trim(),
      'customerId': isNewCustomer ? 0 : customerIdValue,
      'newCustomers': isNewCustomer ? [
        {
          'customerId': 0,
          'codeCustomer': customer.codeController.text.trim(),
          'name': customer.fullNameController.text.trim(),
          'email': customer.emailController.text.trim(),
          'phone': customer.phoneController.text.trim(),
          'direction': '', // Por ahora vacío, se puede agregar después
        }
      ] : null,
      'salesExecutives': salesExecutiveControllers
          .map((controller) => controller.text.trim())
          .where((executive) => executive.isNotEmpty)
          .toList(),
      'followups': followups.map((followup) => followup.toJson()).toList(),
      'products': <Map<String, dynamic>>[], // Vacío por ahora
      'idConfigSys': 0, // Se establecerá en el servicio
    };
  }

  /// Convenience factory from raw data
  factory QuotationReq.fromRaw({
    String? generalComment,
    int? folio,
    List<String>? salesExecutives,
    List<FollowupReq>? followups,
    CustomerReq? customer,
    DateTime? saleDate,
  }) =>
      QuotationReq(
        generalComment: generalComment,
        folio: folio,
        salesExecutives: salesExecutives,
        followups: followups,
        customer: customer,
        saleDate: saleDate,
      );

  /// Add a new sales executive controller
  void addSalesExecutive() {
    salesExecutiveControllers.add(TextEditingController());
  }

  /// Remove a sales executive controller
  void removeSalesExecutive(int index) {
    if (index >= 0 && index < salesExecutiveControllers.length) {
      salesExecutiveControllers[index].dispose();
      salesExecutiveControllers.removeAt(index);
    }
  }

  /// Add a new followup
  void addFollowup() {
    followups.add(FollowupReq());
  }

  /// Remove a followup
  void removeFollowup(int index) {
    if (index >= 0 && index < followups.length) {
      followups[index].dispose();
      followups.removeAt(index);
    }
  }

  /// Dispose all controllers
  void dispose() {
    generalCommentController.dispose();
    folioController.dispose();
    
    for (final controller in salesExecutiveControllers) {
      controller.dispose();
    }
    
    for (final followup in followups) {
      followup.dispose();
    }
    
    customer.dispose();
  }
}