import 'package:flutter/material.dart';
import 'package:get/get.dart';
import '../models/response/quotation_res.module.dart';
import '../../../Core/theme/app_colors.dart';

class QuotationListWidget extends StatelessWidget {
  final List<QuotationRes> quotations;
  final bool isLoading;
  final VoidCallback onRefresh;
  final Function(QuotationRes) onEdit;
  final Function(QuotationRes) onDelete;
  final Function(QuotationRes) onView;

  const QuotationListWidget({
    super.key,
    required this.quotations,
    required this.isLoading,
    required this.onRefresh,
    required this.onEdit,
    required this.onDelete,
    required this.onView,
  });

  @override
  Widget build(BuildContext context) {
    if (isLoading) {
      return Center(
        child: CircularProgressIndicator(color: AppColors.primaryColor),
      );
    }

    if (quotations.isEmpty) {
      return Center(
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
            const SizedBox(height: 24),
            ElevatedButton.icon(
              onPressed: onRefresh,
              style: ElevatedButton.styleFrom(
                backgroundColor: AppColors.primaryColor,
                foregroundColor: Colors.white,
                padding: const EdgeInsets.symmetric(
                  horizontal: 24,
                  vertical: 12,
                ),
              ),
              icon: const Icon(Icons.refresh),
              label: const Text('Actualizar'),
            ),
          ],
        ),
      );
    }

    return ListView.builder(
      padding: const EdgeInsets.all(16),
      itemCount: quotations.length,
      itemBuilder: (context, index) {
        final quotation = quotations[index];
        return _buildQuotationCard(context, quotation);
      },
    );
  }

  Widget _buildQuotationCard(BuildContext context, QuotationRes quotation) {
    return Card(
      margin: const EdgeInsets.only(bottom: 12),
      elevation: 2,
      child: Padding(
        padding: const EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            // Header con folio y estado
            Row(
              mainAxisAlignment: MainAxisAlignment.spaceBetween,
              children: [
                Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      'Folio: ${quotation.folio}',
                      style: const TextStyle(
                        fontSize: 16,
                        fontWeight: FontWeight.bold,
                      ),
                    ),
                    const SizedBox(height: 4),
                    Text(
                      quotation.status,
                      style: TextStyle(
                        fontSize: 12,
                        color: _getStatusColor(quotation.status),
                        fontWeight: FontWeight.w500,
                      ),
                    ),
                  ],
                ),
                _buildStatusBadge(quotation.status),
              ],
            ),
            const SizedBox(height: 12),
            Divider(color: Colors.grey.shade300),
            const SizedBox(height: 12),

            // Información del cliente
            Row(
              children: [
                Icon(Icons.person, size: 18, color: Colors.grey.shade600),
                const SizedBox(width: 8),
                Expanded(
                  child: Text(
                    quotation.customer.fullName,
                    style: const TextStyle(fontSize: 14),
                    overflow: TextOverflow.ellipsis,
                  ),
                ),
              ],
            ),
            const SizedBox(height: 8),

            // Fecha de venta
            Row(
              children: [
                Icon(
                  Icons.calendar_today,
                  size: 18,
                  color: Colors.grey.shade600,
                ),
                const SizedBox(width: 8),
                Text(
                  _formatDate(quotation.saleDate),
                  style: const TextStyle(fontSize: 14),
                ),
              ],
            ),
            const SizedBox(height: 8),

            // Ejecutivos de ventas
            if (quotation.salesExecutives.isNotEmpty)
              Row(
                children: [
                  Icon(Icons.group, size: 18, color: Colors.grey.shade600),
                  const SizedBox(width: 8),
                  Expanded(
                    child: Text(
                      quotation.salesExecutives.join(', '),
                      style: const TextStyle(fontSize: 14),
                      overflow: TextOverflow.ellipsis,
                    ),
                  ),
                ],
              ),
            const SizedBox(height: 8),

            // Seguimientos
            if (quotation.followups.isNotEmpty)
              Row(
                children: [
                  Icon(Icons.comment, size: 18, color: Colors.grey.shade600),
                  const SizedBox(width: 8),
                  Text(
                    '${quotation.followups.length} seguimiento(s)',
                    style: const TextStyle(fontSize: 14),
                  ),
                ],
              ),

            const SizedBox(height: 16),
            Divider(color: Colors.grey.shade300),
            const SizedBox(height: 12),

            // Botones de acción
            Row(
              mainAxisAlignment: MainAxisAlignment.end,
              children: [
                TextButton.icon(
                  onPressed: () => onView(quotation),
                  icon: const Icon(Icons.visibility),
                  label: const Text('Ver'),
                  style: TextButton.styleFrom(
                    foregroundColor: AppColors.primaryColor,
                  ),
                ),
                const SizedBox(width: 8),
                TextButton.icon(
                  onPressed: () => onEdit(quotation),
                  icon: const Icon(Icons.edit),
                  label: const Text('Editar'),
                  style: TextButton.styleFrom(foregroundColor: Colors.blue),
                ),
                const SizedBox(width: 8),
                TextButton.icon(
                  onPressed: () => onDelete(quotation),
                  icon: const Icon(Icons.delete),
                  label: const Text('Eliminar'),
                  style: TextButton.styleFrom(foregroundColor: Colors.red),
                ),
              ],
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildStatusBadge(String status) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 6),
      decoration: BoxDecoration(
        color: _getStatusBackgroundColor(status),
        borderRadius: BorderRadius.circular(16),
        border: Border.all(color: _getStatusColor(status), width: 1),
      ),
      child: Text(
        status,
        style: TextStyle(
          fontSize: 12,
          fontWeight: FontWeight.bold,
          color: _getStatusColor(status),
        ),
      ),
    );
  }

  Color _getStatusColor(String status) {
    switch (status.toLowerCase()) {
      case 'pending':
        return Colors.orange;
      case 'approved':
        return Colors.green;
      case 'rejected':
        return Colors.red;
      case 'converted':
        return Colors.blue;
      default:
        return Colors.grey;
    }
  }

  Color _getStatusBackgroundColor(String status) {
    return _getStatusColor(status).withOpacity(0.1);
  }

  String _formatDate(DateTime date) {
    return '${date.day}/${date.month}/${date.year}';
  }
}
