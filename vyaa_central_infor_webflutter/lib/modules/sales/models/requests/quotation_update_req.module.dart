class QuotationUpdateReq {
  final int folio;
  final String generalComment;
  final List<String> salesExecutives;

  QuotationUpdateReq({
    required this.folio,
    required this.generalComment,
    required this.salesExecutives,
  });

  Map<String, dynamic> toJson() {
    return {
      'folio': folio,
      'generalComment': generalComment,
      'salesExecutives': salesExecutives,
    };
  }

  factory QuotationUpdateReq.fromJson(Map<String, dynamic> json) {
    return QuotationUpdateReq(
      folio: json['folio'] ?? 0,
      generalComment: json['generalComment'] ?? '',
      salesExecutives: List<String>.from(json['salesExecutives'] ?? []),
    );
  }
}
