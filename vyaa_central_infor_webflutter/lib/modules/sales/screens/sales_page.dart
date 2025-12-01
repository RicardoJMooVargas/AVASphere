import 'package:flutter/material.dart';
import 'package:get/get.dart';
import '../../../core/layouts/home.layout.dart';
import '../../../Core/theme/app_colors.dart';
import '../controllers/home_screen.getx.dart';
import '../models/response/quotation_res.module.dart';
import '../widgets/quotation_form.widget.dart';
import '../widgets/quotation_list.widget.dart';
import '../widgets/quotation_detail.widget.dart';

class SalesPage extends StatelessWidget {
  const SalesPage({super.key});

  @override
  Widget build(BuildContext context) {
    // Usar Get.find si ya existe, o Get.put si no existe
    final HomeScreenController controller =
        Get.isRegistered<HomeScreenController>()
        ? Get.find<HomeScreenController>()
        : Get.put(HomeScreenController());

    return Scaffold(
      backgroundColor: Colors.grey[200],
      appBar: AppBar(
        title: const Text('Módulo de Ventas'),
        backgroundColor: AppColors.primaryColor,
        foregroundColor: Colors.white,
        elevation: 0,
        automaticallyImplyLeading: false,
      ),
      body: Container(
        color: Colors.grey[200],
        child: HomeLayout(
          backgroundColor: Colors.grey[200],
          borderWidth: 0,

          // Columna izquierda - Formulario de creación
          leftColumn: Obx(
            () => QuotationFormWidget(
              quotationModel: controller.currentQuotation.value,
              isLoading: controller.isCreating.value,
              onSave: () => controller.createQuotation(),
              onCancel: () => controller.resetForm(),
            ),
          ),

          // Header derecho - vacío por ahora
          rightHeader: const SizedBox.shrink(),

          // Body derecho - Lista de cotizaciones
          rightBody: Obx(
            () => QuotationListWidget(
              quotations: controller.quotations,
              isLoading: controller.isLoading.value,
              onRefresh: () => controller.refreshData(),
              onEdit: (quotation) =>
                  _showEditDialog(context, controller, quotation),
              onDelete: (quotation) =>
                  _showDeleteConfirmation(context, controller, quotation),
              onView: (quotation) => showDialog(
                context: context,
                builder: (context) => QuotationDetailWidget(
                  quotation: quotation,
                  onEdit: () {
                    Navigator.of(context).pop();
                    _showEditDialog(context, controller, quotation);
                  },
                  onClose: () => Navigator.of(context).pop(),
                  onAddFollowup: () {
                    // TODO: Implementar agregar seguimiento
                  },
                ),
              ),
            ),
          ),
        ),
      ),
    );
  }

  void _showEditDialog(
    BuildContext context,
    HomeScreenController controller,
    QuotationRes quotation,
  ) {
    // TODO: Implementar edición de cotización
    ScaffoldMessenger.of(context).showSnackBar(
      SnackBar(content: Text('Editar cotización: ${quotation.folio}')),
    );
  }

  void _showDeleteConfirmation(
    BuildContext context,
    HomeScreenController controller,
    QuotationRes quotation,
  ) {
    showDialog(
      context: context,
      builder: (context) => AlertDialog(
        title: const Text('Confirmar eliminación'),
        content: Text(
          '¿Estás seguro de que deseas eliminar la cotización #${quotation.folio}?',
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(context),
            child: const Text('Cancelar'),
          ),
          TextButton(
            onPressed: () {
              // TODO: Implementar eliminación
              Navigator.pop(context);
              ScaffoldMessenger.of(context).showSnackBar(
                SnackBar(
                  content: Text('Cotización #${quotation.folio} eliminada'),
                ),
              );
            },
            child: const Text('Eliminar', style: TextStyle(color: Colors.red)),
          ),
        ],
      ),
    );
  }
}
