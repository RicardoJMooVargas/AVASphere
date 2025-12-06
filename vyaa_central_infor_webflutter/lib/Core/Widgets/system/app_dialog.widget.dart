import 'package:flutter/material.dart';
import '../../theme/app_colors.dart';

/// Modelo para las acciones del header del diálogo
class DialogAction {
  final IconData icon;
  final VoidCallback onPressed;
  final String? tooltip;
  final Color? color;

  const DialogAction({
    required this.icon,
    required this.onPressed,
    this.tooltip,
    this.color,
  });
}

/// Tipos de botón para mostrar el diálogo
enum DialogButtonType { icon, simple }

/// Configuración para el botón que abre el diálogo
class DialogButtonConfig {
  final DialogButtonType type;
  final IconData? icon;
  final String? label;
  final Color? color;
  final VoidCallback? onPressed; // opcional, si no se proporciona, abre el diálogo

  const DialogButtonConfig({
    required this.type,
    this.icon,
    this.label,
    this.color,
    this.onPressed,
  });
}

/// Widget genérico para diálogos con header, body y footer
class AppDialogWidget extends StatelessWidget {
  final Color headerColor;
  final String title;
  final List<DialogAction> actions;
  final Widget body;
  final Widget footer;
  final double? minWidth;
  final double? maxWidth;
  final double? minHeight;
  final double? maxHeight;
  final DialogButtonConfig? buttonConfig;
  /// Si es false, el diálogo no se puede cerrar haciendo clic fuera de él
  final bool barrierDismissible;

  const AppDialogWidget({
    super.key,
    this.headerColor = AppColors.primaryColor,
    required this.title,
    this.actions = const [],
    required this.body,
    required this.footer,
    this.minWidth,
    this.maxWidth,
    this.minHeight,
    this.maxHeight,
    this.buttonConfig,
    this.barrierDismissible = true, // Por defecto permite cerrar
  });

  @override
  Widget build(BuildContext context) {
    if (buttonConfig != null) {
      return _buildButton(context);
    } else {
      return _buildDialog(context);
    }
  }

  Widget _buildButton(BuildContext context) {
    final config = buttonConfig!;
    final color = config.color ?? AppColors.primaryColor;

    if (config.type == DialogButtonType.icon) {
      return IconButton(
        icon: Icon(config.icon ?? Icons.add, color: color),
        onPressed: config.onPressed ?? () => _showDialog(context),
        tooltip: config.label,
      );
    } else {
      return ElevatedButton(
        style: ElevatedButton.styleFrom(
          backgroundColor: color,
          foregroundColor: Colors.white,
        ),
        onPressed: config.onPressed ?? () => _showDialog(context),
        child: Row(
          mainAxisSize: MainAxisSize.min,
          children: [
            if (config.icon != null) ...[
              Icon(config.icon),
              const SizedBox(width: 8),
            ],
            if (config.label != null) Text(config.label!),
          ],
        ),
      );
    }
  }

  Widget _buildDialog(BuildContext context) {
    return Dialog(
      child: Container(
        constraints: BoxConstraints(
          minWidth: minWidth ?? 400,
          maxWidth: maxWidth ?? 800,
          minHeight: minHeight ?? 300,
          maxHeight: maxHeight ?? 600,
        ),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            // Header
            Container(
              color: headerColor,
              padding: const EdgeInsets.all(16),
              child: Row(
                children: [
                  Text(
                    title,
                    style: const TextStyle(
                      color: Colors.white,
                      fontSize: 18,
                      fontWeight: FontWeight.bold,
                    ),
                  ),
                  const Spacer(),
                  ...actions.map((action) => IconButton(
                        icon: Icon(action.icon, color: Colors.white),
                        onPressed: action.onPressed,
                        tooltip: action.tooltip,
                        color: action.color,
                      )),
                ],
              ),
            ),
            // Body
            Flexible(child: body),
            // Footer
            Container(
              color: headerColor,
              child: footer,
            ),
          ],
        ),
      ),
    );
  }

  void _showDialog(BuildContext context) {
    showDialog(
      context: context,
      barrierDismissible: barrierDismissible,
      builder: (context) => AppDialogWidget(
        headerColor: headerColor,
        title: title,
        actions: actions,
        body: body,
        footer: footer,
        minWidth: minWidth,
        maxWidth: maxWidth,
        minHeight: minHeight,
        maxHeight: maxHeight,
        barrierDismissible: barrierDismissible,
      ),
    );
  }

  /// Función estática para mostrar el diálogo
  static void show({
    required BuildContext context,
    required String title,
    required Widget body,
    required Widget footer,
    Color headerColor = AppColors.primaryColor,
    List<DialogAction> actions = const [],
    double? minWidth,
    double? maxWidth,
    double? minHeight,
    double? maxHeight,
    bool barrierDismissible = true,
  }) {
    showDialog(
      context: context,
      barrierDismissible: barrierDismissible,
      builder: (context) => AppDialogWidget(
        headerColor: headerColor,
        title: title,
        actions: actions,
        body: body,
        footer: footer,
        minWidth: minWidth,
        maxWidth: maxWidth,
        minHeight: minHeight,
        maxHeight: maxHeight,
        barrierDismissible: barrierDismissible,
      ),
    );
  }
}
