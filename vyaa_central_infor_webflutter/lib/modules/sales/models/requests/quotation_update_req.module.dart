/// Modelo para actualizar una cotización existente
/// Mapea el DTO del backend QuotationUpdateDto
class QuotationUpdateReq {
  final int? folio;
  final DateTime? saleDate;
  final int? status; // 1=Pendiente, 2=Aceptado, 3=Rechazado
  final String? generalComment;
  final int? customerId;
  final List<String>? salesExecutives;
  final List<QuotationFollowupUpdateDto>? followups;
  final List<QuotationProductUpdateDto>? products;
  final int? idConfigSys;

  QuotationUpdateReq({
    this.folio,
    this.saleDate,
    this.status,
    this.generalComment,
    this.customerId,
    this.salesExecutives,
    this.followups,
    this.products,
    this.idConfigSys,
  });

  Map<String, dynamic> toJson() {
    final json = <String, dynamic>{};

    if (folio != null) json['folio'] = folio;
    if (saleDate != null) json['saleDate'] = saleDate!.toIso8601String().split('T')[0]; // Solo fecha YYYY-MM-DD
    if (status != null) json['status'] = status;
    if (generalComment != null) json['generalComment'] = generalComment;
    if (customerId != null) json['customerId'] = customerId;
    if (salesExecutives != null) json['salesExecutives'] = salesExecutives;
    if (followups != null) json['followups'] = followups!.map((f) => f.toJson()).toList();
    if (products != null) json['products'] = products!.map((p) => p.toJson()).toList();
    if (idConfigSys != null) json['idConfigSys'] = idConfigSys;

    return json;
  }

  factory QuotationUpdateReq.fromJson(Map<String, dynamic> json) {
    return QuotationUpdateReq(
      folio: json['folio'],
      saleDate: json['saleDate'] != null ? DateTime.parse(json['saleDate']) : null,
      status: json['status'],
      generalComment: json['generalComment'],
      customerId: json['customerId'],
      salesExecutives: json['salesExecutives'] != null
          ? List<String>.from(json['salesExecutives'])
          : null,
      followups: json['followups'] != null
          ? (json['followups'] as List).map((f) => QuotationFollowupUpdateDto.fromJson(f)).toList()
          : null,
      products: json['products'] != null
          ? (json['products'] as List).map((p) => QuotationProductUpdateDto.fromJson(p)).toList()
          : null,
      idConfigSys: json['idConfigSys'],
    );
  }
}

/// DTO para seguimientos en actualizaciones de cotización
class QuotationFollowupUpdateDto {
  final DateTime date;
  final String comment;
  final String userId;

  QuotationFollowupUpdateDto({
    DateTime? date,
    required this.comment,
    required this.userId,
  }) : date = date ?? DateTime.now();

  Map<String, dynamic> toJson() {
    return {
      'date': date.toIso8601String(),
      'comment': comment,
      'userId': userId,
    };
  }

  factory QuotationFollowupUpdateDto.fromJson(Map<String, dynamic> json) {
    return QuotationFollowupUpdateDto(
      date: json['date'] != null ? DateTime.parse(json['date']) : null,
      comment: json['comment'] ?? '',
      userId: json['userId'] ?? '',
    );
  }
}

/// DTO para productos en actualizaciones de cotización
class QuotationProductUpdateDto {
  final int productId;
  final double quantity;
  final String description;
  final double unitPrice;
  final double totalPrice;
  final String unit;

  QuotationProductUpdateDto({
    required this.productId,
    required this.quantity,
    required this.description,
    required this.unitPrice,
    required this.totalPrice,
    required this.unit,
  });

  Map<String, dynamic> toJson() {
    return {
      'productId': productId,
      'quantity': quantity,
      'description': description,
      'unitPrice': unitPrice,
      'totalPrice': totalPrice,
      'unit': unit,
    };
  }

  factory QuotationProductUpdateDto.fromJson(Map<String, dynamic> json) {
    return QuotationProductUpdateDto(
      productId: json['productId'] ?? 0,
      quantity: (json['quantity'] ?? 0.0).toDouble(),
      description: json['description'] ?? '',
      unitPrice: (json['unitPrice'] ?? 0.0).toDouble(),
      totalPrice: (json['totalPrice'] ?? 0.0).toDouble(),
      unit: json['unit'] ?? '',
    );
  }
}
