import 'package:flutter/material.dart';

/// Un botón altamente configurable y reutilizable para usar en toda la aplicación.
/// 
/// El [AppButton] ofrece múltiples opciones de personalización incluyendo:
/// - Colores predefinidos según el tipo de acción
/// - Tamaños predefinidos (small, medium, large)
/// - Dimensiones personalizadas
/// - Funcionalidad de toggle
/// - Múltiples tipos de interacción (tap, double tap, long press)
/// - Adaptación automática del tamaño de fuente
/// 
/// ## Ejemplos de uso:
/// 
/// ```dart
/// // Botón básico
/// AppButton(
///   label: "Aceptar",
///   onPressed: () => print("Presionado"),
/// )
/// 
/// // Botón con tamaño predefinido
/// AppButton(
///   label: "Guardar",
///   size: 'large',
///   colorType: 'success',
///   onPressed: () => guardarDatos(),
/// )
/// 
/// // Botón de ancho completo
/// AppButton(
///   label: "Continuar",
///   fullWidth: true,
///   onPressed: () => navegarSiguiente(),
/// )
/// 
/// // Botón toggle
/// AppButton(
///   label: "Estado",
///   toggleValues: ['Activo', 'Inactivo'],
///   onToggleChanged: (value) => print("Nuevo estado: $value"),
/// )
/// ```
class AppButton extends StatefulWidget {
  /// El texto que se mostrará en el botón.
  final String label;

  /// El tipo de color del botón.
  /// 
  /// Valores disponibles:
  /// - `'primary'` (por defecto): Color índigo
  /// - `'success'`: Verde
  /// - `'danger'` o `'cancel'`: Rojo
  /// - `'warning'`: Naranja
  /// - `'info'`: Azul
  /// - `'secondary'`: Gris
  final String colorType;

  /// Callback que se ejecuta cuando se presiona el botón.
  /// 
  /// Si [toggleValues] está definido, este callback se ignora
  /// y se usa [onToggleChanged] en su lugar.
  final VoidCallback? onPressed;

  /// Callback que se ejecuta cuando se mantiene presionado el botón.
  final VoidCallback? onLongPressed;

  /// Callback que se ejecuta cuando se hace doble tap en el botón.
  final VoidCallback? onDoublePressed;

  /// Si es `true`, el botón ocupará todo el ancho disponible
  /// y tendrá bordes cuadrados (sin border radius).
  /// 
  /// Cuando es `false`, el botón mantiene su tamaño natural
  /// o el especificado por [width] y [height].
  final bool fullWidth;

  /// Ancho específico del botón en píxeles.
  /// 
  /// Si se especifica junto con [size], este valor tiene prioridad.
  /// Si [fullWidth] es `true`, este valor se ignora.
  final double? width;

  /// Alto específico del botón en píxeles.
  /// 
  /// Si se especifica junto con [size], este valor tiene prioridad.
  final double? height;

  /// Tamaño predefinido del botón.
  /// 
  /// Valores disponibles:
  /// - `'small'`: 80x32px, fuente 12px
  /// - `'medium'`: 120x48px, fuente 16px (por defecto si no se especifica)
  /// - `'large'`: 160x56px, fuente 18px
  /// 
  /// Si se especifica [width] o [height], esos valores tienen prioridad
  /// sobre las dimensiones del tamaño predefinido.
  final String? size;

  /// Lista de valores para el modo toggle.
  /// 
  /// Si se proporciona, el botón funcionará como un toggle que cicla
  /// entre los valores de esta lista cada vez que se presiona.
  /// 
  /// Ejemplo:
  /// ```dart
  /// toggleValues: ['Opción A', 'Opción B', 'Opción C']
  /// ```
  final List<dynamic>? toggleValues;

  /// Callback que se ejecuta cuando cambia el valor del toggle.
  /// 
  /// Solo se usa si [toggleValues] está definido.
  /// Recibe el nuevo valor seleccionado como parámetro.
  final ValueChanged<dynamic>? onToggleChanged;

  /// Crea un [AppButton] con las propiedades especificadas.
  /// 
  /// El parámetro [label] es requerido y define el texto del botón.
  /// 
  /// Para un botón normal, proporciona [onPressed].
  /// Para un botón toggle, proporciona [toggleValues] y [onToggleChanged].
  /// 
  /// ## Parámetros de tamaño (en orden de prioridad):
  /// 1. [fullWidth] - Si es true, ignora [width]
  /// 2. [width] y [height] - Dimensiones específicas
  /// 3. [size] - Tamaño predefinido ('small', 'medium', 'large')
  /// 4. Tamaño natural del botón (si no se especifica nada)
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
  }) : super(key: key);

  @override
  State<AppButton> createState() => _AppButtonState();
}

class _AppButtonState extends State<AppButton> {
  /// Índice actual del toggle (para modo toggle).
  int _toggleIndex = 0;

  /// Maneja el evento de presionar el botón.
  /// 
  /// Si [widget.toggleValues] está definido, cicla entre los valores
  /// del toggle y llama a [widget.onToggleChanged].
  /// De lo contrario, ejecuta [widget.onPressed].
  void _handlePress() {
    // Si está en modo toggle
    if (widget.toggleValues != null && widget.toggleValues!.isNotEmpty) {
      setState(() {
        _toggleIndex = (_toggleIndex + 1) % widget.toggleValues!.length;
      });
      widget.onToggleChanged?.call(widget.toggleValues![_toggleIndex]);
    } else {
      widget.onPressed?.call();
    }
  }

  /// Obtiene el color correspondiente al tipo especificado.
  /// 
  /// Mapea los tipos de string a colores de Material Design:
  /// - 'primary': [Colors.indigo]
  /// - 'success': [Colors.green]
  /// - 'danger'/'cancel': [Colors.red]
  /// - 'warning': [Colors.orange]
  /// - 'info': [Colors.blue]
  /// - 'secondary': [Colors.grey]
  /// 
  /// Por defecto retorna [Colors.indigo] para tipos no reconocidos.
  Color _getColorFromType(String type) {
    switch (type.toLowerCase()) {
      case 'success':
        return Colors.green;
      case 'cancel':
      case 'danger':
        return Colors.red;
      case 'warning':
        return Colors.orange;
      case 'info':
        return Colors.blue;
      case 'secondary':
        return Colors.grey;
      case 'primary':
      default:
        return Colors.indigo;
    }
  }

  /// Obtiene las configuraciones predefinidas para un tamaño específico.
  /// 
  /// Retorna un [Map] con las siguientes claves:
  /// - 'width': Ancho en píxeles
  /// - 'height': Alto en píxeles  
  /// - 'fontSize': Tamaño de fuente en píxeles
  /// - 'padding': Padding horizontal en píxeles
  /// 
  /// Tamaños disponibles:
  /// - 'small': 80x32px, fuente 12px, padding 8px
  /// - 'medium': 120x48px, fuente 16px, padding 12px (por defecto)
  /// - 'large': 160x56px, fuente 18px, padding 16px
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

  /// Calcula el tamaño de fuente apropiado basado en las configuraciones del botón.
  /// 
  /// La lógica de cálculo sigue este orden de prioridad:
  /// 1. Si hay [widget.size] definido, usa el fontSize del preset
  /// 2. Si hay [widget.height] específica, calcula proporcionalmente (10-20px)
  /// 3. Si hay [widget.width] específico, ajusta para evitar overflow (8px mínimo)
  /// 4. Por defecto usa 16px
  /// 
  /// El tamaño se adapta automáticamente para evitar que el texto se desborde.
  double _calculateFontSize() {
    final sizePreset = _getSizePreset(widget.size);
    
    // Si hay tamaño predefinido, usar su fontSize
    if (widget.size != null) {
      return sizePreset['fontSize']!;
    }
    
    // Tamaño base de fuente
    double baseFontSize = 16.0;
    
    // Si hay height específica, ajustar el tamaño de fuente
    if (widget.height != null) {
      // Calcular tamaño de fuente basado en la altura
      // Dejamos espacio para padding vertical (16px total)
      double availableHeight = widget.height! - 16;
      baseFontSize = (availableHeight * 0.6).clamp(10.0, 20.0);
    }
    
    // Si hay width específico, también considerar el ancho
    if (widget.width != null) {
      // Estimar caracteres que caben y ajustar tamaño si es necesario
      double availableWidth = widget.width! - 24; // padding horizontal
      double estimatedCharWidth = baseFontSize * 0.6;
      double maxCharsForWidth = availableWidth / estimatedCharWidth;
      
      if (widget.label.length > maxCharsForWidth) {
        baseFontSize = (baseFontSize * maxCharsForWidth / widget.label.length).clamp(8.0, baseFontSize);
      }
    }
    
    return baseFontSize;
  }

  /// Construye el widget del botón con todas las configuraciones aplicadas.
  /// 
  /// El método ensambla el botón aplicando:
  /// 1. Color según [widget.colorType]
  /// 2. Tamaño de fuente calculado dinámicamente
  /// 3. Padding basado en [widget.size] o valores por defecto
  /// 4. Dimensiones finales considerando prioridades
  /// 5. Border radius (0 si [widget.fullWidth], 8px en caso contrario)
  /// 6. Gestos adicionales ([onDoubleTap], [onLongPress])
  @override
  Widget build(BuildContext context) {
    final buttonColor = _getColorFromType(widget.colorType);
    final fontSize = _calculateFontSize();
    final sizePreset = _getSizePreset(widget.size);

    Widget buttonContent = Center(
      child: FittedBox(
        fit: BoxFit.scaleDown,
        child: Text(
          widget.label,
          style: TextStyle(
            color: Colors.white,
            fontWeight: FontWeight.bold,
            fontSize: fontSize,
          ),
          maxLines: 1,
          overflow: TextOverflow.ellipsis,
        ),
      ),
    );

    // Determinar el padding basado en el tamaño predefinido
    EdgeInsets buttonPadding = widget.size != null
        ? EdgeInsets.symmetric(
            horizontal: sizePreset['padding']!,
            vertical: sizePreset['padding']! * 0.6,
          )
        : const EdgeInsets.symmetric(horizontal: 12, vertical: 8);

    // GestureDetector para manejar doble tap y long press personalizados
    Widget button = ElevatedButton(
      onPressed: _handlePress,
      style: ElevatedButton.styleFrom(
        backgroundColor: buttonColor,
        shape: RoundedRectangleBorder(
          borderRadius: BorderRadius.circular(widget.fullWidth ? 0 : 8),
        ),
        padding: buttonPadding,
      ),
      child: buttonContent,
    );

    // Determinar dimensiones finales
    double? finalWidth = widget.fullWidth
        ? double.infinity
        : (widget.width ?? (widget.size != null ? sizePreset['width'] : null));
    
    double? finalHeight = widget.height ?? (widget.size != null ? sizePreset['height'] : null);

    // Solo usar SizedBox si necesitamos controlar el tamaño
    if (widget.fullWidth || widget.width != null || widget.height != null || widget.size != null) {
      button = SizedBox(
        width: finalWidth,
        height: finalHeight,
        child: button,
      );
    }

    return GestureDetector(
      onDoubleTap: widget.onDoublePressed,
      onLongPress: widget.onLongPressed,
      child: button,
    );
  }
}
