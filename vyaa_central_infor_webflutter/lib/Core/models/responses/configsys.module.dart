
class ConfigSys {
  final int idConfigSys;
  final String companyName;
  final String branchName;
  final String logoUrl;
  final List<ColorConfig> colors;
  final List<NotUseModule> notUseModules;
  final DateTime createdAt;

  ConfigSys({
    required this.idConfigSys,
    required this.companyName,
    required this.branchName,
    required this.logoUrl,
    required this.colors,
    required this.notUseModules,
    required this.createdAt,
  });

  factory ConfigSys.fromJson(Map<String, dynamic> json) {
    return ConfigSys(
      idConfigSys: json['idConfigSys'] ?? 0,
      companyName: json['companyName'] ?? '',
      branchName: json['branchName'] ?? '',
      logoUrl: json['logoUrl'] ?? '',
      colors: (json['colors'] as List<dynamic>?)
          ?.map((color) => ColorConfig.fromJson(color as Map<String, dynamic>))
          .toList() ?? <ColorConfig>[],
      notUseModules: (json['notUseModules'] as List<dynamic>?)
          ?.map((module) => NotUseModule.fromJson(module as Map<String, dynamic>))
          .toList() ?? <NotUseModule>[],
      createdAt: json['createdAt'] != null 
          ? DateTime.parse(json['createdAt'] as String)
          : DateTime.now(),
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'idConfigSys': idConfigSys,
      'companyName': companyName,
      'branchName': branchName,
      'logoUrl': logoUrl,
      'colors': colors.map((color) => color.toJson()).toList(),
      'notUseModules': notUseModules.map((module) => module.toJson()).toList(),
      'createdAt': createdAt.toIso8601String(),
    };
  }
}

class ColorConfig {
  final int index;
  final String nameColor;
  final String colorCode;
  final String colorRgb;

  ColorConfig({
    required this.index,
    required this.nameColor,
    required this.colorCode,
    required this.colorRgb,
  });

  factory ColorConfig.fromJson(Map<String, dynamic> json) {
    return ColorConfig(
      index: json['index'] ?? 0,
      nameColor: json['nameColor'] ?? '',
      colorCode: json['colorCode'] ?? '',
      colorRgb: json['colorRgb'] ?? '',
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'index': index,
      'nameColor': nameColor,
      'colorCode': colorCode,
      'colorRgb': colorRgb,
    };
  }
}

class NotUseModule {
  final int index;
  final String nameModule;

  NotUseModule({
    required this.index,
    required this.nameModule,
  });

  factory NotUseModule.fromJson(Map<String, dynamic> json) {
    return NotUseModule(
      index: json['index'] ?? 0,
      nameModule: json['nameModule'] ?? '',
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'index': index,
      'nameModule': nameModule,
    };
  }
}