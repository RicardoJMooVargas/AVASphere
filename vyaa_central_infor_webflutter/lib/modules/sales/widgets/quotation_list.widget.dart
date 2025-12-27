import 'package:flutter/material.dart';
import 'package:vyaa_central_infor_webflutter/core/widgets/buttons/app_button.widget.dart';
import '../models/response/quotation_res.module.dart';
import '../../../core/theme/app_colors.dart';
import '../../../core/widgets/tables/app_table.widget.dart';

class QuotationListWidget extends StatefulWidget {
  final List<QuotationRes> quotations;
  final bool isLoading;
  final VoidCallback onRefresh;
  final Function(QuotationRes) onEdit;
  final Function(QuotationRes) onDelete;
  final Function(QuotationRes) onView;
  final Function(QuotationRes, String)? onStatusChange; // Nuevo callback para cambio de status

  const QuotationListWidget({
    super.key,
    required this.quotations,
    required this.isLoading,
    required this.onRefresh,
    required this.onEdit,
    required this.onDelete,
    required this.onView,
    this.onStatusChange, // Opcional
  });

  @override
  State<QuotationListWidget> createState() => _QuotationListWidgetState();
}

class _QuotationListWidgetState extends State<QuotationListWidget> {

  @override
  void initState() {
    super.initState();
  }

  @override
  Widget build(BuildContext context) {
    if (widget.isLoading) {
      return Center(
        child: CircularProgressIndicator(color: AppColors.primaryColor),
      );
    }

    return Column(
      children: [
        // Header con botón de actualizar
        Padding(
          padding: const EdgeInsets.all(16),
          child: Row(
            mainAxisAlignment: MainAxisAlignment.end,
            children: [
              Text('Cotizaciones', style: TextStyle(fontSize: 24, fontWeight: FontWeight.bold)),
              Spacer(),
              AppButton(
                label: 'Actualizar',
                icon: Icons.refresh,
                colorType: 'primary',
                onPressed: widget.onRefresh,
              ),
            ],
          ),
        ),
        // Tabla
        Expanded(
          child: Padding(
            padding: const EdgeInsets.all(16),
            child: AppTable<QuotationRes>(
              items: widget.quotations,
              onRowTap: widget.onView,
              emptyStateWidget: Center(
                child: Column(
                  mainAxisAlignment: MainAxisAlignment.center,
                  children: [
                    Icon(Icons.note_add, size: 64, color: Colors.grey.shade400),
                    const SizedBox(height: 16),
                    Text(
                      'No hay cotizaciones',
                      style: TextStyle(
                        fontSize: 18,
                        fontWeight: FontWeight.bold,
                        color: Colors.grey.shade600,
                      ),
                    ),
                    const SizedBox(height: 8),
                    Text(
                      'Crea una nueva cotización para comenzar',
                      style: TextStyle(fontSize: 14, color: Colors.grey.shade500),
                    ),
                  ],
                ),
              ),
              columns: [
                AppTableColumn.text(
                  'Folio',
                  (q) => q.folio.toString(),
                ),
                AppTableColumn.text(
                  'Cliente',
                  (q) => q.customer.fullName,
                ),
                AppTableColumn.text(
                  'Fecha',
                  (q) => _formatDate(q.saleDate),
                ),
                AppTableColumn.list(
                  'Ejecutivos',
                  (q) => q.salesExecutives,
                ),
                AppTableColumn.text(
                  'Seguimientos',
                  (q) => '${q.followups.length}',
                ),
                AppTableColumn.dropdown(
                  'Estado',
                  (q) => q.status,
                  options: ['Pendiente', 'Aceptado', 'Rechazado'],
                  onChanged: (quotation, newStatus) {
                    // Llamar al callback si existe
                    if (widget.onStatusChange != null) {
                      widget.onStatusChange!(quotation, newStatus);
                    }
                  },
                  colors: {
                    'Pendiente': StatusEnum.getColorByName('Pendiente'),
                    'Aceptado': StatusEnum.getColorByName('Aceptado'),
                    'Rechazado': StatusEnum.getColorByName('Rechazado'),
                  },
                ),
                AppTableColumn.actions(
                  'Acciones',
                  [
                    TableAction.view(widget.onView),
                    TableAction.edit((quotation) => widget.onEdit(quotation)),
                    TableAction.delete(widget.onDelete),
                  ],
                ),
              ],
            ),
          ),
        ),
      ],
    );
  }

  String _formatDate(DateTime date) {
    return '${date.day}/${date.month}/${date.year}';
  }
}
