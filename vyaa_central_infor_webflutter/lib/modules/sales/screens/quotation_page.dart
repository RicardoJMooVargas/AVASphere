import 'package:flutter/material.dart';
import 'package:get/get.dart';
import 'package:vyaa_central_infor_webflutter/core/layouts/home.layout.dart';
import 'package:vyaa_central_infor_webflutter/core/widgets/app_sidebar.widget.dart';
import 'package:vyaa_central_infor_webflutter/modules/sales/controllers/home_screen.getx.dart';
import 'package:vyaa_central_infor_webflutter/modules/sales/models/response/quotation_res.module.dart';
import '../widgets/quotation_list.widget.dart';
import '../widgets/quotation_form.widget.dart';
import '../widgets/quotation_detail.widget.dart';

class QuotationPage extends StatelessWidget {
  const QuotationPage({super.key});

  @override
  Widget build(BuildContext context) {
    final HomeScreenController controller = Get.put(HomeScreenController());

    return AppSidebar(
      sidebarItems: [
        SidebarItem(
          name: 'Home',
          icon: Icons.dashboard,
          onPress: () {
            print('Home pressed');
          },
        ),
        SidebarItem(
          name: 'Refrescar',
          icon: Icons.refresh,
          onPress: () => controller.refreshData(),
        ),
      ],
      userAvatarTooltip: 'John Smith - Administrador',
      showLogout: true,
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

          // Header derecho - Panel de control con estadísticas
          rightHeader: _buildStatsPanel(controller),

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

  Widget _buildStatsPanel(HomeScreenController controller) {
    return Card(
      color: Colors.white,
      elevation: 2,
      margin: const EdgeInsets.all(16),
      child: Container(
        width: double.infinity,
        padding: const EdgeInsets.all(12),
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            Icon(
              Icons.insert_chart_outlined,
              size: 48,
              color: Colors.grey.withOpacity(0.4),
            ),
            const SizedBox(height: 12),
            Obx(
              () => Text(
                'Total: ${controller.quotations.length} cotizaciones',
                style: const TextStyle(
                  fontSize: 16,
                  fontWeight: FontWeight.bold,
                ),
                textAlign: TextAlign.center,
              ),
            ),
          ],
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
