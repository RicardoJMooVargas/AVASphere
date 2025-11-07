class CustomerRes {
  final String customerId;
  final String code;
  final String fullName;
  final String email;
  final List<String> phones;
  final DateTime createdAt;
  final bool status;

  CustomerRes({
    required this.customerId,
    required this.code,
    required this.fullName,
    required this.email,
    required this.phones,
    required this.createdAt,
    required this.status,
  });

  factory CustomerRes.fromJson(Map<String, dynamic> json) {
    return CustomerRes(
      customerId: json['customerId'] ?? '',
      code: json['code'] ?? '',
      fullName: json['fullName'] ?? '',
      email: json['email'] ?? '',
      phones: List<String>.from(json['phones'] ?? []),
      createdAt: DateTime.parse(json['createdAt']),
      status: json['status'] ?? false,
    );
  }
  /*
  Map<String, dynamic> toJson() {
    return {
      'customerId': customerId,
      'code': code,
      'fullName': fullName,
      'email': email,
      'phones': phones,
      'createdAt': createdAt.toIso8601String(),
      'status': status,
    };
  }
  */
}
