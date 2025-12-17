import 'package:flutter/material.dart';

/// Tabla genérica con columnas tipificadas y apariencia mejorada
///
/// Características:
/// - Fondo blanco con bordes redondeados (configurable)
/// - 6 tipos de columna: texto, icono, dropdown, lista, imagen, acciones, widget
/// - Footer automático con conteo de elementos
/// - Estado vacío personalizable
/// - Hover effects en filas
/// - Scroll horizontal automático
///
/// Tipos de columna:
/// 1. Texto: Muestra texto con tooltip opcional para textos largos
/// 2. Icono: Iconos clickeables o de solo vista
/// 3. Dropdown: Lista desplegable para cambiar valores
/// 4. Lista: Muestra arrays con tooltip completo
/// 5. Imagen: Imágenes con preview opcional
/// 6. Acciones: Botones predefinidos (editar, eliminar, ver)
/// 7. Widget: Widgets personalizados
///
/// Uso básico:
/// ```dart
/// AppTable<MyModel>(
///   items: myItems,
///   columns: [
///     AppTableColumn.text('Nombre', (item) => item.name),
///     AppTableColumn.dropdown('Estado', (item) => item.status,
///       options: ['Pendiente', 'Aceptado', 'Rechazado'],
///       onChanged: (item, value) => {},
///       colors: {
///         'Pendiente': Colors.orange,
///         'Aceptado': Colors.green,
///         'Rechazado': Colors.red,
///       }),
///     AppTableColumn.actions('Acciones', [
///       TableAction.edit(onEdit),
///       TableAction.delete(onDelete),
///     ]),
///   ],
/// )
/// ```
class AppTable<T> extends StatefulWidget {
  /// Lista de elementos a mostrar
  final List<T> items;
  /// Columnas de la tabla con tipos específicos
  final List<AppTableColumn<T>> columns;
  /// Color de fondo de la tabla (default: blanco)
  final Color? backgroundColor;
  /// Border radius de la tabla (default: 8.0)
  final double? borderRadius;
  /// Callback cuando se selecciona una fila
  final void Function(T item)? onRowTap;
  /// Widget personalizado para estado vacío
  final Widget? emptyStateWidget;

  const AppTable({
    Key? key,
    required this.items,
    required this.columns,
    this.backgroundColor,
    this.borderRadius,
    this.onRowTap,
    this.emptyStateWidget,
  }) : super(key: key);

  @override
  _AppTableState<T> createState() => _AppTableState<T>();
}

class _AppTableState<T> extends State<AppTable<T>> {
  @override
  Widget build(BuildContext context) {
    final borderRadius = widget.borderRadius ?? 8.0;
    final backgroundColor = widget.backgroundColor ?? Colors.white;

    return Container(
      decoration: BoxDecoration(
        color: backgroundColor,
        borderRadius: BorderRadius.circular(borderRadius),
        boxShadow: [
          BoxShadow(
            color: Colors.black.withValues(alpha: 0.1),
            blurRadius: 4,
            offset: const Offset(0, 2),
          ),
        ],
      ),
      child: Column(
        children: [
          // Tabla principal
          Expanded(
            child: widget.items.isEmpty
                ? _buildEmptyState()
                : _buildTable(),
          ),
          // Footer con conteo
          _buildFooter(),
        ],
      ),
    );
  }

  /// Construye el estado vacío cuando no hay datos
  Widget _buildEmptyState() {
    return widget.emptyStateWidget ??
        Center(
          child: Column(
            mainAxisAlignment: MainAxisAlignment.center,
            children: [
              Icon(
                Icons.inbox_outlined,
                size: 64,
                color: Colors.grey.shade400,
              ),
              const SizedBox(height: 16),
              Text(
                'No hay datos disponibles',
                style: TextStyle(
                  fontSize: 16,
                  color: Colors.grey.shade600,
                  fontWeight: FontWeight.w500,
                ),
              ),
            ],
          ),
        );
  }

  /// Construye la tabla con los datos
  Widget _buildTable() {
    return SingleChildScrollView(
      scrollDirection: Axis.horizontal,
      child: SingleChildScrollView(
        child: ConstrainedBox(
          constraints: BoxConstraints(
            minWidth: MediaQuery.of(context).size.width - 32,
          ),
          child: DataTable(
            columns: _buildColumns(),
            rows: _buildRows(),
            headingRowHeight: 56,
            dataRowMinHeight: 52,
            dataRowMaxHeight: 52,
            horizontalMargin: 16,
            columnSpacing: 24,
            showCheckboxColumn: false,
            dividerThickness: 0.5,
            headingTextStyle: const TextStyle(
              fontWeight: FontWeight.bold,
              fontSize: 14,
            ),
            dataTextStyle: const TextStyle(fontSize: 13),
          ),
        ),
      ),
    );
  }

  /// Construye el footer con el conteo de elementos
  Widget _buildFooter() {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 12),
      decoration: BoxDecoration(
        color: Colors.grey.shade50,
        borderRadius: BorderRadius.only(
          bottomLeft: Radius.circular(widget.borderRadius ?? 8.0),
          bottomRight: Radius.circular(widget.borderRadius ?? 8.0),
        ),
        border: Border(
          top: BorderSide(color: Colors.grey.shade200),
        ),
      ),
      child: Row(
        mainAxisAlignment: MainAxisAlignment.spaceBetween,
        children: [
          Text(
            'Total de elementos: ${widget.items.length}',
            style: TextStyle(
              fontSize: 12,
              color: Colors.grey.shade600,
              fontWeight: FontWeight.w500,
            ),
          ),
        ],
      ),
    );
  }

  /// Construye las columnas de la tabla
  List<DataColumn> _buildColumns() {
    return widget.columns.map((column) => DataColumn(
      label: Expanded(
        child: Text(
          column.title,
          overflow: TextOverflow.ellipsis,
        ),
      ),
      tooltip: column.tooltip,
    )).toList();
  }

  /// Construye las filas de la tabla
  List<DataRow> _buildRows() {
    return widget.items.asMap().entries.map((entry) {
      final index = entry.key;
      final item = entry.value;

      final cells = widget.columns.map((column) {
        return DataCell(_buildCellContent(column, item));
      }).toList();

      return DataRow(
        cells: cells,
        color: WidgetStateProperty.resolveWith<Color?>(
          (Set<WidgetState> states) {
            if (states.contains(WidgetState.hovered)) {
              return Theme.of(context).colorScheme.primary.withValues(alpha: 0.05);
            }
            if (index % 2 == 0) {
              return Colors.grey.shade50;
            }
            return null;
          },
        ),
        onSelectChanged: widget.onRowTap != null
            ? (_) => widget.onRowTap!(item)
            : null,
      );
    }).toList();
  }

  /// Construye el contenido de cada celda según su tipo
  Widget _buildCellContent(AppTableColumn<T> column, T item) {
    switch (column.type) {
      case ColumnType.text:
        return _buildTextCell(column, item);
      case ColumnType.icon:
        return _buildIconCell(column, item);
      case ColumnType.dropdown:
        return _buildDropdownCell(column, item);
      case ColumnType.list:
        return _buildListCell(column, item);
      case ColumnType.image:
        return _buildImageCell(column, item);
      case ColumnType.actions:
        return _buildActionsCell(column, item);
      case ColumnType.widget:
        return column.widgetBuilder != null
            ? column.widgetBuilder!(item)
            : const SizedBox.shrink();
    }
  }

  /// Construye celda de texto con tooltip opcional
  Widget _buildTextCell(AppTableColumn<T> column, T item) {
    final text = column.valueExtractor!(item)?.toString() ?? '';
    final widget = Text(
      text,
      overflow: TextOverflow.ellipsis,
      maxLines: 1,
    );

    if (column.showTooltip && text.length > 30) {
      return Tooltip(message: text, child: widget);
    }
    return widget;
  }

  /// Construye celda de icono
  Widget _buildIconCell(AppTableColumn<T> column, T item) {
    final iconData = column.valueExtractor!(item) as IconData?;
    if (iconData == null) return const SizedBox.shrink();

    final icon = Icon(iconData, size: 20, color: column.iconColor);

    if (column.isSelectable) {
      return IconButton(
        icon: icon,
        onPressed: () => column.onCellTap?.call(item),
        tooltip: column.tooltip,
      );
    }
    return icon;
  }

  /// Construye celda dropdown
  Widget _buildDropdownCell(AppTableColumn<T> column, T item) {
    final currentValue = column.valueExtractor!(item)?.toString();

    if (!column.isSelectable) {
      // Mostrar texto con color si está definido
      final color = column.dropdownColors?[currentValue];
      return Text(
        currentValue ?? '',
        style: color != null ? TextStyle(color: color) : null,
      );
    }

    return DropdownButton<String>(
      value: column.dropdownOptions?.contains(currentValue) == true
          ? currentValue : null,
      items: column.dropdownOptions?.map((option) {
        final color = column.dropdownColors?[option];
        return DropdownMenuItem<String>(
          value: option,
          child: Text(
            option,
            style: color != null ? TextStyle(color: color) : null,
          ),
        );
      }).toList(),
      onChanged: (newValue) {
        if (newValue != null && column.onValueChanged != null) {
          column.onValueChanged!(item, newValue);
        }
      },
      underline: const SizedBox.shrink(),
      isDense: true,
    );
  }

  /// Construye celda de lista
  Widget _buildListCell(AppTableColumn<T> column, T item) {
    final list = column.valueExtractor!(item) as List?;
    if (list == null || list.isEmpty) return const Text('Sin datos');

    final displayText = list.length > 3
        ? '${list.take(2).join(', ')}... (+${list.length - 2})'
        : list.join(', ');

    return Tooltip(
      message: list.join('\n'),
      child: Text(displayText, overflow: TextOverflow.ellipsis),
    );
  }

  /// Construye celda de imagen
  Widget _buildImageCell(AppTableColumn<T> column, T item) {
    final imageUrl = column.valueExtractor!(item)?.toString();
    if (imageUrl == null || imageUrl.isEmpty) {
      return const Icon(Icons.image_not_supported, size: 20);
    }

    Widget imageWidget = ClipRRect(
      borderRadius: BorderRadius.circular(4),
      child: Image.network(
        imageUrl,
        width: 32,
        height: 32,
        fit: BoxFit.cover,
        errorBuilder: (context, error, stackTrace) =>
            const Icon(Icons.broken_image, size: 20),
      ),
    );

    if (column.imageText != null) {
      final text = column.imageText!(item);
      imageWidget = Row(
        mainAxisSize: MainAxisSize.min,
        children: [
          imageWidget,
          const SizedBox(width: 8),
          Flexible(child: Text(text, overflow: TextOverflow.ellipsis)),
        ],
      );
    }

    // Mostrar imagen grande al hover
    return MouseRegion(
      onEnter: (_) => _showImagePreview(context, imageUrl),
      child: imageWidget,
    );
  }

  /// Construye celda de acciones
  Widget _buildActionsCell(AppTableColumn<T> column, T item) {
    if (column.actions == null || column.actions!.isEmpty) {
      return const SizedBox.shrink();
    }

    return Row(
      mainAxisSize: MainAxisSize.min,
      children: column.actions!.map((action) {
        return Padding(
          padding: const EdgeInsets.only(right: 4),
          child: IconButton(
            icon: Icon(action.icon, size: 18),
            onPressed: () => action.onPressed(item),
            tooltip: action.tooltip,
            color: action.color,
          ),
        );
      }).toList(),
    );
  }

  /// Muestra preview de imagen (implementación básica)
  void _showImagePreview(BuildContext context, String imageUrl) {
    // TODO: Implementar preview de imagen en overlay
  }
}

/// Enumeración de tipos de columna
enum ColumnType { text, icon, dropdown, list, image, actions, widget }

/// Modelo para definir una columna de la tabla
class AppTableColumn<T> {
  final String title;
  final ColumnType type;
  final dynamic Function(T)? valueExtractor;
  final Widget Function(T)? widgetBuilder; // Para columnas personalizadas
  final String? tooltip;
  final bool isSelectable;
  final bool showTooltip;

  // Para dropdown
  final List<String>? dropdownOptions;
  final void Function(T item, String newValue)? onValueChanged;
  final Map<String, Color>? dropdownColors;

  // Para iconos
  final Color? iconColor;

  // Para imágenes
  final String Function(T)? imageText;

  // Para acciones
  final List<TableAction<T>>? actions;

  // Callback genérico para tap en celda
  final void Function(T)? onCellTap;

  AppTableColumn({
    required this.title,
    required this.type,
    this.valueExtractor,
    this.tooltip,
    this.isSelectable = false,
    this.showTooltip = true,
    this.dropdownOptions,
    this.onValueChanged,
    this.dropdownColors,
    this.iconColor,
    this.imageText,
    this.actions,
    this.onCellTap,
    this.widgetBuilder,
  });

  /// Constructor para columna de texto
  factory AppTableColumn.text(
    String title,
    String Function(T) extractor, {
    String? tooltip,
    bool showTooltip = true,
  }) {
    return AppTableColumn(
      title: title,
      type: ColumnType.text,
      valueExtractor: extractor,
      tooltip: tooltip,
      showTooltip: showTooltip,
    );
  }

  /// Constructor para columna de icono
  factory AppTableColumn.icon(
    String title,
    IconData Function(T) extractor, {
    String? tooltip,
    bool isSelectable = false,
    Color? iconColor,
    void Function(T)? onTap,
  }) {
    return AppTableColumn(
      title: title,
      type: ColumnType.icon,
      valueExtractor: extractor,
      tooltip: tooltip,
      isSelectable: isSelectable,
      iconColor: iconColor,
      onCellTap: onTap,
    );
  }

  /// Constructor para columna dropdown
  factory AppTableColumn.dropdown(
    String title,
    String Function(T) extractor, {
    String? tooltip,
    required List<String> options,
    required void Function(T item, String newValue) onChanged,
    Map<String, Color>? colors,
  }) {
    return AppTableColumn(
      title: title,
      type: ColumnType.dropdown,
      valueExtractor: extractor,
      tooltip: tooltip,
      isSelectable: true,
      dropdownOptions: options,
      onValueChanged: onChanged,
      dropdownColors: colors,
    );
  }

  /// Constructor para columna de lista
  factory AppTableColumn.list(
    String title,
    List Function(T) extractor, {
    String? tooltip,
  }) {
    return AppTableColumn(
      title: title,
      type: ColumnType.list,
      valueExtractor: extractor,
      tooltip: tooltip,
    );
  }

  /// Constructor para columna de imagen
  factory AppTableColumn.image(
    String title,
    String Function(T) extractor, {
    String? tooltip,
    String Function(T)? textExtractor,
  }) {
    return AppTableColumn(
      title: title,
      type: ColumnType.image,
      valueExtractor: extractor,
      tooltip: tooltip,
      imageText: textExtractor,
    );
  }

  /// Constructor para columna de acciones
  factory AppTableColumn.actions(
    String title,
    List<TableAction<T>> actions,
  ) {
    return AppTableColumn(
      title: title,
      type: ColumnType.actions,
      actions: actions,
    );
  }
}

/// Modelo para definir una acción en la columna de acciones
class TableAction<T> {
  final IconData icon;
  final String? tooltip;
  final Color? color;
  final void Function(T) onPressed;

  TableAction({
    required this.icon,
    required this.onPressed,
    this.tooltip,
    this.color,
  });

  /// Acción de editar predefinida
  factory TableAction.edit(void Function(T) onPressed) {
    return TableAction(
      icon: Icons.edit,
      tooltip: 'Editar',
      onPressed: onPressed,
    );
  }

  /// Acción de eliminar predefinida
  factory TableAction.delete(void Function(T) onPressed) {
    return TableAction(
      icon: Icons.delete,
      tooltip: 'Eliminar',
      color: Colors.red,
      onPressed: onPressed,
    );
  }

  /// Acción de ver predefinida
  factory TableAction.view(void Function(T) onPressed) {
    return TableAction(
      icon: Icons.visibility,
      tooltip: 'Ver',
      onPressed: onPressed,
    );
  }
}

// Mantener compatibilidad con código existente
typedef TableColumn<T> = AppTableColumn<T>;
class TableActionButtons<T> extends StatelessWidget {
  final T item;
  final VoidCallback? onEditPressed;
  final VoidCallback? onDeletePressed;
  final List<Widget>? customActions;

  const TableActionButtons({
    Key? key,
    required this.item,
    this.onEditPressed,
    this.onDeletePressed,
    this.customActions,
  }) : super(key: key);

  @override
  Widget build(BuildContext context) {
    final actions = <Widget>[];

    if (onEditPressed != null) {
      actions.add(
        IconButton(
          icon: const Icon(Icons.edit, size: 20),
          onPressed: onEditPressed,
          tooltip: 'Editar',
        ),
      );
    }

    if (onDeletePressed != null) {
      actions.add(
        IconButton(
          icon: const Icon(Icons.delete, size: 20),
          onPressed: onDeletePressed,
          tooltip: 'Eliminar',
          color: Theme.of(context).colorScheme.error,
        ),
      );
    }

    if (customActions != null) {
      actions.addAll(customActions!);
    }

    return Row(
      mainAxisSize: MainAxisSize.min,
      children: actions,
    );
  }
}
