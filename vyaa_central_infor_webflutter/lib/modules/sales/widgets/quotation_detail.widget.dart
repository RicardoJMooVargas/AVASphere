import 'package:flutter/material.dart';
import '../models/response/quotation_res.module.dart';
import '../../../Core/theme/app_colors.dart';
import '../../../Core/Widgets/Buttons/app_button.widget.dart';

class QuotationDetailWidget extends StatelessWidget {
  final QuotationRes quotation;
  final VoidCallback onEdit;
  final VoidCallback onClose;
  final VoidCallback onAddFollowup;

  const QuotationDetailWidget({
    super.key,
    required this.quotation,
    required this.onEdit,
    required this.onClose,
    required this.onAddFollowup,
  });

  @override
  Widget build(BuildContext context) {
    return Dialog(
      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
      child: SingleChildScrollView(
        child: Container(
          padding: const EdgeInsets.all(24),
          constraints: BoxConstraints(
            maxWidth: 800,
            maxHeight: MediaQuery.of(context).size.height * 0.8,
          ),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            mainAxisSize: MainAxisSize.min,
            children: [
              // Header
              Row(
                mainAxisAlignment: MainAxisAlignment.spaceBetween,
                children: [
                  Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text(
                        'Cotización #${quotation.folio}',
                        style: const TextStyle(
                          fontSize: 24,
                          fontWeight: FontWeight.bold,
                        ),
                      ),
                      const SizedBox(height: 4),
                      Container(
                        padding: const EdgeInsets.symmetric(
                          horizontal: 12,
                          vertical: 6,
                        ),
                        decoration: BoxDecoration(
                          color: _getStatusBackgroundColor(quotation.status),
                          borderRadius: BorderRadius.circular(16),
                          border: Border.all(
                            color: _getStatusColor(quotation.status),
                            width: 1,
                          ),
                        ),
                        child: Text(
                          quotation.status,
                          style: TextStyle(
                            fontSize: 12,
                            fontWeight: FontWeight.bold,
                            color: _getStatusColor(quotation.status),
                          ),
                        ),
                      ),
                    ],
                  ),
                  AppButton(
                    label: '',
                    icon: Icons.close,
                    iconOnly: true,
                    variant: 'text',
                    colorType: 'secondary',
                    onPressed: onClose,
                  ),
                ],
              ),
              const SizedBox(height: 24),
              Divider(color: Colors.grey.shade300),
              const SizedBox(height: 24),

              // Información General
              _buildSection('Información General', [
                _buildDetailRow('Folio:', quotation.folio.toString()),
                _buildDetailRow('Fecha:', _formatDate(quotation.saleDate)),
                _buildDetailRow('Estado:', quotation.status),
                if (quotation.generalComment.isNotEmpty)
                  _buildDetailRow('Comentario:', quotation.generalComment),
              ]),
              const SizedBox(height: 24),

              // Información del Cliente
              _buildSection('Información del Cliente', [
                _buildDetailRow('Nombre:', quotation.customer.fullName),
                _buildDetailRow('ID Cliente:', quotation.customerId),
                _buildDetailRow('Email:', quotation.customer.email),
              ]),
              const SizedBox(height: 24),

              // Ejecutivos de Ventas
              if (quotation.salesExecutives.isNotEmpty)
                _buildSection('Ejecutivos de Ventas', [
                  Wrap(
                    spacing: 8,
                    runSpacing: 8,
                    children: quotation.salesExecutives
                        .map(
                          (executive) => Container(
                            padding: const EdgeInsets.symmetric(
                              horizontal: 12,
                              vertical: 6,
                            ),
                            decoration: BoxDecoration(
                              color: AppColors.primaryColor.withOpacity(0.1),
                              borderRadius: BorderRadius.circular(16),
                              border: Border.all(
                                color: AppColors.primaryColor.withOpacity(0.3),
                              ),
                            ),
                            child: Text(
                              executive,
                              style: TextStyle(
                                fontSize: 12,
                                color: AppColors.primaryColor,
                                fontWeight: FontWeight.w500,
                              ),
                            ),
                          ),
                        )
                        .toList(),
                  ),
                ]),
              const SizedBox(height: 24),

              // Seguimientos
              _buildSection('Seguimientos', [
                if (quotation.followups.isEmpty)
                  Center(
                    child: Text(
                      'No hay seguimientos registrados',
                      style: TextStyle(
                        color: Colors.grey.shade500,
                        fontStyle: FontStyle.italic,
                      ),
                    ),
                  )
                else
                  Column(
                    children: quotation.followups.map((followup) {
                      return Container(
                        margin: const EdgeInsets.only(bottom: 12),
                        padding: const EdgeInsets.all(12),
                        decoration: BoxDecoration(
                          color: Colors.grey.shade50,
                          borderRadius: BorderRadius.circular(8),
                          border: Border.all(color: Colors.grey.shade200),
                        ),
                        child: Column(
                          crossAxisAlignment: CrossAxisAlignment.start,
                          children: [
                            Row(
                              mainAxisAlignment: MainAxisAlignment.spaceBetween,
                              children: [
                                Text(
                                  _formatDate(followup.date),
                                  style: const TextStyle(
                                    fontSize: 12,
                                    fontWeight: FontWeight.bold,
                                  ),
                                ),
                                Text(
                                  followup.userId,
                                  style: TextStyle(
                                    fontSize: 12,
                                    color: Colors.grey.shade600,
                                  ),
                                ),
                              ],
                            ),
                            const SizedBox(height: 8),
                            Text(
                              followup.comment,
                              style: const TextStyle(fontSize: 13),
                            ),
                          ],
                        ),
                      );
                    }).toList(),
                  ),
              ]),
              const SizedBox(height: 24),

              // Botones de acción
              Row(
                mainAxisAlignment: MainAxisAlignment.end,
                children: [
                  AppButton(
                    label: 'Cerrar',
                    icon: Icons.close,
                    colorType: 'secondary',
                    onPressed: onClose,
                  ),
                  const SizedBox(width: 12),
                  AppButton(
                    label: 'Agregar Seguimiento',
                    icon: Icons.add_comment,
                    customColor: Colors.blue,
                    onPressed: onAddFollowup,
                  ),
                  const SizedBox(width: 12),
                  AppButton(
                    label: 'Editar',
                    icon: Icons.edit,
                    colorType: 'primary',
                    onPressed: onEdit,
                  ),
                ],
              ),
            ],
          ),
        ),
      ),
    );
  }

  Widget _buildSection(String title, List<Widget> children) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Text(
          title,
          style: const TextStyle(fontSize: 16, fontWeight: FontWeight.bold),
        ),
        const SizedBox(height: 12),
        ...children,
      ],
    );
  }

  Widget _buildDetailRow(String label, String value) {
    return Padding(
      padding: const EdgeInsets.only(bottom: 8),
      child: Row(
        mainAxisAlignment: MainAxisAlignment.spaceBetween,
        children: [
          Text(
            label,
            style: TextStyle(
              fontSize: 13,
              fontWeight: FontWeight.w600,
              color: Colors.grey.shade700,
            ),
          ),
          Expanded(
            child: Text(
              value,
              textAlign: TextAlign.end,
              style: const TextStyle(fontSize: 13),
              overflow: TextOverflow.ellipsis,
            ),
          ),
        ],
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
