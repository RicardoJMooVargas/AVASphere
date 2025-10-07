import 'package:flutter/material.dart';

class GenericTable<T> extends StatefulWidget {
  final List<T> items;
  final List<TableColumn<T>> columns;
  final Widget Function(T)? actionButtonsBuilder;
  final List<Widget>? topActions;
  final Widget? emptyStateWidget;
  final bool shrinkWrap;
  final ScrollPhysics? physics;
  final void Function(T? item)? onRowSelected;
  final bool selectable; // New parameter to enable/disable row selection
  final Color? selectedRowColor; // Color for selected row
  final void Function(T item)? onRowTap; // New parameter for row tap without selection

  const GenericTable({
    Key? key,
    required this.items,
    required this.columns,
    this.actionButtonsBuilder,
    this.topActions,
    this.emptyStateWidget,
    this.shrinkWrap = false,
    this.physics,
    this.onRowSelected,
    this.selectable = false, // Default to false
    this.selectedRowColor,
    this.onRowTap, // New parameter
  }) : super(key: key);

  @override
  _GenericTableState<T> createState() => _GenericTableState<T>();
}

class _GenericTableState<T> extends State<GenericTable<T>> {
  T? _selectedItem; // Track the currently selected item

  @override
  Widget build(BuildContext context) {
    return LayoutBuilder(
      builder: (context, constraints) {
        // Detect if we're on a mobile device (screen width < 900px)
        final isMobile = MediaQuery.of(context).size.width < 900;
        
        return Column(
          crossAxisAlignment: CrossAxisAlignment.stretch,
          children: [
            if (widget.topActions != null)
              Padding(
                padding: EdgeInsets.only(bottom: isMobile ? 4.0 : 8.0),
                child: Row(
                  mainAxisAlignment: MainAxisAlignment.end,
                  children: widget.topActions!,
                ),
              ),
            if (widget.items.isEmpty)
              widget.emptyStateWidget ??
                  Center(
                    child: Padding(
                      padding: EdgeInsets.all(isMobile ? 8.0 : 16.0),
                      child: Text(
                        'No hay datos disponibles',
                        style: Theme.of(context).textTheme.titleMedium,
                      ),
                    ),
                  )
            else
              Container(
                // On mobile, remove horizontal padding to use full width
                margin: isMobile ? EdgeInsets.zero : null,
                child: SingleChildScrollView(
                  scrollDirection: Axis.horizontal,
                  physics: widget.physics ?? const AlwaysScrollableScrollPhysics(),
                  child: ConstrainedBox(
                    constraints: BoxConstraints(
                      minWidth: isMobile ? 
                        MediaQuery.of(context).size.width : 
                        constraints.maxWidth
                    ),
                    child: DataTable(
                      columns: _buildColumns(),
                      rows: _buildRows(),
                      headingRowHeight: isMobile ? 48 : 56,
                      dataRowHeight: isMobile ? 40 : 48,
                      // Reduce horizontal margin on mobile for more space
                      horizontalMargin: isMobile ? 8 : 16,
                      // Reduce column spacing on mobile
                      columnSpacing: isMobile ? 12 : 24,
                      showCheckboxColumn: false,
                      dividerThickness: 1,
                      dataTextStyle: Theme.of(context).textTheme.bodyMedium?.copyWith(
                        fontSize: isMobile ? 12 : null,
                      ),
                      headingTextStyle: Theme.of(context)
                          .textTheme
                          .titleMedium
                          ?.copyWith(
                            fontWeight: FontWeight.bold,
                            fontSize: isMobile ? 13 : null,
                          ),
                      // Removed border decoration to eliminate black border
                      decoration: null,
                    ),
                  ),
                ),
              ),
          ],
        );
      },
    );
  }

  List<DataColumn> _buildColumns() {
    final columns = widget.columns
        .map((column) => DataColumn(
              label: Expanded(
                child: Text(
                  column.title,
                  overflow: TextOverflow.ellipsis,
                  style: const TextStyle(fontWeight: FontWeight.bold),
                ),
              ),
              tooltip: column.tooltip,
            ))
        .toList();

    if (widget.actionButtonsBuilder != null) {
      columns.add(
        const DataColumn(
          label: Expanded(
            child: Text(
              'Acciones',
              overflow: TextOverflow.ellipsis,
              style: TextStyle(fontWeight: FontWeight.bold),
            ),
          ),
        ),
      );
    }

    return columns;
  }

  List<DataRow> _buildRows() {
    return widget.items.map((item) {
      final isSelected = widget.selectable && _selectedItem == item;
      
      // Si tenemos onRowTap, no usamos onCellTap en las celdas individuales
      final useRowTap = widget.onRowTap != null;
      
      final cells = widget.columns
          .map((column) => DataCell(
                column.builder(item),
                onTap: !useRowTap && column.onCellTap != null
                    ? () => column.onCellTap!(item)
                    : null,
              ))
          .toList();

      if (widget.actionButtonsBuilder != null) {
        cells.add(DataCell(widget.actionButtonsBuilder!(item)));
      }

      // Si tenemos onRowTap o alguna celda tiene onCellTap, agregamos hover effect
      final hasClickableCell = widget.columns.any((column) => column.onCellTap != null);
      final isClickable = useRowTap || hasClickableCell;

      return DataRow(
        cells: cells,
        selected: isSelected,
        color: isSelected
            ? MaterialStateProperty.resolveWith<Color>(
                (Set<MaterialState> states) {
                  return widget.selectedRowColor ??
                      Theme.of(context).colorScheme.primary.withOpacity(0.1);
                },
              )
            : isClickable
                ? MaterialStateProperty.resolveWith<Color?>(
                    (Set<MaterialState> states) {
                      if (states.contains(MaterialState.hovered)) {
                        return Theme.of(context).colorScheme.primary.withOpacity(0.05);
                      }
                      return null;
                    },
                  )
                : null,
        onSelectChanged: widget.selectable
            ? (selected) {
                setState(() {
                  if (selected == null || selected == false) {
                    _selectedItem = null;
                  } else {
                    _selectedItem = item;
                  }
                });
                if (widget.onRowSelected != null) {
                  widget.onRowSelected!(selected == true ? item : null);
                }
              }
            : useRowTap
                ? (selected) {
                    // Para onRowTap, siempre llamamos la función sin cambiar el estado de selección
                    widget.onRowTap!(item);
                  }
                : null,
      );
    }).toList();
  }
}

class TableColumn<T> {
  final String title;
  final Widget Function(T item) builder;
  final String? tooltip;
  final void Function(T item)? onCellTap;

  TableColumn({
    required this.title,
    required this.builder,
    this.tooltip,
    this.onCellTap,
  });
}

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
