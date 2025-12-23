import 'package:flutter/widgets.dart';
import 'package:vyaa_central_infor_webflutter/configs/enums.dart';
// Clase para representar un color del sistema
class SystemColor {
  final int index;
  final String nameColor;
  final String colorCode;
  final String colorRgb;
  
  SystemColor({
    required this.index,
    required this.nameColor,
    required this.colorCode,
    required this.colorRgb,
  });

  Map<String, dynamic> toJson() {
    return {
      'index': index,
      'nameColor': nameColor,
      'colorCode': colorCode,
      'colorRgb': colorRgb,
    };
  }

  factory SystemColor.fromJson(Map<String, dynamic> json) {
    return SystemColor(
      index: json['index'] ?? 0,
      nameColor: json['nameColor'] ?? '',
      colorCode: json['colorCode'] ?? '',
      colorRgb: json['colorRgb'] ?? '',
    );
  }
}

class ConfigSysReq {
  final TextEditingController companyNameController;
  final TextEditingController branchNameController;
  final TextEditingController logoUrlController;
  
  // Lista de colores del sistema
  final List<SystemColor> colors;
  
  // Lista de módulos que no se usarán
  final List<SystemModule> notUseModules;

  ConfigSysReq({
    String? companyName,
    String? branchName,
    String? logoUrl,
    List<SystemColor>? colors,
    List<SystemModule>? notUseModules,
  })  : companyNameController = TextEditingController(text: companyName ?? ''),
        branchNameController = TextEditingController(text: branchName ?? ''),
        logoUrlController = TextEditingController(text: logoUrl ?? ''),
        colors = colors ?? [],
        notUseModules = notUseModules ?? [];

  /// Create from existing controllers
  ConfigSysReq.fromControllers({
    required this.companyNameController,
    required this.branchNameController,
    required this.logoUrlController,
    required this.colors,
    required this.notUseModules,
  });

  /// Produce a Map ready for JSON encoding
  Map<String, dynamic> toJson() {
    return {
      'companyName': companyNameController.text.trim(),
      'branchName': branchNameController.text.trim(),
      'logoUrl': logoUrlController.text.trim(),
      'colors': colors.map((color) => color.toJson()).toList(),
      'notUseModules': notUseModules.map((module) => module.value).toList(),
    };
  }

  /// Convenience factory from raw data
  factory ConfigSysReq.fromRaw({
    String? companyName,
    String? branchName,
    String? logoUrl,
    List<SystemColor>? colors,
    List<SystemModule>? notUseModules,
  }) =>
      ConfigSysReq(
        companyName: companyName,
        branchName: branchName,
        logoUrl: logoUrl,
        colors: colors,
        notUseModules: notUseModules,
      );

  /// Factory from JSON
  factory ConfigSysReq.fromJson(Map<String, dynamic> json) {
    final colorsJson = json['colors'] as List<dynamic>? ?? [];
    final colors = colorsJson
        .map((colorJson) => SystemColor.fromJson(colorJson as Map<String, dynamic>))
        .toList();

    final notUseModulesJson = json['notUseModules'] as List<dynamic>? ?? [];
    final notUseModules = notUseModulesJson
        .map((moduleValue) {
          final intValue = moduleValue as int;
          return SystemModule.values.firstWhere(
            (module) => module.value == intValue,
            orElse: () => SystemModule.general,
          );
        })
        .toList();

    return ConfigSysReq(
      companyName: json['companyName'] as String?,
      branchName: json['branchName'] as String?,
      logoUrl: json['logoUrl'] as String?,
      colors: colors,
      notUseModules: notUseModules,
    );
  }

  /// Add a color to the system
  void addColor(SystemColor color) {
    colors.add(color);
  }

  /// Remove a color by index
  void removeColorAt(int index) {
    if (index >= 0 && index < colors.length) {
      colors.removeAt(index);
    }
  }

  /// Add a module to not use list
  void addNotUseModule(SystemModule module) {
    if (!notUseModules.contains(module)) {
      notUseModules.add(module);
    }
  }

  /// Remove a module from not use list
  void removeNotUseModule(SystemModule module) {
    notUseModules.remove(module);
  }

  /// Check if a module is disabled
  bool isModuleDisabled(SystemModule module) {
    return notUseModules.contains(module);
  }

  /// Get all available modules that are enabled
  List<SystemModule> getEnabledModules() {
    return SystemModule.values
        .where((module) => !notUseModules.contains(module))
        .toList();
  }

  /// Dispose all controllers
  void dispose() {
    companyNameController.dispose();
    branchNameController.dispose();
    logoUrlController.dispose();
  }
}
