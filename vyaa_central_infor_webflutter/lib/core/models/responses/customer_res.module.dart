class CustomerSettingsJson {
  final int index;
  final String route;
  final String type;
  final double discount;

  CustomerSettingsJson({
    required this.index,
    required this.route,
    required this.type,
    required this.discount,
  });

  factory CustomerSettingsJson.fromJson(Map<String, dynamic> json) {
    return CustomerSettingsJson(
      index: json['index'] ?? 0,
      route: json['route'] ?? '',
      type: json['type'] ?? '',
      discount: (json['discount'] ?? 0).toDouble(),
    );
  }
}

class DirectionJson {
  final int index;
  final String interiorNumber;
  final String exteriorNumber;
  final String neighboringStreet;
  final String neighboringStreet2;
  final String colony;
  final String city;
  final String municipality;

  DirectionJson({
    required this.index,
    required this.interiorNumber,
    required this.exteriorNumber,
    required this.neighboringStreet,
    required this.neighboringStreet2,
    required this.colony,
    required this.city,
    required this.municipality,
  });

  factory DirectionJson.fromJson(Map<String, dynamic> json) {
    return DirectionJson(
      index: json['index'] ?? 0,
      interiorNumber: json['interiorNumber'] ?? '',
      exteriorNumber: json['exteriorNumber'] ?? '',
      neighboringStreet: json['neighboringStreet'] ?? '',
      neighboringStreet2: json['neighboringStreet2'] ?? '',
      colony: json['colony'] ?? '',
      city: json['city'] ?? '',
      municipality: json['municipality'] ?? '',
    );
  }
}

class PaymentMethodsJson {
  final int index;
  final String code;
  final String description;
  final String bank;
  final int accountNumber;
  final String referencePayment;
  final String currency;

  PaymentMethodsJson({
    required this.index,
    required this.code,
    required this.description,
    required this.bank,
    required this.accountNumber,
    required this.referencePayment,
    required this.currency,
  });

  factory PaymentMethodsJson.fromJson(Map<String, dynamic> json) {
    return PaymentMethodsJson(
      index: json['index'] ?? 0,
      code: json['code'] ?? '',
      description: json['description'] ?? '',
      bank: json['bank'] ?? '',
      accountNumber: json['accountNumber'] ?? 0,
      referencePayment: json['referencePayment'] ?? '',
      currency: json['currency'] ?? '',
    );
  }
}

class PaymentTermsJson {
  final int index;
  final String paymentType;
  final DateTime expirationDate;
  final String typeOfCurrency;

  PaymentTermsJson({
    required this.index,
    required this.paymentType,
    required this.expirationDate,
    required this.typeOfCurrency,
  });

  factory PaymentTermsJson.fromJson(Map<String, dynamic> json) {
    return PaymentTermsJson(
      index: json['index'] ?? 0,
      paymentType: json['paymentType'] ?? '',
      expirationDate: DateTime.tryParse(json['expirationDate'] ?? '') ?? DateTime.now(),
      typeOfCurrency: json['typeOfCurrency'] ?? '',
    );
  }
}

class CustomerRes {
  final int idCustomer;
  final int externalId;
  final String name;
  final String lastName;
  final String phoneNumber;
  final String email;
  final String taxId;
  final CustomerSettingsJson? settingsCustomerJson;
  final DirectionJson? directionJson;
  final PaymentMethodsJson? paymentMethodsJson;
  final PaymentTermsJson? paymentTermsJson;

  CustomerRes({
    required this.idCustomer,
    required this.externalId,
    required this.name,
    required this.lastName,
    required this.phoneNumber,
    required this.email,
    required this.taxId,
    this.settingsCustomerJson,
    this.directionJson,
    this.paymentMethodsJson,
    this.paymentTermsJson,
  });

  // Propiedades de conveniencia para mantener compatibilidad
  String get customerId => idCustomer.toString();
  String get code => externalId.toString();
  String get fullName => '$name $lastName'.trim();
  List<String> get phones => phoneNumber.isNotEmpty ? [phoneNumber] : [];

  factory CustomerRes.fromJson(Map<String, dynamic> json) {
    return CustomerRes(
      idCustomer: json['idCustomer'] ?? 0,
      externalId: json['externalId'] ?? 0,
      name: json['name'] ?? '',
      lastName: json['lastName'] ?? '',
      phoneNumber: json['phoneNumber'] ?? '',
      email: json['email'] ?? '',
      taxId: json['taxId'] ?? '',
      settingsCustomerJson: json['settingsCustomerJson'] != null
          ? CustomerSettingsJson.fromJson(json['settingsCustomerJson'])
          : null,
      directionJson: json['directionJson'] != null
          ? DirectionJson.fromJson(json['directionJson'])
          : null,
      paymentMethodsJson: json['paymentMethodsJson'] != null
          ? PaymentMethodsJson.fromJson(json['paymentMethodsJson'])
          : null,
      paymentTermsJson: json['paymentTermsJson'] != null
          ? PaymentTermsJson.fromJson(json['paymentTermsJson'])
          : null,
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
