// lib/Core/Widgets/app_button.widget.dart
import 'package:flutter/material.dart';
import 'package:vyaa_central_infor_webflutter/Core/core.dart';

class AppButton extends StatefulWidget {
  final String label;
  final String colorType;
  final VoidCallback? onPressed;
  final VoidCallback? onLongPressed;
  final VoidCallback? onDoublePressed;
  final bool fullWidth;
  final double? width;
  final double? height;
  final String? size;
  final List<dynamic>? toggleValues;
  final ValueChanged<dynamic>? onToggleChanged;
  final bool isLoading;
  final Color? loaderColor;
  // Nuevas propiedades para íconos y estilos
  final IconData? icon;
  final bool iconOnly;
  final String variant;
  final Color? customColor;

  const AppButton({
    Key? key,
    required this.label,
    this.colorType = 'primary',
    this.onPressed,
    this.onLongPressed,
    this.onDoublePressed,
    this.fullWidth = false,
    this.width,
    this.height,
    this.size,
    this.toggleValues,
    this.onToggleChanged,
    this.isLoading = false,
    this.loaderColor,
    this.icon,
    this.iconOnly = false,
    this.variant = 'filled',
    this.customColor,
  }) : super(key: key);

  @override
  State<AppButton> createState() => _AppButtonState();
}

class _AppButtonState extends State<AppButton> {
  int _toggleIndex = 0;
  bool _isHovering = false;

  void _handlePress() {
    // Si está cargando, no hacer nada
    if (widget.isLoading) return;
    
    if (widget.toggleValues != null && widget.toggleValues!.isNotEmpty) {
      setState(() {
        _toggleIndex = (_toggleIndex + 1) % widget.toggleValues!.length;
      });
      widget.onToggleChanged?.call(widget.toggleValues![_toggleIndex]);
    } else {
      widget.onPressed?.call();
    }
  }

  Color _getColorFromType(String type) {
    // Si hay un color personalizado, usarlo
    if (widget.customColor != null) {
      return widget.customColor!;
    }

    switch (type.toLowerCase()) {
      case 'success':
        return AppColors.success;
      case 'cancel':
      case 'danger':
        return AppColors.danger;
      case 'warning':
        return AppColors.warning;
      case 'info':
        return AppColors.info;
      case 'secondary':
        return AppColors.danger;
      case 'primary':
        return AppColors.primaryColor;
      default:
        return AppColors.success;
    }
  }

  Map<String, double> _getSizePreset(String? size) {
    switch (size?.toLowerCase()) {
      case 'small':
        return {'width': 80.0, 'height': 32.0, 'fontSize': 12.0, 'padding': 8.0};
      case 'large':
        return {'width': 160.0, 'height': 56.0, 'fontSize': 18.0, 'padding': 16.0};
      case 'medium':
      default:
        return {'width': 120.0, 'height': 48.0, 'fontSize': 16.0, 'padding': 12.0};
    }
  }

  double _calculateFontSize() {
    final sizePreset = _getSizePreset(widget.size);
    
    if (widget.size != null) {
      return sizePreset['fontSize']!;
    }
    
    double baseFontSize = 16.0;
    
    if (widget.height != null) {
      double availableHeight = widget.height! - 16;
      baseFontSize = (availableHeight * 0.6).clamp(10.0, 20.0);
    }
    
    if (widget.width != null) {
      double availableWidth = widget.width! - 24;
      double estimatedCharWidth = baseFontSize * 0.6;
      double maxCharsForWidth = availableWidth / estimatedCharWidth;
      
      if (widget.label.length > maxCharsForWidth) {
        baseFontSize = (baseFontSize * maxCharsForWidth / widget.label.length).clamp(8.0, baseFontSize);
      }
    }
    
    return baseFontSize;
  }

  // Método para construir el contenido del botón (texto, ícono o loader)
  Widget _buildButtonContent() {
    final textColor = _getTextColor();

    if (widget.isLoading) {
      return Row(
        mainAxisAlignment: MainAxisAlignment.center,
        mainAxisSize: MainAxisSize.min,
        children: [
          SizedBox(
            width: 16,
            height: 16,
            child: CircularProgressIndicator(
              strokeWidth: 2,
              valueColor: AlwaysStoppedAnimation<Color>(
                widget.loaderColor ?? textColor,
              ),
            ),
          ),
          if (widget.size != 'small' && !widget.iconOnly) const SizedBox(width: 12),
          if (widget.size != 'small' && !widget.iconOnly)
            Text(
              'Cargando...',
              style: TextStyle(
                color: textColor,
                fontWeight: FontWeight.w600,
                fontSize: _calculateFontSize(),
                letterSpacing: 0.5,
              ),
            ),
        ],
      );
    }

    // Solo ícono
    if (widget.iconOnly && widget.icon != null) {
      return Icon(
        widget.icon,
        color: textColor,
        size: _calculateIconSize(),
      );
    }

    // Ícono con texto
    if (widget.icon != null && !widget.iconOnly) {
      return Row(
        mainAxisAlignment: MainAxisAlignment.center,
        mainAxisSize: MainAxisSize.min,
        children: [
          Icon(
            widget.icon,
            color: textColor,
            size: _calculateIconSize(),
          ),
          const SizedBox(width: 8),
          Text(
            widget.label,
            style: TextStyle(
              color: textColor,
              fontWeight: FontWeight.w600,
              fontSize: _calculateFontSize(),
              letterSpacing: 0.5,
            ),
            maxLines: 1,
            overflow: TextOverflow.ellipsis,
          ),
        ],
      );
    }

    // Solo texto
    return Text(
      widget.label,
      style: TextStyle(
        color: textColor,
        fontWeight: FontWeight.w600,
        fontSize: _calculateFontSize(),
        letterSpacing: 0.5,
      ),
      maxLines: 1,
      overflow: TextOverflow.ellipsis,
    );
  }

  // Método para calcular el color del texto según la variante
  Color _getTextColor() {
    final buttonColor = _getColorFromType(widget.colorType);

    switch (widget.variant) {
      case 'outlined':
      case 'text':
        return buttonColor;
      case 'filled':
      default:
        return Colors.white;
    }
  }

  // Método para calcular el tamaño del ícono
  double _calculateIconSize() {
    final sizePreset = _getSizePreset(widget.size);

    if (widget.size != null) {
      return sizePreset['fontSize']! + 2;
    }

    if (widget.iconOnly) {
      return widget.height != null ? widget.height! * 0.4 : 20.0;
    }

    return _calculateFontSize() + 2;
  }

  @override
  Widget build(BuildContext context) {
    final buttonColor = _getColorFromType(widget.colorType);
    final sizePreset = _getSizePreset(widget.size);
    final textColor = _getTextColor();

    EdgeInsets buttonPadding;

    if (widget.iconOnly) {
      // Para botones de solo ícono, usar padding cuadrado
      final padding = widget.size != null ? sizePreset['padding']! : 12.0;
      buttonPadding = EdgeInsets.all(padding);
    } else {
      // Padding normal para botones con texto
      buttonPadding = widget.size != null
          ? EdgeInsets.symmetric(
              horizontal: sizePreset['padding']!,
              vertical: sizePreset['padding']! * 0.75,
            )
          : const EdgeInsets.symmetric(horizontal: 24, vertical: 16);
    }

    Widget button;

    switch (widget.variant) {
      case 'outlined':
        button = OutlinedButton(
          onPressed: widget.isLoading ? null : _handlePress,
          style: OutlinedButton.styleFrom(
            foregroundColor: _isHovering ? Colors.white : textColor,
            backgroundColor: widget.isLoading
                ? buttonColor.withOpacity(0.1)
                : (_isHovering ? buttonColor : Colors.transparent),
            side: BorderSide(
              color: widget.isLoading ? buttonColor.withOpacity(0.3) : buttonColor,
              width: 1,
            ),
            elevation: 0,
            shape: RoundedRectangleBorder(
              borderRadius: BorderRadius.circular(6),
            ),
            padding: buttonPadding,
            disabledForegroundColor: buttonColor.withOpacity(0.5),
          ),
          child: _buildButtonContent(),
        );
        break;

      case 'text':
        button = TextButton(
          onPressed: widget.isLoading ? null : _handlePress,
          style: TextButton.styleFrom(
            foregroundColor: widget.isLoading ? buttonColor.withOpacity(0.5) : textColor,
            backgroundColor: _isHovering ? buttonColor.withOpacity(0.1) : Colors.transparent,
            elevation: 0,
            shape: RoundedRectangleBorder(
              borderRadius: BorderRadius.circular(6),
            ),
            padding: buttonPadding,
            disabledForegroundColor: buttonColor.withOpacity(0.3),
          ),
          child: _buildButtonContent(),
        );
        break;

      case 'filled':
      default:
        final backgroundColor = widget.isLoading
            ? buttonColor.withOpacity(0.7)
            : (_isHovering ? _getHoverColor(buttonColor) : buttonColor);

        button = ElevatedButton(
          onPressed: widget.isLoading ? null : _handlePress,
          style: ElevatedButton.styleFrom(
            backgroundColor: backgroundColor,
            foregroundColor: Colors.white,
            elevation: widget.isLoading ? 1 : (_isHovering ? 4 : 2),
            shadowColor: buttonColor.withOpacity(widget.isLoading ? 0.1 : 0.3),
            shape: RoundedRectangleBorder(
              borderRadius: BorderRadius.circular(6),
            ),
            padding: buttonPadding,
            disabledBackgroundColor: buttonColor.withOpacity(0.7),
            disabledForegroundColor: Colors.white.withOpacity(0.8),
          ),
          child: _buildButtonContent(),
        );
        break;
    }

    double? finalWidth = widget.fullWidth
        ? double.infinity
        : (widget.width ?? (widget.size != null && !widget.iconOnly ? sizePreset['width'] : null));

    double? finalHeight = widget.height ?? (widget.size != null ? sizePreset['height'] : null);

    if (widget.fullWidth || widget.width != null || widget.height != null || (widget.size != null && !widget.iconOnly)) {
      button = SizedBox(
        width: finalWidth,
        height: finalHeight,
        child: button,
      );
    }

    return MouseRegion(
      onEnter: (_) {
        if (!widget.isLoading) {
          setState(() => _isHovering = true);
        }
      },
      onExit: (_) => setState(() => _isHovering = false),
      child: GestureDetector(
        onDoubleTap: widget.isLoading ? null : widget.onDoublePressed,
        onLongPress: widget.isLoading ? null : widget.onLongPressed,
        child: button,
      ),
    );
  }

  // Método para obtener el color hover (oscurece el color original)
  Color _getHoverColor(Color baseColor) {
    final hsl = HSLColor.fromColor(baseColor);
    return hsl.withLightness((hsl.lightness - 0.1).clamp(0.0, 1.0)).toColor();
  }
}