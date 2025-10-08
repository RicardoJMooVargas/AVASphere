import 'package:flutter/widgets.dart';
import 'customer_req.module.dart';
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

  /// Produce a Map ready for JSON encoding (solo campos necesarios según el JSON)
  Map<String, dynamic> toJson() {
    final folioValue = int.tryParse(folioController.text.trim()) ?? 0;
    return {
      'folio': folioValue,
      'saleDate': saleDate.toIso8601String(),
      'generalComment': generalCommentController.text.trim(),
      'customer': customer.toJson(),
      'followups': followups.map((followup) => followup.toJson()).toList(),
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