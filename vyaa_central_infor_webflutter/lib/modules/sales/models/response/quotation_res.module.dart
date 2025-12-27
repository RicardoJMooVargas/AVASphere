import 'package:vyaa_central_infor_webflutter/configs/config.dart';
import 'package:vyaa_central_infor_webflutter/core/models/responses/customer_res.module.dart';
import 'package:vyaa_central_infor_webflutter/modules/sales/models/response/followups_res.module.dart';
import 'package:flutter/material.dart';

/// Enum para los estados de cotización con colores asociados
///
/// Uso:
/// - StatusEnum.pending.color → Colors.orange
/// - StatusEnum.getColor(1) → Colors.orange (por valor numérico)
/// - StatusEnum.getColorByName('Pendiente') → Colors.orange (por nombre)
enum StatusEnum {
  pending(1, 'Pendiente', Color(0xFFFFA500)), // Naranja
  accepted(2, 'Aceptado', Color(0xFF008000)), // Verde
  rejected(3, 'Rechazado', Color(0xFFFF0000)); // Rojo

  const StatusEnum(this.value, this.displayName, this.color);
  final int value;
  final String displayName;
  final Color color;

  static StatusEnum fromValue(int value) {
    return StatusEnum.values.firstWhere(
      (status) => status.value == value,
      orElse: () => StatusEnum.pending,
    );
  }

  static String getDisplayName(int value) {
    return fromValue(value).displayName;
  }

  static Color getColor(int value) {
    return fromValue(value).color;
  }

  static Color getColorByName(String name) {
    return StatusEnum.values.firstWhere(
      (status) => status.displayName.toLowerCase() == name.toLowerCase(),
      orElse: () => StatusEnum.pending,
    ).color;
  }
}

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
  final List<ProductRes> products; // Agregado para manejar los productos
  final int? linkedSaleId;
  final int? linkedSaleFolio;
  final bool isLinkedToSale;
  final int idConfigSys;

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
    required this.products, // Inicializar productos
    this.linkedSaleId,
    this.linkedSaleFolio,
    required this.isLinkedToSale,
    required this.idConfigSys,
  });

  factory QuotationRes.fromJson(Map<String, dynamic> json) {
    return QuotationRes(
      // Usar idQuotation en lugar de quotationId
      quotationId: (json['idQuotation'] ?? json['quotationId'] ?? 0).toString(),
      saleDate: DateTime.parse(json['saleDate']),
      // Convertir status usando el enum para obtener el nombre en español
      status: StatusEnum.getDisplayName(json['status'] ?? 0),
      salesExecutives: List<String>.from(json['salesExecutives'] ?? []),
      folio: json['folio'] ?? 0,
      // Usar idCustomer y convertir a string si es int
      customerId: (json['idCustomer'] ?? json['customerId'] ?? 0).toString(),
      // Customer puede ser null en la respuesta
      customer: json['customer'] != null
          ? CustomerRes.fromJson(json['customer'])
          : CustomerRes(
              idCustomer: json['idCustomer'] ?? 0,
              externalId: 0,
              name: '',
              lastName: '',
              phoneNumber: '',
              email: '',
              taxId: '',
              settingsCustomerJson: null,
              directionJson: null,
              paymentMethodsJson: null,
              paymentTermsJson: null,
            ),
      generalComment: json['generalComment'] ?? '',
      // Mapear followups desde la respuesta (el endpoint devuelve 'followups', no 'followupsJson')
      followups: (json['followups'] as List<dynamic>?)
              ?.map((e) => FollowupRes.fromJson(e))
              .toList() ??
          (json['followupsJson'] as List<dynamic>?)
              ?.map((e) => FollowupRes.fromJson(e))
              .toList() ??
          [],
      createdAt: DateTime.parse(json['createdAt']),
      updatedAt: DateTime.parse(json['updatedAt']),
      // Mapear productos desde la respuesta
      products: (json['products'] as List<dynamic>?)
              ?.map((e) => ProductRes.fromJson(e))
              .toList() ??
          [],
      linkedSaleId: json['linkedSaleId'],
      linkedSaleFolio: json['linkedSaleFolio'],
      isLinkedToSale: json['isLinkedToSale'] ?? false,
      idConfigSys: json['idConfigSys'] ?? 0,
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
      'products': products.map((e) => e.toJson()).toList(), // Agregado
    };
  }
  */
}

class ProductRes {
  final int id;
  final String name;
  final String description;
  final int quantity;
  final double unitPrice;
  final double subtotal;
  final double discount;
  final double total;

  ProductRes({
    required this.id,
    required this.name,
    required this.description,
    required this.quantity,
    required this.unitPrice,
    required this.subtotal,
    required this.discount,
    required this.total,
  });

  factory ProductRes.fromJson(Map<String, dynamic> json) {
    return ProductRes(
      id: json['id'] ?? 0,
      name: json['name'] ?? '',
      description: json['description'] ?? '',
      quantity: json['quantity'] ?? 0,
      unitPrice: (json['unitPrice'] ?? 0).toDouble(),
      subtotal: (json['subtotal'] ?? 0).toDouble(),
      discount: (json['discount'] ?? 0).toDouble(),
      total: (json['total'] ?? 0).toDouble(),
    );
  }
}
