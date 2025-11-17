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
  final bool isLoading; // Nueva propiedad para mostrar loader
  final Color? loaderColor; // Color opcional para el loader

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
    this.isLoading = false, // Por defecto no está cargando
    this.loaderColor, // Color opcional, si es null usa blanco
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
        return AppColors.secondaryColor;
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

  // Método para construir el contenido del botón (texto o loader)
  Widget _buildButtonContent() {
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
                widget.loaderColor ?? Colors.white,
              ),
            ),
          ),
          if (widget.size != 'small') const SizedBox(width: 12),
          if (widget.size != 'small')
            Text(
              'Cargando...',
              style: TextStyle(
                color: Colors.white,
                fontWeight: FontWeight.w600,
                fontSize: _calculateFontSize(),
                letterSpacing: 0.5,
              ),
            ),
        ],
      );
    }

    return Text(
      widget.label,
      style: TextStyle(
        color: Colors.white,
        fontWeight: FontWeight.w600,
        fontSize: _calculateFontSize(),
        letterSpacing: 0.5,
      ),
      maxLines: 1,
      overflow: TextOverflow.ellipsis,
    );
  }

  @override
  Widget build(BuildContext context) {
    final buttonColor = _getColorFromType(widget.colorType);
    final sizePreset = _getSizePreset(widget.size);

    EdgeInsets buttonPadding = widget.size != null
        ? EdgeInsets.symmetric(
            horizontal: sizePreset['padding']!,
            vertical: sizePreset['padding']! * 0.75,
          )
        : const EdgeInsets.symmetric(horizontal: 24, vertical: 16);

    // Color del botón cuando está en estado de carga
    final backgroundColor = widget.isLoading 
        ? buttonColor.withOpacity(0.7) 
        : (_isHovering ? _getHoverColor(buttonColor) : buttonColor);

    Widget button = ElevatedButton(
      onPressed: widget.isLoading ? null : _handlePress, // Deshabilitar si está cargando
      style: ElevatedButton.styleFrom(
        backgroundColor: backgroundColor,
        foregroundColor: Colors.white,
        elevation: widget.isLoading ? 1 : (_isHovering ? 4 : 2),
        shadowColor: buttonColor.withOpacity(widget.isLoading ? 0.1 : 0.3),
        shape: RoundedRectangleBorder(
          borderRadius: BorderRadius.circular(6),
        ),
        padding: buttonPadding,
        // Efecto de deshabilitado cuando está cargando
        disabledBackgroundColor: buttonColor.withOpacity(0.7),
        disabledForegroundColor: Colors.white.withOpacity(0.8),
      ),
      child: _buildButtonContent(),
    );

    double? finalWidth = widget.fullWidth
        ? double.infinity
        : (widget.width ?? (widget.size != null ? sizePreset['width'] : null));
    
    double? finalHeight = widget.height ?? (widget.size != null ? sizePreset['height'] : null);

    if (widget.fullWidth || widget.width != null || widget.height != null || widget.size != null) {
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