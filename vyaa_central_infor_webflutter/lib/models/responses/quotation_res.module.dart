import 'package:vyaa_central_infor_webflutter/models/responses/customer_res.module.dart';
import 'package:vyaa_central_infor_webflutter/models/responses/followups_res.module.dart';

class QuotationRes {
  final String quotationId;
  final DateTime saleDate;
  final String status;
  final List<String> salesExecutives;
  final int folio;
  final String customerId;
  final CustomerRes customer;
  final String generalComment;
  final List<FollowupRes> followups;
  final DateTime createdAt;
  final DateTime updatedAt;

  QuotationRes({
    required this.quotationId,
    required this.saleDate,
    required this.status,
    required this.salesExecutives,
    required this.folio,
    required this.customerId,
    required this.customer,
    required this.generalComment,
    required this.followups,
    required this.createdAt,
    required this.updatedAt,
  });

  factory QuotationRes.fromJson(Map<String, dynamic> json) {
    return QuotationRes(
      quotationId: json['quotationId'] ?? '',
      saleDate: DateTime.parse(json['saleDate']),
      status: json['status'] ?? '',
      salesExecutives: List<String>.from(json['salesExecutives'] ?? []),
      folio: json['folio'] ?? 0,
      customerId: json['customerId'] ?? '',
      customer: CustomerRes.fromJson(json['customer']),
      generalComment: json['generalComment'] ?? '',
      followups: (json['followups'] as List<dynamic>?)
              ?.map((e) => FollowupRes.fromJson(e))
              .toList() ??
          [],
      createdAt: DateTime.parse(json['createdAt']),
      updatedAt: DateTime.parse(json['updatedAt']),
    );
  }
  
  /*
  Map<String, dynamic> toJson() {
    return {
      'quotationId': quotationId,
      'saleDate': saleDate.toIso8601String(),
      'status': status,
      'salesExecutives': salesExecutives,
      'folio': folio,
      'customerId': customerId,
      'customer': customer.toJson(),
      'generalComment': generalComment,
      'followups': followups.map((e) => e.toJson()).toList(),
      'createdAt': createdAt.toIso8601String(),
      'updatedAt': updatedAt.toIso8601String(),
    };
  }
  */
}
