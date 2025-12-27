class CheckInitConfigData {
  final bool hasConfiguration;
  final bool tableExists;
  final bool requiresMigration;

  CheckInitConfigData({
    required this.hasConfiguration,
    required this.tableExists,
    required this.requiresMigration,
  });

  factory CheckInitConfigData.fromJson(Map<String, dynamic> json) {
    return CheckInitConfigData(
      hasConfiguration: json['hasConfiguration'] ?? false,
      tableExists: json['tableExists'] ?? false,
      requiresMigration: json['requiresMigration'] ?? false
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'hasConfiguration': hasConfiguration,
      'tableExists': tableExists,
      'requiresMigration': requiresMigration,
    };
  }
}